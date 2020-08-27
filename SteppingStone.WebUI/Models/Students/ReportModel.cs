using SteppingStone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gloroius.WebUI.Models.Students
{
    public class ReportModel
    {
        public ReportModel()
        {
            Students = new List<Student>();
        }

        public List<Student> Students { get; set; }
    }
}