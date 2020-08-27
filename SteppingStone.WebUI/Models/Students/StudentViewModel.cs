using SteppingStone.Domain.Entities;
using SteppingStone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SteppingStone.WebUI.Models.Students
{
    public class StudentViewModel
    {
        public StudentViewModel()
        {
            Joined = DateTime.Now;
            ShowEnrollOption = true;
            RegistrationFee = 100000;
            Swimming = Transport = Medical = BreaktimeFee = Uniforms = 0;
        }

        public StudentViewModel(Student entity)
        {
            StudentId = entity.StudentId;
            FirstName = entity.FirstName;
            LastName = entity.LastName;
            DOB = entity.DOB;
            Gender = entity.Gender;
            Joined = entity.Joined;
            Transport = entity.Transport;
            Swimming = entity.Swimming;
            DisplayPic = entity.Dp;
            Stream = entity.Stream;
            Medical = entity.Medical;
            Uniforms = entity.Uniforms;
            BreaktimeFee = entity.BreaktimeFee;
            ClubFee = entity.ClubFee;
            RegistrationFee = entity.RegistrationFee;

            if (entity.CurrentLevelId.HasValue)
            {
                CurrentLevelId = entity.CurrentLevelId.Value;
            }
            if ((entity.CurrentTermId.HasValue && !entity.CurrentTerm.IsCurrentTerm) || !entity.CurrentTermId.HasValue)
            {
                ShowEnrollOption = true;
            }

        }

        public int StudentId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Forename(s)")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [UIHint("_DatePicker")]
        [Display(Name = "Date of Birth")]
        public DateTime? DOB { get; set; }

        [UIHint("_DatePicker")]
        [Display(Name = "Date Joined")]
        public DateTime Joined { get; set; }

        [Required]
        [Display(Name = "Pupil Gender")]
        public int Gender { get; set; }

        [Display(Name = "Current Class")]
        public int CurrentLevelId { get; set; }

        [UIHint("_Money")]
        [Display(Name = "Transport Charge")]
        public double? Transport { get; set; }

        [UIHint("_Money")]
        [Display(Name = "Swimming Charge")]
        public double? Swimming { get; set; }

        [UIHint("_Checkbox")]
        [Display(Name = "Enroll to Current Term")]
        public bool Enroll { get; set; }

        [UIHint("_Money")]
        [Display(Name = "Uniform Fee")]
        public double? Uniforms { get; set; }

        [UIHint("_Money")]
        [Display(Name = "Medical Fee")]
        public double? Medical { get; set; }

        [UIHint("_Money")]
        [Display(Name = "Breaktime Fee")]
        public double? BreaktimeFee { get; set; }

        [UIHint("_Money")]
        [Display(Name = "Club Fee")]
        public double? ClubFee { get; set; }

        [UIHint("_Money")]
        [Display(Name = "Registration Fee")]
        public double? RegistrationFee { get; set; }
        
        public bool ShowEnrollOption { get; set; }

        public HttpPostedFileBase Dp { get; set; }

        public string DisplayPic { get; set; }

        public Stream Stream { get; set; }

        public IEnumerable<ClassLevel> ClassLevels { get; set; }

        public IDictionary<int, string> Genders()
        {
            return new Dictionary<int, string> {
                {1, "Male" },
                {2, "Female" },
                {3, "Unknown" }
            };
        }

        public IDictionary<int, string> StudyModes()
        {
            return new Dictionary<int, string> {
                {1, "Day" },
                {2, "Boarding" },
                {3, "Unknown" }
            };
        }
        public Student ParseAsEntity(Student entity)
        {
            if (entity == null)
            {
                entity = new Student();
            }

            entity.FirstName = FirstName;
            entity.LastName = LastName;
            entity.DOB = DOB;
            entity.Gender = Gender;
            entity.CurrentLevelId = CurrentLevelId;
            entity.Joined = Joined;
            entity.Transport = Transport ?? 0;
            entity.Swimming = Swimming ?? 0;
            entity.Stream = Stream;
            entity.Medical = Medical ?? 0;
            entity.Uniforms = Uniforms ?? 0;
            entity.BreaktimeFee = BreaktimeFee ?? 0;
            entity.ClubFee = ClubFee ?? 0;
            entity.RegistrationFee = RegistrationFee ?? 0;

            return entity;
        }

    }
}