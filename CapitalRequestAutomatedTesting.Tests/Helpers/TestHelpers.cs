using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalRequestAutomatedTesting.Tests.Helpers
{

    public static class TestHelpers
    {
        public static void AssertTimestampsClose(DateTime? expected, DateTime? actual, double maxDifferenceMinutes = 10)
        {
            if (expected == null && actual == null)
            {
                return; // Both are null, considered equal
            }

            Assert.True(expected.HasValue && actual.HasValue,
                $"One of the timestamps is null. Expected: {expected}, Actual: {actual}");

            var difference = Math.Abs((expected.Value - actual.Value).TotalMinutes);
            Assert.True(
                difference <= maxDifferenceMinutes,
                $"Timestamps differ by more than {maxDifferenceMinutes} minutes. Expected: {expected}, Actual: {actual}"
            );
        }
    }


}
