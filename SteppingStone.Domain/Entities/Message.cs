using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteppingStone.Domain.Entities
{
    public class Message
    {
        public Message() { Added = DateTime.Now; }

        [Key]
        public int MessageId { get; set; }

        [StringLength(160)]
        [Display(Name = "Message")]
        public string MessageDescription { get; set; }

        public DateTime? Scheduled { get; set; }

        [Display(Name ="Send Now?")]
        public bool SendNow { get; set; }

        public DateTime? Sent { get; set; }

        public DateTime? Failed { get; set; }

        public DateTime Added { get; set; }

        public string GetStatus()
        {
            if (Sent.HasValue)
            {
                return "Sent";
            }
            else if (!Sent.HasValue)
            {
                return "Pending";
            }
            else if (Failed.HasValue)
            {
                return "Failed";
            }else
            {
                return "";
            }
        }

        public string GetStatusCssClass()
        {
            string status = GetStatus();

            if(status == "Sent")
            {
                return "success";
            }
            else if(status == "Pending")
            {
                return "warning";
            }
            else if (status == "Failed")
            {
                return "danger";
            }
            else
            {
                return "primary";
            }
        }
    }
}
