using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Infrastructure;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    [RoutePrefix("Payments")]
    public class PaymentsController : BaseController
    {
        [Authorize(Roles = "Developer, Admin")]
        //[Route("{id:int}", Order = 1)]
        //[Route("{status:regex(^(active|inactive)$)}/Page-{page:int}", Order = 11)]
        //[Route("{status:regex(^(active|inactive)$)}", Order = 12)]
        //[Route("Page-{page:int}", Order = 13)]
        [Route("", /*Order = 21,*/ Name = "Payments_Index")]
        public ActionResult Index(SearchPaymentsModel search, int page = 1)
        {
            // Return all Payments
            // If not a post-back (i.e. initial load) set the searchModel to session
            if (Request.Form.Count <= 0)
            {
                if (search.IsEmpty() && Session["SearchPaymentsModel"] != null)
                {
                    search = (SearchPaymentsModel)Session["SearchPaymentsModel"];
                }
            }

            var helper = new PaymentHelper();
            var model = helper.GetPaymentList(search, search.ParsePage(page));

            Session["SearchPaymentsModel"] = search;

            ParseSearchDefaults(search);

            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        public ActionResult New(int? StudentId)
        {
            if (!IsRoutingOK(null))
            {
                return RedirectOnError();
            }

            var model = GetPaymentModel(null);

            if (StudentId.HasValue)
            {
                var student = context.Students.Find(StudentId);
                if (student != null)
                {
                    model.StudentId = student.StudentId;
                    if (student.CurrentLevelId.HasValue)
                    {
                        model.ClassLevelId = student.CurrentLevelId.Value;
                    }

                    var term = context.Terms.FirstOrDefault(p => p.IsCurrentTerm);
                    if (term != null)
                    {
                        model.TermId = term.TermId;
                    }
                }
            }



            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PaymentViewModel model)
        {

            if (!IsRoutingOK(null) && Verified(model))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(null, model);

            if (success)
            {
                return RedirectToAction("Index", "Students");
            }

            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{PaymentId:int}")]
        public ActionResult Show(int PaymentId)
        {
            if (!IsRoutingOK(PaymentId))
            {
                return RedirectOnError();
            }

            var Payment = GetPayment(PaymentId);

            return View(Payment);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{PaymentId:int}/Edit")]
        public ActionResult Edit(int PaymentId)
        {
            var model = GetPaymentModel(PaymentId);
            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int PaymentId, PaymentViewModel model)
        {
            if (!IsRoutingOK(PaymentId) && Verified(model))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(PaymentId, model);

            if (success)
            {
                return RedirectOnSuccess(PaymentId);
            }

            // If we got this far, an error occurred
            return View("New", model);
        }

        // GET: Payments
        [Authorize(Roles = "Developer, Admin")]
        [Route("{PaymentId:int}/Delete")]
        public ActionResult Delete(int PaymentId)
        {
            if (!IsRoutingOK(PaymentId))
            {
                return RedirectOnError();
            }

            return View(GetPayment(PaymentId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Destroy(int PaymentId)
        {
            if (!IsRoutingOK(PaymentId))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(PaymentId);
            var upsert = await helper.DeletePayment();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Delete", new { PaymentId = PaymentId });
        }

        private async Task<bool> Upsert(int? PaymentId, PaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var helper = (PaymentId.HasValue ? GetHelper(PaymentId.Value) : new PaymentHelper() { ServiceUserId = GetUserId() });
                var upsert = await helper.UpsertPayment(UpsertMode.Admin, model);

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

        private PaymentHelper GetHelper(int PaymentId)
        {
            PaymentHelper helper = new PaymentHelper(PaymentId);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private PaymentHelper GetHelper(Payment Payment)
        {
            var helper = new PaymentHelper(Payment);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        //private bool UpdateStudentClass(Payment payment)
        //{
        //    var helper = new PaymentHelper(Payment);

        //    helper.ServiceUserId = GetUserId();

        //    return helper;
        //}

        private PaymentViewModel GetPaymentModel(int? PaymentId)
        {
            PaymentViewModel model;


            if (PaymentId.HasValue)
            {
                var Payment = GetPayment(PaymentId.Value);
                model = new PaymentViewModel(Payment);
            }
            else
            {
                model = new PaymentViewModel();
            }

            // pass needed lists
            ParseDefaults(model);

            return model;
        }

        private void ParseDefaults(PaymentViewModel model)
        {
            // model.TermId = context.Terms.Where(x => x.IsCurrentTerm).First().TermId;
            model.Terms = context.Terms.ToList();
            model.Classes = context.ClassLevels.ToList().OrderBy(x => x.Level);
            model.Students = context.Students.ToList().Where(x => !x.Terminated.HasValue).ToList();
            model.Banks = context.Banks.ToList().Where(x => !x.DeActivated.HasValue).ToList();
        }

        private void ParseSearchDefaults(SearchPaymentsModel model)
        {
            model.Terms = context.Terms.ToList().OrderByDescending(m => m.StartDate).ToList();
            model.ClassLevels = context.ClassLevels.ToList().OrderBy(x => x.SchoolLevel).ThenBy(o => o.Level);
            model.Banks = context.Banks.ToList().Where(x => !x.DeActivated.HasValue).ToList();
        }

        private Payment GetPayment(int PaymentId)
        {
            return context.Payments.Find(PaymentId);
        }

        private RedirectToRouteResult RedirectOnError()
        {
            return RedirectToAction("Index");
        }

        private RedirectToRouteResult RedirectOnSuccess(int PaymentId)
        {
            return RedirectToAction("Show", new { PaymentId = PaymentId });
        }

        public PartialViewResult GetBreadcrumb(int PaymentId, bool mainAsLink = true)
        {
            var Payment = GetPayment(PaymentId);
            ViewBag.MainAsLink = mainAsLink;

            return PartialView("Partials/_Breadcrumb", Payment);
        }

        private bool IsRoutingOK(int? PaymentId)
        {

            // Check Payment
            if (PaymentId.HasValue)
            {
                var Payment = context.Payments.ToList().SingleOrDefault(x => x.PaymentId == PaymentId);

                if (Payment == null)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Verified(PaymentViewModel model)
        {

            if (model.StudentId > 0)
            {
                var Student = context.Students.ToList().SingleOrDefault(x => x.StudentId == model.StudentId);

                if (Student == null)
                {
                    return false;
                }
            }

            if (model.BankId > 0)
            {
                var Bank = context.Banks.ToList().SingleOrDefault(x => x.BankId == model.BankId);

                if (Bank == null)
                {
                    return false;
                }
            }

            if (model.ClassLevelId > 0)
            {
                var ClassLevel = context.ClassLevels.ToList().SingleOrDefault(x => x.ClassLevelId == model.ClassLevelId);

                if (ClassLevel == null)
                {
                    return false;
                }
            }

            if (model.TermId > 0)
            {
                var Term = context.Terms.ToList().SingleOrDefault(x => x.TermId == model.TermId);

                if (Term == null)
                {
                    return false;
                }
            }

            return true;
        }

        public PartialViewResult GetActivities(int PaymentId)
        {
            var activities = context.Activities.ToList()
                .Where(x => x.PaymentId == PaymentId)
                .OrderBy(o => o.Recorded);

            return PartialView("Partials/_Activities", activities);
        }

        public PartialViewResult GetPaymentStats()
        {
            var currentTerm = context.Terms.FirstOrDefault(x => x.IsCurrentTerm) ?? new Term();

            var model = new PaymentStatsModel
            {
                Term = currentTerm,
                TotalCollections = context.Payments.ToList().Sum(x => x.Amount),
                TotalParents = context.Parents.ToList().Count()
            };

            return PartialView("Dashboard/_PaymentStats", model);
        }
    }
}