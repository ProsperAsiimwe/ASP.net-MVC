using System.Collections.Generic;
using MagicApps.Models;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Models.Events
{
    public class EventListViewModel
    {
        public IEnumerable<Event> Events { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public SearchEventsModel SearchModel { get; set; }

        public string[] Roles { get; set; }
    }
}