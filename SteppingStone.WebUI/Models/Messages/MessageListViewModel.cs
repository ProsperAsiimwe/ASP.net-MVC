using System.Collections.Generic;
using MagicApps.Models;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Messages
{
    public class MessageListViewModel
    {
        public IEnumerable<Message> Messages { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public SearchMessagesModel SearchModel { get; set; }

        public string MessagesBalance { get; set; }

        public string[] Roles { get; set; }
    }
}