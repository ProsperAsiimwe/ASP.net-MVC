using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SteppingStone.Domain.Enums;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Payments
{
    public class SearchPaymentsModel : ListModel
    {
        public SearchPaymentsModel()
        {
            Terms = new List<Term>();
            Banks = new List<Bank>();
            ClassLevels = new List<ClassLevel>();
        }

        [Display(Name = "Student name")]
        public string Name { get; set; }

        [Display(Name = "Parent name")]
        public string ParentName { get; set; }

        [Display(Name = "Depositor's name")]
        public string Depositor { get; set; }

        [Display(Name = "Bank name")]
        public int? Bank { get; set; }

        [Display(Name = "Class")]
        public int? ClassLevel { get; set; }

        [Display(Name = "Bank Slip No")]
        public int? SlipNo { get; set; }

        public int? Term { get; set; }

        [UIHint("_DatePicker")]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [UIHint("_DatePicker")]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        public IEnumerable<Term> Terms { get; set; }

        public IEnumerable<Bank> Banks { get; set; }

        public IEnumerable<ClassLevel> ClassLevels { get; set; }


        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(this.Name) || !String.IsNullOrEmpty(this.ParentName) || !String.IsNullOrEmpty(this.Depositor) || SlipNo.HasValue || ClassLevel.HasValue || StartDate.HasValue || EndDate.HasValue || Bank.HasValue) {
                return false;
            }
            else {
                return true;
            }
        }
    }
}