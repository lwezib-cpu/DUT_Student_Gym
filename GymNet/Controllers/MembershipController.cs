using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using GymNet.Data;
using GymNet.Models;
using GymNet.ViewModels;

namespace GymNet.Controllers
{
    [Authorize(Roles = "Member")]
    public class MembershipController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        #region Membership Plans

        // GET: /Membership/Plans
        [AllowAnonymous]
        public ActionResult Plans()
        {
            var plans = db.MembershipPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .Select(p => new MembershipPlanViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DurationInMonths = p.DurationInMonths,
                    IsActive = p.IsActive
                })
                .ToList();

            return View(plans);
        }

        // GET: /Membership/SelectPlan/{id}
        public async Task<ActionResult> SelectPlan(int id)
        {
            var plan = await db.MembershipPlans.FindAsync(id);
            if (plan == null || !plan.IsActive)
            {
                TempData["ErrorMessage"] = "Selected membership plan is not available.";
                return RedirectToAction("Plans");
            }

            // Check if user already has an active membership
            var userId = User.Identity.GetUserId();
            var hasActiveMembership = await db.MemberMemberships
                .AnyAsync(m => m.UserId == userId && m.Status == "Active" && m.EndDate > DateTime.Now);

            if (hasActiveMembership)
            {
                TempData["ErrorMessage"] = "You already have an active membership.";
                return RedirectToAction("MyMembership");
            }

            var model = new SelectMembershipViewModel
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                Price = plan.Price,
                DurationInMonths = plan.DurationInMonths
            };

            return View(model);
        }

        #endregion

        #region Payment Processing

        // GET: /Membership/Payment/{planId}
        public async Task<ActionResult> Payment(int planId)
        {
            var plan = await db.MembershipPlans.FindAsync(planId);
            if (plan == null || !plan.IsActive)
            {
                TempData["ErrorMessage"] = "Invalid membership plan.";
                return RedirectToAction("Plans");
            }

            var model = new PaymentViewModel
            {
                MembershipPlanId = plan.Id
            };

            ViewBag.PlanName = plan.Name;
            ViewBag.Price = plan.Price;
            ViewBag.DurationInMonths = plan.DurationInMonths;

            return View(model);
        }

        // POST: /Membership/Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Payment(PaymentViewModel model)
        {
            // Validate based on payment method
            ValidatePaymentMethod(model);

            if (!ModelState.IsValid)
            {
                var plan = await db.MembershipPlans.FindAsync(model.MembershipPlanId);
                ViewBag.PlanName = plan?.Name;
                ViewBag.Price = plan?.Price;
                ViewBag.DurationInMonths = plan?.DurationInMonths;
                return View(model);
            }

            var planInfo = await db.MembershipPlans.FindAsync(model.MembershipPlanId);
            if (planInfo == null)
            {
                return HttpNotFound();
            }

            // Generate a demo transaction reference
            var transactionRef = GenerateTransactionReference();

            // Create the membership record
            var membership = new MemberMembership
            {
                UserId = User.Identity.GetUserId(),
                MembershipPlanId = model.MembershipPlanId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(planInfo.DurationInMonths),
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            db.MemberMemberships.Add(membership);

            // Create payment record with method-specific details
            var payment = new Payment
            {
                MemberMembership = membership,
                Amount = planInfo.Price,
                PaymentMethod = model.PaymentMethod,
                Status = "Completed",
                PaymentDate = DateTime.Now,
                TransactionReference = transactionRef
            };

            // Set method-specific fields
            if (model.PaymentMethod == "Credit Card" || model.PaymentMethod == "Debit Card")
            {
                payment.CardHolderName = model.CardHolderName;
                payment.CardNumber = MaskCardNumber(model.CardNumber.Replace(" ", ""));
                payment.ExpiryDate = model.ExpiryDate;
                payment.BankName = GetCardBankName(model.CardNumber);
            }
            else if (model.PaymentMethod == "EFT")
            {
                payment.BankName = model.BankName;
                payment.AccountNumber = MaskAccountNumber(model.AccountNumber);
                payment.CardHolderName = model.AccountHolderName;
                payment.AccountType = model.AccountType;
                payment.BranchCode = model.BranchCode;
            }

            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payment successful! Your {planInfo.Name} is now active. Transaction reference: {transactionRef}";
            return RedirectToAction("PaymentConfirmation", new { membershipId = membership.Id });
        }

        // GET: /Membership/PaymentConfirmation/{membershipId}
        public async Task<ActionResult> PaymentConfirmation(int membershipId)
        {
            var userId = User.Identity.GetUserId();

            var membership = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .FirstOrDefaultAsync(m => m.Id == membershipId && m.UserId == userId);

            if (membership == null)
            {
                return HttpNotFound();
            }

            return View(membership);
        }

        #endregion

        #region Membership Management

        // GET: /Membership/MyMembership
        public async Task<ActionResult> MyMembership()
        {
            var userId = User.Identity.GetUserId();

            var membership = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            if (membership == null)
            {
                return RedirectToAction("Plans");
            }

            var model = new MemberMembershipViewModel
            {
                Id = membership.Id,
                PlanName = membership.MembershipPlan.Name,
                PlanDescription = membership.MembershipPlan.Description,
                Price = membership.MembershipPlan.Price,
                DurationInMonths = membership.MembershipPlan.DurationInMonths,
                StartDate = membership.StartDate,
                EndDate = membership.EndDate,
                Status = membership.Status,
                DaysRemaining = (membership.EndDate - DateTime.Now).Days
            };

            return View(model);
        }

        #endregion

        #region Payment History & Receipts

        // GET: /Membership/PaymentHistory
        public async Task<ActionResult> PaymentHistory()
        {
            var userId = User.Identity.GetUserId();

            var payments = await db.Payments
                .Where(p => p.MemberMembership.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentHistoryViewModel
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    TransactionReference = p.TransactionReference,
                    CardHolderName = p.CardHolderName,
                    MaskedCardNumber = p.CardNumber,
                    BankName = p.BankName,
                    MembershipPlanName = p.MemberMembership.MembershipPlan.Name,
                    MembershipStartDate = p.MemberMembership.StartDate,
                    MembershipEndDate = p.MemberMembership.EndDate
                })
                .ToListAsync();

            return View(payments);
        }

        // GET: /Membership/Receipt/{paymentId}
        public async Task<ActionResult> Receipt(int paymentId)
        {
            var receipt = await GetReceiptViewModel(paymentId);

            if (receipt == null)
            {
                TempData["ErrorMessage"] = "Receipt not found.";
                return RedirectToAction("PaymentHistory");
            }

            return View(receipt);
        }

        // GET: /Membership/PrintReceipt/{paymentId}
        public async Task<ActionResult> PrintReceipt(int paymentId)
        {
            var receipt = await GetReceiptViewModel(paymentId);

            if (receipt == null)
            {
                return HttpNotFound();
            }

            // Return a view optimized for printing
            return View("Receipt", receipt);
        }

        #endregion

        #region Helper Methods

        // Validate payment method specific fields
        private void ValidatePaymentMethod(PaymentViewModel model)
        {
            // First, remove any existing validation errors for fields we'll validate manually
            foreach (var key in ModelState.Keys.ToList())
            {
                if (key != "MembershipPlanId" && key != "PaymentMethod")
                {
                    ModelState.Remove(key);
                }
            }

            if (model.PaymentMethod == "Credit Card" || model.PaymentMethod == "Debit Card")
            {
                // Validate card fields
                if (string.IsNullOrWhiteSpace(model.CardHolderName))
                {
                    ModelState.AddModelError("CardHolderName", "Card holder name is required");
                }

                if (string.IsNullOrWhiteSpace(model.CardNumber))
                {
                    ModelState.AddModelError("CardNumber", "Card number is required");
                }
                else
                {
                    var cleanCardNumber = model.CardNumber.Replace(" ", "");
                    if (!IsValidCardNumber(cleanCardNumber))
                    {
                        ModelState.AddModelError("CardNumber", "Please enter a valid 16-digit card number");
                    }
                }

                if (string.IsNullOrWhiteSpace(model.ExpiryDate))
                {
                    ModelState.AddModelError("ExpiryDate", "Expiry date is required");
                }
                else if (!IsValidExpiryDate(model.ExpiryDate))
                {
                    ModelState.AddModelError("ExpiryDate", "Card has expired or invalid date");
                }

                if (string.IsNullOrWhiteSpace(model.CVV))
                {
                    ModelState.AddModelError("CVV", "CVV is required");
                }
                else if (model.CVV.Length < 3 || model.CVV.Length > 4)
                {
                    ModelState.AddModelError("CVV", "CVV must be 3 or 4 digits");
                }
            }
            else if (model.PaymentMethod == "EFT")
            {
                // Validate EFT fields
                if (string.IsNullOrWhiteSpace(model.BankName))
                {
                    ModelState.AddModelError("BankName", "Bank name is required");
                }

                if (string.IsNullOrWhiteSpace(model.AccountHolderName))
                {
                    ModelState.AddModelError("AccountHolderName", "Account holder name is required");
                }

                if (string.IsNullOrWhiteSpace(model.AccountNumber))
                {
                    ModelState.AddModelError("AccountNumber", "Account number is required");
                }
                else if (model.AccountNumber.Length < 8 || model.AccountNumber.Length > 20)
                {
                    ModelState.AddModelError("AccountNumber", "Account number must be between 8 and 20 digits");
                }

                if (string.IsNullOrWhiteSpace(model.BranchCode))
                {
                    ModelState.AddModelError("BranchCode", "Branch code is required");
                }
                else if (model.BranchCode.Length != 6)
                {
                    ModelState.AddModelError("BranchCode", "Branch code must be 6 digits");
                }

                if (string.IsNullOrWhiteSpace(model.AccountType))
                {
                    ModelState.AddModelError("AccountType", "Account type is required");
                }
            }
            else
            {
                ModelState.AddModelError("PaymentMethod", "Please select a valid payment method");
            }
        }

        // Private helper to get receipt view model
        private async Task<ReceiptViewModel> GetReceiptViewModel(int paymentId)
        {
            try
            {
                var userId = User.Identity.GetUserId();

                var payment = await db.Payments
                    .Include(p => p.MemberMembership)
                    .Include(p => p.MemberMembership.MembershipPlan)
                    .Include(p => p.MemberMembership.User)
                    .FirstOrDefaultAsync(p => p.Id == paymentId && p.MemberMembership.UserId == userId);

                if (payment == null)
                {
                    return null;
                }

                var member = payment.MemberMembership.User;

                return new ReceiptViewModel
                {
                    PaymentId = payment.Id,
                    TransactionReference = payment.TransactionReference,
                    PaymentDate = payment.PaymentDate,
                    MembershipPlanName = payment.MemberMembership.MembershipPlan.Name,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.Status,
                    BankName = payment.BankName,
                    CardHolderName = payment.CardHolderName,
                    MaskedCardNumber = payment.CardNumber,
                    AccountNumber = payment.AccountNumber,
                    AccountType = payment.AccountType,
                    BranchCode = payment.BranchCode,
                    MembershipStartDate = payment.MemberMembership.StartDate,
                    MembershipEndDate = payment.MemberMembership.EndDate,
                    MemberName = $"{member.FirstName} {member.LastName}",
                    MemberEmail = member.Email
                };
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error retrieving receipt: {ex.Message}");
                return null;
            }
        }

        // Generate transaction reference
        private string GenerateTransactionReference()
        {
            return "GYM-" + DateTime.Now.ToString("yyyyMMdd") + "-" +
                   Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        // Mask card number
        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
            {
                return cardNumber;
            }

            return "**** **** **** " + cardNumber.Substring(cardNumber.Length - 4);
        }

        // Mask account number
        private string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
            {
                return accountNumber;
            }

            return "****" + accountNumber.Substring(accountNumber.Length - 4);
        }

        // Validate card number using Luhn algorithm
        private bool IsValidCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length != 16)
                return false;

            if (!cardNumber.All(char.IsDigit))
                return false;

            int sum = 0;
            bool isEven = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';

                if (isEven)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                isEven = !isEven;
            }

            return (sum % 10 == 0);
        }

        // Validate expiry date
        private bool IsValidExpiryDate(string expiryDate)
        {
            if (string.IsNullOrEmpty(expiryDate) || expiryDate.Length != 5)
                return false;

            var parts = expiryDate.Split('/');
            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
                return false;

            if (month < 1 || month > 12)
                return false;

            // Convert YY to YYYY
            year += 2000;

            var expiryDateTime = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            return expiryDateTime >= DateTime.Now;
        }

        // Get card bank name from card number
        private string GetCardBankName(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return "Unknown";

            var cleanNumber = cardNumber.Replace(" ", "");

            // Detect card type from number prefix
            if (cleanNumber.StartsWith("4"))
                return "Visa";
            else if (cleanNumber.StartsWith("51") || cleanNumber.StartsWith("52") ||
                     cleanNumber.StartsWith("53") || cleanNumber.StartsWith("54") ||
                     cleanNumber.StartsWith("55"))
                return "Mastercard";
            else if (cleanNumber.StartsWith("34") || cleanNumber.StartsWith("37"))
                return "American Express";
            else if (cleanNumber.StartsWith("6011"))
                return "Discover";
            else if (cleanNumber.StartsWith("36"))
                return "Diners Club";
            else
                return "Unknown";
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}