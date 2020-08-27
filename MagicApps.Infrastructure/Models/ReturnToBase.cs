using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MagicApps.Models
{
    public static class ReturnToBase
    {
        public static string BaseUrl
        {
            get
            {
                string url = HttpContext.Current.Session["RTB_BASE_URL"] as string;

                if (url != null) {
                    return url.ToString();
                }
                else {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session["RTB_BASE_URL"] = value;
            }
        }

        public static string ItemSingle
        {
            get
            {
                string item = HttpContext.Current.Session["RTB_ITEM"] as string;

                if (item != null) {
                    return item.ToString();
                }
                else {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session["RTB_ITEM"] = value;
            }
        }

        public static string ItemMany
        {
            get
            {
                string items = HttpContext.Current.Session["RTB_ITEMS"] as string;

                if (items != null) {
                    return items.ToString();
                }
                else {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session["RTB_ITEMS"] = value;
            }
        }

        public static string PageTitle
        {
            get
            {
                string title = HttpContext.Current.Session["RTB_TITLE"] as string;

                if (title != null) {
                    return title.ToString();
                }
                else {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session["RTB_TITLE"] = value;
            }
        }

        public static IDictionary<string, string> Params
        {
            get
            {
                object items = HttpContext.Current.Session["RTB_PARAMS"];

                if (items != null) {
                    return (Dictionary<string, string>)HttpContext.Current.Session["RTB_PARAMS"];
                }
                else {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session["RTB_PARAMS"] = value;
            }
        }

        public static string GetParamValue(string key)
        {
            if (Params == null || !Params.ContainsKey(key)) {
                return null;
            }

            return Params[key];
        }

        public static void InitParams()
        {
            Params = new Dictionary<string, string>();
        }

        public static bool HasBaseUrl()
        {
            return !string.IsNullOrEmpty(BaseUrl);
        }

        public static void Rest()
        {
            BaseUrl = null;
            ItemSingle = null;
            ItemMany = null;
            PageTitle = null;
            Params = null;
        }
    }
}