using Glorious.Entities;
using Glorious.Reminders.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Glorious.Reminders.Infrastructure
{
    public class ReminderHelper
    {
        ApplicationDbContext context;
        public ReminderHelper()
        {
            context = new ApplicationDbContext();
        }
        //public bool Notify()
        //{
        //    using (WebClient client = new WebClient())
        //    {
        //        try
        //        {
        //            //var parent = context.StudentParents.Where(x => x.StudentId == 1).First();
        //            var message = string.Format("Hello from Glorious, please clear {0}'s balance of {1}/=", "Patricia's", "100,000");

        //            //bulk sms
        //            string url = "https://bulksms.vsms.net/eapi/submission/send_sms/2/2.0";
        //            string data = SMSHelper.unicode_message("phlyp", "Uganda.01", "+256780717630", message);
        //            Hashtable result = SMSHelper.send_sms(data, url);
        //            string response = SMSHelper.formatted_server_response(result);

        //            if ((int)result["success"] == 1)
        //            {
        //                Console.WriteLine(response);
        //            }
        //            else
        //            {
        //                Console.WriteLine(SMSHelper.formatted_server_response(result));
        //            }

        //        }
        //        catch (WebException ex)
        //        {
        //            return false;
        //        }

        //        // If we got this far, an error occurred

        //    }

        //    return true;
        //}

        // birthdays
        public bool NotifyBirthdays()
        {
            // first get all kids in this term with birthdays today
            var term = context.Terms.FirstOrDefault(m => m.IsCurrentTerm);

            var students = context.StudentParents.ToList().Where(m =>  m.Student.CurrentTermId.HasValue && m.Student.CurrentTermId == term.TermId && m.Student.DOB.HasValue && (m.Student.DOB.Value.Month == DateTime.Today.Month && m.Student.DOB.Value.Day == DateTime.Today.Day));

            var sampleStudents = context.Students.ToList().Where(m => m.DOB.HasValue && (m.DOB.Value.Month == DateTime.Today.Month && m.DOB.Value.Day == DateTime.Today.Day));
            // notify all parents about the child's birthday
            //foreach(var student in students)
            //{
            //    var message = string.Format("Hello, we would like to thank God for your child's life. Happy Birthday to {0}", student.Student.FullName);
            //    var result = sendTextMessage(student.Parent.Contacts, message);
            //    string msgTitle;
            //    System.Text.StringBuilder msgBuilder;

            //    if (result.Contains("1701"))
            //    {
            //        msgTitle = "Message Sent";
            //        msgBuilder = new System.Text.StringBuilder()
            //            .Append("The birthday Message has been sent:")
            //            .AppendLine();

            //    }
            //    else
            //    {
            //        msgTitle = "Message Sending Failed";
            //        msgBuilder = new System.Text.StringBuilder()
            //            .Append("A birthday Message has failed to be sent:")
            //            .AppendLine();
            //    }

            //    msgBuilder.AppendLine().AppendFormat("Student: {0}", student.Student.FullName)
            //            .AppendLine().AppendFormat("Parent: {0}", student.Parent.FullName);


            //    // Record activity
            //    var msgActivity = CreateActivity(msgTitle, msgBuilder.ToString());
            //    msgActivity.UserId = null;
            //    msgActivity.ParentId = student.ParentId;
            //    msgActivity.StudentId = student.StudentId;

            //}
            foreach (var student in sampleStudents)
            {
                var contacts = new string[] { "0780717630" };
                var message = string.Format("Hello, we would like to thank God for your child's life. Happy Birthday to {0}", student.FullName);
                var result = sendTextMessage(contacts, message);
                string msgTitle;
                System.Text.StringBuilder msgBuilder;

                if (result.Contains("1701"))
                {
                    msgTitle = "Message Sent";
                    msgBuilder = new System.Text.StringBuilder()
                        .Append("The birthday Message has been sent:")
                        .AppendLine();

                }
                else
                {
                    msgTitle = "Message Sending Failed";
                    msgBuilder = new System.Text.StringBuilder()
                        .Append("A birthday Message has failed to be sent:")
                        .AppendLine();
                }

                msgBuilder.AppendLine().AppendFormat("Student: {0}", student.FullName);


                // Record activity
                var msgActivity = CreateActivity(msgTitle, msgBuilder.ToString());
                msgActivity.UserId = null;
                msgActivity.StudentId = student.StudentId;

            }

            context.SaveChanges();

            return true;
        }

        // events
        public bool NotifyEvents()
        {
            // notify all parents of current term about events

            // first get the events
            var students = context.StudentEvents.ToList().Where(m => m.Event.NotificationDate.HasValue && m.Event.NotificationDate.Value == DateTime.Today);
            
            foreach (var student in students)
            {
                // from each student, get their parents
                foreach(var parent in student.Student.Parents)
                {
                    var result = sendTextMessage(parent.Parent.Contacts, student.Event.Description);
                    string msgTitle;
                    System.Text.StringBuilder msgBuilder;

                    if (result.Contains("1701"))
                    {
                        msgTitle = "Message Sent";
                        msgBuilder = new System.Text.StringBuilder()
                            .Append("The following message has been sent:")
                            .AppendLine();
                        

                    }
                    else
                    {
                        msgTitle = "Message Sending Failed";
                        msgBuilder = new System.Text.StringBuilder()
                            .Append("The following message has failed to be sent:")
                            .AppendLine();
                    }

                    msgBuilder.AppendLine().AppendFormat("Student: {0}", student.Student.FullName)
                            .AppendLine().AppendFormat("Parent: {0}", parent.Parent.FullName)
                            .AppendLine().AppendFormat("Event: {0}", student.Event.Name);


                    // Record activity
                    var msgActivity = CreateActivity(msgTitle, msgBuilder.ToString());
                    msgActivity.UserId = null;
                    msgActivity.EventId = student.EventId;
                    msgActivity.StudentId = student.StudentId;
                    msgActivity.ParentId = parent.ParentId;
                }

                student.Event.Notified = DateTime.Now;
                context.Entry(student.Event).State = System.Data.Entity.EntityState.Modified;
                
            }

            context.SaveChanges();

            return true;
        }

        // scheduled messages
        public bool NotifyMessages()
        {
            //first get all scheduled messages
            var messages = context.Messages.ToList().Where(m => m.Scheduled.HasValue && !m.Sent.HasValue && m.Scheduled.Value == DateTime.Today);

            var term = context.Terms.FirstOrDefault(x => x.IsCurrentTerm);

            var parents = context.StudentParents.ToList().Where(x => x.Student.CurrentTermId == term.TermId && x.Parent.HasContact).ToList();

            foreach(var message in messages)
            {
                foreach (var parent in parents)
                {
                    var result = sendTextMessage(parent.Parent.Contacts, message.MessageDescription);
                    string msgTitle;
                    System.Text.StringBuilder msgBuilder;

                    if (result.Contains("1701"))
                    {
                        msgTitle = "Message Sent";
                        msgBuilder = new System.Text.StringBuilder()
                            .Append("The following Message has been sent:")
                            .AppendLine();

                    }
                    else
                    {
                        msgTitle = "Message Sending Failed";
                        msgBuilder = new System.Text.StringBuilder()
                            .Append("The following Message has failed to be sent:")
                            .AppendLine();
                    }

                    msgBuilder.AppendLine().AppendFormat("Message: {0}", message.MessageDescription)
                            .AppendLine().AppendFormat("Student: {0}", parent.Student.FullName)
                            .AppendLine().AppendFormat("Parent: {0}", parent.Parent.FullName);


                    // Record activity
                    var msgActivity = CreateActivity(msgTitle, msgBuilder.ToString());
                    msgActivity.UserId = null;
                    msgActivity.ParentId = parent.ParentId;
                    msgActivity.StudentId = parent.StudentId;
                    msgActivity.MessageId = message.MessageId;
                }

                message.Sent = DateTime.Now;
                context.Entry(message).State = System.Data.Entity.EntityState.Modified;
            }

            context.SaveChanges();

            return true;
        }

        // oustanding fees
        public async Task<bool> NotifyParents()
        {
            try
            {

                var parents = context.Students.ToList().Where(x => x.HasOutstanding).SelectMany(x => x.Parents.Where(y => y.Parent.HasContact && y.Parent.RemindDate.HasValue && y.Parent.RemindDate.Value.Date == DateTime.Today.Date)).ToList();

                var total = parents.Select(x => x.Student).Sum(m => m.Outstanding);

                await Notify(parents);

                string title = "Parents Notified";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("Students' Parents with outstanding arrears have been reminded:")
                    .AppendLine()
                    .AppendLine().AppendFormat("No of Parents: {0}", parents.Count())
                    .AppendLine().AppendFormat("Total Arrears: {0}/=", total.ToString("n0"));

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = null;

                await context.SaveChangesAsync();
                               
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> Notify(IEnumerable<StudentParent> parents)
        {

            foreach (var parent in parents)
            {                    
                        
                var message = string.Format("Hello, this is a reminder to kindly clear {0}'s balance of {1}/=", parent.Student.FullName, parent.Student.Outstanding.ToString("n0"));

                var result = sendTextMessage(parent.Parent.Contacts, message);

                string title = "School Fees Reminder Notification";
                string description = result.Contains("1701") ? "Notification Reminder sent to " + parent.Parent.FullName : "Text Failed to send to " + parent.Parent.FullName;

                //update parent notification info
                parent.Parent.Notified = DateTime.Now;
                parent.Parent.RemindDate = DateTime.Today.AddDays(14);
                parent.Parent.RemindCount += 1;

                context.Entry(parent.Parent).State = System.Data.Entity.EntityState.Modified;

                // Record activity
                var activity = CreateActivity(title, description);
                activity.UserId = null;
                activity.ParentId = parent.ParentId;
                activity.StudentId = parent.StudentId;

                await context.SaveChangesAsync();
            }

            return true;
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
                        const string sender = "Glorious PS";
                        string receiver = destination.Count() > 1 ? String.Join(",", destination.Select(p => p.ToString()).ToArray()) : destination[0];

                        string url = "http://boxuganda.com/api.php?"
                + "user=" + HttpUtility.UrlEncode(username)
                + "&password=" + HttpUtility.UrlEncode(password)
                + "&sender=" + HttpUtility.UrlEncode(sender)
                + "&message=" + HttpUtility.UrlEncode(message)
                + "&reciever=" + HttpUtility.UrlEncode("0780717630"); //receiver.Trim(chars)

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

        private int Balance()
        {
            int bulkBalance;

            try
            {
                using (WebClient client = new WebClient())
                {
                    var url = "http://boxuganda.com/balance.php?user=phlyp&password=Uganda.01";
                    var balance = client.DownloadString(url);

                    int.TryParse(balance, out bulkBalance);
                }

                return bulkBalance;
            }
            catch(Exception ex)
            {
                return 0;
            }
        }

        public bool NotifyBulkInsufficiency()
        {
            int bulkBalance;
            bool send = false;
            try
            {
                bulkBalance = Balance();

                var admins = new string[] { "0780717630"/*, "0772506635" Ensure to top up immediately.*/ };
                var message = String.Empty;

                if (bulkBalance < 0)
                {
                    send = true;
                    message = "You have run out of bulk SMS messages to run reminders. Ensure to top up immediately.";
                }
                else if (bulkBalance <= 493)
                {
                    send = true;
                    message = string.Format("Please be advised that your bulk SMS count has gone down below 50. Balance is {0} at {1:ddd, dd MMM yyyy HH:mm}", bulkBalance, DateTime.Now);
                }

                if (send) { sendTextMessage(admins, message); }
                

            }
            catch(Exception ex)
            {
               
            }

            return send;
        }

        public Activity CreateActivity(string title, string description)
        {
            var activity = new Activity
            {
                Title = title,
                Description = description,
                RecordedById = null
            };
            
            context.Activities.Add(activity);

            return activity;
        }
    }
}
