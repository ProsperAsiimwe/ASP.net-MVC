using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models
{
    public class ListModel
    {
        public int ParsePage(int page)
        {
            if (page >= 1) {
                return page;
            }
            else {
                return 1;
            }
        }
    }
}