using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SteppingStone.Domain.Enums;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Messages
{
    public class SearchMessagesModel : ListModel
    {
        public SearchMessagesModel()
        {

        }
                
        public string Description { get; set; }

        [UIHint("_DatePicker")]
        public DateTime? StartDate { get; set; }

        [UIHint("_DatePicker")]
        public DateTime? EndDate { get; set; }

        public bool IsEmpty()
        {
            if (StartDate.HasValue || EndDate.HasValue || !String.IsNullOrEmpty(this.Description)) {
                return false;
            }
            else {
                return true;
            }
        }
        
        public string[] Statuses()
        {
            return new string[] { "Sent", "Pending", "Failed" };
        }        
    }
}