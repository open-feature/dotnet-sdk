using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
    public static async Task AssertUntilAsync(Action<CancellationToken> assertionFunc, int timeoutMillis = 1000, int pollIntervalMillis = 100)
    {
        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(default(CancellationToken)))
        {
            
            cts.CancelAfter(timeoutMillis);

            var exceptions = new List<Exception>();
            var message = "AssertUntilAsync timeout reached.";

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    assertionFunc(cts.Token);
                    return;
                }
                catch (TaskCanceledException) when (cts.IsCancellationRequested)
                {
                    throw new AggregateException(message, exceptions);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }

                try
                {
                    await Task.Delay(pollIntervalMillis, cts.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    throw new AggregateException(message, exceptions);
                }
            }
            throw new AggregateException(message, exceptions);
        }
    }
}
