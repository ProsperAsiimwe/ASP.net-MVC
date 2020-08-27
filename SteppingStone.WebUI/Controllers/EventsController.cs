using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Infrastructure;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    [RoutePrefix("Events")]
    public class EventsController : BaseController
    {
        // GET: Events
        [Authorize(Roles = "Developer, Admin")]
        //[Route("{id:int}", Order = 1)]
        [Route("{status:regex(^(Coming Soon|Very Soon|Completed|Cancelled)$)}/Page-{page:int}", Order = 11)]
        [Route("{status:regex(^(Coming Soon|Very Soon|Completed|Cancelled)$)}", Order = 12)]
        [Route("Page-{page:int}", Order = 13)]
        [Route("", /*Order = 21,*/ Name = "Events_Index")]
        public ActionResult Index(SearchEventsModel search, int page = 1)
        {
            // Return all Events
            // If not a post-back (i.e. initial load) set the searchModel to session
            if (Request.Form.Count <= 0)
            {
                if (search.IsEmpty() && Session["SearchEventsModel"] != null)
                {
                    search = (SearchEventsModel)Session["SearchEventsModel"];
                }
            }

            var helper = new EventHelper();
            var model = helper.GetEventList(search, search.ParsePage(page));

            Session["SearchEventsModel"] = search;

            //(search);

            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        public ActionResult New()
        {
            if (!IsRoutingOK(null))
            {
                return RedirectOnError();
            }

            var model = GetEventModel(null);

            return View(model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EventViewModel model)
        {

            if (!IsRoutingOK(null))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(null, model);

            if (success)
            {
                return RedirectOnError();
            }

            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{EventId:int}")]
        public ActionResult Show(int EventId)
        {
            if (!IsRoutingOK(EventId))
            {
                return RedirectOnError();
            }

            var Event = GetEvent(EventId);            

            return View(Event);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{EventId:int}/Edit")]
        public ActionResult Edit(int EventId)
        {
            var model = GetEventModel(EventId);
            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int EventId, EventViewModel model)
        {
            if (!IsRoutingOK(EventId))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(EventId, model);

            if (success)
            {
                return RedirectOnSuccess(EventId);
            }

            // If we got this far, an error occurred
            return View("New", model);
        }

        // GET: Events
        [Authorize(Roles = "Developer, Admin")]
        [Route("{EventId:int}/Delete")]
        public ActionResult Delete(int EventId)
        {
            if (!IsRoutingOK(EventId))
            {
                return RedirectOnError();
            }

            return View(GetEvent(EventId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Destroy(int EventId)
        {
            if (!IsRoutingOK(EventId))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(EventId);
            var upsert = await helper.DeleteEvent();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Delete", new { EventId = EventId });
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{StudentEventId:int}/Exclude")]
        public ActionResult Exclude(int StudentEventId)
        {
            var student = context.StudentEvents.Find(StudentEventId);

            if (student == null)
            {
                return RedirectOnError();
            }

            return View(student);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Remove(int StudentEventId)
        {
            var student = context.StudentEvents.Find(StudentEventId);
            var Event = student.Event;

            if (student == null)
            {
                return RedirectOnError();
            }

            var helper = GetHelper(Event);
            var upsert = await helper.Exclude(StudentEventId);

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Exclude", new { StudentEventId = StudentEventId });
        }
        public ActionResult Calendar()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetEvents()
        {
            var model = new List<EventDto> {
                new EventDto() {
                    Description = "Take her to Chinese",
                    EventDate = DateTime.Today.AddDays(2),
                    Name = "Dianas Birthday"                    
                },
                new EventDto() {
                    Description = "Take for drinks",
                    EventDate = DateTime.Today.AddDays(2),
                    Name = "Our anniversary"
                }
            };
            //var eventModels = context.Events.ToList().Select(m => new EventDto(m)).ToList();
          
            var events = Json(model);

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private async Task<bool> Upsert(int? EventId, EventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var helper = (EventId.HasValue ? GetHelper(EventId.Value) : new EventHelper() { ServiceUserId = GetUserId() });
                var upsert = await helper.UpsertEvent(UpsertMode.Admin, model);

                if (upsert.i_RecordId() > 0)
                {
                    ShowSuccess(upsert.ErrorMsg);

                    return true;
                }
                else
                {
                    ShowError(upsert.ErrorMsg);
                }
            }

            //ParseDefaults(model);
            return false;
        }

        private EventHelper GetHelper(int EventId)
        {
            EventHelper helper = new EventHelper(EventId);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private EventHelper GetHelper(Event Event)
        {
            var helper = new EventHelper(Event);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        //private bool UpdateStudentClass(Event Event)
        //{
        //    var helper = new EventHelper(Event);

        //    helper.ServiceUserId = GetUserId();

        //    return helper;
        //}

        private EventViewModel GetEventModel(int? EventId)
        {
            EventViewModel model;


            if (EventId.HasValue)
            {
                var Event = GetEvent(EventId.Value);
                model = new EventViewModel(Event);
            }
            else
            {
                model = new EventViewModel();
            }

            // pass needed lists
            //ParseDefaults(model);

            return model;
        }

        //private void ParseDefaults(EventViewModel model)
        //{

        //}

        //private void ParseSearchDefaults(SearchEventsModel model)
        //{
        //    model.Events = context.Events.OrderByDescending(m => m.StartDate).ToList();
        //    model.Events = context.Events.ToList();
        //    model.Banks = context.Banks.Where(x => !x.DeActivated.HasValue).ToList();
        //}

        private Event GetEvent(int EventId)
        {
            return context.Events.Find(EventId);
        }

        private RedirectToRouteResult RedirectOnError()
        {
            return RedirectToAction("Index");
        }

        private RedirectToRouteResult RedirectOnSuccess(int EventId)
        {
            return RedirectToAction("Show", new { EventId = EventId });
        }

        public PartialViewResult GetBreadcrumb(int EventId, bool mainAsLink = true)
        {
            var Event = GetEvent(EventId);
            ViewBag.MainAsLink = mainAsLink;

            return PartialView("Partials/_Breadcrumb", Event);
        }

        private bool IsRoutingOK(int? EventId)
        {

            // Check Event
            if (EventId.HasValue)
            {
                var Event = context.Events.ToList().SingleOrDefault(x => x.EventId == EventId);

                if (Event == null)
                {
                    return false;
                }
            }

            return true;
        }


        public PartialViewResult GetActivities(int EventId)
        {
            var activities = context.Activities.ToList()
                .Where(x => x.EventId == EventId)
                .OrderBy(o => o.Recorded);

            return PartialView("Partials/_Activities", activities);
        }

        //public PartialViewResult GetEventstats()
        //{
        //    var currentEvent = context.Events.Where(x => x.IsCurrentEvent).First();

        //    var model = new EventstatsModel
        //    {
        //        Event = currentEvent
        //    };

        //    return PartialView("Dashboard/_Eventstats", model);
        //}
    }
}