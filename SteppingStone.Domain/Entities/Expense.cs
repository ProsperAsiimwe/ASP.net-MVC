using SteppingStone.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteppingStone.Domain.Entities
{
    public class Expense
    {
        public Expense()
        {
            Date = UgandaDateTime.DateNow();
        }

        [Key]
        public int ExpenseId { get; set; }

        [Display(Name ="Amount (Ugx)")]
        public double Amount { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public int Category { get; set; }

        [StringLength(100)]
        public string By { get; set; }
        
        public DateTime Date { get; set; }
        
        [NotMapped]
        public string CategoryValue
        {
            get
            {
                return Categories()[Category];
            }
        }
        
        public static IDictionary<int, string> Categories()
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
