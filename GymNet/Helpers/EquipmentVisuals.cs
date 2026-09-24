using System;
using System.Collections.Generic;

namespace GymNet.Helpers
{
    /// <summary>
    /// One shared place for how each piece of equipment is shown visually
    /// (real photo, or an icon/animation fallback when no photo exists).
    /// Used by both the member-facing Equipment pages and the admin
    /// Equipment Bookings page, so they never drift out of sync.
    /// </summary>
    public static class EquipmentVisuals
    {
        // Real uploaded photos - see Content/images/equipment/. Matched by exact
        // equipment Name from the seed data in Migrations/Configuration.cs.
        private static readonly Dictionary<string, string> ImageByName = new Dictionary<string, string>
        {
            { "Bench Press Station", "~/Content/images/equipment/bench-press-station.png" },
            { "Dumbbell Set", "~/Content/images/equipment/dumbbell-set.png" },
            { "Kettlebell Set", "~/Content/images/equipment/kettlebell-set.png" },
            { "Rowing Machine", "~/Content/images/equipment/rowing-machine.png" },
            { "Squat Rack", "~/Content/images/equipment/squat-rack.png" },
            { "Treadmill", "~/Content/images/equipment/treadmill.png" },
        };

        public static string ImageFor(string name)
        {
            return ImageByName.TryGetValue(name ?? "", out var url) ? url : null;
        }

        // Fill this in with real LottieFiles animation URLs once you have them.
        private static readonly Dictionary<string, string> LottieByName = new Dictionary<string, string>
        {
        };

        public static string LottieFor(string name)
        {
            return LottieByName.TryGetValue(name ?? "", out var url) ? url : null;
        }

        // Cosmetic fallback icon, used only when a piece of equipment has no photo yet.
        public static string IconFor(string name)
        {
            var n = (name ?? "").ToLowerInvariant();
            if (n.Contains("treadmill")) return "bi-activity";
            if (n.Contains("row")) return "bi-water";
            if (n.Contains("squat") || n.Contains("rack") || n.Contains("bench")) return "bi-dash-lg";
            if (n.Contains("dumbbell") || n.Contains("kettlebell")) return "bi-record-circle";
            return "bi-gear-fill";
        }

        private static readonly string[] AnimationStyles = { "icon-anim-bounce", "icon-anim-pulse", "icon-anim-sway" };

        public static string AnimationFor(string name)
        {
            var index = Math.Abs((name ?? "").GetHashCode()) % AnimationStyles.Length;
            return AnimationStyles[index];
        }
    }
}
