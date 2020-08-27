using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glorious.Entities
{
    public class Activity
    {
        public Activity()
        {
            Recorded = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActivityId { get; set; }

        [ForeignKey("User")]
        [StringLength(128)]
        public string UserId { get; set; }

        public int? ReferenceId { get; set; }

        public int? ParentId { get; set; }

        public int? StudentId { get; set; }

        public int? MessageId { get; set; }

        public int? EventId { get; set; }

        public int? BankId { get; set; }

        public int? ClassLevelId { get; set; }

        public int? PaymentId { get; set; }

        public int? TermId { get; set; }

        [StringLength(128)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Recorded { get; set; }

        [ForeignKey("RecordedBy")]
        [StringLength(128)]
        public string RecordedById { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ApplicationUser RecordedBy { get; set; }
    }
}
