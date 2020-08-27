using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Infrastructure;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.Banks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    [RoutePrefix("Banks")]
    public class BanksController : BaseController
    {
        // GET: Banks
        [Authorize(Roles = "Developer, Admin")]
        //[Route("{id:int}", Order = 1)]
        //[Route("{status:regex(^(active|inactive)$)}/Page-{page:int}", Order = 11)]
        //[Route("{status:regex(^(active|inactive)$)}", Order = 12)]
        //[Route("Page-{page:int}", Order = 13)]
        [Route("", /*Order = 21,*/ Name = "Banks_Index")]
        public ActionResult Index(SearchBanksModel search, int page = 1)
        {
            // Return all Banks
            // If not a post-back (i.e. initial load) set the searchModel to session
            if (Request.Form.Count <= 0)
            {
                if (search.IsEmpty() && Session["SearchBanksModel"] != null)
                {
                    search = (SearchBanksModel)Session["SearchBanksModel"];
                }
            }

            var helper = new BankHelper();
            var model = helper.GetBankList(search, search.ParsePage(page));

            Session["SearchBanksModel"] = search;

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

            var model = GetBankModel(null);

            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BankViewModel model)
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
        [Route("{BankId:int}")]
        public ActionResult Show(int BankId)
        {
            if (!IsRoutingOK(BankId))
            {
                return RedirectOnError();
            }

            var Bank = GetBank(BankId);

            return View(Bank);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{BankId:int}/Edit")]
        public ActionResult Edit(int BankId)
        {
            var model = GetBankModel(BankId);
            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int BankId, BankViewModel model)
        {
            if (!IsRoutingOK(BankId))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(BankId, model);

            if (success)
            {
                return RedirectOnSuccess(BankId);
            }

            // If we got this far, an error occurred
            return View("New", model);
        }

        // GET: Banks
        [Authorize(Roles = "Developer, Admin")]
        [Route("{BankId:int}/Delete")]
        public ActionResult Delete(int BankId)
        {
            if (!IsRoutingOK(BankId))
            {
                return RedirectOnError();
            }

            return View(GetBank(BankId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Destroy(int BankId)
        {
            if (!IsRoutingOK(BankId))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(BankId);
            var upsert = await helper.DeleteBank();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Delete", new { BankId = BankId });
        }

        private async Task<bool> Upsert(int? BankId, BankViewModel model)
        {
            if (ModelState.IsValid)
            {                

                var helper = (BankId.HasValue ? GetHelper(BankId.Value) : new BankHelper() { ServiceUserId = GetUserId() });
                var upsert = await helper.UpsertBank(UpsertMode.Admin, model);

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

        private BankHelper GetHelper(int BankId)
        {
            BankHelper helper = new BankHelper(BankId);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private BankHelper GetHelper(Bank Bank)
        {
            var helper = new BankHelper(Bank);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        //private bool UpdateStudentClass(Bank Bank)
        //{
        //    var helper = new BankHelper(Bank);

        //    helper.ServiceUserId = GetUserId();

        //    return helper;
        //}

        private BankViewModel GetBankModel(int? BankId)
        {
            BankViewModel model;


            if (BankId.HasValue)
            {
                var Bank = GetBank(BankId.Value);
                model = new BankViewModel(Bank);
            }
            else
            {
                model = new BankViewModel();
            }

            // pass needed lists
            //ParseDefaults(model);

            return model;
        }

        //private void ParseDefaults(BankViewModel model)
        //{

        //}

        //private void ParseSearchDefaults(SearchBanksModel model)
        //{
        //    model.Banks = context.Banks.OrderByDescending(m => m.StartDate).ToList();
        //    model.Banks = context.Banks.ToList();
        //    model.Banks = context.Banks.Where(x => !x.DeActivated.HasValue).ToList();
        //}

        private Bank GetBank(int BankId)
        {
            return context.Banks.Find(BankId);
        }

        private RedirectToRouteResult RedirectOnError()
        {
            return RedirectToAction("Index");
        }

        private RedirectToRouteResult RedirectOnSuccess(int BankId)
        {
            return RedirectToAction("Show", new { BankId = BankId });
        }

        public PartialViewResult GetBreadcrumb(int BankId, bool mainAsLink = true)
        {
            var Bank = GetBank(BankId);
            ViewBag.MainAsLink = mainAsLink;

            return PartialView("Partials/_Breadcrumb", Bank);
        }

        private bool IsRoutingOK(int? BankId)
        {

            // Check Bank
            if (BankId.HasValue)
            {
                var Bank = context.Banks.ToList().SingleOrDefault(x => x.BankId == BankId);

                if (Bank == null)
                {
                    return false;
                }
            }

            return true;
        }


        public PartialViewResult GetActivities(int BankId)
        {
            var activities = context.Activities.ToList()
                .Where(x => x.BankId == BankId)
                .OrderBy(o => o.Recorded);

            return PartialView("Partials/_Activities", activities);
        }

        //public PartialViewResult GetBankstats()
        //{
        //    var currentBank = context.Banks.Where(x => x.IsCurrentBank).First();

        //    var model = new BankstatsModel
        //    {
        //        Bank = currentBank
        //    };

        //    return PartialView("Dashboard/_Bankstats", model);
        //}
    }
}