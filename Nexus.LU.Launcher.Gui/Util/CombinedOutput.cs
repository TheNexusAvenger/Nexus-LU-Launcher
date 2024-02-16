using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Util;

namespace Nexus.LU.Launcher.Gui.Util;

public class CombinedOutput
{
    /// <summary>
    /// Current output.
    /// </summary>
    public string Output => this.outputStringBuilder.ToString();
    
    /// <summary>
    /// Event for the output updating.
    /// </summary>
    public event Action<string>? OutputUpdated;

    /// <summary>
    /// Semaphore for adding text.
    /// </summary>
    private readonly SemaphoreSlim outputSemaphore = new SemaphoreSlim(1);
    
    /// <summary>
    /// Current output string builder.
    /// </summary>
    private readonly StringBuilder outputStringBuilder = new StringBuilder();

    /// <summary>
    /// Adds a line.
    /// </summary>
    /// <param name="line">Line to add.</param>
    private async Task AddLineAsync(string line)
    {
        await this.outputSemaphore.WaitAsync();
        this.outputStringBuilder.Append((outputStringBuilder.Length == 0 ? "" : "\n") + line);
        OutputUpdated?.Invoke(this.outputStringBuilder.ToString());
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
            // Read the existing messages in bulk.
            await this.outputSemaphore.WaitAsync();
            while (true)
            {
                var line = await storedLogOutput.GetNextLineAsync();
                if (line == null) break;
                this.outputStringBuilder.Append((outputStringBuilder.Length == 0 ? "" : "\n") + line);
            }
            OutputUpdated?.Invoke(this.outputStringBuilder.ToString());
            this.outputSemaphore.Release();

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