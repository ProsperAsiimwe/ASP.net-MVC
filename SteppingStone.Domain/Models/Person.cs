using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteppingStone.Domain.Models
{
    public class Person : Address
    {
        [StringLength(4)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [StringLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [StringLength(60)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [NotMapped]
        [Display(Name = "Full name")]
        public string FullName {
            get {
                if (!string.IsNullOrEmpty(Title)) {
                    return string.Format("{0} {1} {2}", Title, FirstName, LastName);
                }
                else {
                    return string.Format("{0} {1}", FirstName, LastName);
                }
            }
        }

        [NotMapped]
        [Display(Name = "Full name")]
        public string Name
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }

        [EmailAddress]
        [StringLength(80)]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Telephone No")]
        public string PhoneNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Mobile No")]
        public string Mobile { get; set; }

        [StringLength(80)]
        public string Organisation { get; set; }

        [StringLength(80)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        public string EmailNameTuple()
        {
            return string.Format("{0}:{1}", Email, FullName);
        }

        public static string[] GetPersonTitles()
        {
            return new string[] {
                "Mr",
                "Mrs",
                "Miss",
                "Ms",
                "Dr",
                "Prof"
            };
        }
    }
}
