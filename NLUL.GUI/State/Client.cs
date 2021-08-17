/*
 * TheNexusAvenger
 *
 * State for the play user interface.
 */

using System;
using System.IO;
using System.Net;
using System.Threading;
using Avalonia.Threading;
using NLUL.Core;
using NLUL.Core.Client;
using NLUL.Core.Client.Patch;

namespace NLUL.GUI.State
{
    
    /*
     * States the client can be in.
     */
    public class PlayState
    {
        // Uninitialized state.
        public static readonly PlayState Uninitialized = new PlayState(false);
        
        // Download client requirement.
        public static readonly PlayState DownloadClient = new PlayState(false);
        public static readonly PlayState DownloadRuntime = new PlayState(false);
        public static readonly PlayState DownloadRuntimeAndClient = new PlayState(false);
        public static readonly PlayState DownloadingRuntime = new PlayState(true);
        public static readonly PlayState DownloadingClient = new PlayState(true);
        public static readonly PlayState ExtractingClient = new PlayState(true);
        public static readonly PlayState VerifyingClient = new PlayState(true);
        public static readonly PlayState PatchingClient = new PlayState(true);
        public static readonly PlayState DownloadFailed = new PlayState(false);
        public static readonly PlayState VerifyFailed = new PlayState(false);
        
        // Ready to play requirements.
        public static readonly PlayState NoSelectedServer = new PlayState(false);
        public static readonly PlayState Ready = new PlayState(false);
        public static readonly PlayState Launching = new PlayState(true);
        public static readonly PlayState Launched = new PlayState(true);
        
        // External setup.
        public static readonly PlayState ManualRuntimeNotInstalled = new PlayState(false);
            
        public bool ManualChangeOnly;
        
        private PlayState(bool manualChangeOnly)
        {
            this.ManualChangeOnly = manualChangeOnly;
        }
    }
    
    /*
     * Class for the storing the state.
     */
    public class Client
    {
        public const double ByteToGigabyte = 1000000000;

        public static ClientRunner clientRunner = new ClientRunner(SystemInfo.GetDefault());
        
        public delegate void EmptyEventHandler();
        public static event EmptyEventHandler StateChanged;
        
        public static PlayState state = PlayState.Uninitialized;
        
        /*
         * Returns the message to display to the user if the runtime
         * isn't installed and can't be automatically installed.
         */
        public static string GetManualRuntimeInstallMessage()
        {
            return clientRunner.Runtime.ManualRuntimeInstallMessage ?? "(No runtime install message)";
        }
        
        /*
         * Returns the name of the runtime to install.
         */
        public static string GetRuntimeName()
        {
            return clientRunner.Runtime.Name ?? "(No runtime name)";
        }
        
        /*
         * Returns the client patcher.
         */
        public static ClientPatcher GetPatcher()
        {
            return clientRunner.Patcher;
        }
        
        /*
         * Updates the state.
         */
        public static void UpdateState()
        {
            // Check for the runtime to be installed.
            if (!clientRunner.Runtime.IsInstalled && !clientRunner.Runtime.CanInstall)
            {
                SetState(PlayState.ManualRuntimeNotInstalled);
                return;
            }
            
            // Check for the download to be complete.
            if (!File.Exists(Path.Combine(SystemInfo.GetDefault().ClientLocation,"legouniverse.exe")))
            {
                if (!state.ManualChangeOnly)
                {
                    if (clientRunner.Runtime.IsInstalled)
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
                if (!state.ManualChangeOnly && !clientRunner.Runtime.IsInstalled)
                {
                    SetState(PlayState.DownloadRuntime);
                    return;
                }
            }
            
            // Set the game state.
            if (!state.ManualChangeOnly)
            {
                // Verify the client.
                if (clientRunner.CanVerifyExtractedClient)
                {
                    // Set the state as the verified failed.
                    if (!clientRunner.VerifyExtractedClient())
                    {
                        SetState(PlayState.VerifyFailed);
                        return;
                    }
                }

                // Set the select state.
                if (PersistentState.GetSelectedServer() == null)
                {
                    SetState(PlayState.NoSelectedServer);
                }
                else
                {
                    SetState(PlayState.Ready);
                }
            }
        }
        
        /*
         * Sets the state.
         */
        public static void SetState(PlayState newState)
        {
            state = newState;
            StateChanged?.Invoke();
        }
        
        /*
         * Sets the state using a thread safe method.
         */
        public static void SetStateThreadSafe(PlayState newState)
        {
            Dispatcher.UIThread.InvokeAsync(() => { SetState(newState); });
        }

        /*
         * Runs the runtime download.
         */
        public static void DownloadRuntime(Action callback)
        {
            // Download the runtime.
            clientRunner.Runtime.Install();
            
            // Update the state and invoke the callback.
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                SetState(PlayState.Uninitialized);
                UpdateState();
                callback();
            });
        }
        
        /*
         * Runs the client download.
         * Calls back a method with the loading message
         * and percent.
         */
        public static void RunDownload(Action<string,double> callback)
        {
            // Start the download.
            SetStateThreadSafe(PlayState.DownloadingClient);
            var errorOccured = false;
            EventHandler<string> eventHandler = (_, downloadState) =>
            {
                if (downloadState.Equals("Download"))
                {
                    // Set the state as downloading.
                    SetStateThreadSafe(PlayState.DownloadingClient);
                    
                    // Start updating the size.
                    new Thread(() =>
                    {
                        while (state == PlayState.DownloadingClient)
                        {
                            // Update the text.
                            var downloadedClientSize = (double) clientRunner.DownloadedClientSize;
                            callback("Downloading client (" + (downloadedClientSize/ByteToGigabyte).ToString("F") + " GB / " + (clientRunner.ClientDownloadSize/ByteToGigabyte).ToString("F") + " GB)",downloadedClientSize/clientRunner.ClientDownloadSize);
                            
                            // Wait to update again.
                            Thread.Sleep(100);
                        }
                    }).Start();
                }
                else if (downloadState.Equals("Extract"))
                {
                    // Set the state as extracting.
                    SetStateThreadSafe(PlayState.ExtractingClient);
                }
                else if (downloadState.Equals("Verify"))
                {
                    // Set the state as verifying.
                    SetStateThreadSafe(PlayState.VerifyingClient);
                }
                else if (downloadState.Equals("VerifyFailed"))
                {
                    // Set the state as the verified failed.
                    SetStateThreadSafe(PlayState.VerifyFailed);
                    errorOccured = true;
                }
            };
            clientRunner.DownloadStateChanged += eventHandler;
            
            try {
                // Run the download.
                clientRunner.Download();
            }
            catch (WebException)
            {
                // Set the state as the download failed.
                SetStateThreadSafe(PlayState.DownloadFailed);
                return; 
            }
            clientRunner.DownloadStateChanged -= eventHandler;
            
            // Run the patch.
            if (errorOccured) return;
            SetStateThreadSafe(PlayState.PatchingClient);
            clientRunner.PatchClient();
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                SetState(PlayState.Uninitialized);
                UpdateState();
            });
        }
        
        /*
         * Launches the client.
         */
        public static void Launch()
        {
            var selectedServer = PersistentState.GetSelectedServer();
            if (selectedServer != null)
            {
                clientRunner.Launch(selectedServer.serverAddress,false);
            }
        }
    }
}