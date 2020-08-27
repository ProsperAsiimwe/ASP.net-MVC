using SteppingStone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SteppingStone.WebUI.Models.Banks
{
    public class BankViewModel
    {
        public BankViewModel()
        {

        }

        public BankViewModel(Bank entity)
        {
            BankId = entity.BankId;
            AccountNo = entity.AccountNo;
            Name = entity.Name;
        }

        public int BankId { get; set; }

        [StringLength(20)]
        [Display(Name = "Account Number")]
        public string AccountNo { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        public Bank ParseAsEntity(Bank entity)
        {
            if(entity == null)
            {
                entity = new Bank();
            }

            entity.AccountNo = AccountNo;
            entity.Name = Name;

            return entity;
        }
    }
}