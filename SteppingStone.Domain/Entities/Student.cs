using SteppingStone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteppingStone.Domain.Entities
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

        [Display(Name = "Sponsee Gender")]
        public int Gender { get; set; }

        [Display(Name = "Transport Charge")]
        public double Transport { get; set; }

        [Display(Name = "Swimming Charge")]
        public double Swimming { get; set; }

        [Display(Name = "Uniform Fee")]
        public double Uniforms { get; set; }

        [Display(Name = "Medical Fee")]
        public double Medical { get; set; }

        [Display(Name = "Breaktime Fee")]
        public double BreaktimeFee { get; set; }

        [Display(Name ="Club Fee")]
        public double ClubFee { get; set; }

        [Display(Name ="Registration Fee")]
        public double RegistrationFee { get; set; }

        [Display(Name = "Half Day?")]
        public bool HalfDay { get; set; }

        public int StudyMode { get; set; }

        public Stream Stream { get; set; }

        public DateTime? Terminated { get; set; }

        public double OldDebt { get; set; }

        public DateTime Joined { get; set; }

        public double Balance { get; set; }

        [StringLength(50)]
        public string Dp { get; set; }

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
        public List<Payment> CurrentTermPayments
        {
            get
            {
                return Payments.Where(x => x.Term.IsCurrentTerm).ToList();
            }
        }

        [NotMapped]
        public double FirstCurrentTermPayment
        {
            get
            {
                var payt = CurrentTermPayments.Count() > 0 ? CurrentTermPayments.OrderBy(x => x.Date).First() : null;
                return payt == null ? 0 : payt.Amount;
            }
        }

        [NotMapped]
        public double OtherCurrentTermPayments
        {
            get
            {
                var payts = CurrentTermPayments.Count() > 0 ? CurrentTermPayments.OrderBy(x => x.Date).ToList() : null;

                if (payts == null)
                {
                    return 0;
                }

                payts.Remove(payts.First());

                if (payts.Count() <= 0)
                {
                    return 0;
                }

                return payts.Sum(p => p.Amount);
            }
        }

        public double FirstTermPayment(int termId)
        {
            var payt = Payments.Count() > 0 ? Payments.Where(p => p.TermId == termId).OrderBy(x => x.Date).First() : null;

            return payt == null ? 0 : payt.Amount;
        }

        public double OtherTermPayments(int termId)
        {
            var payts = Payments.Count() > 0 ? Payments.Where(p => p.TermId == termId).OrderBy(x => x.Date).ToList() : null;

            if (payts == null)
            {
                return 0;
            }

            payts.Remove(payts.First());

            if (payts.Count() <= 0)
            {
                return 0;
            }

            return payts.Sum(p => p.Amount);
        }

        [NotMapped]
        public Payment LastPayment
        {
            get
            {
                return Payments.Count() > 0 ? Payments.OrderByDescending(x => x.Date).First() : null;
            }
        }

        [NotMapped]
        public double Outstanding
        {
            get
            {
                return AmountToPay - StudentTermPaymentsTotal();
            }
        }

        [NotMapped]
        public double AmountToPay
        {
            get
            {
                var schoolFees = CurrentLevelId.HasValue ? CurrentLevel.SchoolFee : 0;
                var extras = Swimming + Transport + RegistrationFee + Uniforms + Medical + ClubFee + BreaktimeFee; // update 
                schoolFees = (CurrentTermId.HasValue && CurrentTerm.IsCurrentTerm) ? schoolFees + extras : 0;
                return schoolFees + OldDebt;
            }
        }

        public double TermBalance(int TermId)
        {
            var schoolFees = CurrentLevelId.HasValue ? CurrentLevel.SchoolFee : 0;
            var extras = Swimming + Transport + Medical + ClubFee + BreaktimeFee;
            var payments = Payments.ToList().Where(p => p.TermId == TermId).Sum(k => k.Amount);
            return schoolFees + extras - payments;
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

        public string GetName()
        {
            return string.Format("{0} - {1}", FullName, CurrentLevel.GetClass());
        }

    }
}
