using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Infrastructure;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.Parents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    [RoutePrefix("Parents")]
    public class ParentsController : BaseController
    {
        // GET: Parents
        [Authorize(Roles = "Developer, Admin")]
        //[Route("{id:int}", Order = 1)]
        [Route("{status:regex(^(terminated|active|inactive)$)}/Page-{page:int}", Order = 11)]
        [Route("{status:regex(^(terminated|active|inactive)$)}", Order = 12)]
        [Route("Page-{page:int}", Order = 13)]
        [Route("", /*Order = 21,*/ Name = "Parents_Index")]
        public ActionResult Index(SearchParentsModel search, int page = 1)
        {
            // Return all Parents
            // If not a post-back (i.e. initial load) set the searchModel to session
            if (Request.Form.Count <= 0)
            {
                if (search.IsEmpty() && Session["SearchParentsModel"] != null)
                {
                    search = (SearchParentsModel)Session["SearchParentsModel"];
                }
            }

            var helper = new ParentHelper();
            var model = helper.GetParentList(search, search.ParsePage(page));

            Session["SearchParentsModel"] = search;
            
            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("New", Order = 10)]//StudentId
        [Route("New/{StudentId:int}", Order = 4)]
        public ActionResult New(int? StudentId)
        {
            if (!IsRoutingOK(null, StudentId))
            {
                return RedirectOnError();
            }

            var model = GetParentModel(null);

            // add student to list of selected
            if (StudentId.HasValue)
            {
                model.SelectedStudents = new int[] { StudentId.Value };
            }
            

            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ParentViewModel model)
        {

            if (!IsRoutingOK(null, null) && Verified(model))
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
        [Route("{ParentId:int}")]
        public ActionResult Show(int ParentId)
        {
            if (!IsRoutingOK(ParentId, null))
            {
                return RedirectOnError();
            }

            var Parent = GetParent(ParentId);

            return View(Parent);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{ParentId:int}/Edit")]
        public ActionResult Edit(int ParentId)
        {
            if (!IsRoutingOK(ParentId, null))
            {
                return RedirectOnError();
            }

            var model = GetParentModel(ParentId);
            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int ParentId, ParentViewModel model)
        {
            if (!IsRoutingOK(ParentId, null) && Verified(model))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(ParentId, model);

            if (success)
            {
                return RedirectOnSuccess(ParentId);
            }

            // If we got this far, an error occurred
            return View("New", model);
        }

        // GET: Parents
        [Authorize(Roles = "Developer, Admin")]
        [Route("{ParentId:int}/Delete")]
        public ActionResult Delete(int ParentId)
        {
            if (!IsRoutingOK(ParentId, null))
            {
                return RedirectOnError();
            }

            return View(GetParent(ParentId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Destroy(int ParentId)
        {
            if (!IsRoutingOK(ParentId, null))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(ParentId);
            var upsert = await helper.DeleteParent();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Delete", new { ParentId = ParentId });
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{ParentId:int}/Restore")]
        public ActionResult Restore(int ParentId)
        {
            if (!IsRoutingOK(ParentId, null))
            {
                return RedirectOnError();
            }

            return View(GetParent(ParentId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Restored(int ParentId)
        {
            if (!IsRoutingOK(ParentId, null))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(ParentId);
            var upsert = await helper.RestoreParent();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Restore", new { ParentId = ParentId });
        }

        
        private async Task<bool> Upsert(int? ParentId, ParentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var helper = (ParentId.HasValue ? GetHelper(ParentId.Value) : new ParentHelper() { ServiceUserId = GetUserId() });
                var upsert = await helper.UpsertParent(UpsertMode.Admin, model);

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

        private ParentHelper GetHelper(int ParentId)
        {
            ParentHelper helper = new ParentHelper(ParentId);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private ParentHelper GetHelper(Parent Parent)
        {
            var helper = new ParentHelper(Parent);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        //private bool UpdateParentClass(Parent Parent)
        //{
        //    var helper = new ParentHelper(Parent);

        //    helper.ServiceUserId = GetUserId();

        //    return helper;
        //}

        private ParentViewModel GetParentModel(int? ParentId)
        {
            ParentViewModel model;


            if (ParentId.HasValue)
            {
                var Parent = GetParent(ParentId.Value);
                model = new ParentViewModel(Parent);

                model.SelectedStudents = Parent.Students.Select(x => x.StudentId).ToArray();

            }
            else
            {
                model = new ParentViewModel();
            }

            // pass needed lists
            ParseDefaults(model);

            return model;
        }

        private void ParseDefaults(ParentViewModel model)
        {
            model.Students = context.Students.ToList().Where(x => !x.Terminated.HasValue).ToList();
            model.SetLists();
        }

        private Parent GetParent(int ParentId)
        {
            return context.Parents.Find(ParentId);
        }

        private Student GetStudent(int StudentId)
        {
            return context.Students.Find(StudentId);
        }

        private RedirectToRouteResult RedirectOnError()
        {
            return RedirectToAction("Index");
        }

        private RedirectToRouteResult RedirectOnSuccess(int ParentId)
        {
            return RedirectToAction("Show", new { ParentId = ParentId });
        }

        public PartialViewResult GetBreadcrumb(int ParentId, bool mainAsLink = true)
        {
            var Parent = GetParent(ParentId);
            ViewBag.MainAsLink = mainAsLink;

            return PartialView("Partials/_Breadcrumb", Parent);
        }

        private bool IsRoutingOK(int? ParentId, int? StudentId)
        {
            // Check Parent
            if (ParentId.HasValue)
            {
                var Parent = context.Parents.ToList().SingleOrDefault(x => x.ParentId == ParentId);

                if (Parent == null)
                {
                    return false;
                }
            }

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

        public bool Verified(ParentViewModel model)
        {

            foreach(var studentId in model.SelectedStudents)
            {
                var student = GetStudent(studentId);
                if(student == null)
                {
                    return false;
                }
            }

            return true;
        }

        public PartialViewResult GetActivities(int ParentId)
        {
            var activities = context.Activities.ToList()
                .Where(x => x.ParentId == ParentId)
                .OrderBy(o => o.Recorded);

            return PartialView("Partials/_Activities", activities);
        }

        //public PartialViewResult GetParentStats()
        //{
        //    var currentTerm = context.Terms.Where(x => x.IsCurrentTerm).First();

        //    var model = new ParentStatsModel
        //    {
        //        Term = currentTerm
        //    };

        //    return PartialView("Dashboard/_ParentStats", model);
        //}
    }
}