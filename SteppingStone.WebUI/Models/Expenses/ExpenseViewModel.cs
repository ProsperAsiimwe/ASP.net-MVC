using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Expenses
{
    public class ExpenseViewModel
    {
        public ExpenseViewModel() { Amount = 0; }

        public ExpenseViewModel(Expense Expense)
        {
            SetFromExpense(Expense);
        }

        public int ExpenseId { get; set; }

        [UIHint("_Money")]
        [Display(Name = "Amount (Ugx)")]
        public double? Amount { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public int Category { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name ="Expense By")]
        public string By { get; set; }
        
        public Expense ParseAsEntity(Expense Expense)
        {
            if (Expense == null) {
                Expense = new Expense();
            }
            
            Expense.Amount = Amount ?? 0;
            Expense.By = By;
            Expense.Description = Description;
            Expense.Category = Category;

            return Expense;
        }

        protected void SetFromExpense(Expense Expense)
        {
            this.ExpenseId = Expense.ExpenseId;
            this.Amount = Expense.Amount;
            this.By = Expense.By;
            this.Description = Expense.Description;
            this.Category = Expense.Category;
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