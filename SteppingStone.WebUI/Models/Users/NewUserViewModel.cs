using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Users
{
    public class NewUserViewModel : ProfileViewModel
    {
        public NewUserViewModel() { }

        public NewUserViewModel(ApplicationUser user)
        {
            SetFromEntity(user);
        }

        //[Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}