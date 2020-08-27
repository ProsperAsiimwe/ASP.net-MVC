using SteppingStone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Messages
{
    public class MessageViewModel
    {
        public MessageViewModel() { }
        public MessageViewModel(Message entity) {
            MessageId = entity.MessageId;
            MessageDescription = entity.MessageDescription;
            Scheduled = entity.Scheduled ?? null;
            SendNow = entity.SendNow;
        }

        public int MessageId { get; set; }

        [StringLength(160)]
        [Display(Name ="Message")]
        public string MessageDescription { get; set; }
        
        [UIHint("_DatePicker")]
        public DateTime? Scheduled { get; set; }

        [UIHint("_Checkbox")]
        [Display(Name = "Send Now?")]
        public bool SendNow { get; set; }

        public Message ParseAsEntity(Message entity)
        {
            if(entity == null)
            {
                entity = new Message();
            }
            
            entity.MessageDescription = MessageDescription;
            entity.Scheduled = Scheduled ?? null;
            entity.SendNow = SendNow;

            return entity;
        }

    }
}