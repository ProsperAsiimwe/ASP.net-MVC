using System.Collections.Generic;
using MagicApps.Models;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Payments
{
    public class PaymentListViewModel
    {
        public IEnumerable<Payment> Payments { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public SearchPaymentsModel SearchModel { get; set; }

        public double TotalPaid { get; set; }

        public double TotalBalance { get; set; }

        public string[] Roles { get; set; }
    }
}