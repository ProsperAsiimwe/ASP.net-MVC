using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SteppingStone.Domain.Enums;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Parents
{
    public class SearchParentsModel : ListModel
    {
        public SearchParentsModel()
        {

        }

        [Display(Name = "Student name")]
        public string Name { get; set; }

        [Display(Name = "Parent name")]
        public string ParentName { get; set; }        


        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(this.Name) || !String.IsNullOrEmpty(this.ParentName)) {
                return false;
            }
            else {
                return true;
            }
        }
    }
}