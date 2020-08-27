using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MagicApps.Infrastructure.Helpers;

namespace SteppingStone.WebUI.Infrastructure.Helpers
{
    public class CookieHelper
    {
        public void SetCookies()
        {
            Authorised = true;
        }

        public bool Authorised {
            get {
                bool _id = bool.Parse(CustomHelper.GetCookieValue("SteppingStone-Authorised", Boolean.FalseString));
                return _id;
            }
            set {
                CustomHelper.CreateCookie("SteppingStone-Authorised", value.ToString());
            }
        }
        

        public void Flush()
        {
            CustomHelper.CreateCookie("SteppingStone-Authorised", Boolean.FalseString, -1);
        }
    }
}