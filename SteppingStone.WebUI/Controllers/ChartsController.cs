using MagicApps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SteppingStone.WebUI.Models.Dashboard;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    public class ChartsController : BaseController
    {
        public PartialViewResult ActivityAsLine(ActivitiesModel model)
        {
            if (!model.StartDate.HasValue || !model.EndDate.HasValue)
            {
                model.EndDate = DateTime.Today;
                model.StartDate = model.EndDate.Value.AddMonths(-1).AddSeconds(1);
            }

            if (!ModelState.IsValid)
            {
                model.ErrorMessage = string.Join(", ", GetModelStateErrors().ToArray());
            }

            DateTime startDate = model.StartDate.Value;
            DateTime endDate = model.EndDate.Value.AddDays(1).AddSeconds(-1);

            var activity = context.Activities.ToList()
                .Where(x => startDate <= x.Recorded && endDate >= x.Recorded)
                .ToList();

            if (model.AdminId.HasValue && model.AdminId.Value > 0)
            {
                activity = activity.Where(x => x.User.DisplayId == model.AdminId.Value).ToList();
            }

            model.Total = activity.Count();

            var activities = activity
                .GroupBy(x => new { date = x.Recorded.ToString("yyyy-MM-dd") })
                .Select(x => new AjaxItem { name = x.Key.date, count = x.Count() })
                .ToList();

            // List of dates to include dates where nothing happened in the 10 day window
            do
            {
                string temp = startDate.ToString("yyyy-MM-dd");

                if (!activities.Select(x => x.name).Contains(temp))
                {
                    activities.Add(new AjaxItem { name = temp, count = 0 });
                }

                startDate = startDate.AddDays(1);

            } while (startDate <= endDate);

            model.Readings = activities
                .OrderBy(o => o.name);

            return PartialView("Partials/_Line", model);
        }
    }
}