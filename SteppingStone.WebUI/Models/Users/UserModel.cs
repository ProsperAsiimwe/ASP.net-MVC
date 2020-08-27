using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Users
{
    public class UserModel
    {
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }
    }
}