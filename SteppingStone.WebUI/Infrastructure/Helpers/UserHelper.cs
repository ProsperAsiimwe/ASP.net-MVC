using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MagicApps.Infrastructure.Helpers;
using MagicApps.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Models.Users;

namespace SteppingStone.WebUI.Infrastructure.Helpers
{
    public class UserHelper
    {
        private ApplicationDbContext db;
        private ApplicationUserManager UserManager;

        public string UserId;

        public string ServiceUserId;
        
        public ApplicationUser User { get; set; }

        public UserHelper()
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public UserHelper(string userId)
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            this.UserId = userId;
            this.User = db.Users.Find(userId);
        }

        public UserHelper(ApplicationUser user)
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            this.UserId = user.Id;
            this.User = user;
        }

        public UserListViewModel GetUserList(SearchUsersModel searchModel, int page = 1)
        {
            int pageSize = 4;

            if (page < 1) {
                page = 1;
            }

            IEnumerable<ApplicationUser> users = GetUsers().ToList();

            if (!String.IsNullOrEmpty(searchModel.Name)) {
                string name = searchModel.Name.ToLower();
                users = users.Where(x => x.FirstName.ToLower().Contains(name) || x.LastName.ToLower().Contains(name));
            }
            if (!String.IsNullOrEmpty(searchModel.Email)) {
                string email = searchModel.Email.ToLower();
                users = users.Where(x => x.Email.ToLower().Contains(email));
            }
            if (!String.IsNullOrEmpty(searchModel.RoleName)) {
                searchModel.RoleName = SingulariseRoleName(searchModel.RoleName);
                string roleId = db.Roles.Single(x => x.Name.ToLower() == searchModel.RoleName.ToLower()).Id;
                users = users.Where(x => x.Roles.Select(y => y.RoleId).Contains(roleId));
            }
            if (searchModel.DisplayId.HasValue) {
                users = users.Where(x => x.DisplayId == searchModel.DisplayId.Value);
            }

            searchModel.Roles = GetRoles();

            return new UserListViewModel {
                Users = users
                    .OrderBy(o => o.LastName)
                    .ThenBy(o => o.FirstName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize),
                SearchModel = searchModel,
                PagingInfo = new PagingInfo {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = users.Count()
                }
            };
        }

        public async Task<string> DeleteUser()
        {
            string status;

            try {
                string title = "User Account Deleted";
                string msg = string.Format("User account deleted for '{0}', {1}.", User.FullName, User.Email);
                var activity = CreateActivity(title, msg);

                await UserManager.DeleteAsync(User);

                status = "User Account deleted successfully";
            }
            catch (Exception ex) {
                if (ex.InnerException != null) {
                    status = "Error: " + ex.InnerException.InnerException.Message;
                }
                else {
                    status = "Error: " + ex.Message;
                }
            }

            return status;
        }

        public async Task<Microsoft.AspNet.Identity.IdentityResult> ResetPassword(string newPassword)
        {
            var code = await UserManager.GeneratePasswordResetTokenAsync(User.Id);
            var result = await UserManager.ResetPasswordAsync(User.Id, code, newPassword);

            // Create an activity
            var activity = CreateActivity("Password Reset for User", string.Format("The account password for user '{1}', {0} has been reset.", User.Email, User.FullName));
            await db.SaveChangesAsync();

            return result;
        }

        public string[] GetUserRoles()
        {
            string[] roleIds = User.Roles.Select(x => x.RoleId).ToArray();
            return db.Roles
                .OrderBy(o => o.Name)
                .Where(x => roleIds.Contains(x.Id))
                .Select(x => x.Name).ToArray();
        }

        public async Task<string> UpdateRoles(string[] newRoles)
        {
            string status;

            try {
                var userRoles = GetUserRoles();
                await UserManager.RemoveFromRolesAsync(UserId, userRoles);
                await UserManager.AddToRolesAsync(UserId, newRoles);

                string s_oldRoles = string.Join(",", userRoles);
                string s_newRoles = string.Join(",", newRoles.OrderBy(o => o));

                // Create an activity (if updated)
                if (s_oldRoles != s_newRoles) {
                    var description = new System.Text.StringBuilder()
                        .AppendFormat("User roles updated for '{0}', '{1}:", User.FullName, User.Email);

                    if (userRoles.Count() > 0) {
                        description
                            .AppendLine()
                            .AppendLine()
                            .Append("Old roles:").AppendLine().Append(string.Join(", ", userRoles));
                    }

                    description
                        .AppendLine()
                        .AppendLine()
                        .Append("New roles:").AppendLine().Append(string.Join(", ", newRoles));

                    var activity = CreateActivity("User Roles Updated", description.ToString());

                    db.Entry(User).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();
                }

                status = "User Roles updated successfully";
            }
            catch (Exception ex) {
                status = "Error: " + ex.Message;
            }

            return status;
        }

        // Returns the activation code
        public string SendActivationEmail()
        {
            // Get an instance of request to create the Url
            var httpContext = HttpContext.Current;
            var requestContext = httpContext.Request.RequestContext;
            var urlHelper = new System.Web.Mvc.UrlHelper(requestContext);

            string code = UserManager.GenerateEmailConfirmationToken(User.Id);
            string callbackUrl = urlHelper.Action("ConfirmEmail", "Account", new { userId = User.Id, code = code, returnUrl = urlHelper.Action("Create", "Applications") }, protocol: httpContext.Request.Url.Scheme);
            string abbr = Settings.COMPANY_ABBR;

            System.Text.StringBuilder body = new System.Text.StringBuilder()
                .AppendFormat("<p>Dear <strong>{0} {1}</strong></p>", User.FirstName, User.LastName)
                .AppendFormat("<p>An account as been created for you on {0}. ", Settings.COMPANY_NAME)
                .AppendFormat(@"In order to proceed and login into the system you must verify your email address by clicking this <a href=""{0}"">activation link</a></p>", callbackUrl);

            string subject = abbr + " - Confirm your email";

            UserManager.SendEmail(User.Id, subject, body.ToString());

            return callbackUrl;
        }

        public IEnumerable<ApplicationUser> GetUsers()
        {
            string devId = GetDeveloperId();
            string[] developers = db.Roles.Single(x => x.Name == "Developer").Users.Select(x => x.UserId).ToArray();
            IEnumerable<ApplicationUser> users = db.Users
                .Where(x => !developers.Contains(x.Id));

            return users;
        }

        public IEnumerable<ApplicationUser> GetUsers(string roleName)
        {
            string roleId = db.Roles.Single(x => x.Name == roleName).Id;
            return GetUsers()
                .OrderBy(o => o.LastName)
                .ThenBy(o => o.FirstName)
                .Where(x => x.Roles.Select(y => y.RoleId).Contains(roleId));
        }

        public string[] GetRoles()
        {
            string devId = GetDeveloperId();
            string[] roles = db.Roles
                .Where(x => x.Id != devId)
                .OrderBy(o => o.Name)
                .Select(x => x.Name)
                .ToArray();

            return roles;
        }

        public void CreateRole(string roleName)
        {
            try {
                var role = db.Roles.SingleOrDefault(x => x.Name == roleName);

                if (role == null) {
                    role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole(roleName);
                    db.Roles.Add(role);
                    db.SaveChanges();
                }
            }
            catch {
                // do nothing
            }
        }

        public void QuickCreateUser(ProfileViewModel model, string password, string roleName)
        {
            try {
                var user = db.Users.SingleOrDefault(x => x.UserName == model.Email);

                if (user == null) {
                    user = model.ParseAsEntity(new ApplicationUser());
                    var result = UserManager.Create(user, password);
                    UserManager.AddToRole(user.Id, roleName);
                }
            }
            catch {
                // do nothing
            }
        }

        public static string SingulariseRoleName(string roleName)
        {
            // Has the role name been passed as plural?
            if (roleName.EndsWith("s")) {
                roleName = roleName.Substring(0, (roleName.Length - 1));
            }

            return InputHelper.PCase(roleName);
        }

        public Activity CreateActivity(string title, string description)
        {
            var activity = new Activity {
                Title = title,
                Description = description,
                UserId = UserId,
                RecordedById = ServiceUserId
            };
            db.Activities.Add(activity);
            return activity;
        }

        private string GetDeveloperId()
        {
            return db.Roles.Single(x => x.Name == "Developer").Id;
        }
    }
}