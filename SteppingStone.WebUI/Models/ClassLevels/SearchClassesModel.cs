using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SteppingStone.Domain.Enums;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.ClassLevels
{
    public class SearchClassesModel : ListModel
    {
        public SearchClassesModel()
        {

        }

        [Display(Name = "Student name")]
        public string Name { get; set; }

        [Display(Name = "Level")]
        public SchoolLevel? Level { get; set; }

        [Display(Name = "Study Mode")]
        public int? StudyMode { get; set; }

        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(this.Name) ||StudyMode.HasValue || Level.HasValue) {
                return false;
            }
            else {
                return true;
            }
        }

        public IDictionary<int, string> StudyModes()
        {
            return new Dictionary<int, string> {
                {1, "Day" },
                {2, "Boarding" },
                {3, "Unknown" }
            };
        }
    }
}