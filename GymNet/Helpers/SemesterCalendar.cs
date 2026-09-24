using System;
using System.Globalization;
using System.Linq;

namespace GymNet.Helpers
{
    /// <summary>
    /// One place for all semester rules used by memberships.
    ///   Semester 1: sign-up Feb - Mar, ends 30 June
    ///   Semester 2: sign-up Jul - Aug, ends 30 November
    /// Change the constants below if the dates change.
    /// </summary>
    public static class SemesterCalendar
    {
        // ===== EDIT THESE IF THE DATES CHANGE ==================================
        public const int Sem1SignupFirstMonth = 2;   // February
        public const int Sem1SignupLastMonth = 3;    // March
        public const int Sem1EndMonth = 6;           // June (ends on last day of month)

        public const int Sem2SignupFirstMonth = 7;   // July
        public const int Sem2SignupLastMonth = 8;    // August
        public const int Sem2EndMonth = 11;          // November (ends on last day of month)
        // =======================================================================

        private const int MonthsPerSemester = 6;

        /// <summary>True only during the sign-up months (Feb-Mar and Jul-Aug).</summary>
        public static bool IsSignupOpen(DateTime date)
        {
            return (date.Month >= Sem1SignupFirstMonth && date.Month <= Sem1SignupLastMonth)
                || (date.Month >= Sem2SignupFirstMonth && date.Month <= Sem2SignupLastMonth);
        }

        /// <summary>Last moment (23:59:59) of the semester that the given date falls in.</summary>
        public static DateTime CurrentSemesterEnd(DateTime date)
        {
            if (date.Month <= Sem1EndMonth) return EndOfMonth(date.Year, Sem1EndMonth);
            if (date.Month <= Sem2EndMonth) return EndOfMonth(date.Year, Sem2EndMonth);
            return EndOfMonth(date.Year + 1, Sem1EndMonth); // Dec: next semester 1
        }

        /// <summary>
        /// Membership end date. A plan of 1-6 months lasts until the end of the current
        /// semester, a 12-month plan until the end of the following semester, etc.
        /// </summary>
        public static DateTime CalculateEndDate(DateTime start, int durationInMonths)
        {
            var end = CurrentSemesterEnd(start);
            var semesters = Math.Max(1, (int)Math.Ceiling(durationInMonths / (double)MonthsPerSemester));

            for (var i = 1; i < semesters; i++)
            {
                end = end.Month == Sem1EndMonth
                    ? EndOfMonth(end.Year, Sem2EndMonth)
                    : EndOfMonth(end.Year + 1, Sem1EndMonth);
            }

            return end;
        }

        /// <summary>Whole days left until the end date (0 on the last day, never negative).</summary>
        public static int DaysRemaining(DateTime endDate)
        {
            return Math.Max(0, (endDate.Date - DateTime.Today).Days);
        }

        /// <summary>The next day sign-up opens (used in the "locked" message).</summary>
        public static DateTime NextSignupOpens(DateTime date)
        {
            var candidates = new[]
            {
                new DateTime(date.Year, Sem1SignupFirstMonth, 1),
                new DateTime(date.Year, Sem2SignupFirstMonth, 1),
                new DateTime(date.Year + 1, Sem1SignupFirstMonth, 1)
            };
            return candidates.First(c => c > date.Date);
        }

        /// <summary>Message shown when packages are locked; null when sign-up is open.</summary>
        public static string LockedMessage(DateTime date)
        {
            if (IsSignupOpen(date)) return null;

            var next = NextSignupOpens(date);
            var semester = next.Month == Sem1SignupFirstMonth ? "Semester 1" : "Semester 2";
            return "Membership sign-up is closed because the semester has already started. " +
                   semester + " packages open on " + next.ToString("d MMMM yyyy", CultureInfo.InvariantCulture) + ".";
        }

        /// <summary>
        /// A plan of 1 month or less (the Monthly plan) is available all year round and
        /// always lasts 30 days from purchase - it isn't tied to the semester calendar.
        /// Longer plans (Semester, Annual) are locked to the sign-up windows above.
        /// </summary>
        public static bool IsPlanLocked(int durationInMonths, DateTime date)
        {
            if (durationInMonths <= 1) return false;
            return !IsSignupOpen(date);
        }

        /// <summary>End date for a plan, honouring the Monthly-plan exception above.</summary>
        public static DateTime CalculateEndDateForPlan(DateTime start, int durationInMonths)
        {
            if (durationInMonths <= 1) return start.AddDays(30);
            return CalculateEndDate(start, durationInMonths);
        }

        /// <summary>e.g. "Semester 1: February - March | Semester 2: July - August"</summary>
        public static string SignupWindowsText()
        {
            var months = CultureInfo.InvariantCulture.DateTimeFormat;
            return "Semester 1: " + months.GetMonthName(Sem1SignupFirstMonth) + " - " + months.GetMonthName(Sem1SignupLastMonth) +
                   " | Semester 2: " + months.GetMonthName(Sem2SignupFirstMonth) + " - " + months.GetMonthName(Sem2SignupLastMonth);
        }

        private static DateTime EndOfMonth(int year, int month)
        {
            return new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
        }
    }
}
