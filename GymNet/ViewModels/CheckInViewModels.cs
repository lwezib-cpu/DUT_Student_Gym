using System;
using System.Collections.Generic;

namespace GymNet.ViewModels
{
    public class CheckInViewModel
    {
        public string QRCodeContent { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string MembershipStatus { get; set; }
        public string MembershipPlan { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public bool CanCheckIn { get; set; }
        public string Message { get; set; }
    }

    public class CheckInResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime? CheckInTime { get; set; }
    }

    public class CheckInHistoryViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime CheckInTime { get; set; }
        public string CheckInMethod { get; set; }
        public string QRCode { get; set; }
    }

    public class AdminCheckInViewModel
    {
        public List<CheckInHistoryViewModel> TodayCheckIns { get; set; }
        public List<CheckInHistoryViewModel> AllCheckIns { get; set; }
        public int TotalCheckInsToday { get; set; }
        public int TotalCheckInsAllTime { get; set; }
        public int UniqueMembersToday { get; set; }
    }

    public class GenerateQRCodeViewModel
    {
        public string QRCodeContent { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}