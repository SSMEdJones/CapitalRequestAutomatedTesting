using OpenQA.Selenium;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public static class SeleniumRetryHelper
    {
        public static async Task<T> RetryAsync<T>(
            Func<Task<T>> action,
            int maxRetries = 3,
            TimeSpan? delayBetweenRetries = null)
        {
            delayBetweenRetries ??= TimeSpan.FromSeconds(1);
            Exception lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Debug.WriteLine($"Retry attempt {attempt} failed: {ex.Message}");

                    if (attempt < maxRetries)
                        await Task.Delay(delayBetweenRetries.Value);
                }
            }

            throw new Exception($"All {maxRetries} retry attempts failed.", lastException);
        }
    }


}
