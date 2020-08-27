using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MagicApps.Models
{
    public class PagingInfo
    {
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public string Term { get; set; }
        public bool ShowCounters { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }

        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / PageSize); }
        }

        public int ItemsFrom
        {
            // (CurrentPage - 1) because MVC paging is 1-Indexed, not 0-Indexed
            get {
                if (TotalItems <= 0) {
                    return 0;
                }

                return (((CurrentPage - 1) * PageSize) + 1);
            }
        }

        public int ItemsTo
        {
            get {
                int total = (((ItemsFrom + PageSize) - 1) > TotalItems) ? TotalItems : ((ItemsFrom + PageSize) - 1);

                return total; 
            }
        }
    }
}