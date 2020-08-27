using SteppingStone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Terms
{
    public class TermViewModel
    {
        public TermViewModel()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(3);
        }

        public TermViewModel(Term entity)
        {
            TermId = entity.TermId;
            Position = entity.Position;
            StartDate = entity.StartDate;
            EndDate = entity.EndDate;
            IsCurrentTerm = entity.IsCurrentTerm;
        }

        public int TermId { get; set; }

        [Required]
        [Display(Name = "Term Position")]
        public int Position { get; set; }

        [Required]
        [UIHint("_DatePicker")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [UIHint("_DatePicker")]
        [Display(Name ="End Date")]
        public DateTime EndDate { get; set; }

        [UIHint("_Checkbox")]
        [Display(Name = "Current Term?")]
        public bool IsCurrentTerm { get; set; }

        public IDictionary<int, string> Positions()
        {
            return new Dictionary<int, string> {
                {0, "First" },
                {1, "Second" },
                {2, "Third" },
                {3, "Holiday" }
            };
        }

        public Term ParseAsEntity(Term entity)
        {
            if(entity == null)
            {
                entity = new Term();
            }

            //entity.Year = StartDate.Year;            
            entity.Position = Position;
            entity.StartDate = StartDate;
            entity.EndDate = EndDate;
            entity.IsCurrentTerm = IsCurrentTerm;
            entity.Updated = DateTime.Now;

            return entity;
        }

    }
}