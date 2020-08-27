using SteppingStone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Events
{
    public class EventViewModel
    {
        public EventViewModel() {
            Data = new int[] { };
        }

        public EventViewModel(Event entity) {

            EventId = entity.EventId;
            EventDate = entity.EventDate;
            Description = entity.Description;
            NotificationDate = entity.NotificationDate;
            Cancel = entity.Cancelled;
            Name = entity.Name;
            Data = entity.Data;
        }


        public int EventId { get; set; }


        [Display(Name = "Event name")]
        public string Name { get; set; }

        [Display(Name = "Event Date")]
        [UIHint("_DatePicker")]
        public DateTime? EventDate { get; set; }

        [StringLength(160)]
        [Display(Name = "Message")]
        public string Description { get; set; }

        [Display(Name = "Notification Date")]
        [UIHint("_DatePicker")]
        public DateTime? NotificationDate { get; set; }
        
        [UIHint("_Checkbox")]
        public bool Cancel { get; set; }

        [UIHint("_Checkbox")]
        [Display(Name = "For All Pupils")]
        public bool IsGeneral { get; set; }

        public ICollection<Student> Students { get; set; }

        public int[] Data { get; set; }

        public Event ParseAsEntity(Event entity)
        {
            if(entity == null)
            {
                entity = new Event();
            }

            entity.EventDate = EventDate;
            entity.Description = Description;
            entity.NotificationDate = NotificationDate;
            entity.IsGeneral = IsGeneral;
            entity.Cancelled = Cancel;
            entity.Name = Name;
            entity.Data = Data;            

            return entity;

        }

        public bool IsEmpty() {
            return (NotificationDate == null) || (EventDate == null) || string.IsNullOrEmpty(Description) || string.IsNullOrEmpty(Name);
        }
    }
}