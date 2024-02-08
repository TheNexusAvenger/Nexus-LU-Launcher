using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Util;

namespace Nexus.LU.Launcher.Gui.Util;

public class CombinedOutput
{
    /// <summary>
    /// Current output.
    /// </summary>
    public string Output { get; private set; } = "";
    
    /// <summary>
    /// Event for the output updating.
    /// </summary>
    public event Action<string>? OutputUpdated;

    /// <summary>
    /// Semaphore for adding text.
    /// </summary>
    private readonly SemaphoreSlim outputSemaphore = new SemaphoreSlim(1);

    private async Task AddLineAsync(string line)
    {
        await this.outputSemaphore.WaitAsync();
        this.Output += (this.Output == "" ? "" : "\n") + line;
        OutputUpdated?.Invoke(this.Output);
        this.outputSemaphore.Release();
    }

    /// <summary>
    /// Adds an output.
    /// </summary>
    /// <param name="reader">Reader to read the output from.</param>
    /// <param name="cancellationToken">Cancellation token to request stopping reads.</param>
    public void AddOutput(StreamReader reader, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line == null) continue;
                await this.AddLineAsync(line);
            }
        }, cancellationToken);
    }
    
    /// <summary>
    /// Adds an output.
    /// </summary>
    /// <param name="storedLogOutput">Stored log output to read.</param>
    public void AddOutput(StoredLogOutput storedLogOutput)
    {
        Task.Run(async () =>
        {
            // Read the existing messages.
            while (true)
            {
                var line = await storedLogOutput.GetNextLineAsync();
                if (line == null) break;
                await this.AddLineAsync(line);
            }

            // Connect new lines being added.
            storedLogOutput.MessageAdded += async () =>
            {
                while (true)
                {
                    var line = await storedLogOutput.GetNextLineAsync();
                    if (line == null) break;
                    await this.AddLineAsync(line);
                }
            };
        });
    }
}