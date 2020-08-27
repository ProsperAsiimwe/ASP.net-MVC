using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Infrastructure
{
    public static class Settings
    {
        public static string COMPANY_ABBR = ConfigurationManager.AppSettings["Settings.Company.Abbr"];

        public static string COMPANY_NAME = ConfigurationManager.AppSettings["Settings.Company.Name"];

        public static string COMPANY_EMAIL = ConfigurationManager.AppSettings["Settings.Company.Email"];

        public static string COMPANY_PHONE = ConfigurationManager.AppSettings["Settings.Company.Telephone"];

        public static string COMPANY_URL = ConfigurationManager.AppSettings["Settings.Company.Url"];

        public static string[] COMPANY_ADDRESS = ConfigurationManager.AppSettings["Settings.Company.Address"].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

        public static string ENVIRONMENT = ConfigurationManager.AppSettings["Settings.Environment"];

        public static string DOCFOLDER = ConfigurationManager.AppSettings["Settings.Site.DocFolder"];

        public static int REFERENCE_REMINDER_DAYS = 4;
    }
}