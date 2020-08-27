using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Models
{
    public class FilterDropdownModel
    {
        public string FieldName { get; set; }

        public SelectList ItemList { get; set; }

        public string SelectedValue { get; set; }

        public string Label { get; set; }
    }
}