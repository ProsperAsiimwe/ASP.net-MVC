using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.Messages;
using MagicApps.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using TwitterBootstrap3;

namespace SteppingStone.WebUI.Infrastructure.Helpers
{
    public class MessageHelper
    {
        private ApplicationDbContext db;
        private ApplicationUserManager UserManager;

        int MessageId;

        public Message Message { get; private set; }

        public string ServiceUserId { get; set; }

        public MessageHelper()
        {
            Set();
        }

        public MessageHelper(int MessageId)
        {
            Set();

            this.MessageId = MessageId;
            this.Message = db.Messages.Find(MessageId);
        }

        public MessageHelper(Message Message)
        {
            Set();

            this.MessageId = Message.MessageId;
            this.Message = Message;
        }

        private void Set()
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public MessageListViewModel GetMessageList(SearchMessagesModel searchModel, int page = 1)
        {
            int pageSize = 20;

            if (page < 1)
            {
                page = 1;
            }

            IEnumerable<Message> records = db.Messages.ToList();

            // Remove any default information
            //searchModel.ParseRouteInfo();

            if (!String.IsNullOrEmpty(searchModel.Description))
            {
                string name = searchModel.Description.ToLower();
                records = records.Where(x => x.MessageDescription.ToLower().Contains(name));
            }
            
            if (searchModel.StartDate.HasValue)
            {
                records = records.Where(x => x.Scheduled.HasValue && x.Scheduled.Value >= searchModel.StartDate.Value);
            }

            if (searchModel.EndDate.HasValue)
            {
                records = records.Where(x => x.Scheduled.HasValue && x.Scheduled.Value <= searchModel.EndDate.Value);
            }
            
            return new MessageListViewModel
            {
                Messages = records
                    .OrderByDescending(o => o.Added)
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

        public async Task<UpsertModel> UpsertMessage(UpsertMode mode, MessageViewModel model)
        {
            var upsert = new UpsertModel();

            try
            {
                                
                // Apply changes
                Message = model.ParseAsEntity(Message);

                if (model.SendNow)
                {
                    var term = db.Terms.FirstOrDefault(x => x.IsCurrentTerm);

                    var parents = db.StudentParents.ToList().Where(x => x.Student.CurrentTermId == term.TermId && x.Parent.HasContact).ToList()/*.Select(m => m.Parent)*/;

                    foreach (var parent in parents)
                    {
                        var result = sendTextMessage(parent.Parent.Contacts, model.MessageDescription);
                        string msgTitle;
                        System.Text.StringBuilder msgBuilder;

                        if (result.Contains("1701"))
                        {
                            msgTitle = "Message Sent";
                            msgBuilder = new System.Text.StringBuilder()
                                .Append("The following Message has been sent:")
                                .AppendLine();
                                
                        }else
                        {
                            msgTitle = "Message Sending Failed";
                            msgBuilder = new System.Text.StringBuilder()
                                .Append("The following Message has failed to be sent:")
                                .AppendLine();
                        }

                        msgBuilder.AppendLine().AppendFormat("Message: {0}", Message.MessageDescription)
                                .AppendLine().AppendFormat("Student: {0}", parent.Student.FullName)
                                .AppendLine().AppendFormat("Parent: {0}", parent.Parent.FullName);


                        // Record activity
                        var msgActivity = CreateActivity(msgTitle, msgBuilder.ToString());
                        msgActivity.UserId = ServiceUserId;
                        msgActivity.ParentId = parent.ParentId;
                        msgActivity.StudentId = parent.StudentId;

                    }

                    Message.Sent = DateTime.Now;
                    // make sure you create activity here after testing
                }

                Activity activity;
                string title;
                System.Text.StringBuilder builder;

                builder = new System.Text.StringBuilder();

                if (model.MessageId == 0)
                {
                    db.Messages.Add(Message);
                                        
                    title = "Message Recorded";
                    builder.Append("A Message record has been made. ").AppendLine();
                }
                else
                {
                    db.Entry(Message).State = System.Data.Entity.EntityState.Modified;

                    title = "Message Updated";
                    builder.Append("The following changes have been made to the Message details");

                    if (mode == UpsertMode.Admin)
                    {
                        builder.Append(" (by the Admin)");
                    }

                    builder.Append(":").AppendLine();
                }
                
                await db.SaveChangesAsync();

                MessageId = Message.MessageId;
                                

                // Save activity now so we have a MessageId. Not ideal, but hey
                activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                if (model.MessageId == 0)
                {
                    upsert.ErrorMsg = "Message record created successfully";
                }
                else
                {
                    upsert.ErrorMsg = "Message record updated successfully";
                }

                upsert.RecordId = Message.MessageId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Update Message Error", ex);
            }

            return upsert;
        }

        public async Task<UpsertModel> DeleteMessage()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Message Deleted";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Message has been deleted:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Message: {0}", Message.MessageDescription)
                    .AppendLine().AppendFormat("Scheduled: {0}", Message.Scheduled.HasValue ? Message.Scheduled.Value.ToString("dd/MM/yyyy") : "");

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                upsert.ErrorMsg = string.Format("Message: '{0}' deleted successfully", Message.MessageDescription);
                upsert.RecordId = Message.MessageId.ToString();

                // Remove Message
                db.Messages.Remove(Message);

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Delete Message Error", ex);
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

            if(Message != null)
            {
                activity.MessageId = MessageId;
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

        public string sendTextMessage(string[] destination, string message)
        {
            if (destination != null)
            {
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        const string username = "phlyp";
                        const string password = "Uganda.01";
                        char[] chars = { '.', '+', ' ' };
                        string receiver = destination.Count() > 1 ? String.Join(",", destination.Select(p => p.ToString()).ToArray()) : destination[0];

                        string url = "http://boxuganda.com/api.php?"
                + "user=" + HttpUtility.UrlEncode(username)
                + "&password=" + HttpUtility.UrlEncode(password)
                + "&sender=" + HttpUtility.UrlEncode(Settings.COMPANY_ABBR)
                + "&message=" + HttpUtility.UrlEncode(message)
                + "&reciever=" + HttpUtility.UrlEncode(receiver.Trim(chars));

                        string result = client.DownloadString(url);

                        return result;
                    }
                    catch (WebException ex)
                    {
                        return ex.Message;
                    }
                }

            }
            else
            {
                return "No contact found";
            }

        }

        private void RecordException(string title, Exception ex)
        {
            var activity = CreateActivity(title, ex.Message);

            if (Message != null)
            {
                activity.UserId = ServiceUserId;
                activity.MessageId = Message.MessageId;
            }
            db.SaveChanges();
        }

    }
}