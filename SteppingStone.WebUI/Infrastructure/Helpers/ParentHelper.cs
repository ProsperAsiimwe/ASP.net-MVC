using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.Parents;
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
    public class ParentHelper
    {
        private ApplicationDbContext db;
        private ApplicationUserManager UserManager;

        int ParentId;

        public Parent Parent { get; private set; }

        public string ServiceUserId { get; set; }

        public ParentHelper()
        {
            Set();
        }

        public ParentHelper(int ParentId)
        {
            Set();

            this.ParentId = ParentId;
            this.Parent = db.Parents.Find(ParentId);
        }

        public ParentHelper(Parent Parent)
        {
            Set();

            this.ParentId = Parent.ParentId;
            this.Parent = Parent;
        }

        private void Set()
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public ParentListViewModel GetParentList(SearchParentsModel searchModel, int page = 1)
        {
            int pageSize = 20;

            if (page < 1)
            {
                page = 1;
            }

            IEnumerable<Parent> records = db.Parents.ToList();

            // Remove any default information
            //searchModel.ParseRouteInfo();

            if (!String.IsNullOrEmpty(searchModel.Name))
            {
                string name = searchModel.Name.ToLower();
                records = records.SelectMany(x => x.Students).Where(y => y.Student.Matches(name)).Select(m => m.Parent);
            }

            if (!String.IsNullOrEmpty(searchModel.ParentName))
            {
                string parentName = searchModel.ParentName.ToLower();
                records = records.Where(x => x.Matches(parentName));
            }

            records = records.Where(x => !x.Terminated.HasValue);

            return new ParentListViewModel
            {
                Parents = records
                    .OrderByDescending(o => o.Added)
                    .ThenBy(o => o.FullName)
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

        public async Task<UpsertModel> UpsertParent(UpsertMode mode, ParentViewModel model)
        {
            var upsert = new UpsertModel();

            try
            {
                Activity activity;
                string title;
                System.Text.StringBuilder builder;

                // Get Student
                //var student = db.Students.FirstOrDefault(x => x.StudentId == model.StudentId);

                // Apply changes
                Parent = model.ParseAsEntity(Parent);

                builder = new System.Text.StringBuilder();

                if (model.ParentId == 0)
                {
                    db.Parents.Add(Parent);
                    

                    title = "Parent Recorded";
                    builder.Append("A Parent record has been made : " + model.FirstName + " "+model.LastName).AppendLine();
                }
                else
                {
                    db.Entry(Parent).State = System.Data.Entity.EntityState.Modified;

                    title = "Parent Updated";
                    builder.Append("The following changes have been made to the Parent details");

                    if (mode == UpsertMode.Admin)
                    {
                        builder.Append(" (by the Admin)");
                    }

                    builder.Append(":").AppendLine();
                }

                await db.SaveChangesAsync();

                ParentId = Parent.ParentId;

                // update student parent relationship
                UpdateStudents(model.SelectedStudents);

                // Save activity now so we have a ParentId. Not ideal, but hey
                activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                if (model.ParentId == 0)
                {
                    upsert.ErrorMsg = "Parent record created successfully";
                }
                else
                {
                    upsert.ErrorMsg = "Parent record updated successfully";
                }

                upsert.RecordId = Parent.ParentId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Update Parent Error", ex);
            }

            return upsert;
        }

        public void UpdateStudents(int[] students)
        {
            // add students who didnt exists before
            foreach (var studentId in students)
            {
                var exists = Parent.Students.FirstOrDefault(x => x.StudentId == studentId) != null;

                if (!exists)
                {
                    var student = new StudentParent(studentId, ParentId);

                    Parent.Students.Add(student);
                }                
            }

            // remove old students who arent in the new list
            foreach(var student in Parent.Students)
            {
                if (!students.Contains(student.StudentId))
                {
                    Parent.Students.Remove(student);
                }
            }

            db.Entry(Parent).State = System.Data.Entity.EntityState.Modified;
        }

        public async Task<UpsertModel> RestoreParent()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Parent Restored";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Parent has been restored:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Parent name: {0}", Parent.FullName)
                    .AppendLine().AppendFormat("No of Students: {0}", Parent.Students.Count);

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                // Remove Parent
                Parent.Terminated = null;
                db.Entry(Parent).State = System.Data.Entity.EntityState.Modified;

                await db.SaveChangesAsync();

                upsert.ErrorMsg = string.Format("Parent: '{0}' restored successfully", Parent.FullName);
                upsert.RecordId = Parent.ParentId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Delete Parent Error", ex);
            }

            return upsert;
        }

        public async Task<UpsertModel> DeleteParent()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Parent Terminated";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Parent has been terminated:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Parent name: {0}", Parent.FullName)
                    .AppendLine().AppendFormat("No of Students: {0}", Parent.Students.Count);

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                // Remove Parent
                
                if(Parent.Students.Count() > 0)
                {
                    Parent.Terminated = DateTime.Now;
                    db.Entry(Parent).State = System.Data.Entity.EntityState.Modified;
                }else
                {
                    db.Parents.Remove(Parent);
                    db.Entry(Parent).State = System.Data.Entity.EntityState.Deleted;
                }


                await db.SaveChangesAsync();

                upsert.ErrorMsg = string.Format("Parent - '{0}' terminated successfully", Parent.FullName);
                upsert.RecordId = Parent.ParentId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Delete Parent Error", ex);
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

            if (Parent != null)
            {
                activity.ParentId = ParentId;
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

            if (Parent != null)
            {
                activity.UserId = ServiceUserId;
                activity.ParentId = Parent.ParentId;
            }
            db.SaveChanges();
        }
    }
}