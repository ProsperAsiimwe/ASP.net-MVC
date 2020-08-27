using SteppingStone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Payments
{
    public class PaymentViewModel
    {
        public PaymentViewModel()
        {
            Students = new List<Student>();
            Banks = new List<Bank>();
            Classes = new List<ClassLevel>();
            Terms = new List<Term>();
            Date = DateTime.Today;
            Amount = 0;
        }

        public PaymentViewModel(Payment entity)
        {
            PaymentId = entity.PaymentId;
            StudentId = entity.StudentId;
            TermId = entity.TermId;
            Amount = entity.Amount;
            Date = entity.Date;
            BankId = entity.BankId;
            SlipNo = entity.SlipNo;
            PaidInBy = entity.PaidInBy;
            ClassLevelId = entity.ClassLevelId;
        }   

        public int PaymentId { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required]
        [Display(Name = "Term")]
        public int TermId { get; set; }

        [Required]
        [Display(Name = "Class Level")]
        public int ClassLevelId { get; set; }

        [Required]
        [UIHint("_Money")]
        [Display(Name = "Total Amount (Ugx)")]
        public double? Amount { get; set; }

        [Required]
        [UIHint("_DatePicker")]
        [Display(Name = "Payment Date")]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Bank")]
        public int BankId { get; set; }

        //[Required]
        [Display(Name = "Bank Slip No/Reference Id")]
        public int SlipNo { get; set; }

        [Display(Name = "Paid in by")]
        [StringLength(200)]
        public string PaidInBy { get; set; }

        //[Display(Name = "Payment Method")]
        //public int Method { get; set; }

        public DateTime? Deleted { get; set; }

        public IEnumerable<Student> Students { get; set; }

        public IEnumerable<ClassLevel> Classes { get; set; }

        public IEnumerable<Bank> Banks { get; set; }

        public IEnumerable<Term> Terms { get; set; }

        public IDictionary<int, string> Methods()
        {
            return new Dictionary<int, string> {
                {0, "Cash" },
                {1, "Cheque" },
                {2, "Mobile Money" },
                {3, "Unknown" }
            };
        }

        public Payment ParseAsEntity(Payment entity)
        {
            if(entity == null)
            {
                entity = new Payment();
            }

            entity.StudentId = StudentId;
            entity.TermId = TermId;
            entity.Amount = Amount ?? 0;
            entity.Date = Date;
            entity.BankId = BankId;
            entity.SlipNo = SlipNo;
            entity.PaidInBy = PaidInBy;
            entity.ClassLevelId = ClassLevelId;

            return entity;
        }
    }
}