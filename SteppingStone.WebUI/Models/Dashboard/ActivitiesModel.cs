using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Foolproof;
using MagicApps.Models;

namespace SteppingStone.WebUI.Models.Dashboard
{
    public class ActivitiesModel
    {
        [UIHint("_DatePicker")]
        public DateTime? StartDate { get; set; }

        [UIHint("_DatePicker")]
        [GreaterThanOrEqualTo("StartDate", ErrorMessage = "End date must be greater than start date")]
        public DateTime? EndDate { get; set; }

        public string ErrorMessage { get; set; }

        public int? AdminId { get; set; }

        public int Total { get; set; }

        public IEnumerable<AjaxItem> Readings { get; set; }

        public int Duration()
        {
            if (!StartDate.HasValue || !EndDate.HasValue) {
                return 0;
            }

            return (int)(EndDate.Value - StartDate.Value).TotalDays + 1;
        }
    }
}