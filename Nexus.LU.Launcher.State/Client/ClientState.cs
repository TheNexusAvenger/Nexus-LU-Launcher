using System;
using System.IO;
using Nexus.LU.Launcher.State.Client.Archive;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client;

public class ClientState {
    /// <summary>
    /// Current state of the launcher.
    /// </summary>
    public LauncherState CurrentLauncherState => this.CurrentLauncherProgress.LauncherState;

    /// <summary>
    /// Current progress of the launcher.
    /// </summary>
    public LauncherProgress CurrentLauncherProgress { get; private set; } = new LauncherProgress()
    {
        LauncherState = LauncherState.Uninitialized,
        ProgressBarState = ProgressBarState.Inactive
    };

    /// <summary>
    /// Event for when the launcher state changes.
    /// </summary>
    public event Action<LauncherState> LauncherStateChanged;

    /// <summary>
    /// Event for when the launcher progress changes.
    /// This event is fired with every progress bar change. LauncherStateChanged is recommended.
    /// </summary>
    public event Action<LauncherProgress> LauncherProgressChanged;

    /// <summary>
    /// Static instance of the client state.
    /// </summary>
    private static ClientState _clientState;

    /// <summary>
    /// Creates the client state.
    /// </summary>
    private ClientState() {
        this.Initialize();
    }

    /// <summary>
    /// Returns a static instance of ClientState.
    /// </summary>
    /// <returns>Static instance of the client state.</returns>
    public static ClientState Get()
    {
        if (_clientState == null)
        {
            _clientState = new ClientState();
        }
        return _clientState;
    }

    /// <summary>
    /// Sets the launcher progress.
    /// </summary>
    /// <param name="progress">New progress of the launcher.</param>
    private void SetLauncherProgress(LauncherProgress progress)
    {
        var previousState = this.CurrentLauncherState;
        this.CurrentLauncherProgress = progress;
        this.LauncherProgressChanged?.Invoke(progress);
        if (previousState != progress.LauncherState)
        {
            this.LauncherStateChanged?.Invoke(progress.LauncherState);
        }
    }

    /// <summary>
    /// Initializes the client state from the file system.
    /// </summary>
    private void Initialize() {
        // Set the initial state if the runtime is not downloaded (mainly Linux with WINE).
        // TODO

        // Set the initial state if no client is extracted.
        // Due to legal reasons, an automatic download is not provided.
        var systemInfo = SystemInfo.GetDefault();
        if (!File.Exists(Path.Combine(systemInfo.ClientLocation, "legouniverse.exe")))
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.PendingExtractSelection,
            });
            return;
        }

        // Set the initial state if an automated runtime needs to be installed (mainly macOS with WINE).
        // The user extracting the client will automatically handle this state afterwards.
        // TODO

        // Prepare the client to play.
        if (systemInfo.Settings.GetServerEntry(systemInfo.Settings.SelectedServer) == null)
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.NoSelectedServer,
            });
        }
        else
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.ReadyToLaunch,
            });
        }
    }
    
    /// <summary>
    /// Extracts the client.
    /// </summary>
    /// <param name="archivePath">Path to the archive to extract from.</param>
    public void Extract(string archivePath)
    {
        // Set the state as archiving.
        this.SetLauncherProgress(new LauncherProgress()
        {
            LauncherState = LauncherState.ExtractingClient,
        });
        
        // Get the archive.
        var archive = ClientArchive.GetArchive(archivePath);
        if (archive == null)
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.ExtractFailed,
                AdditionalData = "InvalidArchive",
            });
            return;
        }
        
        // Extract the files.
        var clientLocation = SystemInfo.GetDefault().ClientLocation;
        archive.ExtractProgress += (progress) =>
        {
            if (this.CurrentLauncherState != LauncherState.ExtractingClient) return;
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.ExtractingClient,
                ProgressBarState = ProgressBarState.PercentFill,
                ProgressBarFill = progress,
            });
        };
        try
        {
            archive.ExtractTo(clientLocation);
        }
        catch (Exception)
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.ExtractFailed,
                AdditionalData = "ExceptionWhileExtracting",
            });
            return;
        }
        
        // Verify the files.
        this.SetLauncherProgress(new LauncherProgress()
        {
            LauncherState = LauncherState.VerifyingClient,
            ProgressBarState = ProgressBarState.Progressing,
        });
        if (!archive.Verify(clientLocation))
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.VerifyFailed,
            });
            return;
        }
        
        // Download the runtime.
        // TODO
        
        // Apply the default patches.
        // TODO
        
        // Re-initialize the state.
        this.Initialize();
    }
}