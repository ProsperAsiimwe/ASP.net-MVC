using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.Account;
using SteppingStone.WebUI.Models.Users;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    [RoutePrefix("Admin/{roleName:regex(^(admin|candidate)s$)?}")]
    public class UsersController : BaseController
    {
        public UsersController()
        {
            ViewBag.Area = "Admin";
            
        }

        [AllowAnonymous]
        public ActionResult Init()
        {
            var helper = new UserHelper();

            helper.CreateRole("Developer");
            helper.CreateRole("Admin");

            var model = new ProfileViewModel {
                TitleId = "Mr",
                FirstName = "Prosper",
                LastName = "Asiimwe",
                Email = "prosper@softwire.co.ug",
                Activate = true
            };

            helper.QuickCreateUser(model, "uganda001", "Developer");

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Users/
        [Authorize(Roles = "Admin, Developer")]
        [Route("page-{page:int:min(1):max(999)}", Order = 1)]
        [Route("", Order = 2)]
        public ViewResult Index(SearchUsersModel search, int page = 1)
        {
            // If not a post-back (i.e. initial load) set the searchModel to session
            if (Request.Form.Count <= 0) {
                if (search.IsEmpty() && Session["SearchUsersModel"] != null) {
                    search = (SearchUsersModel)Session["SearchUsersModel"];
                }
            }

            var helper = new UserHelper();
            var model = helper.GetUserList(search, page);

            if (!search.IsEmpty()) {
                Session["SearchUsersModel"] = search;
            }
            else {
                Session["SearchUsersModel"] = null;
            }

            return View(model);
        }

        //
        // GET: /Users/
        [Authorize(Roles = "Admin, Developer")]
        [Route("New")]
        public ViewResult New()
        {
            var model = new ProfileViewModel();
            model.SetLists(GetUserId());
            return View(model);
        }

        // POST: /Users/{Id}/Profile
        [Authorize(Roles = "Admin, Developer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<ActionResult> Create(string roleName, ProfileViewModel model)
        {
            if (ModelState.IsValid) {
                var user = model.ParseAsEntity(new ApplicationUser());
                // Create a user without a password
                var result = await UserManager.CreateAsync(user);

                if (result.Succeeded) {
                    string userId = GetUserId();
                    var helper = GetHelper(user);

                    // Create an activity
                    var activity = helper.CreateActivity("User Account Created", string.Format("User account created for '{1} {2}', {0}", model.Email, model.FirstName, model.LastName));
                    activity.UserId = userId;

                    context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    await context.SaveChangesAsync();

                    roleName = GetRoleName(roleName);

                    // Send the activation email to the user (if they are an admin)
                    // TO DO: When live with time sheets, this should be anyone
                    if (roleName == "Admin") {
                        helper.SendActivationEmail();
                    }

                    //ShowSuccess("The user account has been created successfully - the activation email has been sent to the user");
                    //return RedirectToAction("Roles", new { id = user.DisplayId });

                    // Automatically add the user into the role
                    await helper.UpdateRoles(new string[] { roleName });

                    ShowSuccess("The user account has been created successfully");
                    return RedirectToAction("Index");
                }
                else {
                    ShowIdentityErrors(result);
                }
            }
            else {
                ShowModelStateErrorsInMessage();
            }

            // If we got this far, something failed, redisplay form
            model.SetLists(GetUserId());
            return View("New", model);
        }

        //
        // GET: /Users/{Id}
        [Route("{id:int:min(1)}", Order = 9)]
        [Authorize(Roles = "Admin, Developer")]
        public ViewResult Show(int id)
        {
            var user = GetUser(id);

            // Is the logged in user a Developer?
            string userId = GetUserId();
            bool developer = false;

            if (UserManager.IsInRole(userId, "Developer")) {
                developer = true;
            }

            ViewBag.Developer = developer;
            return View(user);
        }

       
        //
        // GET: /Users/{Id}/Profile
        //[Authorize(Roles = "Admin, Developer")]
        [Route("{id:int:min(1)}/Edit", Order = 3)]
        [Route("~/Account/Profile", Name = "Users-UserEdit", Order = 11)]
        public ActionResult Edit(int? id)
        {
            // Allow access for all users. If id not specified, get logged in user
            ApplicationUser user = (id.HasValue ? GetUser(id.Value) : GetUser());

            if (user == null) {
                return RedirectOnError();
            }

            var model = new ProfileViewModel(user);
            model.SetLists(GetUserId());

            return View("New", model);
        }

        //
        // POST: /Users/{Id}/Profile
        //[Authorize(Roles = "Admin, Developer")]
        [Route("{id:int:min(1)}/Update", Order = 4)]
        [Route("~/Account/Profile", Order = 11)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int? id, ProfileViewModel model)
        {
            // Allow access for all users. If id not specified, get logged in user
            ApplicationUser user = (id.HasValue ? GetUser(id.Value) : GetUser());

            if (user == null) {
                ShowError("Invalid user");
                return RedirectOnError();
            }

            if (ModelState.IsValid) {
                user = model.ParseAsEntity(user);

                // Create an activity
                var helper = GetHelper(user);
                var activity = helper.CreateActivity("User Account Updated", string.Format("User account updated for '{1} {2}', {0}", model.Email, model.FirstName, model.LastName));
                activity.UserId = GetUserId();

                context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                await context.SaveChangesAsync();

                ShowSuccess("User profile updated successfully");

                if (id.HasValue) {
                    return RedirectToAction("Show", new { id = id });
                }
                else {
                    // If user access, bounce out to dashboard
                    return RedirectToAction("Index", "Account");
                }
            }

            // Something went wrong, redisplay the form
            model.SetLists(GetUserId());
            model.User = user;
            return View("new", model);
        }

        //
        // GET: /Users/{Id}/Roles
        [Authorize(Roles = "Admin, Developer")]
        public ViewResult Roles(int id)
        {
            var user = GetUser(id);
            var helper = GetHelper(user);
            var roles = helper.GetUserRoles();

            var model = new UserRolesViewModel {
                UserId = user.DisplayId.ToString(),
                User = user,
                CurrentRoles = roles,
                NewRoles = roles,
                RoleList = GetRoles()
            };

            return View(model);
        }

        //
        // POST: /Users/{Id}/Roles
        [Authorize(Roles = "Admin, Developer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Roles(int id, UserRolesViewModel model)
        {
            var user = GetUser(id);

            if (ModelState.IsValid) {
                var helper = GetHelper(user);
                string userId = GetUserId();
                string status = await helper.UpdateRoles(model.NewRoles);

                if (status.StartsWith("Error")) {
                    ModelState.AddModelError("", status);
                }
                else {
                    ShowSuccess(status);
                    return RedirectToAction("Show", new { id = id });
                }
            }

            // Something went wrong, redisplay the form
            model.User = user;
            model.RoleList = GetRoles();

            return View(model);
        }

        //
        // /Users/{Id}/ResetPassword
        [Authorize(Roles = "Admin, Developer")]
        public ViewResult ResetPassword(int id)
        {
            var user = GetUser(id);

            ViewBag.UserId = user.Id;
            ViewBag.FullName = user.FullName;

            return View(new ResetPasswordViewModel { Email = user.Email });
        }

        //
        // /Users/{Id}/ResetPassword
        [Authorize(Roles = "Admin, Developer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(int id, ResetPasswordViewModel model)
        {
            var user = GetUser(id);
            
            if (ModelState.IsValid) {
                var helper = GetHelper(user);
                var result = await helper.ResetPassword(model.Password);

                if (result.Succeeded) {
                    ShowSuccess("The Password has been updated successfully");
                    return RedirectToAction("Show", new { id = id });
                }
                else {
                    ShowIdentityErrors(result);
                }
            }

            // Something went wrong, redisplay the form
            ViewBag.UserId = user.Id;
            ViewBag.FullName = user.FullName;

            return View(model);
        }

        // POST: /Admin/Users/{id}/Info
        [HttpPost]
        public JsonResult Info(int id)
        {
            var user = GetUser(id);

            if (user == null) {
                return Json(data: new { phoneNo = "", email = "" });
            }

            return Json(data: new { phoneNo = user.PhoneNumber, email = user.Email });
        }

        //
        // GET: /Users/{Id}/Delete
        [Authorize(Roles = "Admin, Developer")]
        public ViewResult Delete(int id)
        {
            var user = GetUser(id);
            return View(user);
        }

        //
        // /Users/{Id}/Destroy
        [Authorize(Roles = "Admin, Developer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Destroy(int id)
        {
            string deletedBy = GetUserId();
            var user = GetUser(id);
            var helper = GetHelper(user);
            string status = await helper.DeleteUser();

            if (status.StartsWith("Error")) {
                ShowError(status);
                return RedirectToAction("Show", new { id = id });
            }
            else {
                ShowSuccess(status);
                return RedirectToAction("Index", routeValues: new { id = "" });
            }
        }

        #region Child Actions

        public PartialViewResult GetBreadcrumb(string roleName, int displayId, bool mainAsLink = true)
        {
            var user = GetUser(displayId);
            ViewBag.MainAsLink = mainAsLink;
            ViewBag.RoleName = roleName;
            return PartialView("Partials/_Breadcrumb", user);
        }
        
        #endregion

        #region Controller Helpers

        private RedirectToRouteResult RedirectOnError()
        {
            if (IsAdmin()) {
                return RedirectToAction("Index");
            }
            else {
                return RedirectToAction("Index", "Account");
            }
        }

        private UserHelper GetHelper(ApplicationUser user)
        {
            var helper = new UserHelper(user);

            helper.ServiceUserId = GetUserId();

            return helper;
        }

        private string GetRoleName(string roleName)
        {
            return UserHelper.SingulariseRoleName(roleName);
        }

        #endregion

    }
}