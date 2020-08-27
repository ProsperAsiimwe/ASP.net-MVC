using SteppingStone.Domain.Entities;
using SteppingStone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Students
{
    public class SearchStudentsModel : ListModel
    {
        public SearchStudentsModel()
        {
            Terms = new List<Term>();
            ClassLevels = new List<ClassLevel>();
        }

        [Display(Name = "Pupil name")]
        public string Name { get; set; }

        [Display(Name = "Parent name")]
        public string ParentName { get; set; }
        
        [Display(Name = "Class")]
        public int? ClassLevel { get; set; }
                
        public int? Term { get; set; }

        public int? Gender { get; set; }

        public int? StudyMode { get; set; }

        public Stream? Stream { get; set; }

        public string Status { get; set; }

        [UIHint("_Checkbox")]
        [Display(Name = "Parents Unavailable")]
        public bool NoParents { get; set; }

        public IEnumerable<Term> Terms { get; set; }
        
        public IEnumerable<ClassLevel> ClassLevels { get; set; }

        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(this.Name) || !String.IsNullOrEmpty(this.ParentName) || !String.IsNullOrEmpty(this.Status) || ClassLevel.HasValue || ClassLevel.HasValue || Gender.HasValue || StudyMode.HasValue || Term.HasValue || Stream.HasValue)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public IDictionary<int, string> Genders
        {
            get
            {
                return new Dictionary<int, string> {
                    {1, "Male" },
                    {2, "Female" },
                    {3, "Unknown" }
                };
            }
        }

        public string[] Statuses
        {
            get
            {
                return new string[] { "Terminated", "Active", "Has Oustanding"/*, "No Payments"*/ };
            }
        }

        public IDictionary<int, string> StudyModes
        {
           get
            {
                return new Dictionary<int, string> {
                    {1, "Day" },
                    {2, "Boarding" },
                    {3, "Unknown" }
                };
            }
        }
    }
}