using System.Collections.Generic;
using MagicApps.Models;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Terms
{
    public class TermListViewModel
    {
        public IEnumerable<Term> Terms { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public SearchTermsModel SearchModel { get; set; }

        public string[] Roles { get; set; }
    }
}