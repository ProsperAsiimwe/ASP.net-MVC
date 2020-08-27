using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SteppingStone.WebUI.Models.Expenses
{
    public class SearchExpensesModel : ListModel
    {
        public SearchExpensesModel()
        {

        }

        [Display(Name = "Expense By")]
        public string Name { get; set; }
        
        [Display(Name = "Branch")]
        public int? BranchId { get; set; }

        public string Status { get; set; }

        [UIHint("_DatePicker")]
        public DateTime? StartDate { get; set; }

        [UIHint("_DatePicker")]
        public DateTime? EndDate { get; set; }
        
        public int? Category { get; set; }

        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(this.Name) || BranchId.HasValue || StartDate.HasValue || EndDate.HasValue || Category.HasValue) {
                return false;
            }
            else {
                return true;
            }
        }

        public IDictionary<int, string> Categories()
        {
            return new Dictionary<int, string>
            {
                {1, "Shopping" },
                {2, "Food" },
                {3, "NSSF" },
                {4, "Salaries" },
                {5, "Purchases" },
                {6, "Transport" },
                {7, "Airtime" },
                {8, "Electricity" },
                {9, "Water" },
                {10, "Other" }
            };
        }

    }
}