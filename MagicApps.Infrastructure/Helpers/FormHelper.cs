using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MagicApps.Infrastructure.Helpers
{
    public static class FormHelper
    {
        public static IDictionary<bool, string> YesNo() {
            return new Dictionary<bool, string> {
                { true, "Yes" },
                { false, "No" }
            };
        }

        public static IDictionary<byte, string> YesNoNA()
        {
            return new Dictionary<byte, string> {
                { 1, "Yes" },
                { 2, "No" },
                { 99, "N/A" }
            };
        }
    }
}