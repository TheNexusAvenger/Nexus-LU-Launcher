using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Threading;
using NLUL.Core;
using NLUL.Core.Client;
using NLUL.Core.Client.Patch;
using NLUL.Core.Client.Source;

namespace NLUL.GUI.State
{
    public class PlayState
    {
        // Uninitialized state.
        public static readonly PlayState Uninitialized = new PlayState(false, true);
        
        // Download client requirement.
        public static readonly PlayState DownloadClient = new PlayState(false, true);
        public static readonly PlayState DownloadRuntime = new PlayState(false, true);
        public static readonly PlayState DownloadRuntimeAndClient = new PlayState(false, true);
        public static readonly PlayState DownloadingRuntime = new PlayState(true, false);
        public static readonly PlayState DownloadingClient = new PlayState(true, false);
        public static readonly PlayState ExtractingClient = new PlayState(true, false);
        public static readonly PlayState VerifyingClient = new PlayState(true, false);
        public static readonly PlayState PatchingClient = new PlayState(true, false);
        public static readonly PlayState DownloadFailed = new PlayState(false, true);
        public static readonly PlayState VerifyFailed = new PlayState(false, true);
        
        // Ready to play requirements.
        public static readonly PlayState NoSelectedServer = new PlayState(false, true);
        public static readonly PlayState Ready = new PlayState(false, true);
        public static readonly PlayState Launching = new PlayState(true, true);
        public static readonly PlayState Launched = new PlayState(true, true);
        
        // External setup.
        public static readonly PlayState ManualRuntimeNotInstalled = new PlayState(false, true);
        
        /// <summary>
        /// Whether the state can only be manually set.
        /// </summary>
        public bool ManualChangeOnly { get; }
        
        /// <summary>
        /// Whether the state is safe to close.
        /// </summary>
        public bool SafeToClose { get; }
        
        /// <summary>
        /// Creates the play state.
        /// </summary>
        /// <param name="manualChangeOnly">Whether the state can only be manually set.</param>
        /// <param name="safeToClose">Whether the state is safe to close.</param>
        private PlayState(bool manualChangeOnly, bool safeToClose)
        {
            this.ManualChangeOnly = manualChangeOnly;
            this.SafeToClose = safeToClose;
        }
    }
    
    public class Client
    {
        /// <summary>
        /// Constant for converting bytes to gigabytes. Used for
        /// the download progress bar.
        /// </summary>
        private const double ByteToGigabyte = 1000000000;

        /// <summary>
        /// Client runner for the launcher.
        /// </summary>
        private static readonly ClientRunner ClientRunner = new ClientRunner(SystemInfo.GetDefault());
        
        /// <summary>
        /// Delegate for an event with no parameters.
        /// </summary>
        public delegate void EmptyEventHandler();
        
        /// <summary>
        /// Event for the state changing.
        /// </summary>
        public static event EmptyEventHandler StateChanged;
        
        /// <summary>
        /// Runtime name for the client.
        /// </summary>
        public static string RuntimeName => ClientRunner.Runtime.Name ?? "(No runtime name)";
        
        /// <summary>
        /// The message to display to the user if the runtime
        /// isn't installed and can't be automatically installed.
        /// </summary>
        public static string RuntimeInstallMessage => ClientRunner.Runtime.ManualRuntimeInstallMessage ?? "(No runtime install message)";

        /// <summary>
        /// Patcher of the client.
        /// </summary>
        public static ClientPatcher Patcher => ClientRunner.Patcher;

        /// <summary>
        /// Selected client source.
        /// </summary>
        public static ClientSourceEntry ClientSource => ClientRunner.ClientSource;

        /// <summary>
        /// Client source options.
        /// </summary>
        public static SourceList ClientSourcesList => ClientRunner.ClientSourcesList;
        
        /// <summary>
        /// Current state of the client.
        /// </summary>
        public static PlayState State { get; private set; } = PlayState.Uninitialized;

        /// <summary>
        /// Updates the state.
        /// </summary>
        public static void UpdateState()
        {
            // Check for the runtime to be installed.
            if (!ClientRunner.Runtime.IsInstalled && !ClientRunner.Runtime.CanInstall)
            {
                SetState(PlayState.ManualRuntimeNotInstalled);
                return;
            }
            
            // Check for the download to be complete.
            if (!File.Exists(Path.Combine(SystemInfo.GetDefault().ClientLocation, "legouniverse.exe")) || (SystemInfo.GetDefault().Settings.RequestedClientSourceName != SystemInfo.GetDefault().Settings.InstalledClientSourceName && SystemInfo.GetDefault().Settings.InstalledClientSourceName != null))
            {
                if (!State.ManualChangeOnly)
                {
                    if (ClientRunner.Runtime.IsInstalled)
                    {
                        SetState(PlayState.DownloadClient);
                        return;
                    }
                    else
                    {
                        SetState(PlayState.DownloadRuntimeAndClient);
                        return;
                    }
                }
            }
            else
            {
                if (!State.ManualChangeOnly && !ClientRunner.Runtime.IsInstalled)
                {
                    SetState(PlayState.DownloadRuntime);
                    return;
                }
            }
            
            // Set the game state.
            if (!State.ManualChangeOnly)
            {
                // Verify the client.
                if (ClientRunner.CanVerifyExtractedClient)
                {
                    // Set the state as the verified failed.
                    if (!ClientRunner.VerifyExtractedClient())
                    {
                        SetState(PlayState.VerifyFailed);
                        return;
                    }
                }

                // Set the select state.
                if (PersistentState.SelectedServer == null)
                {
                    SetState(PlayState.NoSelectedServer);
                }
                else
                {
                    SetState(PlayState.Ready);
                }
            }
        }
        
        /// <summary>
        /// Sets the state.
        /// </summary>
        /// <param name="newState">New state to use.</param>
        public static void SetState(PlayState newState)
        {
            State = newState;
            Dispatcher.UIThread.InvokeAsync(() => { StateChanged?.Invoke(); });
        }

        /// <summary>
        /// Runs the runtime download.
        /// </summary>
        /// <param name="callback">Callback to run after the download completes.</param>
        public static void DownloadRuntime(Action callback)
        {
            // Download the runtime.
            ClientRunner.Runtime.Install();
            
            // Update the state and invoke the callback.
            SetState(PlayState.Uninitialized);
            UpdateState();
            callback();
        }
        
        /// <summary>
        /// Runs the client download.
        /// Calls back a method with the loading message and percent.
        /// </summary>
        /// <param name="callback">Callback that is sent a message and percent.</param>
        public static void RunDownload(Action<string, double> callback)
        {
            // Start the download.
            SetState(PlayState.DownloadingClient);
            var errorOccured = false;

            void EventHandler(object _, string downloadState)
            {
                if (downloadState.Equals("Download"))
                {
                    // Set the state as downloading.
                    SetState(PlayState.DownloadingClient);

                    // Start updating the size.
                    Task.Run(async () =>
                    {
                        while (State == PlayState.DownloadingClient)
                        {
                            // Update the text.
                            var downloadedClientSize = (double) ClientRunner.DownloadedClientSize;
                            callback(
                                "Downloading client (" + (downloadedClientSize / ByteToGigabyte).ToString("F") +
                                " GB / " + (ClientRunner.ClientDownloadSize / ByteToGigabyte).ToString("F") + " GB)",
                                downloadedClientSize / ClientRunner.ClientDownloadSize);

                            // Wait to update again.
                            await Task.Delay(100);
                        }
                    });
                }
                else if (downloadState.Equals("Extract"))
                {
                    // Set the state as extracting.
                    SetState(PlayState.ExtractingClient);
                }
                else if (downloadState.Equals("Verify"))
                {
                    // Set the state as verifying.
                    SetState(PlayState.VerifyingClient);
                }
                else if (downloadState.Equals("VerifyFailed"))
                {
                    // Set the state as the verified failed.
                    SetState(PlayState.VerifyFailed);
                    errorOccured = true;
                }
            }
            ClientRunner.DownloadStateChanged += EventHandler;
            
            try {
                // Run the download.
                ClientRunner.Download();
            }
            catch (WebException)
            {
                // Set the state as the download failed.
                SetState(PlayState.DownloadFailed);
                return; 
            }
            ClientRunner.DownloadStateChanged -= EventHandler;
            
            // Run the patch.
            if (errorOccured) return;
            SetState(PlayState.PatchingClient);
            ClientRunner.PatchClient();
            SetState(PlayState.Uninitialized);
            UpdateState();
        }
        
        /// <summary>
        /// Launches the client.
        /// </summary>
        /// <returns>Process of the client.</returns>
        public static Process Launch()
        {
            var selectedServer = PersistentState.SelectedServer;
            return selectedServer != null ? ClientRunner.Launch(selectedServer.ServerAddress) : null;
        }
    }
}