using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.Events;
using SteppingStone.WebUI.Models.Students;
using MagicApps.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using TwitterBootstrap3;
using MagicApps.Infrastructure.Services;
using System.Configuration;
using System.IO;

namespace SteppingStone.WebUI.Infrastructure.Helpers
{
    public class StudentHelper
    {
        private ApplicationDbContext db;
        private ApplicationUserManager UserManager;

        int StudentId;

        public Student Student { get; private set; }

        public string ServiceUserId { get; set; }

        public StudentHelper()
        {
            Set();
        }

        public StudentHelper(int StudentId)
        {
            Set();

            this.StudentId = StudentId;
            this.Student = db.Students.Find(StudentId);
        }

        public StudentHelper(Student Student)
        {
            Set();

            this.StudentId = Student.StudentId;
            this.Student = Student;
        }

        private void Set()
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public StudentListViewModel GetStudentList(SearchStudentsModel searchModel, int page = 1)
        {
            int pageSize = 20;

            if (page < 1)
            {
                page = 1;
            }

            IEnumerable<Student> records = db.Students.ToList();

            // Remove any default information
            //searchModel.ParseRouteInfo();

            if (!String.IsNullOrEmpty(searchModel.Name))
            {
                string name = searchModel.Name.ToLower();
                records = records.Where(x => x.FirstName.ToLower().Contains(name) || x.LastName.ToLower().Contains(name) || x.FullName.ToLower().Contains(name));
            }

            if (!String.IsNullOrEmpty(searchModel.ParentName))
            {
                string parentName = searchModel.ParentName.ToLower();
                records = records.SelectMany(x => x.Parents).Where(y => y.Parent.Matches(parentName)).Select(m => m.Student);
            }

            if (searchModel.ClassLevel.HasValue)
            {
                records = records.Where(x => x.CurrentLevel.ClassLevelId == searchModel.ClassLevel);
            }

            if (searchModel.Term.HasValue)
            {
                var term = db.Terms.Find(searchModel.Term);
                records = records.Where(x => x.GetTermPayments(term) > 0);
            }

            if (searchModel.Gender.HasValue)
            {
                records = records.Where(x => x.Gender == searchModel.Gender.Value);
            }

            if (searchModel.StudyMode.HasValue)
            {
                records = records.Where(x => x.CurrentLevel.StudyMode == searchModel.StudyMode.Value);
            }

            if (searchModel.NoParents)
            {
                records = records.Where(x => x.NoParents);
            }

            // return after migration, life will be better
            if (!String.IsNullOrEmpty(searchModel.Status))
            {
                records = records.Where(x => x.GetStatus() == searchModel.Status);
            }

            if (searchModel.Stream.HasValue)
            {
                records = records.Where(x => x.Stream == searchModel.Stream);
            }

            // get only that havent been deleted
            if (searchModel.Status != "Terminated")
            {
                records = records.Where(x => !x.Terminated.HasValue);
            }

            var totalPaid = records.Sum(x => x.Payments.Sum(m => m.Amount));
            var totalBalance = records.Sum(x => x.OldDebt);
            var termTotalBalance = records.Sum(x => x.Outstanding);
            var termTotalPaid = records.Sum(x => x.CurrentTermPayments.Sum(m => m.Amount));


            return new StudentListViewModel
            {
                Students = records
                    .OrderBy(o => o.FullName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize),
                TotalPaid = totalPaid,
                TotalBalance = totalBalance,
                TermTotalBalance = termTotalBalance,
                TermTotalPaid = termTotalPaid,
                SearchModel = searchModel,
                Records = records,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = records.Count()
                }
            };
        }

        public async Task<UpsertModel> UpsertStudent(UpsertMode mode, StudentViewModel model)
        {
            var upsert = new UpsertModel();

            try
            {
                Activity activity;
                string title;
                System.Text.StringBuilder builder;

                // Apply changes
                Student = model.ParseAsEntity(Student);

                if (model.Enroll)
                {
                    var term = db.Terms.FirstOrDefault(p => p.IsCurrentTerm);
                    if (term != null)
                    {
                        Student.CurrentTermId = term.TermId;
                    }
                }

                builder = new System.Text.StringBuilder();

                if (model.StudentId == 0)
                {
                    db.Students.Add(Student);

                    title = "Student Added";
                    builder.AppendFormat("A Student record has been made for: {0} {1}", model.FirstName, model.LastName).AppendLine();
                }
                else
                {
                    db.Entry(Student).State = System.Data.Entity.EntityState.Modified;

                    title = "Student Updated";
                    builder.Append("The following changes have been made to the Student details");

                    if (mode == UpsertMode.Admin)
                    {
                        builder.Append(" (by the Admin)");
                    }

                    builder.Append(":").AppendLine();
                }

                await db.SaveChangesAsync();

                StudentId = Student.StudentId;

                if (model.Dp != null)
                {
                    UploadDp(model.Dp);
                }

                // Save activity now so we have a StudentId. Not ideal, but hey
                activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                if (model.StudentId == 0)
                {
                    upsert.ErrorMsg = "Student record created successfully";
                }
                else
                {
                    upsert.ErrorMsg = "Student record updated successfully";
                }

                upsert.RecordId = Student.StudentId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Update Student Error", ex);
            }

            return upsert;
        }

        public async Task<UpsertModel> UpsertEvent(UpsertMode mode, EventViewModel model)
        {
            var upsert = new UpsertModel();

            try
            {
                Activity activity;
                string title;
                System.Text.StringBuilder builder;

                // Apply changes
                Event Event;

                if (model.EventId > 0)
                {

                    Event = db.Events.Find(model.EventId);
                }
                else
                {
                    Event = new Event();
                }

                Event = model.ParseAsEntity(Event);


                builder = new System.Text.StringBuilder();

                if (model.EventId == 0)
                {
                    db.Events.Add(Event);

                    title = "Event Added";
                    builder.AppendFormat("An Event record has been made for: {0} students' Parents", model.Data.Count()).AppendLine();
                }
                else
                {
                    db.Entry(Event).State = System.Data.Entity.EntityState.Modified;

                    title = "Event Updated";
                    builder.Append("The following changes have been made to the Event details");

                    if (mode == UpsertMode.Admin)
                    {
                        builder.Append(" (by the Admin)");
                    }

                    builder.Append(":").AppendLine();
                }

                await db.SaveChangesAsync();


                // update students here
                //Event.Students = model.Data.Select(p => new StudentEvent(p, Event.EventId)).ToList();
                if (model.Students != null && Event.EventStudents.Count() <= 0)
                {
                    if (model.Students.Count() > 0)
                    {
                        Event.EventStudents = model.Students.Select(p => new StudentEvent(p.StudentId, Event.EventId)).ToList();
                        db.Entry(Event).State = System.Data.Entity.EntityState.Modified;
                    }
                }

                //if (model.Students != null && Event.EventStudents.Count() <= 0)
                //{
                //    if (model.Students.Count() > 0)
                //    {
                //        var students = model.Students.Select(p => new StudentEvent(p.StudentId, Event.EventId)).ToList();
                //        foreach (var student in students)
                //        {
                //            db.StudentEvents.Add(student);
                //        }

                //        //db.Entry(Event).State = System.Data.Entity.EntityState.Modified;
                //    }
                //}

                // Save activity now so we have a StudentId. Not ideal, but hey
                activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                if (model.EventId == 0)
                {
                    upsert.ErrorMsg = "Event record created successfully";
                }
                else
                {
                    upsert.ErrorMsg = "Event record updated successfully";
                }

                upsert.RecordId = Event.EventId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
            }

            return upsert;
        }

        public async Task<UpsertModel> NotifyParents()
        {
            var upsert = new UpsertModel();

            try
            {

                var parents = StudentId <= 0 ? db.Students.ToList().SelectMany(x => x.Parents.Where(y => y.Parent.HasContact)).ToList() : Student.Parents.ToList().Where(p => p.Parent.HasContact);

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
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                upsert.ErrorMsg = string.Format("{0} Parent(s) notified successfully. ", parents.Count());
                upsert.RecordId = Student.StudentId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
            }

            return upsert;
        }

        public async Task<bool> Notify(IEnumerable<StudentParent> parents)
        {

            foreach (var parent in parents)
            {
                var message = string.Format("Hello from SteppingStone PS, this is a notification reminder to kindly clear {0}'s outstanding balance of {1}/=", parent.Student.FullName, parent.Student.Outstanding.ToString("n0"));

                var result = sendTextMessage(parent.Parent.Contacts, message);

                string title = "School Fees Reminder Notification";
                string description = result.Contains("1701") ? "Notification Reminder sent to " + parent.Parent.FullName : "Text Failed to send to " + parent.Parent.FullName;

                //update parent notification info
                parent.Parent.Notified = DateTime.Now;
                parent.Parent.RemindDate = DateTime.Today.AddDays(30);
                parent.Parent.RemindCount += 1;

                db.Entry(parent.Parent).State = System.Data.Entity.EntityState.Modified;

                // Record activity
                var activity = CreateActivity(title, description);
                activity.UserId = ServiceUserId;
                activity.ParentId = parent.ParentId;
                activity.StudentId = parent.StudentId;

                await db.SaveChangesAsync();
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

                        string username = "phlyp";
                        string password = "Uganda.01";
                        char[] chars = { '.', '+', ' ' };
                        string receiver = destination.Count() > 1 ? String.Join(",", destination.Select(p => p.ToString()).ToArray()) : destination[0];

                        string url = "http://boxuganda.com/api.php?"
                + "user=" + HttpUtility.UrlEncode(username)
                + "&password=" + HttpUtility.UrlEncode(password)
                + "&sender=" + HttpUtility.UrlEncode(Settings.COMPANY_NAME)
                + "&message=" + HttpUtility.UrlEncode(message, System.Text.Encoding.GetEncoding("ISO-8859-1"))
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

        public async Task<UpsertModel> DeleteStudent()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Student Terminated";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Student has been terminated:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Student name: {0}", Student.FullName)
                    .AppendLine().AppendFormat("Class: {0}", Student.CurrentLevel.GetClass());

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                // Remove Student
                if (Student.Payments.Count() > 0)
                {
                    Student.Terminated = DateTime.Now;

                    foreach (var parent in Student.Parents.Select(m => m.Parent).ToList())
                    {
                        parent.Terminated = DateTime.Today;
                        db.Entry(parent).State = System.Data.Entity.EntityState.Modified;
                    }

                    db.Entry(Student).State = System.Data.Entity.EntityState.Modified;

                }
                else
                {

                    // delete parents
                    foreach (var parent in Student.Parents.Select(m => m.Parent).ToList())
                    {
                        db.Parents.Remove(parent);
                        db.Entry(parent).State = System.Data.Entity.EntityState.Deleted;
                    }

                    // delete related events
                    foreach (var Event in Student.StudentEvents)
                    {
                        db.StudentEvents.Remove(Event);
                        db.Entry(Event).State = System.Data.Entity.EntityState.Deleted;
                    }

                    DeleteDp();
                    db.Students.Remove(Student);
                    db.Entry(Student).State = System.Data.Entity.EntityState.Deleted;
                }

                await db.SaveChangesAsync();

                upsert.ErrorMsg = string.Format("Student: '{0}' terminated successfully", Student.FullName);
                upsert.RecordId = Student.StudentId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Delete Student Error", ex);
            }

            return upsert;
        }

        public async Task<UpsertModel> RestoreStudent()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Student Restored";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Student has been restored:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Student name: {0}", Student.FullName)
                    .AppendLine().AppendFormat("Class: Primary {0}", Student.CurrentLevel.Level.ToString());

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                // Remove Student
                Student.Terminated = null;
                db.Entry(Student).State = System.Data.Entity.EntityState.Modified;

                await db.SaveChangesAsync();

                upsert.ErrorMsg = string.Format("Student: '{0}' restored successfully", Student.FullName);
                upsert.RecordId = Student.StudentId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
            }

            return upsert;
        }

        public UpsertModel CreatePDF(bool inTransaction, IEnumerable<Student> students)
        {
            var upsert = new UpsertModel();

            try
            {
                var pdf = new PdfHelper(ServiceUserId);
                var status = pdf.CreatePaymentsReport(students);

                if (status.i_RecordId() > 0)
                {
                    string title = "Payments Report Created";
                    string description = string.Format("A payments report has been generated");

                    var activity = CreateActivity(title, description);
                    activity.UserId = ServiceUserId;

                    if (!inTransaction)
                    {
                        db.SaveChanges();
                    }

                    upsert.ErrorMsg = status.ErrorMsg;
                    upsert.RecordId = students.Count().ToString();
                }
                else
                {
                    upsert.ErrorMsg = status.ErrorMsg;
                }
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

            if (Student != null)
            {
                activity.StudentId = StudentId;
            }

            db.Activities.Add(activity);
            return activity;
        }

        public string GetDoc()
        {
            string docName = @"Payments_Report.pdf";
            string destinationFolder = Settings.DOCFOLDER;
            return string.Format(@"{0}\Reports\{1}", destinationFolder, docName);
        }

        private bool UploadDp(HttpPostedFileBase file)
        {
            try
            {
                if (file != null)
                {
                    string folder = string.Format(@"{0}", ConfigurationManager.AppSettings["Settings.Site.ImgFolder"]);//@"~/Content/Imgs";

                    FileService.CreateFolder(folder);

                    //folder = string.Format(@"{0}", ConfigurationManager.AppSettings["Settings.Site.ImgFolder"]);

                    var fileExt = Path.GetExtension((file as HttpPostedFileBase).FileName).Substring(1);
                    var fileName = string.Format("Student-{0}.{1}", StudentId, fileExt);
                    string path = Path.Combine(folder, fileName);

                    // delete the file if it exists
                    FileService.DeleteFile(path);
                    file.SaveAs(path);

                    Student.Dp = fileName;
                    db.Entry(Student).State = System.Data.Entity.EntityState.Modified;
                }
            }
            catch (Exception ex)
            {
                return false;
            }


            return true;
        }

        private bool DeleteDp()
        {
            try
            {
                var file = string.Format(@"{0}/{1}", ConfigurationManager.AppSettings["Settings.Site.ImgFolder"], Student.Dp);
                FileService.DeleteFile(file);
            }
            catch (Exception ex)
            {
                return false;
            }


            return true;
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

            if (Student != null)
            {
                activity.UserId = ServiceUserId;
                activity.StudentId = Student.StudentId;
            }
            db.SaveChanges();
        }
    }
}