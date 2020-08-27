using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteppingStone.Domain.Entities
{
    public class StudentEvent
    {
        public StudentEvent()
        {

        }

        public StudentEvent(int StudentId, int EventId)
        {
            this.EventId = EventId;
            this.StudentId = StudentId;
        }

        [Key]
        public int StudentEventId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Event")]
        public int EventId { get; set; }

        public virtual Student Student { get; set; }

        public virtual Event Event { get; set; }
    }
}
