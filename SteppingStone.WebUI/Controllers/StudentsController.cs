using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Infrastructure;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.Events;
using SteppingStone.WebUI.Models.Students;
using Gloroius.WebUI.Models.Students;
using Nexmo.Api;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    [RoutePrefix("Students")]
    public class StudentsController : BaseController
    {
        // GET: Students
        [Authorize(Roles = "Developer, Admin")]
        //[Route("{id:int}", Order = 1)]
        [Route("{status:regex(^(terminated|active|inactive)$)}/Page-{page:int}", Order = 11)]
        [Route("{status:regex(^(terminated|active|inactive)$)}", Order = 12)]
        [Route("Page-{page:int}", Order = 13)]
        [Route("", /*Order = 21,*/ Name = "Students_Index")]
        public async Task<ActionResult> Index(SearchStudentsModel search, string method, int page = 1)
        {
            // Return all Students
            // If not a post-back (i.e. initial load) set the searchModel to session
            if (Request.Form.Count <= 0)
            {
                if (search.IsEmpty() && Session["SearchStudentsModel"] != null)
                {
                    search = (SearchStudentsModel)Session["SearchStudentsModel"];
                }
            }

            var helper = new StudentHelper();

            var model = helper.GetStudentList(search, search.ParsePage(page));

            Session["SearchStudentsModel"] = search;

            if (method == "Event")
            {
                if (model.Students.Count() <= 0)
                {
                    ShowError("Unfortunately, No students found to generate Event For");
                    return RedirectToAction("Index");
                }

                var eventModel = new EventViewModel()
                {
                    //Data = model.Students.Where(m => !m.NoParents).Select(m => m.StudentId).ToArray()
                    Students = model.Students.ToList().Where(m => !m.NoParents).ToList()
                };

                return await CreateEvent(eventModel);
            }

            if (method == "Report")
            {
                if (model.Students.Count() <= 0)
                {
                    ShowError("Unfortunately, No pupils found to generate Report From");
                    return RedirectToAction("Index");
                }

                var reportModel = new ReportModel() { Students = model.Records.ToList() };

                return CreatePDF(reportModel);
            }

            ParseSearchDefaults(search);

            return View(model);
        }

        public async Task<ActionResult> CreateEvent(EventViewModel model)
        {
            var helper = new StudentHelper();

            var upsert = await helper.UpsertEvent(UpsertMode.Admin, model);

            if (upsert.i_RecordId() > 0)
            {
                if (model.IsEmpty())
                {
                    model.EventId = upsert.i_RecordId();

                    return View("Event", model);

                }
                else
                {
                    ShowSuccess(upsert.ErrorMsg);
                }

            }
            else
            {
                ShowError(upsert.ErrorMsg);
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Developer, Admin")]
        public ActionResult New(int? ClassLevelId)
        {
            if (!IsRoutingOK(null))
            {
                return RedirectOnError();
            }

            var model = GetStudentModel(null);

            if (ClassLevelId.HasValue)
            {
                model.CurrentLevelId = ClassLevelId.Value;
            }

            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(StudentViewModel model)
        {

            if (!IsRoutingOK(null) && Verified(model))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(null, model);

            if (success)
            {
                return RedirectOnError();
            }

            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{StudentId:int}")]
        public ActionResult Show(int StudentId)
        {
            if (!IsRoutingOK(StudentId))
            {
                return RedirectOnError();
            }

            var Student = GetStudent(StudentId);

            return View(Student);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{StudentId:int}/Edit")]
        public ActionResult Edit(int StudentId)
        {
            var model = GetStudentModel(StudentId);
            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int StudentId, StudentViewModel model)
        {
            if (!IsRoutingOK(StudentId) && Verified(model))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(StudentId, model);

            if (success)
            {
                return RedirectOnSuccess(StudentId);
            }

            // If we got this far, an error occurred
            return View("New", model);
        }

        // GET: Students
        [Authorize(Roles = "Developer, Admin")]
        [Route("{StudentId:int}/Delete")]
        public ActionResult Delete(int StudentId)
        {
            if (!IsRoutingOK(StudentId))
            {
                return RedirectOnError();
            }

            return View(GetStudent(StudentId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Destroy(int StudentId)
        {
            if (!IsRoutingOK(StudentId))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(StudentId);
            var upsert = await helper.DeleteStudent();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Delete", new { StudentId = StudentId });
        }


        [Authorize(Roles = "Developer, Admin")]
        [Route("{StudentId:int}/Restore")]
        public ActionResult Restore(int StudentId)
        {
            if (!IsRoutingOK(StudentId))
            {
                return RedirectOnError();
            }

            return View(GetStudent(StudentId));
        }


        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Restored(int StudentId)
        {
            if (!IsRoutingOK(StudentId))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(StudentId);
            var upsert = await helper.RestoreStudent();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Restore", new { StudentId = StudentId });
        }

        [Authorize(Roles = "Developer, Admin")]
        //[HttpPost]
        public async Task<ActionResult> Notify(int? StudentId)
        {
            if (!IsRoutingOK(StudentId))
            {
                return RedirectOnError();
            }

            var helper = (StudentId.HasValue ? GetHelper(StudentId.Value) : new StudentHelper() { ServiceUserId = GetUserId() });
            var upsert = await helper.NotifyParents();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }
            //using (WebClient client = new WebClient())
            //{
            //    try
            //    {
            //        var parent = context.StudentParents.Where(x => x.StudentId == 1).First();
            //        var message = string.Format("Hello, you are kindly reminded to clear {0}'s balance of {1}/=", parent.Student.FullName, parent.Student.Outstanding.ToString("n0"));
            //        string username = "phlyp";
            //        string password = "Uganda.01";
            //        //string destination = "+256779138939";

            //        string url = "http://boxuganda.com/api.php?"
            //+ "user=" + HttpUtility.UrlEncode(username)
            //+ "&password=" + HttpUtility.UrlEncode(password)
            //+ "&sender=" + HttpUtility.UrlEncode(Settings.COMPANY_NAME)
            //+ "&message=" + HttpUtility.UrlEncode(message, System.Text.Encoding.GetEncoding("ISO-8859-1"))
            //+ "&reciever=1";
            //http://boxuganda.com/balance.php?user=phlyp&password=Uganda.01
            //        //        string url = "https://smsc.vianett.no/v3/send?"
            //        //        +"username="+HttpUtility.UrlEncode(username)
            //        //        + "&password="+ HttpUtility.UrlEncode(password)
            //        //        + "&msgid=1"
            //        //        + "&tel=" + HttpUtility.UrlEncode(destination, System.Text.Encoding.GetEncoding("ISO-8859-1"))
            //        //        + "&msg=" + HttpUtility.UrlEncode(message, System.Text.Encoding.GetEncoding("ISO-8859-1"));

            //        //    byte[] response = client.UploadValues("https://api.txtlocal.com/send/", new NameValueCollection()
            //        //{
            //        //{"apikey" , "Pp1sBuQWE7I-mBXYXt9KMASjzurFg6lHzL6EWjql3y"},
            //        //{"numbers" , "+256779138939"},
            //        //{"message" , message},
            //        //{"sender" , "SteppingStone"},
            //        //{"username", "vanderllip21@gmail.com" },
            //        //{"password", "Uganda.01"},
            //        //{"hash", "dc488bbcf12abcb0415c1b5c350a834415511ae066e05e13812a51b605841a36"}
            //        //});
            //        //    string result = System.Text.Encoding.UTF8.GetString(response);

            //        //var results = SMS.Send(new SMS.SMSRequest
            //        //{
            //        //    from = Configuration.Instance.Settings["appsettings:NEXMO_FROM_NUMBER"],
            //        //    to = "+256780717630",
            //        //    text = message
            //        //});

            //        //bulk sms
            //        string url = "https://bulksms.vsms.net/eapi/submission/send_sms/2/2.0";
            //        string data = SMSHelper.unicode_message("phlyp", "Uganda.01", "+256780717630", message);
            //        Hashtable result = SMSHelper.send_sms(data, url);
            //        string response = SMSHelper.formatted_server_response(result);
            //        ShowSuccess(response);

            //        if ((int)result["success"] == 1)
            //        {
            //            Console.WriteLine(response);

            //        }
            //        else
            //        {
            //            Console.WriteLine(SMSHelper.formatted_server_response(result));
            //        }
            //        //return result;

            //        //string result = client.DownloadString(url);
            //        //string description = result.Contains("OK") ? "Notification Reminder sent to " + parent.Parent.FullName : "Text Failed to send to " + parent.Parent.FullName;
            //        //ShowError(description);
            //        //return result;
            //    }
            //    catch (WebException ex)
            //    {
            //        ShowError(ex.Message);
            //    }

            // If we got this far, an error occurred

            //}
            return RedirectOnError();
        }

        [Authorize(Roles = "Developer, Admin, Client")]
        public ActionResult CreatePDF(ReportModel model)
        {
            try
            {
                var helper = GetHelper();
                helper.ServiceUserId = GetUser().Id;
                var upsert = helper.CreatePDF(false, model.Students);

                if (upsert.i_RecordId() <= 0)
                {
                    ShowError(upsert.ErrorMsg);
                    return RedirectOnError();
                }

                // 2017.06.14: Compute correct name for Universe
                var document = helper.GetDoc();
                string fileName = string.Format("{0} Payments Report.pdf", Settings.COMPANY_ABBR);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = fileName,
                    Inline = false
                };

                Response.AddHeader("Content-Disposition", cd.ToString());
                byte[] doc = System.IO.File.ReadAllBytes(document);

                return File(doc, "application/x-download");

            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return RedirectOnError();
            }
        }

        private bool EmptyCell(ExcelRange cell)
        {
            if (cell == null || cell.Value == null || cell.Value.ToString() == string.Empty || cell.Value.ToString().Trim() == string.Empty)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> Upsert(int? StudentId, StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var helper = (StudentId.HasValue ? GetHelper(StudentId.Value) : new StudentHelper() { ServiceUserId = GetUserId() });
                var upsert = await helper.UpsertStudent(UpsertMode.Admin, model);

                if (upsert.i_RecordId() > 0)
                {
                    ShowSuccess(upsert.ErrorMsg);

                    return true;
                }
                else
                {
                    ShowError(upsert.ErrorMsg);
                }
            }

            ParseDefaults(model);
            return false;
        }
        private StudentHelper GetHelper()
        {
            StudentHelper helper = new StudentHelper();

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private StudentHelper GetHelper(int StudentId)
        {
            StudentHelper helper = new StudentHelper(StudentId);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private StudentHelper GetHelper(Student Student)
        {
            var helper = new StudentHelper(Student);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        //private bool UpdateStudentClass(Student Student)
        //{
        //    var helper = new StudentHelper(Student);

        //    helper.ServiceUserId = GetUserId();

        //    return helper;
        //}

        private StudentViewModel GetStudentModel(int? StudentId)
        {
            StudentViewModel model;


            if (StudentId.HasValue)
            {
                var Student = GetStudent(StudentId.Value);
                model = new StudentViewModel(Student);
            }
            else
            {
                model = new StudentViewModel();
            }

            // pass needed lists
            ParseDefaults(model);

            return model;
        }

        private void ParseDefaults(StudentViewModel model)
        {
            model.ClassLevels = context.ClassLevels.ToList().OrderBy(x => x.SchoolLevel).ThenBy(o => o.Level);
        }

        private void ParseSearchDefaults(SearchStudentsModel model)
        {
            model.Terms = context.Terms.ToList().OrderByDescending(m => m.StartDate).ToList();
            model.ClassLevels = context.ClassLevels.ToList().OrderBy(x => x.SchoolLevel).ThenBy(o => o.Level);
        }

        private Student GetStudent(int StudentId)
        {
            return context.Students.Find(StudentId);
        }

        private RedirectToRouteResult RedirectOnError()
        {
            return RedirectToAction("Index");
        }

        private RedirectToRouteResult RedirectOnSuccess(int StudentId)
        {
            return RedirectToAction("Show", new { StudentId = StudentId });
        }

        public PartialViewResult GetBreadcrumb(int StudentId, bool mainAsLink = true)
        {
            var Student = GetStudent(StudentId);
            ViewBag.MainAsLink = mainAsLink;

            return PartialView("Partials/_Breadcrumb", Student);
        }

        private bool IsRoutingOK(int? StudentId)
        {
            // Check Student
            if (StudentId.HasValue)
            {
                var Student = context.Students.ToList().SingleOrDefault(x => x.StudentId == StudentId);

                if (Student == null)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Verified(StudentViewModel model)
        {

            if (model.CurrentLevelId > 0)
            {
                var ClassLevel = context.ClassLevels.ToList().SingleOrDefault(x => x.ClassLevelId == model.CurrentLevelId);

                if (ClassLevel == null)
                {
                    return false;
                }
            }

            return true;
        }

        public PartialViewResult GetActivities(int StudentId)
        {
            var activities = context.Activities.ToList()
                .Where(x => x.StudentId == StudentId)
                .OrderBy(o => o.Recorded);

            return PartialView("Partials/_Activities", activities);
        }

        //public PartialViewResult GetStudentStats()
        //{
        //    var currentTerm = context.Terms.Where(x => x.IsCurrentTerm).First();

        //    var model = new StudentStatsModel
        //    {
        //        Term = currentTerm
        //    };

        //    return PartialView("Dashboard/_StudentStats", model);
        //}
        #region Import from CSV


        [Route("Import")]
        public ActionResult ImportPayments()
        {
            string fileName = "Transport 12.xlsx";
            string folder = "~/App_Data/" + fileName;
            string abs_folder = Server.MapPath(folder);

            // Existing data
            int counter = 0;

            // Get the file we are going to process
            var existingFile = new System.IO.FileInfo(abs_folder);

            // Open and read the XlSX file.
            using (var package = new ExcelPackage(existingFile))
            {
                // Get the Client book in the file
                var Workbook = package.Workbook;
                if (Workbook != null)
                {
                    if (Workbook.Worksheets.Count > 0)
                    {
                        // Get the first Clientsheet
                        var currentWorksheet = Workbook.Worksheets["diamonds"];

                        for (int i = 4; i <= 74; i++)
                        {
                            if (!EmptyCell(currentWorksheet.Cells[i, 2])/* && EmptyCell(currentWorksheet.Cells[i, 3])*/)
                            {
                                // get the stewards name value
                                var studentName = currentWorksheet.Cells[i, 2].Value.ToString().Trim();
                                var amountPaid = !EmptyCell(currentWorksheet.Cells[i, 3]) ? (double)currentWorksheet.Cells[i, 3].Value : 0;

                                // create array to split the name
                                string[] studentNameArray;
                                char[] spaceSeparator = new char[] { ' ' };

                                // prepare value to store the name
                                string stringValue = string.Empty;

                                //create temp variables to keep values
                                string firstName = string.Empty;
                                string lastName = string.Empty;

                                if (!studentName.Contains(",") || !studentName.Contains("/") || !studentName.Contains("&"))
                                {

                                    // if the value contains space, split it
                                    if (studentName.Contains(" "))
                                    {
                                        studentNameArray = studentName.Split(spaceSeparator, StringSplitOptions.None);
                                        int nameCount = studentNameArray.Count();

                                        // count values obtained from split and distribute accordinglys
                                        if (nameCount == 2)
                                        {
                                            stringValue = string.Format("{0} {1}", studentNameArray[0], studentNameArray[1]);
                                            firstName = studentNameArray[0];
                                            lastName = studentNameArray[1];

                                        }
                                        else if (nameCount == 3)
                                        {
                                            stringValue = string.Format("{0} {1} {2}", studentNameArray[0], studentNameArray[1], studentNameArray[2]);
                                            firstName = studentNameArray[0];
                                            lastName = string.Format("{0} {1}", studentNameArray[1], studentNameArray[2]);
                                        }

                                    }
                                    else
                                    {
                                        // if last name doesnt exist, use n/a 
                                        stringValue = string.Format("{0} n/a", studentName);
                                        //email = string.Format("{0}@blucruise.co.ug", clientName);
                                        firstName = studentName;
                                        lastName = "n/a";
                                    }


                                    // if after all that the value isnt empty 
                                    if (!string.IsNullOrEmpty(stringValue))
                                    {
                                        // check if that steward was added already
                                        var student = context.Students.ToList().FirstOrDefault(x => x.FullName.ToLower() == stringValue.ToLower());


                                        // if doesnt exist
                                        if (student != null)
                                        {

                                            counter++;

                                            if (amountPaid > 0)
                                            {
                                                var payment = new Payment()
                                                {
                                                    StudentId = student.StudentId,
                                                    Amount = amountPaid,
                                                    BankId = 1,
                                                    ClassLevelId = student.CurrentLevelId.Value,
                                                    Date = DateTime.Now,
                                                    Method = 1,
                                                    TermId = 1

                                                };

                                                student.Payments.Add(payment);
                                            }

                                            student.CurrentTermId = 1;

                                            if (i >= 66)
                                            {
                                                student.CurrentLevelId = 1016;
                                            }
                                            context.Entry(student).State = System.Data.Entity.EntityState.Modified;

                                            context.SaveChanges();
                                        }

                                        // go on and create the student payment

                                    }

                                }
                            }

                        }

                    }

                }

            }

            return Content(counter + " records added");
        }

        #endregion
    }
}