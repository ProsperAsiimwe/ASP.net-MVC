using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Infrastructure;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.ClassLevels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    [RoutePrefix("Classes")]
    public class ClassesController : BaseController
    {
        // GET: Classes
        [Authorize(Roles = "Developer, Admin")]
        //[Route("{id:int}", Order = 1)]
        //[Route("{status:regex(^(active|inactive)$)}/Page-{page:int}", Order = 11)]
        //[Route("{status:regex(^(active|inactive)$)}", Order = 12)]
        //[Route("Page-{page:int}", Order = 13)]
        [Route("", /*Order = 21,*/ Name = "Classes_Index")]
        public ActionResult Index(SearchClassesModel search, int page = 1)
        {
            // Return all Classes
            // If not a post-back (i.e. initial load) set the searchModel to session
            if (Request.Form.Count <= 0)
            {
                if (search.IsEmpty() && Session["SearchClassesModel"] != null)
                {
                    search = (SearchClassesModel)Session["SearchClassesModel"];
                }
            }

            var helper = new ClassLevelHelper();
            var model = helper.GetClassLevelList(search, search.ParsePage(page));

            Session["SearchClassesModel"] = search;

            //(search);

            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        public ActionResult New()
        {
            if (!IsRoutingOK(null))
            {
                return RedirectOnError();
            }

            var model = GetClassLevelModel(null);

            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ClassLevelViewModel model)
        {

            if (!IsRoutingOK(null))
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
        [Route("{ClassLevelId:int}")]
        public ActionResult Show(int ClassLevelId)
        {
            if (!IsRoutingOK(ClassLevelId))
            {
                return RedirectOnError();
            }

            var ClassLevel = GetClassLevel(ClassLevelId);

            return View(ClassLevel);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{ClassLevelId:int}/Edit")]
        public ActionResult Edit(int ClassLevelId)
        {
            var model = GetClassLevelModel(ClassLevelId);
            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int ClassLevelId, ClassLevelViewModel model)
        {
            if (!IsRoutingOK(ClassLevelId))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(ClassLevelId, model);

            if (success)
            {
                return RedirectOnSuccess(ClassLevelId);
            }

            // If we got this far, an error occurred
            return View("New", model);
        }

        // GET: Classes
        [Authorize(Roles = "Developer, Admin")]
        [Route("{ClassLevelId:int}/Delete")]
        public ActionResult Delete(int ClassLevelId)
        {
            if (!IsRoutingOK(ClassLevelId))
            {
                return RedirectOnError();
            }

            return View(GetClassLevel(ClassLevelId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Destroy(int ClassLevelId)
        {
            if (!IsRoutingOK(ClassLevelId))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(ClassLevelId);
            var upsert = await helper.DeleteClassLevel();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Delete", new { ClassLevelId = ClassLevelId });
        }

        private async Task<bool> Upsert(int? ClassLevelId, ClassLevelViewModel model)
        {
            if (ModelState.IsValid)
            {
                var classLevel = context.ClassLevels.ToList().FirstOrDefault(x => x.Level == model.Level && x.SchoolLevel == model.SchoolLevel && x.StudyMode == model.StudyMode && x.HalfDay == model.HalfDay);

                
                if(classLevel != null && model.ClassLevelId <= 0)
                {
                    ShowError("Such a class already exists. Instead of creating a new one, make changes to the exisiting one.");
                    return false;
                }

                if (!model.IsModelLevelValid())
                {
                    ShowError("You can't create a such a class. Nursery stops at 3, Primary at 7, and Secondary at 6!");
                    return false;
                }

                var helper = (ClassLevelId.HasValue ? GetHelper(ClassLevelId.Value) : new ClassLevelHelper() { ServiceUserId = GetUserId() });
                var upsert = await helper.UpsertClassLevel(UpsertMode.Admin, model);

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

            //ParseDefaults(model);
            return false;
        }

        private ClassLevelHelper GetHelper(int ClassLevelId)
        {
            ClassLevelHelper helper = new ClassLevelHelper(ClassLevelId);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private ClassLevelHelper GetHelper(ClassLevel ClassLevel)
        {
            var helper = new ClassLevelHelper(ClassLevel);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        //private bool UpdateStudentClass(ClassLevel ClassLevel)
        //{
        //    var helper = new ClassLevelHelper(ClassLevel);

        //    helper.ServiceUserId = GetUserId();

        //    return helper;
        //}

        private ClassLevelViewModel GetClassLevelModel(int? ClassLevelId)
        {
            ClassLevelViewModel model;


            if (ClassLevelId.HasValue)
            {
                var ClassLevel = GetClassLevel(ClassLevelId.Value);
                model = new ClassLevelViewModel(ClassLevel);
            }
            else
            {
                model = new ClassLevelViewModel();
            }

            // pass needed lists
            //ParseDefaults(model);

            return model;
        }

        //private void ParseDefaults(ClassLevelViewModel model)
        //{
            
        //}

        //private void ParseSearchDefaults(SearchClassesModel model)
        //{
        //    model.Terms = context.Terms.OrderByDescending(m => m.StartDate).ToList();
        //    model.ClassLevels = context.ClassLevels.ToList();
        //    model.Banks = context.Banks.Where(x => !x.DeActivated.HasValue).ToList();
        //}

        private ClassLevel GetClassLevel(int ClassLevelId)
        {
            return context.ClassLevels.Find(ClassLevelId);
        }

        private RedirectToRouteResult RedirectOnError()
        {
            return RedirectToAction("Index");
        }

        private RedirectToRouteResult RedirectOnSuccess(int ClassLevelId)
        {
            return RedirectToAction("Show", new { ClassLevelId = ClassLevelId });
        }

        public PartialViewResult GetBreadcrumb(int ClassLevelId, bool mainAsLink = true)
        {
            var ClassLevel = GetClassLevel(ClassLevelId);
            ViewBag.MainAsLink = mainAsLink;

            return PartialView("Partials/_Breadcrumb", ClassLevel);
        }

        private bool IsRoutingOK(int? ClassLevelId)
        {

            // Check ClassLevel
            if (ClassLevelId.HasValue)
            {
                var ClassLevel = context.ClassLevels.ToList().SingleOrDefault(x => x.ClassLevelId == ClassLevelId);

                if (ClassLevel == null)
                {
                    return false;
                }
            }

            return true;
        }

        
        public PartialViewResult GetActivities(int ClassLevelId)
        {
            var activities = context.Activities.ToList()
                .Where(x => x.ClassLevelId == ClassLevelId)
                .OrderBy(o => o.Recorded);

            return PartialView("Partials/_Activities", activities);
        }

        //public PartialViewResult GetClassestats()
        //{
        //    var currentTerm = context.Terms.Where(x => x.IsCurrentTerm).First();

        //    var model = new ClassestatsModel
        //    {
        //        Term = currentTerm
        //    };

        //    return PartialView("Dashboard/_Classestats", model);
        //}
    }
}