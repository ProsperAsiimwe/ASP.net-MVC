using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Parents
{
    public class ParentViewModel : CreatePersonModel
    {

        public ParentViewModel() {
            Students = new List<Student>();
            SelectedStudents = new int[] { };
        }

        public ParentViewModel(Parent entity)
        {
            ParentId = entity.ParentId;

            Line1 = entity.Line1;
            Line2 = entity.Line2;
            Line3 = entity.Line3;
            Line4 = entity.Line4;
            SelectedStudents = entity.Students.Select(x => x.StudentId).ToArray();
            Set(entity);
        }
        public int ParentId { get; set; }

        [Required]
        [Display(Name ="Students")]
        public int[] SelectedStudents { get; set; }

        [StringLength(50)]
        [Display(Name = "Address")]
        public string Line1 { get; set; }

        [StringLength(50)]
        [Display(Name = "District")]
        public string Line2 { get; set; }

        [StringLength(50)]
        [Display(Name = "Village")]
        public string Line3 { get; set; }

        [StringLength(50)]
        [Display(Name = "Town/City")]
        public string Line4 { get; set; }

        [StringLength(50)]
        [Display(Name = "Address (continued)")]
        public string Line5 { get; set; }

        public IEnumerable<Student> Students { get; set; }

        public Parent ParseAsEntity(Parent entity)
        {
            if(entity == null)
            {
                entity = new Parent();
            }

            entity.Title = this.TitleId;
            entity.FirstName = FirstName;
            entity.LastName = LastName;
            entity.Email = Email;
            entity.PhoneNumber = Phone;
            entity.Mobile = Mobile;
            
            // Address
            entity.Line1 = Line1;
            entity.Line2 = Line2;
            entity.Line3 = Line3;
            entity.Line4 = Line4;

            return entity;
        }


    }
}