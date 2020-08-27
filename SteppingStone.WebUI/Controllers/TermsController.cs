using SteppingStone.Domain.Entities;
using SteppingStone.Domain.Models;
using SteppingStone.WebUI.Infrastructure;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    [RoutePrefix("Terms")]
    public class TermsController : BaseController
    {
        // GET: Terms
        [Authorize(Roles = "Developer, Admin")]
        //[Route("{id:int}", Order = 1)]
        //[Route("{status:regex(^(active|inactive)$)}/Page-{page:int}", Order = 11)]
        //[Route("{status:regex(^(active|inactive)$)}", Order = 12)]
        //[Route("Page-{page:int}", Order = 13)]
        [Route("", /*Order = 21,*/ Name = "Terms_Index")]
        public ActionResult Index(SearchTermsModel search, int page = 1)
        {
            // Return all Terms
            // If not a post-back (i.e. initial load) set the searchModel to session
            if (Request.Form.Count <= 0)
            {
                if (search.IsEmpty() && Session["SearchTermsModel"] != null)
                {
                    search = (SearchTermsModel)Session["SearchTermsModel"];
                }
            }

            var helper = new TermHelper();
            var model = helper.GetTermList(search, search.ParsePage(page));

            Session["SearchTermsModel"] = search;

            //var term = context.Terms.Count() > 0 ? context.Terms.FirstOrDefault(x => x.IsCurrentTerm) : null;
            //if (term != null)
            //{
            //    helper.UpdateStudentsOldDebt(term);
            //}
            //else
            //{
            //    return View("New", GetTermModel(null));
            //}
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

            var model = GetTermModel(null);

            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TermViewModel model)
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
        [Route("{TermId:int}")]
        public ActionResult Show(int TermId)
        {
            if (!IsRoutingOK(TermId))
            {
                return RedirectOnError();
            }

            var Term = GetTerm(TermId);

            return View(Term);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{TermId:int}/Edit")]
        public ActionResult Edit(int TermId)
        {
            var model = GetTermModel(TermId);
            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int TermId, TermViewModel model)
        {
            if (!IsRoutingOK(TermId))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(TermId, model);

            if (success)
            {
                return RedirectOnSuccess(TermId);
            }

            // If we got this far, an error occurred
            return View("New", model);
        }

        // GET: Terms
        [Authorize(Roles = "Developer, Admin")]
        [Route("{TermId:int}/Delete")]
        public ActionResult Delete(int TermId)
        {
            if (!IsRoutingOK(TermId))
            {
                return RedirectOnError();
            }

            return View(GetTerm(TermId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Destroy(int TermId)
        {
            if (!IsRoutingOK(TermId))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(TermId);
            var upsert = await helper.DeleteTerm();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Delete", new { TermId = TermId });
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{TermId:int}/End")]
        public ActionResult End(int TermId)
        {
            if (!IsRoutingOK(TermId))
            {
                return RedirectOnError();
            }

            return View(GetTerm(TermId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Ended(int TermId)
        {
            if (!IsRoutingOK(TermId))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(TermId);
            var upsert = await helper.EndTerm();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Delete", new { TermId = TermId });
        }

        private async Task<bool> Upsert(int? TermId, TermViewModel model)
        {
            if (ModelState.IsValid)
            {

                // check the current term

                var helper = (TermId.HasValue ? GetHelper(TermId.Value) : new TermHelper() { ServiceUserId = GetUserId() });

                if (model.IsCurrentTerm && model.TermId <= 0)
                {
                    var currentTerm = context.Terms.ToList().FirstOrDefault(m => m.IsCurrentTerm);

                    if (currentTerm != null)
                    {
                        if (currentTerm.EndDate > model.StartDate)
                        {
                            ShowError(string.Format("You cannot set new current term starting {0} since Current term ends on {1} ", model.StartDate.ToString("ddd, dd MMM yyyy"), currentTerm.EndDate.ToString("ddd, dd MMM yyyy")));
                            return false;
                        }

                        if (currentTerm.EndDate > DateTime.Today)
                        {
                            ShowError(string.Format("You cannot set new current term unless the {0} has ended. ", currentTerm.GetTerm()));
                            return false;
                        }

                        helper.UpdateStudentsOldDebt(currentTerm);
                        currentTerm.IsCurrentTerm = false;
                        context.Entry(currentTerm).State = System.Data.Entity.EntityState.Modified;
                    }

                }

                if (model.EndDate < UgandaDateTime.DateNow().Date)
                {
                    ShowError("Term End date seems to have passed according to System Calendar.");
                    return false;
                }

                var upsert = await helper.UpsertTerm(UpsertMode.Admin, model);

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

        private TermHelper GetHelper(int TermId)
        {
            TermHelper helper = new TermHelper(TermId);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private TermHelper GetHelper(Term Term)
        {
            var helper = new TermHelper(Term);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        //private bool UpdateStudentClass(Term Term)
        //{
        //    var helper = new TermHelper(Term);

        //    helper.ServiceUserId = GetUserId();

        //    return helper;
        //}

        private TermViewModel GetTermModel(int? TermId)
        {
            TermViewModel model;


            if (TermId.HasValue)
            {
                var Term = GetTerm(TermId.Value);
                model = new TermViewModel(Term);
            }
            else
            {
                model = new TermViewModel();
            }

            // pass needed lists
            //ParseDefaults(model);

            return model;
        }

        //private void ParseDefaults(TermViewModel model)
        //{

        //}

        //private void ParseSearchDefaults(SearchTermsModel model)
        //{
        //    model.Terms = context.Terms.OrderByDescending(m => m.StartDate).ToList();
        //    model.Terms = context.Terms.ToList();
        //    model.Banks = context.Banks.Where(x => !x.DeActivated.HasValue).ToList();
        //}

        private Term GetTerm(int TermId)
        {
            return context.Terms.Find(TermId);
        }

        private RedirectToRouteResult RedirectOnError()
        {
            return RedirectToAction("Index");
        }

        private RedirectToRouteResult RedirectOnSuccess(int TermId)
        {
            return RedirectToAction("Show", new { TermId = TermId });
        }

        public PartialViewResult GetBreadcrumb(int TermId, bool mainAsLink = true)
        {
            var Term = GetTerm(TermId);
            ViewBag.MainAsLink = mainAsLink;

            return PartialView("Partials/_Breadcrumb", Term);
        }

        private bool IsRoutingOK(int? TermId)
        {

            // Check Term
            if (TermId.HasValue)
            {
                var Term = context.Terms.ToList().SingleOrDefault(x => x.TermId == TermId);

                if (Term == null)
                {
                    return false;
                }
            }

            return true;
        }


        public PartialViewResult GetActivities(int TermId)
        {
            var activities = context.Activities.ToList()
                .Where(x => x.TermId == TermId)
                .OrderBy(o => o.Recorded);

            return PartialView("Partials/_Activities", activities);
        }

        //public PartialViewResult GetTermstats()
        //{
        //    var currentTerm = context.Terms.Where(x => x.IsCurrentTerm).First();

        //    var model = new TermstatsModel
        //    {
        //        Term = currentTerm
        //    };

        //    return PartialView("Dashboard/_Termstats", model);
        //}
    }
}