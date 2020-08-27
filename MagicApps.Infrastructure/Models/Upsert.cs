using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MagicApps.Models
{
    public class UpsertModel
    {
        public string RecordId { get; set; }
        public string ErrorMsg { get; set; }

        public int i_RecordId()
        {
            return Convert.ToInt32(this.RecordId);
        }
    }

    public class AjaxItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public int count { get; set; }

        public string CounterName()
        {
            return string.Format("[{0}] {1}", count, name);
        }
    }
}