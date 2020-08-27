using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SteppingStone.WebUI.Models.Users;

namespace SteppingStone.WebUI.Models.Users
{
    public class UserRolesViewModel : UserModel, IValidatableObject
    {
        public UserRolesViewModel()
        {
            this.CurrentRoles = new string[] { };
            this.NewRoles = new string[] { };
            this.RoleList = new string[] { };
        }

        [UIHint("_UL")]
        [Display(Name = "Current roles")]
        public string[] CurrentRoles { get; set; }

        [UIHint("_CheckboxList")]
        [Display(Name = "New roles")]
        public string[] NewRoles { get; set; }

        public string[] RoleList { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            if (this.NewRoles.Count() <= 0) {
                errors.Add(new ValidationResult("You must choose at least one Role. If you are unsure, choose 'User'"));
            }

            return errors;
        }
    }
}