using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Infrastructure;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.Expenses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Controllers
{
    [RoutePrefix("Expenses")]
    [Authorize]
    public class ExpensesController : BaseController
    {
        // GET: Expenses
        public ExpensesController()
        {
            ViewBag.Area = "Expenses";

        }

        // GET: Expenses
        [Authorize(Roles = "Developer, Admin")]
        [Route("Page-{page:int}", Order = 13)]
        [Route("", Name = "Expenses_Index")]
        public ActionResult Index(SearchExpensesModel search, int page = 1)
        {
            // Return all Expenses
            // If not a post-back (i.e. initial load) set the searchModel to session
            if (Request.Form.Count <= 0)
            {
                if (search.IsEmpty() && Session["SearchExpensesModel"] != null)
                {
                    search = (SearchExpensesModel)Session["SearchExpensesModel"];
                }
            }

            var helper = new ExpenseHelper();
            var model = helper.GetExpenseList(search, search.ParsePage(page));
            
            Session["SearchExpensesModel"] = search;

            return View(model);
        }

        // GET: Expenses
        [Authorize(Roles = "Developer, Admin")]
        [Route("New")]
        [Route("New/{ImportId:int:min(0)}")]
        public PartialViewResult New(int? ImportId)
        {
            var model = GetExpenseModel(null);

            return PartialView("Partials/_New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ExpenseViewModel model)
        {
            if (!IsRoutingOK(null))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(null, model);
            
            return RedirectOnError();
        }

        // GET: Expenses
        [Authorize(Roles = "Developer, Admin")]
        public ActionResult Show(int ExpenseId)
        {
            if (!IsRoutingOK(ExpenseId))
            {
                return RedirectOnError();
            }

            var Expense = GetExpense(ExpenseId);

            return View(Expense);
        }

        // GET: Expenses
        [Authorize(Roles = "Developer, Admin")]
        public ActionResult Edit(int ExpenseId)
        {
            if (!IsRoutingOK(ExpenseId))
            {
                return RedirectOnError();
            }

            var model = GetExpenseModel(ExpenseId);

            return PartialView("Partials/_New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int ExpenseId, ExpenseViewModel model)
        {
            if (!IsRoutingOK(ExpenseId))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(ExpenseId, model);
            
            if (success)
            {
                return RedirectOnSuccess(ExpenseId);
            }

            // If we got this far, an error occurred
            return RedirectOnError();
        }

        // GET: Expenses
        [Authorize(Roles = "Developer, Admin")]
        public ActionResult Delete(int ExpenseId)
        {
            if (!IsRoutingOK(ExpenseId))
            {
                return RedirectOnError();
            }

            return View(GetExpense(ExpenseId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Destroy(int ExpenseId)
        {
            if (!IsRoutingOK(ExpenseId))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(ExpenseId);
            var upsert = await helper.DeleteExpense();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Delete", new { ExpenseId = ExpenseId });
        }

        

        #region Non-CRUD Actions


        #endregion

        #region Child Actions

        public PartialViewResult GetBreadcrumb(int ExpenseId, bool mainAsLink = true)
        {
            var Expense = GetExpense(ExpenseId);
            ViewBag.MainAsLink = mainAsLink;
            return PartialView("Partials/_Breadcrumb", Expense);
        }

        public PartialViewResult GetSummary(int ExpenseId)
        {
            var Expense = GetExpense(ExpenseId);
            return PartialView("Partials/_Summary", Expense);
        }

        public PartialViewResult GetActivities(int ExpenseId)
        {
            var activities = context.Activities.ToList()
                .Where(x => x.ExpenseId == ExpenseId)
                .OrderBy(o => o.Recorded);
            return PartialView("Partials/_Activities", activities);
        }        

        #endregion

        #region Controller Helpers

        private bool IsRoutingOK(int? ExpenseId)
        { 
            // Check Expense
            if (ExpenseId.HasValue)
            {
                var Expense = context.Expenses.SingleOrDefault(x => x.ExpenseId == ExpenseId);

                if (Expense == null)
                {
                    return false;
                }
            }

            return true;
        }

        private ExpenseViewModel GetExpenseModel(int? ExpenseId)
        {
            ExpenseViewModel model;

            if (ExpenseId.HasValue)
            {
                var Expense = GetExpense(ExpenseId.Value);
                model = new ExpenseViewModel(Expense);
            }
            else
            {
                model = new ExpenseViewModel();
            }

            //model.SetLists();
            return model;
        }

        private async Task<bool> Upsert(int? ExpenseId, ExpenseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var helper = (ExpenseId.HasValue ? GetHelper(ExpenseId.Value) : new ExpenseHelper() { ServiceUserId = GetUserId() });
                var upsert = await helper.UpsertExpense(UpsertMode.Admin, model);

                if (upsert.i_RecordId() > 0)
                {
                    ShowSuccess(upsert.ErrorMsg);

                    // OK, has the Expense been sent an email to complete the Expense? If not, send now
                    var Expense = helper.Expense;
                    
                    return true;
                }
                else
                {
                    ShowError(upsert.ErrorMsg);
                }
            }

            //model.SetLists();
            return false;
        }


        private RedirectToRouteResult RedirectOnError()
        {
            return RedirectToAction("Index");
        }

        private RedirectToRouteResult RedirectOnSuccess(int ExpenseId)
        {
            return RedirectToAction("Show", new { ExpenseId = ExpenseId });
        }

        private ExpenseHelper GetHelper(int ExpenseId)
        {
            ExpenseHelper helper = new ExpenseHelper(ExpenseId);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private ExpenseHelper GetHelper(Expense Expense)
        {
            var helper = new ExpenseHelper(Expense);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private Expense GetExpense(int ExpenseId)
        {
            return context.Expenses.Find(ExpenseId);
        }

        #endregion
    }
}