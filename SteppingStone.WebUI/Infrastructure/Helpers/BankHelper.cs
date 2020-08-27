using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.Banks;
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
    public class BankHelper
    {
        private ApplicationDbContext db;
        private ApplicationUserManager UserManager;

        int BankId;

        public Bank Bank { get; private set; }

        public string ServiceUserId { get; set; }

        public BankHelper()
        {
            Set();
        }

        public BankHelper(int BankId)
        {
            Set();

            this.BankId = BankId;
            this.Bank = db.Banks.Find(BankId);
        }

        public BankHelper(Bank Bank)
        {
            Set();

            this.BankId = Bank.BankId;
            this.Bank = Bank;
        }

        private void Set()
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public BankListViewModel GetBankList(SearchBanksModel searchModel, int page = 1)
        {
            int pageSize = 20;

            if (page < 1)
            {
                page = 1;
            }

            IEnumerable<Bank> records = db.Banks.ToList();

            // Remove any default information
            //searchModel.ParseRouteInfo();

            if (!String.IsNullOrEmpty(searchModel.Name))
            {
                string name = searchModel.Name.ToLower();
                records = records.Where(x => x.Name.ToLower().Contains(name));
            }

            records = records.Where(x => !x.DeActivated.HasValue);

            return new BankListViewModel
            {
                Banks = records
                    .OrderByDescending(o => o.BankId)
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

        public async Task<UpsertModel> UpsertBank(UpsertMode mode, BankViewModel model)
        {
            var upsert = new UpsertModel();

            try
            {
                Activity activity;
                string title;
                System.Text.StringBuilder builder;

                // Apply changes
                Bank = model.ParseAsEntity(Bank);

                builder = new System.Text.StringBuilder();

                if (model.BankId == 0)
                {
                    db.Banks.Add(Bank);

                    title = "Bank Recorded";
                    builder.Append("A Bank record has been made").AppendLine();
                }
                else
                {
                    db.Entry(Bank).State = System.Data.Entity.EntityState.Modified;

                    title = "Bank Updated";
                    builder.Append("The following changes have been made to the Bank details");

                    if (mode == UpsertMode.Admin)
                    {
                        builder.Append(" (by the Admin)");
                    }

                    builder.Append(":").AppendLine();
                }

                await db.SaveChangesAsync();

                BankId = Bank.BankId;

                // Save activity now so we have a BankId. Not ideal, but hey
                activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                if (model.BankId == 0)
                {
                    upsert.ErrorMsg = "Bank record created successfully";
                }
                else
                {
                    upsert.ErrorMsg = "Bank record updated successfully";
                }

                upsert.RecordId = Bank.BankId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Update Bank Error", ex);
            }

            return upsert;
        }

        public async Task<UpsertModel> DeleteBank()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Bank Deleted";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Bank has been deleted:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Bank: {0}", Bank.Name);

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                if (Bank.Payments.Count() > 0)
                {
                    Bank.DeActivated = DateTime.Now; ;
                    db.Entry(Bank).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    db.Banks.Remove(Bank);
                    db.Entry(Bank).State = System.Data.Entity.EntityState.Deleted;
                }


                upsert.ErrorMsg = string.Format("Bank: '{0}' deleted successfully", Bank.Name);
                upsert.RecordId = Bank.BankId.ToString();

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Delete Bank Error", ex);
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

            if (Bank != null)
            {
                activity.BankId = BankId;
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

        
    }
}