using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SteppingStone.Domain.Enums;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Terms
{
    public class SearchTermsModel : ListModel
    {
        public SearchTermsModel()
        {

        }

        [Display(Name = "Term name")]
        public string Name { get; set; }

        [UIHint("_DatePicker")]
        public DateTime? StartDate { get; set; }

        [UIHint("_DatePicker")]
        public DateTime? EndDate { get; set; }

        public int? Period { get; set; }

        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(this.Name) || StartDate.HasValue || EndDate.HasValue || Period.HasValue) {
                return false;
            }
            else {
                return true;
            }
        }

        public IDictionary<int, string> Periods()
        {
            return new Dictionary<int, string> {
                {0, "First" },
                {1, "Second" },
                {2, "Third" },
                {3, "Holiday" }
            };
        }
    }
}