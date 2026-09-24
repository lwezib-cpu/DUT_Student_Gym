using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymNet.ViewModels
{
    public class CreateClassViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Date & Time")]
        public DateTime StartTime { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        [Range(15, 240, ErrorMessage = "Duration must be between 15 and 240 minutes")]
        [Display(Name = "Duration (minutes)")]
        public int DurationMinutes { get; set; } = 60;

        [Required]
        [Range(1, 100)]
        public int Capacity { get; set; } = 15;
    }

    public class ClassListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TrainerName { get; set; }
        public string TrainerPhotoUrl { get; set; }
        public string TrainerSpecialty { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public int Capacity { get; set; }
        public int BookedCount { get; set; }
        public string Status { get; set; }

        // Member-facing
        public bool IsBookedByCurrentUser { get; set; }
        public bool IsWaitlistedByCurrentUser { get; set; }
        public int WaitlistCount { get; set; }
        public bool IsFull => BookedCount >= Capacity;
    }

    public class TrainerDashboardViewModel
    {
        public List<ClassListItemViewModel> UpcomingClasses { get; set; } = new List<ClassListItemViewModel>();
        public List<ClassListItemViewModel> PastClasses { get; set; } = new List<ClassListItemViewModel>();
        public int TotalMembersCoached { get; set; }
        public int TotalFeedbackGiven { get; set; }
    }

    public class GiveFeedbackViewModel
    {
        public string MemberId { get; set; }
        public string MemberName { get; set; }

        [Required(ErrorMessage = "Please write a comment")]
        [StringLength(1000)]
        [Display(Name = "Feedback")]
        public string Comment { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } = 5;
    }

    public class TrainerFeedbackListItemViewModel
    {
        public string TrainerName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TrainerMemberListItemViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
