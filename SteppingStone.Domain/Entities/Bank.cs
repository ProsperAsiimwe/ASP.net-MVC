using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteppingStone.Domain.Entities
{
    public class Bank
    {
        public Bank()
        {
            Payments = new List<Payment>();
        }
        public int BankId { get; set; }

        [StringLength(20)]
        [Display(Name = "Account Number")]
        public string AccountNo { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        public DateTime? DeActivated { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }

        public double TotalRevenue
        {

            get
            {
                return Payments.Sum(x => x.Amount);
            }
        }

        [NotMapped]
        public IEnumerable<Payment> TermPayments {

            get {
                return Payments.Count() > 0 ? Payments.ToList().Where(x => x.Term.IsCurrentTerm) : new List<Payment>();
            }
        }


    }
}
