/*
 * TheNexusAvenger
 *
 * State for the play user interface.
 */

using System;
using System.IO;
using System.Threading;
using Avalonia.Threading;
using NLUL.Core.Client;

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
        public static readonly PlayState DownloadingClient = new PlayState(true);
        public static readonly PlayState ExtractingClient = new PlayState(true);
        public static readonly PlayState PatchingClient = new PlayState(true);
        
        // Ready to play requirements.
        public static readonly PlayState NoSelectedServer = new PlayState(false);
        public static readonly PlayState Ready = new PlayState(false);
        public static readonly PlayState Launching = new PlayState(true);
        
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
        public const long ExpectedClientZipSize = 4513866950; // May not be correct at any given point. Only used for the visuals.
        public const double ByteToGigabyte = 1000000000;

        private static ClientRunner clientRunner = new ClientRunner(ProgramSystemInfo.SystemInfo);
        
        public delegate void EmptyEventHandler();
        public static event EmptyEventHandler StateChanged;
        
        public static PlayState state = PlayState.Uninitialized;
        
        /*
         * Returns the message to display to the user if the runtime
         * isn't installed and can't be automatically installed.
         */
        public static string GetManualRuntimeInstallMessage()
        {
            return clientRunner.GetRuntime().GetManualRuntimeInstallMessage();
        }
        
        /*
         * Updates the state.
         */
        public static void UpdateState()
        {
            // Check for the runtime to be installed.
            if (!clientRunner.GetRuntime().IsInstalled() && !clientRunner.GetRuntime().CanInstall())
            {
                SetState(PlayState.ManualRuntimeNotInstalled);
                return;
                
            }
            
            // Check for the download to be complete.
            if (!File.Exists(Path.Combine(ProgramSystemInfo.SystemInfo.ClientLocation,"legouniverse.exe")))
            {
                if (!state.ManualChangeOnly)
                {
                    SetState(PlayState.DownloadClient);
                }
                return;
            }
            
            // Set the game state.
            if (!state.ManualChangeOnly)
            {
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
         * Runs the client download.
         * Calls back a method with the loading message
         * and percent.
         */
        public static void RunDownload(Action<string,double> callback)
        {
            // Run the download.
            SetStateThreadSafe(PlayState.DownloadingClient);
            clientRunner.TryExtractClient(false,(downloadState) =>
            {
                if (downloadState.Equals("Download"))
                {
                    // Set the state as downloading.
                    SetStateThreadSafe(PlayState.DownloadingClient);
                    
                    // Start updating the size.
                    var clientZip = Path.Combine(ProgramSystemInfo.SystemInfo.SystemFileLocation,"client.zip");
                    new Thread(() =>
                    {
                        while (state == PlayState.DownloadingClient)
                        {
                            // Get the current size.
                            double clientSize = 0;
                            if (File.Exists(clientZip))
                            {
                                clientSize = new FileInfo(clientZip).Length;
                            }

                            // Update the text.
                            callback("Downloading client (" + (clientSize/ByteToGigabyte).ToString("F") + " GB / " + (ExpectedClientZipSize/ByteToGigabyte).ToString("F") + " GB)",clientSize/ExpectedClientZipSize);
                            
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
            });
            
            // Run the patch.
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