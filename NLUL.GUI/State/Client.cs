using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using NLUL.Core;
using NLUL.Core.Client;
using NLUL.Core.Client.Archive;
using NLUL.Core.Client.Patch;

namespace NLUL.GUI.State
{
    public class PlayState
    {
        // Uninitialized state.
        public static readonly PlayState Uninitialized = new PlayState(false, true);
        
        // Extract client requirement.
        public static readonly PlayState ExtractClient = new PlayState(false, true);
        public static readonly PlayState ExtractingClient = new PlayState(true, false);
        public static readonly PlayState VerifyingClient = new PlayState(true, false);
        public static readonly PlayState PatchingClient = new PlayState(true, false);
        public static readonly PlayState ExtractFailed = new PlayState(false, true);
        public static readonly PlayState VerifyFailed = new PlayState(false, true);
        public static readonly PlayState DownloadRuntime = new PlayState(false, true);
        public static readonly PlayState DownloadingRuntime = new PlayState(true, false);
        
        // Ready to play requirements.
        public static readonly PlayState NoSelectedServer = new PlayState(false, true);
        public static readonly PlayState Ready = new PlayState(false, true);
        public static readonly PlayState Launching = new PlayState(true, true);
        public static readonly PlayState Launched = new PlayState(true, true);
        
        // External setup.
        public static readonly PlayState ManualRuntimeNotInstalled = new PlayState(false, true);
        
        // Client migration.
        public static readonly PlayState DeletingClient = new PlayState(true, false);
        public static readonly PlayState MovingClientDirectory = new PlayState(true, false);
        
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

    public class ExtractException : Exception
    {
        /// <summary>
        /// Message to display to the user.
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// Creates the extract exception.
        /// </summary>
        /// <param name="message">Message to display to the user.</param>
        public ExtractException(string message)
        {
            this.Message = message;
        }
    }
    
    public class Client
    {
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
            if (!File.Exists(Path.Combine(SystemInfo.GetDefault().ClientLocation, "legouniverse.exe")))
            {
                if (!State.ManualChangeOnly)
                {
                    SetState(PlayState.ExtractClient);
                    return;
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
        /// Runs the client extract.
        /// Calls back a method with the loading message and percent.
        /// </summary>
        /// <param name="archiveLocation">Location of the archive.</param>
        /// <param name="callback">Callback that is sent a message and percent.</param>
        public static void RunExtract(string archiveLocation, Action<string, double> callback)
        {
            // Start the extract.
            SetState(PlayState.ExtractingClient);
            
            // Get the archive.
            var archive = ArchiveResolver.GetArchive(archiveLocation);
            if (archive == null)
            {
                SetState(PlayState.ExtractClient);
                throw new ExtractException("The selected archive file is not readable or does not contain a LEGO Universe client.");
            }
            
            // Extract the files.
            var clientLocation = SystemInfo.GetDefault().ClientLocation;
            try
            {
                archive.ExtractProgress += (progress) =>
                {
                    callback("Extracting client...", progress);
                };
                archive.ExtractTo(clientLocation);
            }
            catch (Exception)
            {
                throw new ExtractException("An error occured extracting the files. Make sure you have enough space and try again.");
            }
            
            // Verify the files.
            SetState(PlayState.VerifyingClient);
            if (!archive.Verify(clientLocation))
            {
                SetState(PlayState.VerifyFailed);
            }
            
            // Run the patches.
            SetState(PlayState.PatchingClient);
            // TODO: Patch client
            SetState(PlayState.Uninitialized);
            UpdateState();
        }

        /// <summary>
        /// Changes the parent directory of the client.
        /// </summary>
        /// <param name="destination">Destination of the client.</param>
        public static void ChangeParentDirectory(string destination)
        {
            // Set the state to deleting.
            SetState(PlayState.MovingClientDirectory);
            
            // Start moving the clients.
            Task.Run(() =>
            {
                // Move the clients.
                ClientRunner.MoveClientParentDirectory(destination);
                
                // Reset the state.
                SetState(PlayState.Uninitialized);
                UpdateState();
            });
        }
        
        /// <summary>
        /// Launches the client.
        /// </summary>
        /// <returns>Process of the client.</returns>
        public static Process Launch()
        {
            var selectedServer = PersistentState.SelectedServer;
            return selectedServer != null ? ClientRunner.Launch(selectedServer) : null;
        }
    }
}