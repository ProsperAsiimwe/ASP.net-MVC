using SteppingStone.Domain.Entities;
using MagicApps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Students
{
    public class StudentListViewModel
    {
        public IEnumerable<Student> Students { get; set; }

        public IEnumerable<Student> Records { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public SearchStudentsModel SearchModel { get; set; }

        public double TotalPaid { get; set; }

        public double TotalBalance { get; set; }

        public double TermTotalPaid { get; set; }

        public double TermTotalBalance { get; set; }

        public string[] Roles { get; set; }
    }
}