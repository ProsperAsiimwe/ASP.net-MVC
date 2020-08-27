using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.Terms;
using MagicApps.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TwitterBootstrap3;
using SteppingStone.Domain.Models;

namespace SteppingStone.WebUI.Infrastructure.Helpers
{
    public class TermHelper
    {
        private ApplicationDbContext db;
        private ApplicationUserManager UserManager;

        int TermId;

        public Term Term { get; private set; }

        public string ServiceUserId { get; set; }

        public TermHelper()
        {
            Set();
        }

        public TermHelper(int TermId)
        {
            Set();

            this.TermId = TermId;
            this.Term = db.Terms.Find(TermId);
        }

        public TermHelper(Term Term)
        {
            Set();

            this.TermId = Term.TermId;
            this.Term = Term;
        }

        private void Set()
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public TermListViewModel GetTermList(SearchTermsModel searchModel, int page = 1)
        {
            int pageSize = 20;

            if (page < 1)
            {
                page = 1;
            }

            IEnumerable<Term> records = db.Terms.ToList();

            // Remove any default information
            //searchModel.ParseRouteInfo();

            if (!String.IsNullOrEmpty(searchModel.Name))
            {
                string name = searchModel.Name.ToLower();
                records = records.Where(x => x.GetTerm().ToLower().Contains(name));
            }

            if (searchModel.StartDate.HasValue)
            {
                records = records.Where(x => x.StartDate >= searchModel.StartDate.Value);
            }

            if (searchModel.EndDate.HasValue)
            {
                records = records.Where(x => x.StartDate <= searchModel.EndDate.Value);
            }

            if (searchModel.Period.HasValue)
            {
                records = records.Where(x => x.Position == searchModel.Period);
            }

            records = records.Where(x => !x.Deleted.HasValue);

            return new TermListViewModel
            {
                Terms = records
                    .OrderByDescending(o => o.StartDate)
                    .ThenBy(o => o.TermId)
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

        public async Task<UpsertModel> UpsertTerm(UpsertMode mode, TermViewModel model)
        {
            var upsert = new UpsertModel();

            try
            {
                Activity activity;
                string title;
                System.Text.StringBuilder builder;

                // Apply changes
                Term = model.ParseAsEntity(Term);

                builder = new System.Text.StringBuilder();

                if (model.TermId == 0)
                {
                    db.Terms.Add(Term);

                    title = "Term Recorded";
                    builder.Append("A Term record has been made").AppendLine();
                }
                else
                {
                    db.Entry(Term).State = System.Data.Entity.EntityState.Modified;

                    title = "Term Updated";
                    builder.Append("The following changes have been made to the Term details");

                    if (mode == UpsertMode.Admin)
                    {
                        builder.Append(" (by the Admin)");
                    }

                    builder.Append(":").AppendLine();
                }

                await db.SaveChangesAsync();

                TermId = Term.TermId;

                // Save activity now so we have a TermId. Not ideal, but hey
                activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                if (model.TermId == 0)
                {
                    upsert.ErrorMsg = "Term record created successfully";
                }
                else
                {
                    upsert.ErrorMsg = "Term record updated successfully";
                }

                upsert.RecordId = Term.TermId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Update Term Error", ex);
            }

            return upsert;
        }

        public void UpdateStudentsOldDebt(Term term)
        {
            var students = db.Students.ToList().Where(p => p.CurrentTermId.HasValue && p.CurrentTermId.Value == term.TermId);

            foreach (var student in students)
            {
                // get outstanding and top up on old debt.
                if (student.HasOutstanding)
                {
                    student.OldDebt = student.OldDebt + student.TermBalance(term.TermId);
                    student.RegistrationFee = 0;
                    student.Uniforms = 0;
                    db.Entry(student).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

            }

            term.Updated = UgandaDateTime.DateNow();
            db.SaveChanges();

        }

        public async Task<UpsertModel> DeleteTerm()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Term Deleted";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Term has been deleted:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Term: {0}", Term.GetTerm());

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                if (Term.Payments.Count() > 0)
                {
                    Term.Deleted = DateTime.Now;
                    db.Entry(Term).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    //reset students
                    Term.ResetStudents();

                    db.Terms.Remove(Term);
                    db.Entry(Term).State = System.Data.Entity.EntityState.Deleted;
                }


                upsert.ErrorMsg = string.Format("Term: '{0}' deleted successfully", Term.GetTerm());
                upsert.RecordId = Term.TermId.ToString();

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Delete Term Error", ex);
            }

            return upsert;
        }

        public async Task<UpsertModel> EndTerm()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Term Ended";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Term has been ended:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Term: {0}", Term.GetTerm());

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                UpdateStudentsOldDebt(Term);
                Term.IsCurrentTerm = false;
                db.Entry(Term).State = System.Data.Entity.EntityState.Modified;


                upsert.ErrorMsg = string.Format("Term: '{0}' ended successfully", Term.GetTerm());
                upsert.RecordId = Term.TermId.ToString();

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
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

            if (Term != null)
            {
                activity.TermId = TermId;
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

            if (Term != null)
            {
                activity.UserId = ServiceUserId;
                activity.TermId = Term.TermId;
            }
            db.SaveChanges();
        }
    }
}