using GymNet.Data;
using GymNet.Models;
using GymNet.Helpers;
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

            model.Email = StudentEmailAttribute.Normalize(model.Email);

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
                        EndDate = SemesterCalendar.CalculateEndDateForPlan(DateTime.Now, plan.DurationInMonths),
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
                AdminName = User.Identity.GetFirstName(),
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

        // GET: /Admin/ExportMembersCsv - same filters as the Members list, as a CSV download
        public async Task<ActionResult> ExportMembersCsv(string searchTerm = "", string membershipStatus = "", string accountStatus = "")
        {
            var memberRoleId = await db.Roles
                .Where(r => r.Name == "Member")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

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

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                membersQuery = membersQuery.Where(m => m.FullName.ToLower().Contains(term) || m.Email.ToLower().Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(membershipStatus))
            {
                if (membershipStatus == "Paid") membersQuery = membersQuery.Where(m => m.HasPaid);
                else if (membershipStatus == "Unpaid") membersQuery = membersQuery.Where(m => !m.HasPaid);
                else membersQuery = membersQuery.Where(m => m.MembershipStatus == membershipStatus);
            }
            if (!string.IsNullOrWhiteSpace(accountStatus))
            {
                if (accountStatus == "Active") membersQuery = membersQuery.Where(m => m.IsActive);
                else if (accountStatus == "Deactivated") membersQuery = membersQuery.Where(m => !m.IsActive);
            }

            var members = await membersQuery.OrderByDescending(m => m.MemberSince).ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Full Name,Email,Phone,Account Status,Membership Status,Plan,Member Since,Paid");
            foreach (var m in members)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    CsvField(m.FullName),
                    CsvField(m.Email),
                    CsvField(m.PhoneNumber),
                    CsvField(m.IsActive ? "Active" : "Deactivated"),
                    CsvField(m.MembershipStatus),
                    CsvField(m.MembershipPlan),
                    CsvField(m.MemberSince.HasValue ? m.MemberSince.Value.ToString("yyyy-MM-dd") : ""),
                    CsvField(m.HasPaid ? "Yes" : "No")
                }));
            }

            var bytes = new System.Text.UTF8Encoding(true).GetBytes(sb.ToString());
            return File(bytes, "text/csv", "gymnet-members-" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
        }

        private static string CsvField(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
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

            model.Email = StudentEmailAttribute.Normalize(model.Email);

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
                    UserId = c.UserId,
                    FullName = c.User.FirstName + " " + c.User.LastName,
                    Email = c.User.Email,
                    CheckInTime = c.CheckInTime,
                    CheckInMethod = c.CheckInMethod,
                    QRCode = c.QRCode,
                    CheckOutTime = c.CheckOutTime,
                    DurationMinutes = c.DurationMinutes
                })
                .ToListAsync();

            var allCheckIns = await db.CheckIns
                .OrderByDescending(c => c.CheckInTime)
                .Take(100)
                .Select(c => new CheckInHistoryViewModel
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = c.User.FirstName + " " + c.User.LastName,
                    Email = c.User.Email,
                    CheckInTime = c.CheckInTime,
                    CheckInMethod = c.CheckInMethod,
                    QRCode = c.QRCode,
                    CheckOutTime = c.CheckOutTime,
                    DurationMinutes = c.DurationMinutes
                })
                .ToListAsync();

            var membersCurrentlyInGym = await db.CheckIns
                .CountAsync(c => c.CheckOutTime == null);

            var model = new AdminCheckInViewModel
            {
                TodayCheckIns = todayCheckIns,
                AllCheckIns = allCheckIns,
                TotalCheckInsToday = todayCheckIns.Count,
                TotalCheckInsAllTime = await db.CheckIns.CountAsync(),
                UniqueMembersToday = todayCheckIns.Select(c => c.Email).Distinct().Count(),
                MembersCurrentlyInGym = membersCurrentlyInGym
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
                                  c.CheckInTime < tomorrow),
                    IsCurrentlyCheckedIn = db.CheckIns
                        .Any(c => c.UserId == u.Id && c.CheckOutTime == null)
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

            // Check if the member already has an open (not checked-out) visit
            var alreadyCheckedIn = await db.CheckIns
                .AnyAsync(c => c.UserId == userId && c.CheckOutTime == null);

            if (alreadyCheckedIn)
            {
                return Json(new { success = false, message = $"{user.FirstName} {user.LastName} is already checked in. Check them out first." });
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

        // POST: /Admin/ToggleEquipmentMaintenance/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleEquipmentMaintenance(int id, string note)
        {
            var equipment = await db.Equipment.FirstOrDefaultAsync(e => e.Id == id);
            if (equipment == null)
            {
                TempData["ErrorMessage"] = "Equipment not found.";
                return RedirectToAction("EquipmentBookings");
            }

            equipment.IsUnderMaintenance = !equipment.IsUnderMaintenance;
            equipment.MaintenanceNote = equipment.IsUnderMaintenance ? (note ?? "").Trim() : null;
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = equipment.Name + (equipment.IsUnderMaintenance
                ? " marked as under maintenance - members can't book it until you mark it available again."
                : " is available for booking again.");
            return RedirectToAction("EquipmentBookings");
        }

        // GET: /Admin/ManageTrainers
        public async Task<ActionResult> ManageTrainers()
        {
            var trainerRoleId = await db.Roles.Where(r => r.Name == "Trainer").Select(r => r.Id).FirstOrDefaultAsync();

            var trainers = await db.Users
                .Where(u => u.Roles.Any(r => r.RoleId == trainerRoleId))
                .Select(u => new TrainerListItemViewModel
                {
                    UserId = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    Specialty = u.Specialty,
                    ProfilePhotoUrl = u.ProfilePhotoUrl,
                    ClassCount = db.GymClasses.Count(c => c.TrainerId == u.Id && c.Status != "Cancelled")
                })
                .ToListAsync();

            return View(trainers);
        }

        // GET: /Admin/RegisterTrainer
        public ActionResult RegisterTrainer()
        {
            return View(new AdminRegisterTrainerViewModel());
        }

        // POST: /Admin/RegisterTrainer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterTrainer(AdminRegisterTrainerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await UserManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                return View(model);
            }

            var trainer = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "N/A",
                Address = "N/A",
                Specialty = model.Specialty
            };

            // Admin sets the password directly now - the earlier approach of
            // auto-generating one and only showing it once in a message was too
            // easy to miss, which is exactly what happened.
            var result = await UserManager.CreateAsync(trainer, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error);
                return View(model);
            }

            await UserManager.AddToRoleAsync(trainer.Id, "Trainer");

            if (model.Photo != null && model.Photo.ContentLength > 0)
            {
                var photoUrl = SaveStaffPhoto(model.Photo, trainer.Id);
                trainer.ProfilePhotoUrl = photoUrl;
                await db.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = trainer.FirstName + " " + trainer.LastName + " added as a trainer.";
            return RedirectToAction("ManageTrainers");
        }

        // GET: /Admin/EditTrainer/{id}
        public async Task<ActionResult> EditTrainer(string id)
        {
            var trainer = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (trainer == null) return HttpNotFound();

            var model = new EditTrainerViewModel
            {
                UserId = trainer.Id,
                FirstName = trainer.FirstName,
                LastName = trainer.LastName,
                Email = trainer.Email,
                PhoneNumber = trainer.PhoneNumber,
                Specialty = trainer.Specialty,
                ExistingPhotoUrl = trainer.ProfilePhotoUrl
            };

            return View(model);
        }

        // POST: /Admin/EditTrainer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditTrainer(EditTrainerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var trainer = await db.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
            if (trainer == null) return HttpNotFound();

            trainer.FirstName = model.FirstName;
            trainer.LastName = model.LastName;
            trainer.Email = model.Email;
            trainer.UserName = model.Email;
            trainer.PhoneNumber = model.PhoneNumber;
            trainer.Specialty = model.Specialty;

            if (model.Photo != null && model.Photo.ContentLength > 0)
            {
                trainer.ProfilePhotoUrl = SaveStaffPhoto(model.Photo, trainer.Id);
            }

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Trainer profile updated.";
            return RedirectToAction("ManageTrainers");
        }

        // POST: /Admin/ResetTrainerPassword - sets a new, admin-chosen password on
        // an existing trainer account. This is how to fix a trainer who currently
        // has no working password.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetTrainerPassword(string userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Password must be at least 6 characters.";
                return RedirectToAction("EditTrainer", new { id = userId });
            }

            var trainer = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (trainer == null)
            {
                TempData["ErrorMessage"] = "Trainer not found.";
                return RedirectToAction("ManageTrainers");
            }

            var removeResult = await UserManager.RemovePasswordAsync(userId);
            if (!removeResult.Succeeded && !removeResult.Errors.Any(e => e.Contains("does not have a password")))
            {
                TempData["ErrorMessage"] = "Could not reset password: " + string.Join(" ", removeResult.Errors);
                return RedirectToAction("EditTrainer", new { id = userId });
            }

            var addResult = await UserManager.AddPasswordAsync(userId, newPassword);
            if (!addResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Could not set new password: " + string.Join(" ", addResult.Errors);
                return RedirectToAction("EditTrainer", new { id = userId });
            }

            TempData["SuccessMessage"] = "Password reset for " + trainer.FirstName + " " + trainer.LastName + ". Give them the new password directly.";
            return RedirectToAction("EditTrainer", new { id = userId });
        }

        // Saves an uploaded staff photo to Content/images/staff/{userId}.{ext} and
        // returns the app-relative URL to store on the account. Overwrites any
        // previous photo for that user (same filename each time).
        private string SaveStaffPhoto(HttpPostedFileBase photo, string userId)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var ext = System.IO.Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext)) ext = ".jpg";

            var folder = Server.MapPath("~/Content/images/staff/");
            if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);

            var fileName = userId + ext;
            var fullPath = System.IO.Path.Combine(folder, fileName);
            photo.SaveAs(fullPath);

            return "~/Content/images/staff/" + fileName;
        }

        // GET: /Admin/EquipmentBookings - all equipment bookings across all members,
        // newest first, with the member's name so admin can see who booked what.
        public async Task<ActionResult> EquipmentBookings()
        {
            var bookings = await db.EquipmentBookings
                .Include(b => b.Equipment)
                .Include(b => b.User)
                .OrderByDescending(b => b.ReservedAt)
                .Select(b => new EquipmentBookingAdminListItemViewModel
                {
                    Id = b.Id,
                    EquipmentName = b.Equipment.Name,
                    MemberName = b.User.FirstName + " " + b.User.LastName,
                    // EquipmentImageUrl/EquipmentIconClass set below (can't call a
                    // static helper method inside an EF LINQ-to-Entities projection)
                    MemberEmail = b.User.Email,
                    ReservedAt = b.ReservedAt,
                    ExpectedDurationMinutes = b.ExpectedDurationMinutes,
                    ExpectedReturnAt = b.ExpectedReturnAt,
                    ActualReturnAt = b.ActualReturnAt,
                    ReservationFeeCharged = b.ReservationFeeCharged,
                    OverageFeeCharged = b.OverageFeeCharged,
                    Status = b.Status,
                    DeclineReason = b.DeclineReason
                })
                .ToListAsync();

            foreach (var b in bookings)
            {
                b.EquipmentImageUrl = EquipmentVisuals.ImageFor(b.EquipmentName);
                b.EquipmentIconClass = EquipmentVisuals.IconFor(b.EquipmentName);
            }

            ViewBag.EquipmentList = await db.Equipment.OrderBy(e => e.Name).ToListAsync();

            return View(bookings);
        }

        // POST: /Admin/ApproveEquipmentBooking/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveEquipmentBooking(int id)
        {
            var booking = await db.EquipmentBookings.FirstOrDefaultAsync(b => b.Id == id && b.Status == "PendingApproval");
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found or already actioned.";
                return RedirectToAction("EquipmentBookings");
            }

            // Start the usage clock from the moment of approval, so an admin taking a
            // while to review doesn't eat into the member's reserved time.
            booking.Status = "Reserved";
            booking.ExpectedReturnAt = DateTime.Now.AddMinutes(booking.ExpectedDurationMinutes);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking approved.";
            return RedirectToAction("EquipmentBookings");
        }

        // POST: /Admin/DeclineEquipmentBooking/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeclineEquipmentBooking(int id, string reason)
        {
            var booking = await db.EquipmentBookings.FirstOrDefaultAsync(b => b.Id == id && b.Status == "PendingApproval");
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found or already actioned.";
                return RedirectToAction("EquipmentBookings");
            }

            booking.Status = "Declined";
            booking.DeclineReason = string.IsNullOrWhiteSpace(reason) ? "No reason given." : reason.Trim();
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking declined. The equipment is free again.";
            return RedirectToAction("EquipmentBookings");
        }

        // POST: /Admin/ConfirmEquipmentReturn/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmEquipmentReturn(int id)
        {
            var booking = await db.EquipmentBookings
                .Include(b => b.Equipment)
                .FirstOrDefaultAsync(b => b.Id == id && b.Status == "Reserved");

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found or already returned.";
                return RedirectToAction("EquipmentBookings");
            }

            var now = DateTime.Now;
            booking.ActualReturnAt = now;

            if (now > booking.ExpectedReturnAt)
            {
                var overMinutes = (now - booking.ExpectedReturnAt).TotalMinutes;
                var overHours = Math.Ceiling(overMinutes / 60.0); // charge per hour or part thereof
                booking.OverageFeeCharged = (decimal)overHours * booking.Equipment.OveragePerHourFee;
            }

            booking.Status = "Returned";
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Return confirmed." + (booking.OverageFeeCharged > 0
                ? " Overuse fee: R" + booking.OverageFeeCharged.ToString("F2") + "."
                : "");
            return RedirectToAction("EquipmentBookings");
        }

        // GET: /Admin/Analytics
        public async Task<ActionResult> Analytics()
        {
            var now = DateTime.Now;
            var model = new AnalyticsViewModel();

            // --- KPI cards ---
            model.TotalRevenueAllTime = await db.Payments.Where(p => p.Status == "Completed").SumAsync(p => (decimal?)p.Amount) ?? 0;

            var monthStart = new DateTime(now.Year, now.Month, 1);
            model.RevenueThisMonth = await db.Payments
                .Where(p => p.Status == "Completed" && p.PaymentDate >= monthStart)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            model.ActiveMembersCount = await db.MemberMemberships.CountAsync(m => m.Status == "Active" && m.EndDate > now);
            model.TotalMembersCount = await UserManager.Users.CountAsync();
            model.CheckInsThisMonth = await db.CheckIns.CountAsync(c => c.CheckInTime >= monthStart);

            var classesWithBookings = await db.GymClasses
                .Include(c => c.Bookings)
                .Where(c => c.Status != "Cancelled")
                .ToListAsync();
            if (classesWithBookings.Any())
            {
                var fillRates = classesWithBookings
                    .Where(c => c.Capacity > 0)
                    .Select(c => (double)c.Bookings.Count(b => b.Status == "Booked") / c.Capacity * 100);
                model.AverageClassFillRatePercent = fillRates.Any() ? (int)fillRates.Average() : 0;
            }

            // --- Revenue by month (last 6 months) ---
            for (var i = 5; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                var rangeStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var rangeEnd = rangeStart.AddMonths(1);
                var total = await db.Payments
                    .Where(p => p.Status == "Completed" && p.PaymentDate >= rangeStart && p.PaymentDate < rangeEnd)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;
                model.RevenueByMonth.Add(new ChartPoint { Label = rangeStart.ToString("MMM yyyy"), Value = total });
            }

            // --- Membership status breakdown ---
            // "Expired" is never stored directly - it's an Active membership whose EndDate has passed.
            var allMemberships = await db.MemberMemberships.ToListAsync();
            var activeCount = allMemberships.Count(m => m.Status == "Active" && m.EndDate > now);
            var expiredCount = allMemberships.Count(m => m.Status == "Active" && m.EndDate <= now);
            var pendingCount = allMemberships.Count(m => m.Status == "PendingPayment");
            var cancelledCount = allMemberships.Count(m => m.Status == "Cancelled");
            model.MembershipStatusBreakdown.Add(new ChartPoint { Label = "Active", Value = activeCount });
            model.MembershipStatusBreakdown.Add(new ChartPoint { Label = "Expired", Value = expiredCount });
            model.MembershipStatusBreakdown.Add(new ChartPoint { Label = "Pending Payment", Value = pendingCount });
            model.MembershipStatusBreakdown.Add(new ChartPoint { Label = "Cancelled", Value = cancelledCount });

            // --- Check-ins per day (last 14 days) ---
            for (var i = 13; i >= 0; i--)
            {
                var day = now.Date.AddDays(-i);
                var nextDay = day.AddDays(1); // computed here, not inside the query - EF can't translate AddDays() in an expression
                var count = await db.CheckIns.CountAsync(c => c.CheckInTime >= day && c.CheckInTime < nextDay);
                model.CheckInsByDay.Add(new ChartPoint { Label = day.ToString("dd MMM"), Value = count });
            }

            // --- Top 5 classes by booking count ---
            model.TopClasses = classesWithBookings
                .Select(c => new NameCountItem { Name = c.Title, Count = c.Bookings.Count(b => b.Status == "Booked") })
                .Where(x => x.Count > 0)
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // --- Top 5 equipment by reservation count ---
            var equipmentWithBookings = await db.Equipment.Include(e => e.Bookings).ToListAsync();
            model.TopEquipment = equipmentWithBookings
                .Select(e => new NameCountItem { Name = e.Name, Count = e.Bookings.Count })
                .Where(x => x.Count > 0)
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            return View(model);
        }

        // POST: /Admin/CheckOutById - close a specific open visit from the admin Check-Ins table
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckOutById(int checkInId)
        {
            var checkIn = await db.CheckIns.FindAsync(checkInId);
            if (checkIn == null)
            {
                return Json(new { success = false, message = "Check-in record not found." });
            }
            if (checkIn.CheckOutTime != null)
            {
                return Json(new { success = false, message = "This visit is already checked out." });
            }

            checkIn.CheckOutTime = DateTime.Now;
            checkIn.DurationMinutes = (int)Math.Round((checkIn.CheckOutTime.Value - checkIn.CheckInTime).TotalMinutes);
            if (checkIn.DurationMinutes < 0) checkIn.DurationMinutes = 0;

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Checked out. Duration: " + checkIn.DurationMinutes + " min." });
        }

        // POST: /Admin/ProcessManualCheckOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProcessManualCheckOut(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Invalid member selected." });
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Member not found." });
            }

            var openCheckIn = await db.CheckIns
                .Where(c => c.UserId == userId && c.CheckOutTime == null)
                .OrderByDescending(c => c.CheckInTime)
                .FirstOrDefaultAsync();

            if (openCheckIn == null)
            {
                return Json(new { success = false, message = $"{user.FirstName} {user.LastName} doesn't have an open check-in." });
            }

            openCheckIn.CheckOutTime = DateTime.Now;
            openCheckIn.DurationMinutes = (int)Math.Round((openCheckIn.CheckOutTime.Value - openCheckIn.CheckInTime).TotalMinutes);
            if (openCheckIn.DurationMinutes < 0) openCheckIn.DurationMinutes = 0;

            await db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"{user.FirstName} {user.LastName} checked out. Duration: {openCheckIn.DurationMinutes} min."
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
                    UserId = c.UserId,
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
            var wasPending = membership.Status == "PendingPayment";
            membership.Status = "Active";

            // If membership is pending, set start date to now
            if (wasPending)
            {
                membership.StartDate = DateTime.Now;
                membership.EndDate = SemesterCalendar.CalculateEndDateForPlan(DateTime.Now, membership.MembershipPlan.DurationInMonths);
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
            membership.EndDate = SemesterCalendar.CalculateEndDateForPlan(DateTime.Now, membership.MembershipPlan.DurationInMonths);

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