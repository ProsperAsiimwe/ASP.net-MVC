using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Models
{
    public class CheckboxListModel
    {
        public string FieldName { get; set; }

        public SelectList ItemList { get; set; }

        public string[] SelectedValues { get; set; }

        public string Label { get; set; }
    }
}