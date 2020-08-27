using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Infrastructure;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    [RoutePrefix("Messages")]
    public class MessagesController : BaseController
    {
        // GET: Messages
        [Route("", /*Order = 21,*/ Name = "Messages_Index")]
        public ActionResult Index(SearchMessagesModel search, int page = 1)
        {
            // Return all Messages
            // If not a post-back (i.e. initial load) set the searchModel to session
            if (Request.Form.Count <= 0)
            {
                if (search.IsEmpty() && Session["SearchMessagesModel"] != null)
                {
                    search = (SearchMessagesModel)Session["SearchMessagesModel"];
                }
            }

            var helper = new MessageHelper();

            var model = helper.GetMessageList(search, search.ParsePage(page));

            using(WebClient client = new WebClient())
            {
                var url = "http://boxuganda.com/balance.php?user=phlyp&password=Uganda.01";
                model.MessagesBalance = client.DownloadString(url);
            }

            Session["SearchMessagesModel"] = search;
            
            return View(model);
        }
                
        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MessageViewModel model)
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

            return RedirectToAction("Index", "Students");
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{MessageId:int}")]
        public ActionResult Show(int MessageId)
        {
            if (!IsRoutingOK(MessageId))
            {
                return RedirectOnError();
            }

            var Message = GetMessage(MessageId);

            return View(Message);
        }

        [Authorize(Roles = "Developer, Admin")]
        [Route("{MessageId:int}/Edit")]
        public ActionResult Edit(int MessageId)
        {
            var message = GetMessage(MessageId);

            if (message.Sent.HasValue)
            {
                ShowError("You can't edit this message as it has already been sent.");
                return View("Show", message);
            }

            var model = GetMessageModel(MessageId);
            return View("New", model);
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int MessageId, MessageViewModel model)
        {
            if (!IsRoutingOK(MessageId))
            {
                return RedirectOnError();
            }

            bool success = await Upsert(MessageId, model);

            if (success)
            {
                return RedirectOnSuccess(MessageId);
            }

            // If we got this far, an error occurred
            return View("New", model);
        }

        // GET: Messages
        [Authorize(Roles = "Developer, Admin")]
        [Route("{MessageId:int}/Delete")]
        public ActionResult Delete(int MessageId)
        {
            if (!IsRoutingOK(MessageId))
            {
                return RedirectOnError();
            }

            return View(GetMessage(MessageId));
        }

        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Destroy(int MessageId)
        {
            if (!IsRoutingOK(MessageId))
            {
                return RedirectOnError();
            }

            var helper = GetHelper(MessageId);
            var upsert = await helper.DeleteMessage();

            if (upsert.i_RecordId() > 0)
            {
                ShowSuccess(upsert.ErrorMsg);
                return RedirectOnError();
            }

            // If we got this far, an error occurred
            ShowError(upsert.ErrorMsg);
            return RedirectToAction("Delete", new { MessageId = MessageId });
        }

        [Authorize(Roles = "Admin, Developer")]
        public ActionResult New()
        {            
            if (!IsRoutingOK(null))
            {
                return null;
            }

            var model = GetMessageModel(null);

            return View("New", model);
        }

        private async Task<bool> Upsert(int? MessageId, MessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var helper = (MessageId.HasValue ? GetHelper(MessageId.Value) : new MessageHelper() { ServiceUserId = GetUserId() });
                var upsert = await helper.UpsertMessage(UpsertMode.Admin, model);

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

        private MessageHelper GetHelper(int MessageId)
        {
            MessageHelper helper = new MessageHelper(MessageId);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private MessageHelper GetHelper(Message Message)
        {
            var helper = new MessageHelper(Message);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        //private bool UpdateStudentClass(Message Message)
        //{
        //    var helper = new MessageHelper(Message);

        //    helper.ServiceUserId = GetUserId();

        //    return helper;
        //}

        private MessageViewModel GetMessageModel(int? MessageId)
        {
            MessageViewModel model;


            if (MessageId.HasValue)
            {
                var Message = GetMessage(MessageId.Value);
                model = new MessageViewModel(Message);
            }
            else
            {
                model = new MessageViewModel();
            }

            // pass needed lists
            // ParseDefaults(model);

            return model;
        }
        
        private Message GetMessage(int MessageId)
        {
            return context.Messages.Find(MessageId);
        }

        private RedirectToRouteResult RedirectOnError()
        {
            return RedirectToAction("Index");
        }

        private RedirectToRouteResult RedirectOnSuccess(int MessageId)
        {
            return RedirectToAction("Show", new { MessageId = MessageId });
        }

        public PartialViewResult GetBreadcrumb(int MessageId, bool mainAsLink = true)
        {
            var Message = GetMessage(MessageId);
            ViewBag.MainAsLink = mainAsLink;

            return PartialView("Partials/_Breadcrumb", Message);
        }

        private bool IsRoutingOK(int? MessageId)
        {

            // Check Message
            if (MessageId.HasValue)
            {
                var Message = context.Messages.ToList().SingleOrDefault(x => x.MessageId == MessageId);

                if (Message == null)
                {
                    return false;
                }
            }

            return true;
        }

        
        public PartialViewResult GetActivities(int MessageId)
        {
            var activities = context.Activities.ToList()
                .Where(x => x.MessageId == MessageId)
                .OrderBy(o => o.Recorded);

            return PartialView("Partials/_Activities", activities);
        }

    }
}