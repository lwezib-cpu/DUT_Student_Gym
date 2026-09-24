using System;

namespace GymNet.Helpers
{
    /// <summary>
    /// Reward badges based on total gym check-ins. Computed on the fly from the
    /// CheckIns table - no database table of its own, so tiers can be changed here
    /// any time without a migration.
    /// </summary>
    public static class BadgeCalculator
    {
        public class BadgeResult
        {
            public string Name { get; set; }
            public string CssClass { get; set; } // Bootstrap color class
            public string Icon { get; set; }      // Bootstrap icon class
            public int Threshold { get; set; }
            public int NextThreshold { get; set; } // 0 = already at the top tier
            public int ProgressPercent { get; set; }
        }

        private static readonly (int Threshold, string Name, string CssClass, string Icon)[] Tiers =
        {
            (0,   "Newcomer",  "bg-secondary", "bi-person-walking"),
            (5,   "Bronze",    "bg-danger",    "bi-award"),
            (20,  "Silver",    "bg-secondary", "bi-award-fill"),
            (50,  "Gold",      "bg-warning",   "bi-trophy"),
            (100, "Platinum",  "bg-dark",      "bi-trophy-fill"),
        };

        public static BadgeResult ForCheckInCount(int totalCheckIns)
        {
            var current = Tiers[0];
            var next = (int?)null;

            for (var i = 0; i < Tiers.Length; i++)
            {
                if (totalCheckIns >= Tiers[i].Threshold)
                {
                    current = Tiers[i];
                    next = i + 1 < Tiers.Length ? Tiers[i + 1].Threshold : (int?)null;
                }
            }

            var progress = 100;
            if (next.HasValue)
            {
                var span = next.Value - current.Threshold;
                var into = totalCheckIns - current.Threshold;
                progress = span > 0 ? Math.Min(100, Math.Max(0, (int)((double)into / span * 100))) : 100;
            }

            return new BadgeResult
            {
                Name = current.Name,
                CssClass = current.CssClass,
                Icon = current.Icon,
                Threshold = current.Threshold,
                NextThreshold = next ?? 0,
                ProgressPercent = progress
            };
        }
    }
}
