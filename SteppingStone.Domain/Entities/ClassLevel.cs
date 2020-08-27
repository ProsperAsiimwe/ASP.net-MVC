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
    public class ClassLevel
    {
        public ClassLevel() {

            Students = new List<Student>();
            Added = DateTime.Now;
        }

        [Key]
        public int ClassLevelId { get; set; }

        public int Level { get; set; }

        [Display(Name ="Study Mode")]
        public int StudyMode { get; set; }

        [Display(Name = "School Fees (Ugx)")]
        public double SchoolFee { get; set; }

        [Display(Name = "School Level")]
        public SchoolLevel SchoolLevel { get; set; }

        [Display(Name = "Half Day?")]
        public bool HalfDay { get; set; }

        public DateTime Added { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime Updated { get; set; }

        public DateTime? Deleted { get; set; }

        //[ForeignKey("Term")]
        //public int TermId { get; set; }

        // virtuals
        //public virtual Term Term { get; set; }

        public virtual ICollection<Student> Students { get; set; }

        // Functions

        public static IDictionary<int, string> StudyModes()
        {
            return new Dictionary<int, string> {
                {1, "Day" },
                {2, "Boarding" },
                {3, "Unknown" }
            };
        }

        public string GetClass()
        {
            

            if (SchoolLevel == SchoolLevel.Nursery)
            {
                string className = String.Empty;

                switch (Level)
                {
                    case 0:
                        className = "Daycare class";
                        break;
                    case 1:
                        className = "Baby class";
                        break;
                    case 2:
                         className = "Middle class";
                        break;
                    case 3:
                        className = "Pre-Primary class";
                        break;
                    default:
                        return "";
                }

                if (HalfDay)
                {
                    className = string.Format("{0} - Half Day", className);
                }

                return className;
            }
            else
            {
                string level = SchoolLevel.ToString() ?? "Primary";

                return string.Format("{0} {1} - {2}", level, Level, Mode);
            }
        }

        public ClassLevel MutateLevel()
        {
            return new ClassLevel {
                Level = this.Level,
                SchoolFee = this.SchoolFee,
                StudyMode = this.StudyMode                
            };
        }

        // Not Mapped
        [NotMapped]
        public List<Payment> Payments
        {
            get
            {
                return Students.Count() > 0 ? Students.SelectMany(m => m.CurrentTermPayments).ToList() : new List<Payment>();
            }
        }

        [NotMapped]
        public double Outstanding
        {
            get
            {
                return StudentsThisTerm.Count() > 0 ? StudentsThisTerm.Where(x => x.HasOutstanding).Sum(x => x.Outstanding) : 0;
            }
        }

        [NotMapped]
        public double Ratio
        {
            get
            {
                if(TotalRevenue == 0 || AmountToBePaid == 0)
                {
                    return 100;
                }

                return (TotalRevenue/AmountToBePaid) * 100;
            }
        }

        [NotMapped]
        public Payment LastPayment
        {
            get
            {
                return Payments == null ? null : Payments.Count() > 0 ? Payments.OrderByDescending(x => x.Date).First() : null;
            }
        }

        [NotMapped]
        [Display(Name = "Study Mode")]
        public string Mode {
            get {
                return StudyModes()[StudyMode];
            }
        }

        public bool HasStudent(string name)
        {
            var students = Students.Where(x => x.FirstName.ToLower().Contains(name) || x.LastName.ToLower().Contains(name));
            return students.Count() > 0;
        }

        public string GetLastPayDate()
        {
            return LastPayment == null ? "-" : LastPayment.Date.ToString("ddd, dd MMM yyyy");
        }

        public string GetTermTotal()
        {
            return Payments == null ? "-" : Payments.Sum(x => x.Amount).ToString("n0");
        }

        [NotMapped]
        [Display(Name = "Total Revenue")]
        public double TotalRevenue
        {
            get
            {
                return Payments.Count() > 0 ? Payments.Sum(x => x.Amount) : 0;
            }
        }

        [NotMapped]
        [Display(Name = "Amount To Be Paid")]
        public double AmountToBePaid
        {
            get
            {
                return StudentsThisTerm.Count() > 0 ? StudentsThisTerm.Sum(x => x.AmountToPay) : 0;
            }
        }

        [NotMapped]
        public IEnumerable<Student> StudentsThisTerm
        {
            get
            {
                return Students.Where(x => x.CurrentTermPayments.Count() > 0).ToList();
            }
        }

        public string GetStatus()
        {
            return "Active";
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
    }
}
