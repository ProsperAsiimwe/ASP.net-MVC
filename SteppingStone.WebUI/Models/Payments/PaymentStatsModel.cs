using SteppingStone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Payments
{
    public class PaymentStatsModel
    {

        public double TotalCollections { get; set; }

        public int TotalParents { get; set; }

        public Term Term { get; set; }
        
        public IEnumerable<Payment> GetLatest()
        {
            return Term.Payments
                .OrderByDescending(o => o.Recorded)
                .Take(5);
        }

        public int Total()
        {
            return Term.Payments.Count();
        }

        public double TotalPayments()
        {
            return Term.Payments.Sum(x => x.Amount);
        }

        public int TotalStudents()
        {
            return Term.Students.Count();
        }
        
        public double TotalBalance()
        {
            return Term.TotalBalance;
        }

        public int FullPayments()
        {
            return Term.Students.Where(x => x.Outstanding <= 0).Count();
        }

        public double IsPortion()
        {
            return Term.Students.Where(x => x.Outstanding > 0).Count();
        }

        public double HasExtra()
        {
            return Term.Payments.Where(x => x.HasExtra()).Count();
        }

        public double Completion()
        {
            double total = Total();

            if (total <= 0)
            {
                return 0;
            }

            double fullPay = FullPayments();

            return (fullPay / total) * 100.0;
        }
    }
}