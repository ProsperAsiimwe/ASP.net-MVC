using SteppingStone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Dashboard
{
    public class DashboardModel
    {
        public IEnumerable<Activity> Activities { get; set; }

        public IEnumerable<Payment> Payments { get; set; }

        public int? TermId { get; set; }

        public IEnumerable<Activity> GetLatestActivity()
        {
            return Activities
                .OrderByDescending(o => o.Recorded)
                .Take(10);
        }

        public IEnumerable<Payment> GetLatestPayments()
        {
            return Payments
                .OrderByDescending(o => o.Recorded)
                .Take(10);
        }

        
    }
}