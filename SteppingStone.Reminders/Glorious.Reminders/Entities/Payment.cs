using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glorious.Entities
{
    public class Payment
    {
        public Payment()
        {
            Recorded = DateTime.Now;
        }

        [Key]
        public int PaymentId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Term")]
        public int TermId { get; set; }

        [ForeignKey("ClassLevel")]
        public int ClassLevelId { get; set; }

        public double Amount { get; set; }

        public DateTime Date { get; set; }

        public DateTime Recorded { get; set; }

        [ForeignKey("Bank")]
        public int BankId { get; set; }

        public int SlipNo { get; set; }
                
        public int Method { get; set; }

        [Display(Name = "Paid in by")]
        [StringLength(200)]
        public string PaidInBy { get; set; }

        public DateTime? Deleted { get; set; }

        // virtuals
        public virtual Bank Bank { get; set; }

        public virtual Student Student { get; set; }

        public virtual Term Term { get; set; }

        public virtual ClassLevel ClassLevel { get; set; }

        public static IDictionary<int, string> Methods()
        {
            return new Dictionary<int, string> {
                {0, "Cash" },
                {1, "Cheque" },
                {2, "Unknown" }
            };
        }

        [NotMapped]
        [Display(Name = "Payment Method")]
        public string PaymentMethod {
            get {
                return Method > 0 ? Methods()[Method] : Methods()[2];
            }
        }

        public string GetAmount()
        {
            return string.Format("{0}/=", Amount.ToString("n0"));
        }
                
        public string GetStatusCssClass()
        {
            string status = GetStatus();

            switch (status)
            {
                case "Less Than Half":
                    return "danger";
                case "Full Payment":
                    return "success";
                case "Installment":
                    return "warning";
                case "Extra":
                    return "info";
                default:
                    return "danger";
            }
        }

        public string GetStatus()
        {
            if (LessThanHalf())
            {
                return "Less Than Half";
            }
            else if (HasExtra())
            {
                return "Extra";
            }
            else if (IsPortion())
            {
                return "Installment";
            }
            else if (IsFullPayment())
            {
                return "Full Payment";
            }
            else
            {
                return "Flag";
            }
        }

        public bool IsFullPayment()
        {
            if (IsTermPaymentComplete())
            {
                return true;
            }

            return Amount == ClassLevel.SchoolFee;
        }

        public bool LessThanHalf()
        {
            return GetStudentTermPayments().Sum(x => x.Amount) <= ((ClassLevel.SchoolFee)/2);
        }

        public bool HasExtra()
        {
            return GetStudentTermPayments().Sum(x => x.Amount) > ClassLevel.SchoolFee;
        }

        public bool IsPortion()
        {            
            return GetStudentTermPayments().Sum(x => x.Amount) < ClassLevel.SchoolFee;
        }

        public IEnumerable<Payment> GetStudentTermPayments()
        {
            return Student.Payments.Where(x => x.TermId == TermId);
        }
        public bool IsTermPaymentComplete()
        {
            return StudentTermPaymentsTotal() >= ClassLevel.SchoolFee;
        }

        public double Balance()
        {
            return ClassLevel.SchoolFee - StudentTermPaymentsTotal();
        }

        public double StudentTermPaymentsTotal()
        {
            return GetStudentTermPayments().Sum(x => x.Amount);
        }
    }
}
