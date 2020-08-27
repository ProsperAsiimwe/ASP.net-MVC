using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Glorious.Entities;

namespace Glorious.Domain.Models
{
    public class Address
    {
        [StringLength(50)]
        [Display(Name = "Address")]
        public string Line1 { get; set; }

        [StringLength(50)]
        [Display(Name = "Address (continued)")]
        public string Line2 { get; set; }

        [StringLength(50)]
        [Display(Name = "Address (continued)")]
        public string Line3 { get; set; }

        [StringLength(50)]
        [Display(Name = "Town/City")]
        public string Line4 { get; set; }

        [StringLength(50)]
        [Display(Name = "County/State/District")]
        public string Line5 { get; set; }

        [StringLength(12)]
        [Display(Name = "Post code")]
        public string PostCode { get; set; }
        
        public string FormatAddress(string concatonator)
        {
            var addr = new List<string>();

            if (!string.IsNullOrEmpty(Line1)) {
                addr.Add(Line1);
            }
            if (!string.IsNullOrEmpty(Line2)) {
                addr.Add(Line2);
            }
            if (!string.IsNullOrEmpty(Line3)) {
                addr.Add(Line3);
            }
            if (!string.IsNullOrEmpty(Line4)) {
                addr.Add(Line4);
            }
            if (!string.IsNullOrEmpty(Line5)) {
                addr.Add(Line5);
            }
            if (!string.IsNullOrEmpty(PostCode)) {
                addr.Add(PostCode);
            }
            //if (CountryId.HasValue && Country != null) {
            //    addr.Add(Country.CountryName);
            //}

            return string.Join(concatonator, addr.ToArray());
        }
    }
}