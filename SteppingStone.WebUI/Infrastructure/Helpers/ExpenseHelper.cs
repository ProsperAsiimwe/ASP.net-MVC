using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MagicApps.Models;
using Microsoft.AspNet.Identity.Owin;
using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.Expenses;
using TwitterBootstrap3;
using SteppingStone.Domain.Models;

namespace SteppingStone.WebUI.Infrastructure.Helpers
{
    public class ExpenseHelper
    {
        private ApplicationDbContext db;
        private ApplicationUserManager UserManager;

        int ExpenseId;
        
        public Expense Expense { get; private set; }

        public string ServiceUserId { get; set; }

        public ExpenseHelper()
        {
            Set();
        }

        public ExpenseHelper(int ExpenseId)
        {
            Set();

            this.ExpenseId = ExpenseId;
            this.Expense = db.Expenses.Find(ExpenseId);
        }

        public ExpenseHelper(Expense Expense)
        {
            Set();

            this.ExpenseId = Expense.ExpenseId;
            this.Expense = Expense;
        }

        private void Set()
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public ExpenseListViewModel GetExpenseList(SearchExpensesModel searchModel, int page = 1)
        {
            int pageSize = 20;

            if (page < 1) {
                page = 1;
            }

            IEnumerable<Expense> records = db.Expenses.ToList();
            
            if (!String.IsNullOrEmpty(searchModel.Name)) {
                string name = searchModel.Name.ToLower();
                records = records.Where(x => x.By.ToLower().Contains(name));
            }
            if (searchModel.StartDate.HasValue)
            {
                records = records.Where(p => p.Date >= searchModel.StartDate);
            }
            if (searchModel.EndDate.HasValue)
            {
                records = records.Where(p => p.Date <= searchModel.EndDate);
            }
            if (searchModel.Category.HasValue)
            {
                records = records.Where(p => p.Category == searchModel.Category);
            }

            return new ExpenseListViewModel {
                Expenses = records
                    .OrderByDescending(o => o.Date)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize),
                SearchModel = searchModel,
                PagingInfo = new PagingInfo {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = records.Count()
                }
            };
        }

        public async Task<UpsertModel> UpsertExpense(UpsertMode mode, ExpenseViewModel model)
        {
            var upsert = new UpsertModel();

            try {
                Activity activity;
                string title;
                System.Text.StringBuilder builder;

                // Record previous values for comparison
                double amount = (Expense != null ? Expense.Amount : 0);
                string by = (Expense != null ? Expense.By : string.Empty);
                string desc = (Expense != null ? Expense.Description : string.Empty);
                int category = (Expense != null ? Expense.Category : 0);
                int changes = 0;
                
                // Apply changes
                Expense = model.ParseAsEntity(Expense);
                
                builder = new System.Text.StringBuilder();

                if (model.ExpenseId == 0) {
                    db.Expenses.Add(Expense);

                    title = "Expense Created";
                    builder.Append(string.Format("A Expense record has been created for: {0}", model.By)).AppendLine();
                }
                else {
                    db.Entry(Expense).State = System.Data.Entity.EntityState.Modified;

                    title = "Expense Updated";
                    builder.Append("The following changes have been made to the Expense details");

                    if (mode == UpsertMode.Admin) {
                        builder.Append(" (by the Admin)");
                    }

                    builder.Append(":").AppendLine();
                }

                if (amount != Expense.Amount) {
                    builder.AppendLine().AppendFormat("Amount: from '{0}' to '{1}'", amount, Expense.Amount);
                    changes += 1;
                }
                if (by != Expense.By) {
                    builder.AppendLine().AppendFormat("Expense By: from '{0}' to '{1}'", by, Expense.By);
                    changes += 1;
                }
                if (desc != Expense.Description) {
                    builder.AppendLine().AppendFormat("Description: from '{0}' to '{1}'", desc, Expense.Description);
                    changes += 1;
                }
                if (category != Expense.Category) {
                    var categoryValue = category == 0 ? "" : Expense.Categories()[category];
                    builder.AppendLine().AppendFormat("Category: from '{0}' to '{1}'", categoryValue, Expense.Categories()[model.Category]);
                    changes += 1;
                }
                
                await db.SaveChangesAsync();

                ExpenseId = Expense.ExpenseId;

                // Save activity now so we have a ExpenseId. Not ideal, but hey
                activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                if (model.ExpenseId == 0) {
                    upsert.ErrorMsg = "Expense record created successfully";
                }
                else {
                    upsert.ErrorMsg = "Expense record updated successfully";
                }

                upsert.RecordId = Expense.ExpenseId.ToString();
            }
            catch (Exception ex) {
                upsert.ErrorMsg = ex.Message;
            }

            return upsert;
        }

        public async Task<UpsertModel> DeleteExpense()
        {
            var upsert = new UpsertModel();

            try {
                string title = "Expense Deleted";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Expense has been deleted:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Amount (Ugx): {0}", Expense.Amount.ToString("n0"))
                    .AppendLine().AppendFormat("Expense By: {0}", Expense.By)
                    .AppendLine().AppendFormat("Description: {0}", Expense.Description)
                    .AppendLine().AppendFormat("Category: {0}", Expense.CategoryValue);

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                upsert.ErrorMsg = string.Format("Expense by '{0}' deleted successfully", Expense.By);
                upsert.RecordId = Expense.ExpenseId.ToString();

                // Remove Expense
                db.Expenses.Remove(Expense);

                await db.SaveChangesAsync();

            }
            catch (Exception ex) {
                upsert.ErrorMsg = ex.Message;
            }

            return upsert;
        }

        
        public Activity CreateActivity(string title, string description)
        {
            var activity = new Activity {
                Title = title,
                Description = description,
                RecordedById = ServiceUserId,
                ExpenseId = ExpenseId
            };
            db.Activities.Add(activity);
            return activity;
        }

        public static ButtonStyle GetButtonStyle(string css)
        {
            ButtonStyle button_css;

            if (css == "warning") {
                button_css = ButtonStyle.Warning;
            }
            else if (css == "success") {
                button_css = ButtonStyle.Success;
            }
            else if (css == "info") {
                button_css = ButtonStyle.Info;
            }
            else {
                button_css = ButtonStyle.Danger;
            }

            return button_css;
        }
        
    }
}