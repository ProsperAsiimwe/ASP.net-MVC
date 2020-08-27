using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.ClassLevels;
using MagicApps.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TwitterBootstrap3;

namespace SteppingStone.WebUI.Infrastructure.Helpers
{
    public class ClassLevelHelper
    {
        private ApplicationDbContext db;
        private ApplicationUserManager UserManager;

        int ClassLevelId;

        public ClassLevel ClassLevel { get; private set; }

        public string ServiceUserId { get; set; }

        public ClassLevelHelper()
        {
            Set();
        }

        public ClassLevelHelper(int ClassLevelId)
        {
            Set();

            this.ClassLevelId = ClassLevelId;
            this.ClassLevel = db.ClassLevels.Find(ClassLevelId);
        }

        public ClassLevelHelper(ClassLevel ClassLevel)
        {
            Set();

            this.ClassLevelId = ClassLevel.ClassLevelId;
            this.ClassLevel = ClassLevel;
        }

        private void Set()
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public ClassLevelListViewModel GetClassLevelList(SearchClassesModel searchModel, int page = 1)
        {
            int pageSize = 20;

            if (page < 1)
            {
                page = 1;
            }

            IEnumerable<ClassLevel> records = db.ClassLevels.ToList();

            // Remove any default information
            //searchModel.ParseRouteInfo();

            if (!String.IsNullOrEmpty(searchModel.Name))
            {
                string name = searchModel.Name.ToLower();
                records = records.Where(x => x.HasStudent(searchModel.Name.ToLower()));
            }
            
            if (searchModel.Level.HasValue)
            {
                records = records.Where(x => x.SchoolLevel == searchModel.Level);
            }

            if (searchModel.StudyMode.HasValue)
            {
                records = records.Where(x => x.StudyMode == searchModel.StudyMode);
            }

            records = records.Where(x => !x.Deleted.HasValue);

            return new ClassLevelListViewModel
            {
                Classes = records
                    .OrderByDescending(o => o.Level)
                    .ThenBy(o => o.ClassLevelId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize),
                SearchModel = searchModel,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = records.Count()
                }
            };
        }

        public async Task<UpsertModel> UpsertClassLevel(UpsertMode mode, ClassLevelViewModel model)
        {
            var upsert = new UpsertModel();

            try
            {
                Activity activity;
                string title;
                System.Text.StringBuilder builder;
                
                // Apply changes
                ClassLevel = model.ParseAsEntity(ClassLevel);

                builder = new System.Text.StringBuilder();

                if (model.ClassLevelId == 0)
                {
                    db.ClassLevels.Add(ClassLevel);
                                        
                    title = "Class Level Recorded";
                    builder.Append("A Class Level record has been made").AppendLine();
                }
                else
                {
                    db.Entry(ClassLevel).State = System.Data.Entity.EntityState.Modified;

                    title = "Class Level Updated";
                    builder.Append("The following changes have been made to the Class Level details");

                    if (mode == UpsertMode.Admin)
                    {
                        builder.Append(" (by the Admin)");
                    }

                    builder.Append(":").AppendLine();
                }

                await db.SaveChangesAsync();

                ClassLevelId = ClassLevel.ClassLevelId;

                // Save activity now so we have a ClassLevelId. Not ideal, but hey
                activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                if (model.ClassLevelId == 0)
                {
                    upsert.ErrorMsg = "Class Level record created successfully";
                }
                else
                {
                    upsert.ErrorMsg = "Class Level record updated successfully";
                }

                upsert.RecordId = ClassLevel.ClassLevelId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Update ClassLevel Error", ex);
            }

            return upsert;
        }

        public async Task<UpsertModel> DeleteClassLevel()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Class Level Deleted";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following ClassLevel has been deleted:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Class: {0}", ClassLevel.GetClass());

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                // Remove ClassLevel
                if(ClassLevel.Students.Count() <= 0)
                {
                    db.ClassLevels.Remove(ClassLevel);
                    db.Entry(ClassLevel).State = System.Data.Entity.EntityState.Deleted;
                }else
                {
                    ClassLevel.Deleted = DateTime.Now;
                    db.Entry(ClassLevel).State = System.Data.Entity.EntityState.Modified;
                }
                


                upsert.ErrorMsg = string.Format("Class Level: '{0}' deleted successfully", ClassLevel.GetClass());
                upsert.RecordId = ClassLevel.ClassLevelId.ToString();

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Delete ClassLevel Error", ex);
            }

            return upsert;
        }

        public Activity CreateActivity(string title, string description)
        {
            var activity = new Activity
            {
                Title = title,
                Description = description,
                RecordedById = ServiceUserId
            };

            if (ClassLevel != null)
            {
                activity.ClassLevelId = ClassLevelId;
            }

            db.Activities.Add(activity);
            return activity;
        }

        public static ButtonStyle GetButtonStyle(string css)
        {
            ButtonStyle button_css;

            if (css == "warning")
            {
                button_css = ButtonStyle.Warning;
            }
            else if (css == "success")
            {
                button_css = ButtonStyle.Success;
            }
            else if (css == "info")
            {
                button_css = ButtonStyle.Info;
            }
            else
            {
                button_css = ButtonStyle.Danger;
            }

            return button_css;
        }

        private void RecordException(string title, Exception ex)
        {
            var activity = CreateActivity(title, ex.Message);

            if (ClassLevel != null)
            {
                activity.UserId = ServiceUserId;
                activity.ClassLevelId = ClassLevel.ClassLevelId;
            }
            db.SaveChanges();
        }
    }
}