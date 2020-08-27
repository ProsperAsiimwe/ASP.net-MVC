using System.Collections.Generic;
using MagicApps.Models;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Parents
{
    public class ParentListViewModel
    {
        public IEnumerable<Parent> Parents { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public SearchParentsModel SearchModel { get; set; }

        public string[] Roles { get; set; }
    }
}