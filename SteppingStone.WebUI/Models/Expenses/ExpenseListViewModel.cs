using System.Collections.Generic;
using MagicApps.Models;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Expenses
{
    public class ExpenseListViewModel
    {
        public IEnumerable<Expense> Expenses { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public SearchExpensesModel SearchModel { get; set; }

        public string[] Roles { get; set; }
    }
}