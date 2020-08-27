using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SteppingStone.Domain.Models;

namespace SteppingStone.WebUI.Models.Users
{
    public class CreatePersonModel
    {
        [StringLength(4)]
        [Display(Name = "Title")]
        public string TitleId { get; set; }

        [Display(Name = "Forename(s)")]
        [Required]
        [StringLength(60)]
        public string FirstName { get; set; }

        [Display(Name = "Surname")]
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [UIHint("_PhoneNo")]
        [StringLength(20)]
        [Display(Name = "Telephone No")]
        public string Phone { get; set; }

        [UIHint("_PhoneNo")]
        [StringLength(20)]
        [Display(Name = "Mobile No")]
        public string Mobile { get; set; }

        //[Required]
        [UIHint("_Email")]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        public string[] Titles { get; set; }

        public virtual void SetLists()
        {
            this.Titles = Person.GetPersonTitles();
        }

        public Person Get()
        {
            return new Person {
                Title = TitleId,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                PhoneNumber = Phone,
                Mobile = Mobile
                
            };
        }

        public void Set(Person person)
        {
            this.TitleId = person.Title;
            this.FirstName = person.FirstName;
            this.LastName = person.LastName;
            this.Phone = person.PhoneNumber;
            this.Email = person.Email;
            this.Mobile = person.Mobile;
        }
    }
}