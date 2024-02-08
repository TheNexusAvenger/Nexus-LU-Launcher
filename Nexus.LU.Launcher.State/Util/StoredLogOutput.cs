using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Logging.Entry;
using Nexus.Logging.Output;

namespace Nexus.LU.Launcher.State.Util;

public class StoredLogOutput : QueuedOutput
{
    /// <summary>
    /// Maximum line width to show in the logs before wrapping.
    /// </summary>
    public const int MaxLineWidth = 88;

    /// <summary>
    /// Event for a message being added.
    /// </summary>
    public event Action? MessageAdded;

    /// <summary>
    /// Messages that have not been read.
    /// </summary>
    private readonly Queue<string> storedMessages = new Queue<string>();

    /// <summary>
    /// Semaphore for reading and writing messages.
    /// </summary>
    private readonly SemaphoreSlim storedMessagesSemaphore = new SemaphoreSlim(1);
    
    /// <summary>
    /// Static instance of a stored log output.
    /// </summary>
    public static readonly StoredLogOutput Instance = new StoredLogOutput();
    
    /// <summary>
    /// Processes a message.
    /// </summary>
    /// <param name="entry">Entry to process</param>
    public override async Task ProcessMessage(LogEntry entry)
    {
        await this.storedMessagesSemaphore.WaitAsync();
        foreach (var line in entry.GetLines(MaxLineWidth))
        {
            this.storedMessages.Enqueue(line);
        }
        this.storedMessagesSemaphore.Release();
        this.MessageAdded?.Invoke();
    }

    /// <summary>
    /// Returns the next stored line, if it exists.
    /// </summary>
    /// <returns>The next stored line in the queue.</returns>
    public async Task<string?> GetNextLineAsync()
    {
        await this.storedMessagesSemaphore.WaitAsync();
        this.storedMessages.TryDequeue(out var line);
        this.storedMessagesSemaphore.Release();
        return line;
    }
}