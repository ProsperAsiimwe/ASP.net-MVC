using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Users
{
    public class SearchUsersModel
    {
        [Display(Name = "First Name or Last Name")]
        public string Name { get; set; }

        public string Email { get; set; }

        public string RoleName { get; set; }

        public string[] Roles { get; set; }

        public int? DisplayId { get; set; }

        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(this.Name) || !String.IsNullOrEmpty(this.Email) || !String.IsNullOrEmpty(this.RoleName) || DisplayId.HasValue) {
                return false;
            }
            else {
                return true;
            }
        }
    }
}