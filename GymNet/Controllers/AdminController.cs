using GymNet.Data;
using GymNet.Models;
using GymNet.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GymNet.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: /Admin/RegisterMember
        public ActionResult RegisterMember()
        {
            // Get available membership plans for dropdown
            ViewBag.MembershipPlans = new SelectList(
                db.MembershipPlans.Where(p => p.IsActive).ToList(),
                "Id",
                "Name");

            return View(new AdminRegisterMemberViewModel());
        }

        // POST: /Admin/RegisterMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterMember(AdminRegisterMemberViewModel model)
        {
            // Get available membership plans for dropdown (in case of validation error)
            ViewBag.MembershipPlans = new SelectList(
                db.MembershipPlans.Where(p => p.IsActive).ToList(),
                "Id",
                "Name");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            // Generate temporary password (6 characters)
            var tempPassword = GenerateTemporaryPassword();

            // Create new user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = model.Address,
                LockoutEnabled = true
            };

            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var result = await userManager.CreateAsync(user, tempPassword);

            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(model);
            }

            // Assign Member role
            var roleManager = HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            if (!await roleManager.RoleExistsAsync("Member"))
            {
                await roleManager.CreateAsync(new IdentityRole("Member"));
            }
            await userManager.AddToRoleAsync(user.Id, "Member");

            // If membership plan selected, create membership
            if (model.MembershipPlanId.HasValue)
            {
                var plan = await db.MembershipPlans.FindAsync(model.MembershipPlanId.Value);
                if (plan != null)
                {
                    var membership = new MemberMembership
                    {
                        UserId = user.Id,
                        MembershipPlanId = plan.Id,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(plan.DurationInMonths),
                        Status = model.CollectPaymentNow ? "PendingPayment" : "Active",
                        CreatedAt = DateTime.Now
                    };

                    db.MemberMemberships.Add(membership);
                    await db.SaveChangesAsync();

                    // If collecting payment now, create a pending payment record
                    if (model.CollectPaymentNow)
                    {
                        var payment = new Payment
                        {
                            MemberMembershipId = membership.Id,
                            Amount = plan.Price,
                            PaymentMethod = "Manual",
                            Status = "Pending",
                            PaymentDate = DateTime.Now,
                            TransactionReference = "ADMIN-" + DateTime.Now.ToString("yyyyMMddHHmmss")
                        };

                        db.Payments.Add(payment);
                        await db.SaveChangesAsync();
                    }
                }
            }

            // Store temp password in TempData to show to admin
            TempData["SuccessMessage"] = $"Member {user.FirstName} {user.LastName} registered successfully!";
            TempData["TempPassword"] = tempPassword;
            TempData["NewMemberEmail"] = user.Email;

            return RedirectToAction("RegistrationSuccess");
        }

        // GET: /Admin/RegistrationSuccess
        public ActionResult RegistrationSuccess()
        {
            if (TempData["TempPassword"] == null)
            {
                return RedirectToAction("Members");
            }

            ViewBag.TempPassword = TempData["TempPassword"];
            ViewBag.NewMemberEmail = TempData["NewMemberEmail"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return View();
        }

        // Helper method to generate temporary password
        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return password;
        }

        // Helper method to add errors
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        // GET: /Admin/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            var memberRoleId = await db.Roles
                .Where(r => r.Name == "Member")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            int totalMembers = memberRoleId == null
                ? 0
                : await db.Users.CountAsync(u => u.Roles.Any(r => r.RoleId == memberRoleId));

            int activeMemberships = await db.MemberMemberships
                .CountAsync(m => m.Status == "Active" && m.EndDate > DateTime.Now);

            int totalPlans = await db.MembershipPlans.CountAsync(p => p.IsActive);

            int totalPayments = await db.Payments.CountAsync(p => p.Status == "Completed");

            decimal totalRevenue = await db.Payments
                .Where(p => p.Status == "Completed")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var model = new AdminDashboardViewModel
            {
                AdminName = User.Identity.GetUserName(),
                TotalMembers = totalMembers,
                ActiveMemberships = activeMemberships,
                TotalMembershipPlans = totalPlans,
                TotalPayments = totalPayments,
                TotalRevenue = totalRevenue
            };

            return View(model);
        }

        // GET: /Admin/Members
        public async Task<ActionResult> Members(string searchTerm = "", string membershipStatus = "", string accountStatus = "")
        {
            var memberRoleId = await db.Roles
                .Where(r => r.Name == "Member")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            // Get all users with Member role
            var membersQuery = db.Users
                .Where(u => u.Roles.Any(r => r.RoleId == memberRoleId))
                .Select(u => new MemberListItemViewModel
                {
                    UserId = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.LockoutEndDateUtc == null,
                    MemberSince = db.MemberMemberships
                        .Where(m => m.UserId == u.Id)
                        .OrderBy(m => m.StartDate)
                        .Select(m => (DateTime?)m.StartDate)
                        .FirstOrDefault(),
                    MembershipStatus = db.MemberMemberships
                        .Where(m => m.UserId == u.Id)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.Status)
                        .FirstOrDefault() ?? "No Membership",
                    MembershipPlan = db.MemberMemberships
                        .Where(m => m.UserId == u.Id)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.MembershipPlan.Name)
                        .FirstOrDefault() ?? "N/A",
                    HasPaid = db.Payments
                        .Any(p => p.MemberMembership.UserId == u.Id && p.Status == "Completed")
                });

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                membersQuery = membersQuery.Where(m =>
                    m.FullName.ToLower().Contains(searchTerm) ||
                    m.Email.ToLower().Contains(searchTerm));
            }

            // Apply membership status filter
            if (!string.IsNullOrWhiteSpace(membershipStatus))
            {
                if (membershipStatus == "Paid")
                {
                    membersQuery = membersQuery.Where(m => m.HasPaid);
                }
                else if (membershipStatus == "Unpaid")
                {
                    membersQuery = membersQuery.Where(m => !m.HasPaid);
                }
                else
                {
                    membersQuery = membersQuery.Where(m => m.MembershipStatus == membershipStatus);
                }
            }

            // Apply account status filter
            if (!string.IsNullOrWhiteSpace(accountStatus))
            {
                if (accountStatus == "Active")
                {
                    membersQuery = membersQuery.Where(m => m.IsActive);
                }
                else if (accountStatus == "Deactivated")
                {
                    membersQuery = membersQuery.Where(m => !m.IsActive);
                }
            }

            var members = await membersQuery
                .OrderByDescending(m => m.MemberSince)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.MembershipStatus = membershipStatus;
            ViewBag.AccountStatus = accountStatus;

            return View(members);
        }

        // GET: /Admin/MemberDetails/{id}
        public async Task<ActionResult> MemberDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var memberships = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Where(m => m.UserId == id)
                .OrderByDescending(m => m.StartDate)
                .ToListAsync();

            var payments = await db.Payments
                .Where(p => p.MemberMembership.UserId == id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var model = new AdminMemberDetailsViewModel
            {
                UserId = user.Id,
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                IsActive = user.LockoutEndDateUtc == null,
                Memberships = memberships.Select(m => new MemberMembershipHistoryViewModel
                {
                    Id = m.Id,
                    PlanName = m.MembershipPlan.Name,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Status = m.Status
                }).ToList(),
                Payments = payments.Select(p => new MemberPaymentHistoryViewModel
                {
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    TransactionReference = p.TransactionReference
                }).ToList()
            };

            return View(model);
        }

        // GET: /Admin/EditMember/{id}
        public async Task<ActionResult> EditMember(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new EditMemberViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address
            };

            return View(model);
        }

        // POST: /Admin/EditMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditMember(EditMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email; // Since we use email as username
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Member {user.FirstName} {user.LastName} has been updated successfully.";
            return RedirectToAction("MemberDetails", new { id = user.Id });
        }

        // POST: /Admin/ToggleAccountStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleAccountStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (user.LockoutEndDateUtc == null || user.LockoutEndDateUtc <= DateTime.UtcNow)
            {
                // Deactivate account
                user.LockoutEnabled = true;
                user.LockoutEndDateUtc = DateTime.UtcNow.AddYears(100);
                TempData["SuccessMessage"] = $"Account for {user.FirstName} {user.LastName} has been deactivated.";
            }
            else
            {
                // Activate account
                user.LockoutEndDateUtc = null;
                user.LockoutEnabled = true; // Keep lockout enabled for failed attempts
                TempData["SuccessMessage"] = $"Account for {user.FirstName} {user.LastName} has been activated.";
            }

            await db.SaveChangesAsync();

            return RedirectToAction("MemberDetails", new { id = user.Id });
        }
        // GET: /Admin/CheckIns
        public async Task<ActionResult> CheckIns()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1); // Calculate this before the query

            var todayCheckIns = await db.CheckIns
                .Where(c => c.CheckInTime >= today && c.CheckInTime < tomorrow)
                .OrderByDescending(c => c.CheckInTime)
                .Select(c => new CheckInHistoryViewModel
                {
                    Id = c.Id,
                    FullName = c.User.FirstName + " " + c.User.LastName,
                    Email = c.User.Email,
                    CheckInTime = c.CheckInTime,
                    CheckInMethod = c.CheckInMethod,
                    QRCode = c.QRCode
                })
                .ToListAsync();

            var allCheckIns = await db.CheckIns
                .OrderByDescending(c => c.CheckInTime)
                .Take(100)
                .Select(c => new CheckInHistoryViewModel
                {
                    Id = c.Id,
                    FullName = c.User.FirstName + " " + c.User.LastName,
                    Email = c.User.Email,
                    CheckInTime = c.CheckInTime,
                    CheckInMethod = c.CheckInMethod,
                    QRCode = c.QRCode
                })
                .ToListAsync();

            var model = new AdminCheckInViewModel
            {
                TodayCheckIns = todayCheckIns,
                AllCheckIns = allCheckIns,
                TotalCheckInsToday = todayCheckIns.Count,
                TotalCheckInsAllTime = await db.CheckIns.CountAsync(),
                UniqueMembersToday = todayCheckIns.Select(c => c.Email).Distinct().Count()
            };

            return View(model);
        }

        // GET: /Admin/GenerateQRCode
        public ActionResult GenerateQRCode()
        {
            var qrContent = "GYMNET-CHECKIN-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" +
                            Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            var model = new GenerateQRCodeViewModel
            {
                QRCodeContent = qrContent,
                GeneratedAt = DateTime.Now,
                ValidUntil = DateTime.Now.AddHours(12) // QR valid for 12 hours
            };

            return View(model);
        }
        // GET: /Admin/ManualCheckIn
        public ActionResult ManualCheckIn(string searchTerm = "")
        {
            // Calculate dates outside the LINQ query
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Get all active members for search
            var memberRoleId = db.Roles
                .Where(r => r.Name == "Member")
                .Select(r => r.Id)
                .FirstOrDefault();

            var members = db.Users
                .Where(u => u.Roles.Any(r => r.RoleId == memberRoleId))
                .Select(u => new ManualCheckInMemberViewModel
                {
                    UserId = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    HasActiveMembership = db.MemberMemberships
                        .Any(m => m.UserId == u.Id &&
                                  m.Status == "Active" &&
                                  m.EndDate > DateTime.Now),
                    MembershipStatus = db.MemberMemberships
                        .Where(m => m.UserId == u.Id)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.Status)
                        .FirstOrDefault() ?? "No Membership",
                    MembershipPlan = db.MemberMemberships
                        .Where(m => m.UserId == u.Id)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.MembershipPlan.Name)
                        .FirstOrDefault() ?? "N/A",
                    HasPaid = db.Payments
                        .Any(p => p.MemberMembership.UserId == u.Id && p.Status == "Completed"),
                    CheckedInToday = db.CheckIns
                        .Any(c => c.UserId == u.Id &&
                                  c.CheckInTime >= today &&
                                  c.CheckInTime < tomorrow)
                })
                .OrderBy(m => m.FullName)
                .ToList();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                members = members.Where(m =>
                    m.FullName.ToLower().Contains(searchTerm) ||
                    m.Email.ToLower().Contains(searchTerm) ||
                    m.PhoneNumber.Contains(searchTerm))
                .ToList();
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.TodayCheckInCount = db.CheckIns
                .Count(c => c.CheckInTime >= today &&
                            c.CheckInTime < tomorrow);

            return View(members);
        }

        // POST: /Admin/ProcessManualCheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProcessManualCheckIn(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Invalid member selected." });
            }

            // Get user details
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Member not found." });
            }

            // Calculate dates outside the LINQ query
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Check if already checked in today
            var alreadyCheckedIn = await db.CheckIns
                .AnyAsync(c => c.UserId == userId &&
                               c.CheckInTime >= today &&
                               c.CheckInTime < tomorrow);

            if (alreadyCheckedIn)
            {
                return Json(new { success = false, message = $"{user.FirstName} {user.LastName} has already checked in today." });
            }

            // Check membership status
            var latestMembership = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestMembership == null)
            {
                return Json(new { success = false, message = "No membership found for this member." });
            }

            if (latestMembership.Status != "Active")
            {
                return Json(new { success = false, message = $"Membership is not active (Status: {latestMembership.Status})." });
            }

            if (latestMembership.EndDate < DateTime.Now)
            {
                return Json(new { success = false, message = "Membership has expired." });
            }

            if (!latestMembership.Payments.Any(p => p.Status == "Completed"))
            {
                return Json(new { success = false, message = "Payment not completed for this membership." });
            }

            // Create manual check-in record
            var checkIn = new CheckIn
            {
                UserId = userId,
                CheckInTime = DateTime.Now,
                CheckInMethod = "Manual",
                QRCode = "MANUAL-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                Notes = "Manual check-in by admin"
            };

            db.CheckIns.Add(checkIn);
            await db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"{user.FirstName} {user.LastName} checked in successfully at {checkIn.CheckInTime.ToString("HH:mm:ss")}."
            });
        }

        // GET: /Admin/MemberCheckInHistory/{id}
        public async Task<ActionResult> MemberCheckInHistory(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var checkIns = await db.CheckIns
                .Where(c => c.UserId == id)
                .OrderByDescending(c => c.CheckInTime)
                .Select(c => new CheckInHistoryViewModel
                {
                    Id = c.Id,
                    FullName = c.User.FirstName + " " + c.User.LastName,
                    Email = c.User.Email,
                    CheckInTime = c.CheckInTime,
                    CheckInMethod = c.CheckInMethod,
                    QRCode = c.QRCode
                })
                .ToListAsync();

            ViewBag.MemberName = user.FirstName + " " + user.LastName;
            ViewBag.MemberEmail = user.Email;

            return View(checkIns);
        }
        // GET: /Admin/PendingPayments
        public async Task<ActionResult> PendingPayments()
        {
            // Get members with pending payments or unpaid memberships
            var pendingPayments = await db.MemberMemberships
                .Include(m => m.User)
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .Where(m => m.Status == "PendingPayment" ||
                            (m.Status == "Active" && !m.Payments.Any(p => p.Status == "Completed")))
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new AdminPaymentListViewModel
                {
                    PaymentId = m.Payments.OrderByDescending(p => p.PaymentDate)
                        .Select(p => p.Id)
                        .FirstOrDefault(),
                    MemberName = m.User.FirstName + " " + m.User.LastName,
                    Email = m.User.Email,
                    PlanName = m.MembershipPlan.Name,
                    Amount = m.MembershipPlan.Price,
                    PaymentMethod = m.Payments.OrderByDescending(p => p.PaymentDate)
                        .Select(p => p.PaymentMethod)
                        .FirstOrDefault() ?? "Not Specified",
                    PaymentDate = m.Payments.OrderByDescending(p => p.PaymentDate)
                        .Select(p => p.PaymentDate)
                        .FirstOrDefault(),
                    Status = m.Payments.OrderByDescending(p => p.PaymentDate)
                        .Select(p => p.Status)
                        .FirstOrDefault() ?? "Pending",
                    TransactionReference = m.Payments.OrderByDescending(p => p.PaymentDate)
                        .Select(p => p.TransactionReference)
                        .FirstOrDefault() ?? "N/A"
                })
                .ToListAsync();

            return View(pendingPayments);
        }

        // GET: /Admin/AllPayments
        public async Task<ActionResult> AllPayments(string searchTerm = "", string paymentStatus = "")
        {
            var paymentsQuery = db.Payments
                .Include(p => p.MemberMembership.User)
                .Include(p => p.MemberMembership.MembershipPlan)
                .Select(p => new AdminPaymentListViewModel
                {
                    PaymentId = p.Id,
                    MemberName = p.MemberMembership.User.FirstName + " " + p.MemberMembership.User.LastName,
                    Email = p.MemberMembership.User.Email,
                    PlanName = p.MemberMembership.MembershipPlan.Name,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    TransactionReference = p.TransactionReference
                });

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                paymentsQuery = paymentsQuery.Where(p =>
                    p.MemberName.ToLower().Contains(searchTerm) ||
                    p.Email.ToLower().Contains(searchTerm) ||
                    p.TransactionReference.ToLower().Contains(searchTerm));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                paymentsQuery = paymentsQuery.Where(p => p.Status == paymentStatus);
            }

            var payments = await paymentsQuery
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.PaymentStatus = paymentStatus;

            return View(payments);
        }

        // GET: /Admin/RecordPayment/{userId}
        public async Task<ActionResult> RecordPayment(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return HttpNotFound();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Get memberships that need payment
            var memberships = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var pendingMembership = memberships
                .FirstOrDefault(m => m.Status == "PendingPayment" ||
                                     (m.Status == "Active" && !m.Payments.Any(p => p.Status == "Completed")));

            if (pendingMembership == null)
            {
                TempData["ErrorMessage"] = "No pending payment found for this member.";
                return RedirectToAction("Members");
            }

            var model = new AdminRecordPaymentViewModel
            {
                UserId = userId,
                MemberMembershipId = pendingMembership.Id,
                Amount = pendingMembership.MembershipPlan.Price
            };

            ViewBag.MemberName = user.FirstName + " " + user.LastName;
            ViewBag.PlanName = pendingMembership.MembershipPlan.Name;
            ViewBag.MembershipStatus = pendingMembership.Status;
            ViewBag.MembershipEndDate = pendingMembership.EndDate;

            return View(model);
        }

        // POST: /Admin/RecordPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecordPayment(AdminRecordPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
                ViewBag.MemberName = user?.FirstName + " " + user?.LastName;
                return View(model);
            }

            var membership = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .FirstOrDefaultAsync(m => m.Id == model.MemberMembershipId);

            if (membership == null)
            {
                return HttpNotFound();
            }

            var userInfo = await db.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);

            // Generate transaction reference if not provided
            var transactionRef = string.IsNullOrWhiteSpace(model.TransactionReference)
                ? "ADMIN-" + DateTime.Now.ToString("yyyyMMddHHmmss")
                : model.TransactionReference;

            // Create payment record
            var payment = new Payment
            {
                MemberMembershipId = membership.Id,
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                Status = "Completed",
                PaymentDate = DateTime.Now,
                TransactionReference = transactionRef,
                CardHolderName = userInfo?.FirstName + " " + userInfo?.LastName,
                BankName = "Manual Payment",
                AccountNumber = "N/A"
            };

            db.Payments.Add(payment);

            // Activate the membership
            membership.Status = "Active";

            // If membership is pending, set start date to now
            if (membership.Status == "PendingPayment")
            {
                membership.StartDate = DateTime.Now;
                membership.EndDate = DateTime.Now.AddMonths(membership.MembershipPlan.DurationInMonths);
            }

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payment of R{model.Amount.ToString("F2")} recorded for {userInfo?.FirstName} {userInfo?.LastName}. Membership activated successfully!";

            return RedirectToAction("AllPayments");
        }

        // POST: /Admin/MarkAsPaid/{membershipId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsPaid(int membershipId)
        {
            var membership = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.User)
                .Include(m => m.Payments)
                .FirstOrDefaultAsync(m => m.Id == membershipId);

            if (membership == null)
            {
                return HttpNotFound();
            }

            // Create a completed payment record
            var payment = new Payment
            {
                MemberMembershipId = membership.Id,
                Amount = membership.MembershipPlan.Price,
                PaymentMethod = "Manual",
                Status = "Completed",
                PaymentDate = DateTime.Now,
                TransactionReference = "ADMIN-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                CardHolderName = membership.User.FirstName + " " + membership.User.LastName,
                BankName = "Manual Payment",
                AccountNumber = "N/A"
            };

            db.Payments.Add(payment);

            // Activate membership
            membership.Status = "Active";
            membership.StartDate = DateTime.Now;
            membership.EndDate = DateTime.Now.AddMonths(membership.MembershipPlan.DurationInMonths);

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payment marked as completed for {membership.User.FirstName} {membership.User.LastName}. Membership activated!";

            return RedirectToAction("PendingPayments");
        }
        // POST: /Admin/CancelMembership
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelMembership(int membershipId)
        {
            var membership = await db.MemberMemberships.FirstOrDefaultAsync(m => m.Id == membershipId);
            if (membership == null)
            {
                return HttpNotFound();
            }

            var userId = membership.UserId;
            membership.Status = "Cancelled";
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Membership has been cancelled successfully.";
            return RedirectToAction("MemberDetails", new { id = userId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}