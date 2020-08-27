using SteppingStone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Events
{
    public class EventDto
    {
        public EventDto() { }

        public EventDto(Event entity)
        {
            EventId = entity.EventId;
            EventDate = entity.EventDate ?? entity.EventDate;
            Description = entity.Description;
            NotificationDate = entity.NotificationDate ?? entity.NotificationDate;
            Name = entity.Name;
        }


        public int EventId { get; set; }
                
        public string Name { get; set; }
        
        public DateTime? EventDate { get; set; }
        
        public string Description { get; set; }
        
        public DateTime? NotificationDate { get; set; }
        

    }
}