using SteppingStone.Domain.Entities;
using SteppingStone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.ClassLevels
{
    public class ClassLevelViewModel
    {
        public ClassLevelViewModel() { SchoolFee = 0; }

        public ClassLevelViewModel(ClassLevel entity) 
        {
            ClassLevelId = entity.ClassLevelId;
            Level = entity.Level;
            StudyMode = entity.StudyMode;
            SchoolFee = entity.SchoolFee;
            SchoolLevel = entity.SchoolLevel;
            HalfDay = entity.HalfDay;
        }

        public int ClassLevelId { get; set; }
        
        [Display(Name ="Stage (e.g 1 or 2)")]
        public int Level { get; set; }

        public int StudyMode { get; set; }

        [Required]
        [UIHint("_Money")]
        public double? SchoolFee { get; set; }

        public SchoolLevel SchoolLevel { get; set; }

        [UIHint("_Checkbox")]
        [Display(Name = "Half Day?")]
        public bool HalfDay { get; set; }

        public IDictionary<int, string> StudyModes()
        {
            return new Dictionary<int, string> {
                {1, "Day" },
                {2, "Boarding" },
                {3, "Unknown" }
            };
        }

        public ClassLevel ParseAsEntity(ClassLevel entity)
        {
            if(entity == null)
            {
                entity = new ClassLevel();
            }

            entity.Level = Level;
            entity.StudyMode = StudyMode;
            entity.SchoolFee = SchoolFee ?? 0;
            entity.SchoolLevel = SchoolLevel;
            entity.Updated = DateTime.Now;
            entity.HalfDay = HalfDay;

            return entity;
        }

        public bool IsModelLevelValid()
        {
            if((SchoolLevel == SchoolLevel.Nursery && Level > 3) || (SchoolLevel == SchoolLevel.Primary && Level > 7) || (SchoolLevel == SchoolLevel.Secondary && Level > 6))
            {
                return false;
            }

            return true;
        }
    }
}