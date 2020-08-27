using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteppingStone.Domain.Entities
{
    public class Term
    {
        public Term()
        {
            Payments = new List<Payment>();
            Students = new List<Student>();
            Added = DateTime.Now;
        }

        [Key]
        public int TermId { get; set; }

        public int Year { get; set; }

        public int Position { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Display(Name = "Is Current Term?")]
        public bool IsCurrentTerm { get; set; }

        public virtual ICollection<Student> Students { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }

        public DateTime Added { get; set; }

        public DateTime Updated { get; set; }

        public DateTime? Deleted { get; set; }

        public static IDictionary<int, string> Positions()
        {
            return new Dictionary<int, string> {
                {0, "First" },
                {1, "Second" },
                {2, "Third" },
                {3, "Holiday" }
            };
        }


        // Not mapped to db
        [NotMapped]
        [Display(Name = "Term Period")]
        public string TermPeriod
        {
            get
            {
                return Positions()[Position];
            }
        }

        public string GetTerm()
        {
            return string.Format("{0} Term : From {1} to {2}", TermPeriod, StartDate.ToString("dd/MM/yyyy"), EndDate.ToString("dd/MM/yyyy"));
        }

        public string GetStatus()
        {
            if (EndDate < DateTime.Today)
            {
                return "Complete";
            }
            else if (StartDate > DateTime.Today)
            {
                return "Upcoming";
            }
            else if (IsCurrentTerm)
            {
                return "Active";
            }
            else
            {
                return "In-Active";
            }

        }

        public string GetStatusCssClass()
        {
            string status = GetStatus();

            switch (status)
            {
                case "Active":
                    return "success";
                case "Complete":
                    return "info";
                case "In-Active":
                    return "warning";
                case "Upcoming":
                    return "warning";
                default:
                    return "danger";
            }
        }

        [NotMapped]
        public bool Ended
        {
            get
            {
                return DateTime.Now > EndDate;
            }
        }

        [NotMapped]
        public double TotalPay
        {
            get
            {
                return Payments.Count() > 0 ? Payments.Sum(p => p.Amount) : 0;
            }
        }

        public double TotalBalance
        {
            get
            {
                return Students.Sum(p => p.Outstanding);
            }
        }

        public void ResetStudents()
        {
            foreach (var student in Students)
            {
                student.CurrentTermId = null;
            }
        }

    }
}
