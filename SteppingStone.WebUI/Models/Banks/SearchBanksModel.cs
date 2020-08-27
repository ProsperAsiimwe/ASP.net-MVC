using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SteppingStone.Domain.Enums;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Banks
{
    public class SearchBanksModel : ListModel
    {
        public SearchBanksModel()
        {

        }

        [Display(Name = "Bank name")]
        public string Name { get; set; }
        
        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(this.Name)) {
                return false;
            }
            else {
                return true;
            }
        }
    }
}