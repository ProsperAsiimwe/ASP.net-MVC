using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;
using SteppingStone.WebUI.Infrastructure;
using SteppingStone.WebUI.Infrastructure.Helpers;
using SteppingStone.WebUI.Models.Account;
using SteppingStone.WebUI.Models.Users;
using SteppingStone.WebUI.Models.Dashboard;
using SteppingStone.Domain.Models;

namespace SteppingStone.WebUI.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        public AccountController()
        {
        }

        public AccountController(ApplicationSignInManager signInManager)
        {
            SignInManager = signInManager;
        }

        private ApplicationSignInManager _signInManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set { _signInManager = value; }
        }

        #region Login

        public Term CreateFakeTerm()
        {
            return new Term()
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(90),
                IsCurrentTerm = true,
                Position = 0
            };
        }

        public ClassLevel CreateFakeClass()
        {
            return new ClassLevel()
            {
                Level = 1,
                SchoolFee = 300000,
                StudyMode = 1
            };
        }

        public Bank CreateFakeBank()
        {
            return new Bank()
            {
                AccountNo = "206253000007",
                Name = "Finance Trust Bank"
            };
        }

        //public Student CreateFakeStudent()
        //{
        //    return new Student()
        //    {
        //        FirstName = "Patricia",
        //        LastName = "Nabikolo",
        //        Gender = 2                
        //    };
        //}
        //
        // GET: /Login
        [Route("Login")]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Login
        [Route("Login")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            var kado = new UsersController();
            kado.Init();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doen't count login failures towards lockout only two factor authentication
            // To enable password failures to trigger lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    var user = await UserManager.FindByNameAsync(model.Email);

                    if (user != null)
                    {
                        ConfirmEmailMsg(user);
                        return View("DisplayEmail");
                    }

                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            new CookieHelper().Flush();
            return RedirectToAction("Index", "Home");
        }

        // GET: /GetLogin
        [AllowAnonymous]
        [ChildActionOnly]
        public PartialViewResult GetLoginForm()
        {
            return PartialView("Partials/_Login", new LoginViewModel());
        }

        #endregion

        #region Account/User actions

        //
        // GET: /Account/
        public ActionResult Index()
        {
            Session["SearchUsersModel"] = null;
            Session["SearchPaymentsModel"] = null;
            Session["SearchStudentsModel"] = null;
            Session["SearchParentsModel"] = null;
            Session["SearchEventsModel"] = null;
            Session["SearchTermsModel"] = null;
            Session["SearchClassesModel"] = null;
            Session["SearchMessagesModel"] = null;
            Session["SearchBanksModel"] = null;

            var model = new DashboardModel();
            model.Activities = context.Activities
             .OrderByDescending(o => o.Recorded)
             .Take(20);

            var term = context.Terms.FirstOrDefault(p => p.IsCurrentTerm);
            model.TermId = term == null ? (int?)null : term.EndDate <= UgandaDateTime.DateNow().Date ? term.TermId : (int?)null;

            return View(model);
        }

        //
        // POST: /Account/Impersonate
        [Authorize(Roles = "Developer, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Impersonate(string userId)
        {
            var user = GetUser(userId);
            await SignInManager.SignInAsync(user, false, false);

            return RedirectToAction("Index");
        }

        //
        // /Account/Manage
        public ViewResult Manage()
        {
            var user = GetUser();
            var model = new ProfileViewModel(user);
            return View(model);
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string userId = User.Identity.GetUserId();
                    var user = UserManager.FindById(userId);

                    // Save the model properties to the user object
                    user = model.ParseAsEntity(user);

                    context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    await context.SaveChangesAsync();

                    ShowSuccess("Account profile updated successfully");
                }
                catch (Exception ex)
                {
                    ShowTryCatchError(ex.Message);
                }
            }

            // Something went wrong, redisplay the form
            model.SetLists(GetUserId());
            return View(model);
        }

        //
        // /Account/ChangePwd
        public ViewResult ChangePwd()
        {
            var user = GetUser();
            return View("ResetPassword", new ResetPasswordViewModel { Verb = "Change", Email = user.UserName });
        }

        //
        // POST: /Account/ChangePwd
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePwd(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string userId = GetUserId();
                var user = UserManager.FindById(userId);
                var helper = new UserHelper(user);
                helper.ServiceUserId = userId;
                var result = await helper.ResetPassword(model.Password);

                if (result.Succeeded)
                {
                    ShowSuccess("Your Password was updated successfully");
                    return RedirectToAction("Index");
                }
                else
                {
                    ShowIdentityErrors(result);
                }
            }

            // Something went wrong, redisplay the form
            return View("ResetPassword", model);
        }

        #endregion

        #region Register (commented out)

        //
        // GET: /Register
        //[Route("Register")]
        //[AllowAnonymous]
        //public ActionResult Register()
        //{
        //    var model = new RegisterViewModel();
        //    model.Titles = GetTitles();
        //    return View(model);
        //}

        //
        // POST: /Register
        //[Route("Register")]
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid) {
        //        var user = new ApplicationUser();
        //        user = model.ParseAsEntity(user);

        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded) {
        //            ConfirmEmailMsg(user);
        //            return View("DisplayEmail");
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    model.Titles = GetTitles();
        //    ShowStandardError();
        //    return View(model);
        //}

        //
        // GET: /ConfirmEmail
        [AllowAnonymous]
        [Route("ConfirmEmail/{userId}")]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            try
            {
                var result = await UserManager.ConfirmEmailAsync(userId, code);

                if (result.Succeeded)
                {
                    // Has the user created a password?
                    bool hasPassword = await UserManager.HasPasswordAsync(userId);

                    if (!hasPassword)
                    {
                        var resetCode = await UserManager.GeneratePasswordResetTokenAsync(userId);
                        var user = GetUser(userId);
                        return RedirectToAction("ResetPassword", "Account", new { code = resetCode, mode = "new", email = user.Email });
                    }

                    // Add to default Role
                    await AddUserToRole(userId);
                }

                return View("ConfirmEmail");
            }
            catch (Exception ex)
            { // The user has supplied a userId that doesn't exist
                ViewBag.ErrorMsg = ex.Message;
                return View("AccountError");
            }
        }

        #endregion

        #region Forgot/Reset password

        //
        // GET: /ForgotPassword
        [Route("ForgotPassword")]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /ForgotPassword
        [Route("ForgotPassword")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null) //|| !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                bool isConfirmed = await UserManager.IsEmailConfirmedAsync(user.Id);

                if (!isConfirmed)
                {
                    ConfirmEmailMsg(user);
                    return View("ForgotPasswordConfirmation");
                }
                else
                {
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: Request.Url.Scheme);
                    var abbr = Settings.COMPANY_ABBR;

                    System.Text.StringBuilder body = new System.Text.StringBuilder()
                            .AppendFormat("<p>Dear <strong>{0} {1}</strong></p>", user.FirstName, user.LastName)
                            .AppendFormat("<p>You have just completed the <strong>Forgot Your Password</strong> process on the {0} website. ", Settings.COMPANY_NAME)
                            .AppendFormat(@"You must now reset your password by clicking this <a href=""{0}"">link</a></p>", callbackUrl);

                    string subject = abbr + " - Reset your password";
                    await UserManager.SendEmailAsync(user.Id, subject, body.ToString());
                    ViewBag.Link = callbackUrl;
                    return View("ForgotPasswordConfirmation");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /ForgotPasswordConfirmation
        [Route("ForgotPasswordConfirmation")]
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /ResetPassword
        [Route("ResetPassword")]
        [AllowAnonymous]
        public ActionResult ResetPassword(string code, string mode)
        {
            return code == null ? View("Error") : View(new ResetPasswordViewModel { Verb = "Reset" });
        }

        //
        // POST: /ResetPassword
        [Route("ResetPassword")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                await AddUserToRole(user.Id);
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            ShowIdentityErrors(result);
            return View(model);
        }

        //
        // GET: /ResetPasswordConfirmation
        [Route("ResetPasswordConfirmation")]
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        #endregion

        #region Further Actions

        [ChildActionOnly]
        public ContentResult GetLoginName()
        {
            var user = GetUser();
            return Content("Welcome " + user.FullName);
        }

        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index");
        }

        private void ConfirmEmailMsg(ApplicationUser user)
        {
            var helper = new UserHelper(user);
            string callbackUrl = helper.SendActivationEmail();

            ViewBag.Link = callbackUrl;
        }

        private async Task AddUserToRole(string userId)
        {
            // Add to Client Role
            IEnumerable<string> roles = await UserManager.GetRolesAsync(userId);

            if (roles.Count() == 0)
            {
                await UserManager.AddToRoleAsync(userId, "Candidate");
            }
        }

        #endregion
    }
}