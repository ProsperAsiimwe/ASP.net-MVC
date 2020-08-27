using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteppingStone.Domain.Entities
{
    public class Event
    {
        public Event()
        {
            EventStudents = new List<StudentEvent>();
            Added = DateTime.Now;
        }

        [Key]
        public int EventId { get; set; }

        [Display(Name = "Event name")]
        public string Name { get; set; }

        [Display(Name ="Event Date")]
        public DateTime? EventDate { get; set; }

        [StringLength(200)]
        [Display(Name ="Message")]
        public string Description { get; set; }

        [Display(Name = "Notification Date")]
        public DateTime? NotificationDate { get; set; }

        [Display(Name = "Completed")]
        public DateTime? Notified { get; set; }

        public string StudentList { get; set; }

        public virtual ICollection<StudentEvent> EventStudents { get; set; }

        public DateTime Added { get; set; }

        public bool Cancelled { get; set; }

        [Display(Name = "For all Pupils?")]
        public bool IsGeneral { get; set; }

        public string GetStatus()
        {
            var totalDays = EventDate.HasValue ? (EventDate.Value - DateTime.Today).TotalDays : -1;

            if (Cancelled)
            {
                return "Cancelled";
            }
            else if (Notified.HasValue)
            {
                return "Completed";
            }
            else if(totalDays != 0 && totalDays <= 30)
            {
                return "Coming Soon";
            }
            else if(totalDays < 10)
            {
                return "Very Soon";
            }
            else if(totalDays > 30)
            {
                return "Pending";
            }else
            {
                return "Past";
            }
        }

        public string GetStatusCssClass()
        {
            string status = GetStatus();

            switch (status)
            {
                case "Cancelled":
                    return "danger";
                case "Completed":
                    return "success";
                case "Very Soon":
                    return "warning";
                case "Coming Soon":
                    return "info";
                default:
                    return "danger";
            }
        }

        public int NoOfParents()
        {
            return EventStudents.Where(m => !m.Student.NoParents).SelectMany(x => x.Student.Parents).Count();
        }

        [NotMapped]
        public int[] Data
        {
            get
            {
                if (string.IsNullOrEmpty(StudentList))
                {
                    return new int[] { };
                }

                // what if its just one

                if (!StudentList.Contains(","))
                {
                    return new int[] { int.Parse(StudentList) };
                }

                string[] tab = this.StudentList.Split(',');

                return new int[] { int.Parse(tab[0]), int.Parse(tab[1]) };
            }
            set
            {
                var _data = value;

                if(value == null)
                {
                    this.StudentList = String.Empty;
                }else
                {
                    if (value.Count() == 1)
                    {
                        this.StudentList = _data[0].ToString();
                    }
                    else
                    {
                        this.StudentList = String.Join(",", value.Select(p => p.ToString()).ToArray());
                    }
                }
                
                
            }
        }
        

    }
}
