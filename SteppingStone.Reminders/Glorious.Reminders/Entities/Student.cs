using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glorious.Entities
{
    public class Student 
    {
        public Student()
        {
            Parents = new List<StudentParent>();
            Payments = new List<Payment>();
            StudentEvents = new List<StudentEvent>();
        }

        [Key]
        public int StudentId { get; set; }

        [ForeignKey("CurrentLevel")]
        public int? CurrentLevelId { get; set; }

        [ForeignKey("CurrentTerm")]
        public int? CurrentTermId { get; set; }

        [StringLength(50)]
        [Display(Name = "Forename(s)")]
        public string FirstName { get; set; }

        [StringLength(60)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime? DOB { get; set; }
        
        [Display(Name = "Pupil Gender")]
        public int Gender { get; set; }

        [Display(Name = "Transport Charge")]
        public double Transport { get; set; }

        [Display(Name = "Swimming Charge")]
        public double Swimming { get; set; }
        
        public int StudyMode { get; set; }

        public DateTime? Terminated { get; set; }

        public double OldDebt { get; set; }

        public DateTime Joined { get; set; }

        public double Balance { get; set; }

        // virtuals 

        public virtual ICollection<Payment> Payments { get; set; }

        public virtual ICollection<StudentParent> Parents { get; set; }

        public virtual ClassLevel CurrentLevel { get; set; }

        public virtual Term CurrentTerm { get; set; }

        public virtual ICollection<StudentEvent> StudentEvents { get; set; }

        // Not Mapped to Db

        [NotMapped]
        [Display(Name = "Full name")]
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }

        // Functions

        public bool UpdateDebt(double Amount)
        {
            OldDebt = OldDebt + Amount;

            return true;
        }

        public static IDictionary<int, string> Genders()
        {
            return new Dictionary<int, string> {
                {1, "Male" },
                {2, "Female" },
                {3, "Unknown" }
            };
        }


        public double GetTermPayments(Term term)
        {
            return Payments.Where(x => x.Term.TermId == term.TermId).Sum(y => y.Amount);
        }

        // Not to be mapped to Db
        [NotMapped]
        public string Sex
        {
            get
            {
                return Genders()[Gender];
            }
        }

        [NotMapped]
        public IEnumerable<Payment> CurrentTermPayments
        {
            get
            {
                return CurrentTermId.HasValue ? Payments.Where(x => x.TermId == CurrentTermId.Value) : null;
            }
        }

        [NotMapped]
        public Payment LastPayment
        {
            get {
                return Payments.Count() > 0 ? Payments.OrderByDescending(x => x.Date).First() : null;
            }
        }

        [NotMapped]
        public double Outstanding
        {
            get
            {
                var schoolFees = CurrentLevelId.HasValue ? CurrentLevel.SchoolFee - StudentTermPaymentsTotal() : 0;
                var extras = Swimming + Transport;
                return schoolFees + extras + OldDebt;
            }
        }

        [NotMapped]
        public bool HasOutstanding
        {
            get
            {
                return Outstanding > 0;
            }
        }
        
        [NotMapped]
        public double Total
        {
            get
            {
                return Payments.Sum(x => x.Amount);
            }
        }

        [NotMapped]
        public bool NoPay
        {
            get
            {
                return CurrentTermPayments == null; /*? CurrentTermPayments.Sum(x => x.Amount) <= 0 : */
            }
        }

        [NotMapped]
        public bool NoParents
        {
            get
            {
                return Parents.Count() <= 0;
            }
        }

        public double StudentTermPaymentsTotal()
        {
            return CurrentTermPayments == null ? 0 : CurrentTermPayments.Sum(x => x.Amount);
        }

        [NotMapped]
        public string Age
        {
            get
            {
                var age = DOB.HasValue ? (int)((DateTime.Today - DOB.Value).TotalDays / 365) : 0;
                return age > 0 ? string.Format("{0} years old", age) : "Age Unknown";
            }
        }

        public string GetStatus()
        {
            if (Terminated.HasValue)
            {
                return "Terminated";
            }
            else if (NoPay)
            {
                return "No Payments";
            }
            else if (HasOutstanding)
            {
                return "Has Oustanding";
            }
            else
            {
                return "Active";
            }
        }

        public string GetStatusCssClass()
        {
            string status = GetStatus();

            switch (status)
            {
                case "Active":
                    return "success";
                case "Has Oustanding":
                    return "warning";
                case "No Payments":
                    return "info";
                case "Terminated":
                    return "danger";
                default:
                    return "danger";
            }
        }
        public string GetLastPayDate()
        {
            return LastPayment == null ? "-" : LastPayment.Date.ToString("ddd, dd MMM yyyy");
        }

        public string GetCurrentTerm()
        {
            return CurrentTermId.HasValue ? CurrentTerm.GetTerm() : "-";
        }

        public string[] GetParents()
        {
            return Parents.Select(x => x.Parent.FullName).ToArray();
        }

        public bool Matches(string name)
        {
            name = name.ToLower();

            return FirstName.ToLower().Contains(name) || LastName.ToLower().Contains(name);
        }

        //public string GetName()
        //{
        //    return string.Format("{0} - {1}", FullName, CurrentLevel.GetClass());
        //}

    }
}
