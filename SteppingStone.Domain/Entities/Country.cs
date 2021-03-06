﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SteppingStone.Domain.Entities
{
    [Table("Countries")]
    public class Country
    {
        [Key]
        public int CountryId { get; set; }

        [Required]
        [StringLength(80)]
        public string CountryName { get; set; }
    }
}