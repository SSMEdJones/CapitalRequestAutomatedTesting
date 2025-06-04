namespace CapitalRequestAutomatedTesting.UI.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns true if the two DateTime values are within the specified number of minutes of each other.
        /// </summary>
        public static bool IsFuzzyMatch(this DateTime dt1, DateTime dt2, int minuteTolerance = 1)
        {
            var diff = Math.Abs((dt1 - dt2).TotalMinutes);
            return diff <= minuteTolerance;
        }
    }

}
