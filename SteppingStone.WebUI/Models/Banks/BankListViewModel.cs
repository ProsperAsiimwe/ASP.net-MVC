using System.Collections.Generic;
using MagicApps.Models;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Banks
{
    public class BankListViewModel
    {
        public IEnumerable<Bank> Banks { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public SearchBanksModel SearchModel { get; set; }

        public string[] Roles { get; set; }
    }
}