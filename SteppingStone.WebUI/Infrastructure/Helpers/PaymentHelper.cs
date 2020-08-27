using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.Payments;
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
    public class PaymentHelper
    {
        private ApplicationDbContext db;
        private ApplicationUserManager UserManager;

        int PaymentId;

        public Payment Payment { get; private set; }

        public string ServiceUserId { get; set; }

        public PaymentHelper()
        {
            Set();
        }

        public PaymentHelper(int PaymentId)
        {
            Set();

            this.PaymentId = PaymentId;
            this.Payment = db.Payments.Find(PaymentId);
        }

        public PaymentHelper(Payment Payment)
        {
            Set();

            this.PaymentId = Payment.PaymentId;
            this.Payment = Payment;
        }

        private void Set()
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public PaymentListViewModel GetPaymentList(SearchPaymentsModel searchModel, int page = 1)
        {
            int pageSize = 20;

            if (page < 1)
            {
                page = 1;
            }

            IEnumerable<Payment> records = db.Payments.ToList();

            // Remove any default information
            //searchModel.ParseRouteInfo();

            if (!String.IsNullOrEmpty(searchModel.Name))
            {
                string name = searchModel.Name.ToLower();
                records = records.Where(x => x.Student.FirstName.ToLower().Contains(name) || x.Student.LastName.ToLower().Contains(name));
            }

            if (!String.IsNullOrEmpty(searchModel.ParentName))
            {
                string parentName = searchModel.ParentName.ToLower();
                records = records.SelectMany(x => x.Student.Parents).Where(y => y.Parent.FirstName.ToLower().Contains(parentName) || y.Parent.LastName.ToLower().Contains(parentName)).SelectMany(m => m.Student.Payments);                
            }

            if (!String.IsNullOrEmpty(searchModel.Depositor))
            {
                string depositorName = searchModel.Depositor.ToLower();
                records = records.Where(x => x.PaidInBy.ToLower().Contains(depositorName));
            }

            if (searchModel.SlipNo.HasValue)
            {
                string slipNo = searchModel.SlipNo.ToString();
                records = records.Where(x => x.SlipNo.ToString().Contains(slipNo));
            }

            if (searchModel.Bank.HasValue)
            {
                records = records.Where(x => x.BankId == searchModel.Bank.Value);
            }

            if (searchModel.Term.HasValue)
            {
                records = records.Where(x => x.TermId == searchModel.Term.Value);
            }

            if (searchModel.ClassLevel.HasValue)
            {
                records = records.Where(x => x.ClassLevelId == searchModel.ClassLevel.Value);
            }

            if (searchModel.StartDate.HasValue)
            {
                records = records.Where(x => x.Date >= searchModel.StartDate.Value);
            }

            if (searchModel.EndDate.HasValue)
            {
                records = records.Where(x => x.Date <= searchModel.EndDate.Value);
            }

            // get only that havent been deleted
            records = records.Where(x => !x.Deleted.HasValue);

            var totalPaid = records.Sum(x => x.Amount);
                     

            return new PaymentListViewModel
            {
                Payments = records
                    .OrderByDescending(o => o.Date)
                    .ThenBy(o => o.ClassLevelId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize),
                TotalPaid = totalPaid,
                SearchModel = searchModel,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = records.Count()
                }
            };
        }

        public async Task<UpsertModel> UpsertPayment(UpsertMode mode, PaymentViewModel model)
        {
            var upsert = new UpsertModel();

            try
            {
                Activity activity;
                string title;
                System.Text.StringBuilder builder;

                // Get Student
                var student = db.Students.Single(x => x.StudentId == model.StudentId);

                // get term
                var term = db.Terms.FirstOrDefault(x => x.TermId == model.TermId);

                // Apply changes
                Payment = model.ParseAsEntity(Payment);

                builder = new System.Text.StringBuilder();

                if (model.PaymentId == 0)
                {
                    student.Payments.Add(Payment);

                    if (term.IsCurrentTerm)
                    {
                        // incase student hasn't been promoted, promote. Carry on debt if any
                        if ((student.CurrentLevelId.HasValue && student.CurrentLevelId != Payment.ClassLevelId) || !student.CurrentLevelId.HasValue)
                        {
                            student.CurrentLevelId = Payment.ClassLevelId;
                            db.Entry(student).State = System.Data.Entity.EntityState.Modified;
                        }

                        if (!student.CurrentTermId.HasValue || (student.CurrentTermId.HasValue && student.CurrentTermId != Payment.TermId))
                        {
                            student.CurrentTermId = Payment.TermId;
                            db.Entry(student).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    

                    title = "Payment Recorded";
                    builder.Append("A Payment record has been made for: " + student.FullName).AppendLine();
                }
                else
                {
                    db.Entry(Payment).State = System.Data.Entity.EntityState.Modified;

                    title = "Payment Updated";
                    builder.Append("The following changes have been made to the Payment details");

                    if (mode == UpsertMode.Admin)
                    {
                        builder.Append(" (by the Admin)");
                    }

                    builder.Append(":").AppendLine();
                }
                
                await db.SaveChangesAsync();

                PaymentId = Payment.PaymentId;


                // change parent next reminder date
                if (!Payment.Student.NoParents)
                {
                    var parents = Payment.Student.Parents.Select(x => x.Parent);

                    DateTime? nextDate;

                    if (Payment.Student.HasOutstanding)
                    {
                        nextDate = Payment.Date.AddDays(21);
                    }
                    else
                    {
                        nextDate = null;
                    }

                    foreach (var parent in parents)
                    {
                        if (Payment.Term.IsCurrentTerm)
                        {
                            parent.RemindDate = nextDate;
                            db.Entry(parent).State = System.Data.Entity.EntityState.Modified;
                        }

                    }
                }

                // Save activity now so we have a PaymentId. Not ideal, but hey
                activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                if (model.PaymentId == 0)
                {
                    upsert.ErrorMsg = "Payment record created successfully";
                }
                else
                {
                    upsert.ErrorMsg = "Payment record updated successfully";
                }

                upsert.RecordId = Payment.PaymentId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Update Payment Error", ex);
            }

            return upsert;
        }

        public async Task<UpsertModel> DeletePayment()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Payment Deleted";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Payment has been deleted:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Student name: {0}", Payment.Student.FullName)
                    .AppendLine().AppendFormat("Bank: {0}", Payment.Bank.Name)
                    .AppendLine().AppendFormat("Term: {0}-{1}", Payment.Term.StartDate.ToString("dd/MM/yyyy"), Payment.Term.EndDate.ToString("dd/MM/yyyy"))
                    .AppendLine().AppendFormat("Class: {0}", Payment.ClassLevel.GetClass());

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                upsert.ErrorMsg = string.Format("Payment for '{0}' deleted successfully", Payment.Student.FullName);
                upsert.RecordId = Payment.PaymentId.ToString();

                // Remove Payment
                db.Payments.Remove(Payment);
                db.Entry(Payment).State = System.Data.Entity.EntityState.Deleted;

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Delete Payment Error", ex);
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

            if(Payment != null)
            {
                activity.PaymentId = PaymentId;
                activity.BankId = Payment.BankId;
                activity.StudentId = Payment.StudentId;
                activity.ClassLevelId = Payment.ClassLevelId;
                activity.TermId = Payment.TermId;
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

            if (Payment != null)
            {
                activity.UserId = ServiceUserId;
                activity.PaymentId = Payment.PaymentId;
            }
            db.SaveChanges();
        }

    }
}