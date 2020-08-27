using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glorious.Entities
{
    public class StudentParent
    {
        public StudentParent()
        {
            
        }

        public StudentParent(int StudentId, int ParentId)
        {
            this.StudentId = StudentId;
            this.ParentId = ParentId;
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Parent")]
        public int ParentId { get; set; }

        public virtual Parent Parent { get; set; }

        public virtual Student Student { get; set; }
    }
}
