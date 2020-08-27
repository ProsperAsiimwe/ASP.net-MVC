using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.Events;
using MagicApps.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TwitterBootstrap3;

namespace SteppingStone.WebUI.Infrastructure.Helpers
{
    public class EventHelper
    {
        private ApplicationDbContext db;
        private ApplicationUserManager UserManager;

        int EventId;

        public Event Event { get; private set; }

        public string ServiceUserId { get; set; }

        public EventHelper()
        {
            Set();
        }

        public EventHelper(int EventId)
        {
            Set();

            this.EventId = EventId;
            this.Event = db.Events.Find(EventId);
        }

        public EventHelper(Event Event)
        {
            Set();

            this.EventId = Event.EventId;
            this.Event = Event;
        }

        private void Set()
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public EventListViewModel GetEventList(SearchEventsModel searchModel, int page = 1)
        {
            int pageSize = 20;

            if (page < 1)
            {
                page = 1;
            }

            IEnumerable<Event> records = db.Events.ToList();

            // Remove any default information
            //searchModel.ParseRouteInfo();

            if (!String.IsNullOrEmpty(searchModel.Name))
            {
                string name = searchModel.Name.ToLower();
                records = records.Where(x => x.Name.ToLower().Contains(name));
            }

            if (searchModel.StartDate.HasValue)
            {
                records = records.Where(x => x.EventDate >= searchModel.StartDate.Value);
            }

            if (searchModel.EndDate.HasValue)
            {
                records = records.Where(x => x.EventDate <= searchModel.EndDate.Value);
            }

            if (!String.IsNullOrEmpty(searchModel.Description))
            {
                records = records.Where(x => x.Description.ToLower().Contains(searchModel.Description.ToLower()));
            }

            return new EventListViewModel
            {
                Events = records
                    .OrderByDescending(o => o.EventDate)
                    .ThenBy(o => o.EventId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize),
                SearchModel = searchModel,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = records.Count()
                }
            };
        }

        public async Task<UpsertModel> UpsertEvent(UpsertMode mode, EventViewModel model)
        {
            var upsert = new UpsertModel();

            try
            {
                Activity activity;
                string title;
                System.Text.StringBuilder builder;

                // Apply changes
                Event = model.ParseAsEntity(Event);


                builder = new System.Text.StringBuilder();

                if (model.EventId == 0)
                {
                    db.Events.Add(Event);

                    title = "Event Recorded";
                    builder.Append("An Event record has been made").AppendLine();
                }
                else
                {
                    db.Entry(Event).State = System.Data.Entity.EntityState.Modified;

                    title = "Event Updated";
                    builder.Append("The following changes have been made to the Event details");

                    if (mode == UpsertMode.Admin)
                    {
                        builder.Append(" (by the Admin)");
                    }

                    builder.Append(":").AppendLine();
                }

                await db.SaveChangesAsync();

                EventId = Event.EventId;

                //update students here

                if (model.IsGeneral && Event.EventStudents.ToList().Count() <= 0)
                {
                    var currentTerm = db.Terms.ToList().FirstOrDefault(m => m.IsCurrentTerm);

                    // take all students to be those of only the current term
                    Event.EventStudents = db.Students.ToList().Where(m => !m.NoParents && m.CurrentTermId == currentTerm.TermId).ToList().Select(p => new StudentEvent(p.StudentId, Event.EventId)).ToList();
                    db.Entry(Event).State = System.Data.Entity.EntityState.Modified;
                }

                //if (model.IsGeneral && Event.EventStudents.Count() <= 0)
                //{
                //    var currentTerm = db.Terms.ToList().FirstOrDefault(m => m.IsCurrentTerm);

                //    // take all students to be those of only the current term
                //    var students = db.Students.ToList().Where(m => !m.NoParents && m.CurrentTermId == currentTerm.TermId).Select(p => new StudentEvent(p.StudentId, Event.EventId)).ToList();
                //    foreach (var student in students)
                //    {
                //        db.StudentEvents.Add(student);
                //    }

                //    //db.Entry(Event).State = System.Data.Entity.EntityState.Modified;
                //}

                // Save activity now so we have a EventId. Not ideal, but hey
                activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                await db.SaveChangesAsync();

                if (model.EventId == 0)
                {
                    upsert.ErrorMsg = "Event record created successfully";
                }
                else
                {
                    upsert.ErrorMsg = "Event record updated successfully";
                }

                upsert.RecordId = Event.EventId.ToString();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Update Event Error", ex);
            }

            return upsert;
        }

        public async Task<UpsertModel> DeleteEvent()
        {
            var upsert = new UpsertModel();

            try
            {
                string title = "Event Deleted";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Event has been deleted:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Event: {0}", Event.Name);

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                // Remove Event
                db.Events.Remove(Event);
                db.Entry(Event).State = System.Data.Entity.EntityState.Deleted;


                upsert.ErrorMsg = string.Format("Event: '{0}' deleted successfully", Event.Name);
                upsert.RecordId = Event.EventId.ToString();

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
                //RecordException("Delete Event Error", ex);
            }

            return upsert;
        }

        public async Task<UpsertModel> Exclude(int StudentEventId)
        {
            var upsert = new UpsertModel();

            var studentEvent = db.StudentEvents.ToList().FirstOrDefault(m => m.StudentEventId == StudentEventId);

            try
            {
                string title = "Student excluded";
                System.Text.StringBuilder builder = new System.Text.StringBuilder()
                    .Append("The following Student has been excluded from Event:")
                    .AppendLine()
                    .AppendLine().AppendFormat("Student: {0}", studentEvent.Student.FullName)
                    .AppendLine().AppendFormat("Event: {0}", studentEvent.Event.Name);

                // Record activity
                var activity = CreateActivity(title, builder.ToString());
                activity.UserId = ServiceUserId;

                // Remove Student Event
                var successMsg = string.Format("Student: '{0}' excluded from {1} successfully", studentEvent.Student.FullName, studentEvent.Event.Name);

                db.StudentEvents.Remove(studentEvent);
                db.Entry(studentEvent).State = System.Data.Entity.EntityState.Deleted;


                upsert.ErrorMsg = successMsg;
                upsert.RecordId = Event.EventId.ToString();

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                upsert.ErrorMsg = ex.Message;
            }

            return upsert;
        }

        public Activity CreateActivity(string title, string description)
        {
            var activity = new Activity
            {
                Title = title,
                Description = description,
                RecordedById = ServiceUserId
            };

            if (Event != null)
            {
                activity.EventId = EventId;
            }

            db.Activities.Add(activity);
            return activity;
        }

        public static ButtonStyle GetButtonStyle(string css)
        {
            ButtonStyle button_css;

            if (css == "warning")
            {
                button_css = ButtonStyle.Warning;
            }
            else if (css == "success")
            {
                button_css = ButtonStyle.Success;
            }
            else if (css == "info")
            {
                button_css = ButtonStyle.Info;
            }
            else
            {
                button_css = ButtonStyle.Danger;
            }

            return button_css;
        }

        private void RecordException(string title, Exception ex)
        {
            var activity = CreateActivity(title, ex.Message);

            if (Event != null)
            {
                activity.UserId = ServiceUserId;
                activity.EventId = Event.EventId;
            }
            db.SaveChanges();
        }
    }
}