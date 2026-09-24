using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace GymNet.ViewModels
{
    /// <summary>
    /// Validates a DUT student email: studentnumber@dut4life.ac.za
    /// (student number = 7 to 9 digits).
    /// </summary>
    public class StudentEmailAttribute : ValidationAttribute
    {
        public const string Domain = "@dut4life.ac.za";

        private static readonly Regex Pattern =
            new Regex(@"^\d{7,9}@dut4life\.ac\.za$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public StudentEmailAttribute()
            : base("Use your DUT student email: studentnumber@dut4life.ac.za")
        {
        }

        public override bool IsValid(object value)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text)) return true; // [Required] handles empty values
            return Pattern.IsMatch(text.Trim());
        }

        /// <summary>
        /// Trims, lower-cases and, if only a student number was typed,
        /// appends the DUT student email domain.
        /// </summary>
        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            var text = input.Trim().ToLowerInvariant();
            if (text.All(char.IsDigit)) text += Domain;
            return text;
        }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Student email is required")]
        [StudentEmail]
        [Display(Name = "Student Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        // Students log in with their student number or full student email.
        // Admin/trainer accounts can still use their normal email address.
        [Required(ErrorMessage = "Student number or email is required")]
        [Display(Name = "Student Number or Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}