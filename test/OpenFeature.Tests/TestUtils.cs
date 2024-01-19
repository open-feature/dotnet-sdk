using System;
using System.Diagnostics;
using System.Threading.Tasks;

internal class Utils
{
    /// <summary>
    /// Repeatedly runs the supplied assertion until it doesn't throw, or the timeout is reached.
    /// </summary>
    /// <param name="assertionFunc">Function which makes an assertion</param>
    /// <param name="timeoutMillis">Timeout in millis (defaults to 1000)</param>
    /// <param name="pollIntervalMillis">Poll interval (defaults to 100</param>
    /// <returns></returns>
    public static async Task AssertUntilAsync(Action assertionFunc, int timeoutMillis = 1000, int pollIntervalMillis = 100)
    {
        Exception lastException;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        do
        {
            try
            {
                assertionFunc();
                return;
            }
            catch (Exception e)
            {
                lastException = e;
            }
            finally
            {
                await Task.Delay(pollIntervalMillis).ConfigureAwait(false);
            }
        }
        while (stopwatch.Elapsed.TotalMilliseconds < timeoutMillis);
        throw lastException;
    }
}
