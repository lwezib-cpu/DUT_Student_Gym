using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using GymNet.Models;
using GymNet.ViewModels;
using System;

namespace GymNet.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        public ApplicationRoleManager RoleManager
        {
            get { return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>(); }
            private set { _roleManager = value; }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = model.Address,
                LockoutEnabled = true // Make sure lockout is enabled for new users
            };

            var result = await UserManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!await RoleManager.RoleExistsAsync("Member"))
                {
                    await RoleManager.CreateAsync(new IdentityRole("Member"));
                }
                await UserManager.AddToRoleAsync(user.Id, "Member");

                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                TempData["SuccessMessage"] = "Registration successful! Welcome to GymNet, " + user.FirstName + ".";
                return RedirectToAction("Dashboard", "Member");
            }

            AddErrors(result);
            return View(model);
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find the user by email
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Check if the account is deactivated (locked out by admin)
            if (user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value > DateTime.UtcNow)
            {
                // Account is deactivated
                var lockoutEnd = user.LockoutEndDateUtc.Value.ToLocalTime();

                // If lockout is for a very long time (like 100 years), it's an admin deactivation
                var isAdminDeactivation = (lockoutEnd - DateTime.Now).TotalDays > 365;

                if (isAdminDeactivation)
                {
                    ModelState.AddModelError("", "Your account has been deactivated. Please contact the gym administrator for assistance.");
                }
                else
                {
                    ModelState.AddModelError("", $"This account has been locked due to multiple failed attempts. Please try again after {lockoutEnd.ToString("dd MMM yyyy HH:mm")}.");
                }

                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(
                user.UserName, model.Password, model.RememberMe, shouldLockout: true);

            switch (result)
            {
                case SignInStatus.Success:
                    TempData["SuccessMessage"] = "Login successful. Welcome back, " + user.FirstName + "!";
                    return await RedirectToRoleDashboard(user, returnUrl);

                case SignInStatus.LockedOut:
                    ModelState.AddModelError("", "This account has been locked due to multiple failed attempts. Please try again in a few minutes.");
                    return View(model);

                case SignInStatus.RequiresVerification:
                    ModelState.AddModelError("", "Two-factor verification is required.");
                    return View(model);

                default:
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View(model);
            }
        }

        // Sends the user to the correct dashboard based on their role.
        private async Task<ActionResult> RedirectToRoleDashboard(ApplicationUser user, string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Dashboard", "Member");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie);
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null) { _userManager.Dispose(); _userManager = null; }
                if (_signInManager != null) { _signInManager.Dispose(); _signInManager = null; }
            }
            base.Dispose(disposing);
        }

        // GET: /Account/Deactivated
        [AllowAnonymous]
        public ActionResult Deactivated()
        {
            return View();
        }
    }
}