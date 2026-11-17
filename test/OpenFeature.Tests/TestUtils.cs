internal class Utils
{
    /// <summary>
    /// Awaits the supplied task, throwing a <see cref="TimeoutException"/> when it does not complete within the timeout.
    /// </summary>
    /// <param name="task">Task to await</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="timeout">Timeout to wait for the task to complete</param>
    public static async Task WaitOrTimeoutAsync(Task task, CancellationToken cancellationToken, TimeSpan timeout)
    {
        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            cts.CancelAfter(timeout);

            if (await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cts.Token)).ConfigureAwait(false) != task)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new TimeoutException($"Timed out after {timeout} waiting for the task to complete.");
            }

            cts.Cancel();
        }

        await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the supplied task, throwing a <see cref="TimeoutException"/> when it does not complete within the timeout.
    /// </summary>
    /// <param name="task">Task to await</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="timeout">Timeout to wait for the task to complete</param>
    /// <returns>The result of the task</returns>
    public static async Task<T> WaitOrTimeoutAsync<T>(Task<T> task, CancellationToken cancellationToken, TimeSpan timeout)
    {
        await WaitOrTimeoutAsync((Task)task, cancellationToken, timeout).ConfigureAwait(false);
        return await task.ConfigureAwait(false);
    }

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
