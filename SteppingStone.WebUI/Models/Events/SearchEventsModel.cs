using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SteppingStone.Domain.Enums;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Events
{
    public class SearchEventsModel : ListModel
    {
        public SearchEventsModel()
        {

        }

        [Display(Name = "Event name")]
        public string Name { get; set; }
        
        public string Description { get; set; }

        [UIHint("_DatePicker")]
        public DateTime? StartDate { get; set; }

        [UIHint("_DatePicker")]
        public DateTime? EndDate { get; set; }

        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(this.Name) || StartDate.HasValue || EndDate.HasValue || !String.IsNullOrEmpty(this.Description)) {
                return false;
            }
            else {
                return true;
            }
        }
        
        public string[] Statuses()
        {
            return new string[] { "Completed", "Cancelled", "Coming Soon", "Very Soon" };
        }        
    }
}