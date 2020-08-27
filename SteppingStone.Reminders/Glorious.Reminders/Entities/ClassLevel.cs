//using Glorious.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glorious.Entities
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
        
        public DateTime Added { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime Updated { get; set; }

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

        //public string GetClass()
        //{
        //    string level = SchoolLevel.ToString() ?? "Primary";
        //    return string.Format("{0} {1}", level, Level);
        //}

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
        public IEnumerable<Payment> Payments
        {
            get
            {
                return Students.SelectMany(x => x.CurrentTermPayments);
            }
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

        //[NotMapped]
        //[Display(Name = "Total Revenue")]
        //public double TotalRevenue
        //{
        //    get
        //    {
        //        return Payments.Sum(x => x.Amount);
        //    }
        //}

        //[NotMapped]
        //public IEnumerable<Student> InDebt
        //{
        //    get
        //    {
        //        return Students.Where(x => x.Student.HasOutstanding).Select(x => x.Student).ToList();
        //    }
        //}
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
