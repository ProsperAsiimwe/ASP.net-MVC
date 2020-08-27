using System.Collections.Generic;
using MagicApps.Models;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.ClassLevels
{
    public class ClassLevelListViewModel
    {
        public IEnumerable<ClassLevel> Classes { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public SearchClassesModel SearchModel { get; set; }

        public string[] Roles { get; set; }
    }
}