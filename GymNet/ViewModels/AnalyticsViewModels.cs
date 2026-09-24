using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Generic;

namespace GymNet.ViewModels
{
    public class ChartPoint
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }

    public class NameCountItem
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class AnalyticsViewModel
    {
        // KPI cards
        public decimal TotalRevenueAllTime { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int ActiveMembersCount { get; set; }
        public int TotalMembersCount { get; set; }
        public int CheckInsThisMonth { get; set; }
        public int AverageClassFillRatePercent { get; set; }

        // Charts
        public List<ChartPoint> RevenueByMonth { get; set; } = new List<ChartPoint>();
        public List<ChartPoint> MembershipStatusBreakdown { get; set; } = new List<ChartPoint>();
        public List<ChartPoint> CheckInsByDay { get; set; } = new List<ChartPoint>();
        public List<NameCountItem> TopClasses { get; set; } = new List<NameCountItem>();
        public List<NameCountItem> TopEquipment { get; set; } = new List<NameCountItem>();
    }
}