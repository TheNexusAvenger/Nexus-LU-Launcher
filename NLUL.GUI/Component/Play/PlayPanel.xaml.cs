using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using NLUL.Core;
using NLUL.GUI.Component.Base;
using NLUL.GUI.Component.Prompt;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Play
{
    public class PlayPanel : DockPanel
    {
        /// <summary>
        /// Color for the play button being clickable.
        /// </summary>
        private static readonly SolidColorBrush ButtonNormalColor = new SolidColorBrush(new Color(255, 0, 170, 255));
        
        /// <summary>
        /// Color for the play button being disabled.
        /// </summary>
        private static readonly SolidColorBrush ButtonDisabledColor = new SolidColorBrush(new Color(255, 44, 44, 50));
        
        /// <summary>
        /// List of loading dots.
        /// </summary>
        private readonly List<LoadingDot> loadingDots = new List<LoadingDot>();
        
        /// <summary>
        /// Loading text of the panel.
        /// </summary>
        private readonly TextBlock loadingText;
        
        /// <summary>
        /// Play button of the 
        /// </summary>
        private readonly RoundedButton playButton;
        
        /// <summary>
        /// Scroll container of the client output.
        /// </summary>
        public ScrollViewer ClientOutputScroll { get; set; }
        
        /// <summary>
        /// Text container of the client output.
        /// </summary>
        public TextBox ClientOutput { get; set; }
        
        /// <summary>
        /// Creates a play panel.
        /// </summary>
        public PlayPanel()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.loadingText = this.Get<TextBlock>("LoadingText");
            this.playButton = this.Get<RoundedButton>("PlayButton");

            // Create the dots.
            var loadingBarContainer = this.Get<DockPanel>("LoadingDotsContainer");
            for (var i = 0; i < 20; i++)
            {
                var dot = new LoadingDot();
                loadingBarContainer.Children.Add(dot);
                this.loadingDots.Add(dot);
            }
            
            // Connect the events.
            Client.StateChanged += this.OnStateChanged;
            this.playButton.ButtonPressed += (sender,args) => this.OnButtonPressed();
            
            // Set up the initial state.
            this.OnStateChanged();
            Client.UpdateState();
        }
        
        /// <summary>
        /// Sets the loading bar percentage.
        /// </summary>
        /// <param name="percent">Percent to fill.</param>
        private void SetLoadingBar(double percent)
        {
            for (var i = 0; i < this.loadingDots.Count; i++)
            {
                // Get the dot and the start and end value for the dot.
                var dot = this.loadingDots[i];
                var startValue = (double) i / this.loadingDots.Count;
                var endValue = (double) (i + 1) / this.loadingDots.Count;

                // Set the fill of the dot.
                if (startValue > percent)
                {
                    // Set the dot as unfilled if the percent is before the dot's range.
                    dot.Set("FillPercent", 0);
                }
                else if (endValue < percent)
                {
                    // Set the dot as filled if the percent is after the dot's range.
                    dot.Set("FillPercent", 1);
                }
                else
                {
                    // Fill the dot based on the percentage between the ranges.
                    dot.Set("FillPercent", (percent - startValue) / (endValue - startValue));
                }
            }
        }
        
        /// <summary>
        /// Sets the loading bar animation point.
        /// Used during the (slow) extracting phase to show
        /// that the system is responsive.
        /// </summary>
        /// <param name="percent">Percent to fill.</param>
        private void SetLoadingAnimation(double percent)
        {
            for (var i = 0; i < this.loadingDots.Count; i++)
            {
                // Get the dot and the start and end value for the dot.
                var dot = this.loadingDots[i];
                var value = (i + 0.5) / this.loadingDots.Count;
                var distanceToValue = Math.Abs(percent - value);

                // Set the fill of the dot.
                dot.Set("FillPercent", Math.Clamp(1 - (distanceToValue * 5), 0, 1));
            }
        }

        /// <summary>
        /// Prompts to extract an archive.
        /// </summary>
        private void PromptExtract()
        {
            // Prompt for the file.
            var dialog = new OpenFileDialog();
            dialog.AllowMultiple = false;
            dialog.Filters = new List<FileDialogFilter>()
            {
                new FileDialogFilter()
                {
                    Name = "Archive File",
                    Extensions = new List<string>() { "rar", "zip", }
                }
            };
            var openFileTask = dialog.ShowAsync(this.GetWindow());
                
            Task.Run(async () =>
            {
                // Get the archive location.
                // Can't be awaited directly with ShowAsync because of a multithreading crash on macOS.
                var archiveLocations = await openFileTask;
                if (archiveLocations.Length == 0) return;
                var archiveLocation = archiveLocations[0];
                    
                try
                {
                    // Start the extract.
                    Client.RunExtract(archiveLocation, (message, percent) =>
                    {
                        this.loadingText.Set("Text", message);
                        this.SetLoadingBar(percent);
                    });
                }
                catch (ExtractException exception)
                {
                    // Show the prompt that it failed and prompt to try again.
                    ConfirmPrompt.OpenPrompt(exception.Message + " Try again?", this.PromptExtract);
                }
            });
        }
        
        /// <summary>
        /// Displays a loading bar animation until
        /// the state is no longer the same.
        /// </summary>
        /// <param name="state">State to check for.</param>
        private void DisplayLoadingBarAnimation(PlayState state)
        {
            Task.Run(async () =>
            {
                var animationPercent = -0.25;
                while (Client.State == state)
                {
                    // Update the animation.
                    this.SetLoadingAnimation(animationPercent);
                    animationPercent += 0.025;
                    if (animationPercent > 1.25)
                    {
                        animationPercent = -0.25;
                    }

                    // Wait to update the animation.
                    await Task.Delay(25);
                }
            });
        }
        
        /// <summary>
        /// Invokes when the state changes.
        /// </summary>
        private void OnStateChanged()
        {
            // Update the text and button based on the state.
            var state = Client.State;
            if (state == PlayState.Uninitialized)
            {
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Loading...";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.ExtractClient)
            {
                this.playButton.Color = ButtonNormalColor;
                this.playButton.Active = true;
                this.loadingText.Text = "Pending client extract. An archive of the client must be downloaded.";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.ExtractFailed)
            {
                this.playButton.Color = ButtonNormalColor;
                this.playButton.Active = true;
                this.loadingText.Text = "Client extract failed. Retry required.";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.VerifyFailed)
            {
                this.playButton.Color = ButtonNormalColor;
                this.playButton.Active = true;
                this.loadingText.Text = "Client extracting failed. Retry required.";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.DownloadRuntime)
            {
                this.playButton.Color = ButtonNormalColor;
                this.playButton.Active = true;
                this.loadingText.Text = "Pending " + Client.RuntimeName + " download.";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.DownloadingRuntime)
            {
                // Set the button and loading text.
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Downloading and installing " + Client.RuntimeName + ".";

                // Run the animation in a thread.
                this.DisplayLoadingBarAnimation(PlayState.DownloadingRuntime);
            }
            else if (state == PlayState.ExtractingClient)
            {
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
            }
            else if (state == PlayState.VerifyingClient)
            {
                // Set the button and loading text.
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Verifying client.";
                
                // Run the animation in a thread.
                this.DisplayLoadingBarAnimation(PlayState.VerifyingClient);
            }
            else if (state == PlayState.PatchingClient)
            {
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Patching client.";
                this.SetLoadingBar(1);
            }
            else if (state == PlayState.NoSelectedServer)
            {
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "No server selected to play.";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.Ready)
            {
                var selectedServer = PersistentState.SelectedServer;
                this.playButton.Color = ButtonNormalColor;
                this.playButton.Active = true;
                this.loadingText.Text = "Ready to launch: " + selectedServer.ServerName + " (" + selectedServer.ServerAddress + ")";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.Launching)
            {
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Launching...";
                this.DisplayLoadingBarAnimation(PlayState.Launching);
            }
            else if (state == PlayState.ManualRuntimeNotInstalled)
            {
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = Client.RuntimeInstallMessage;
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.DeletingClient)
            {
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Deleting client...";
                this.DisplayLoadingBarAnimation(PlayState.DeletingClient);
            }
            else if (state == PlayState.MovingClientDirectory)
            {
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Moving client to new location...";
                this.DisplayLoadingBarAnimation(PlayState.MovingClientDirectory);
            }
        }
        
        /// <summary>
        /// Invoked when the play button is pressed.
        /// </summary>
        private void OnButtonPressed()
        {
            var state = Client.State;
            if (state == PlayState.DownloadRuntime)
            {
                // Start the download in a thread.
                Client.SetState(PlayState.DownloadingRuntime);
                Task.Run(() =>
                {
                    // Start the runtime download.
                    Client.DownloadRuntime(() =>
                    {
                        // Start the extract if the client is required.
                        if (state == PlayState.ExtractClient || Client.State == PlayState.VerifyFailed || Client.State == PlayState.ExtractFailed)
                        {
                            this.OnButtonPressed();
                        }
                    });
                });
            }
            else if (state == PlayState.ExtractClient || state == PlayState.VerifyFailed || state == PlayState.ExtractFailed)
            {
                // Prompt extracting an archive.
                PromptExtract();
            }
            else if (state == PlayState.Ready)
            {
                // Launch the client.
                Client.SetState(PlayState.Launching);
                
                // Launch the client.
                Task.Run(async () =>
                {
                    var process = Client.Launch();

                    // Close the window after the launch is complete.
                    // The launch may get delayed by pre-launch patches.
                    if (!SystemInfo.GetDefault().Settings.LogsEnabled)
                    {
                        this.Run(() =>
                        {
                            this.GetWindow()?.Close();
                        });
                        return;
                    }
                    Client.SetState(PlayState.Launched);

                    // Set up the displaying the output logs.
                    this.ClientOutputScroll.ScrollChanged += (sender, args) =>
                    {
                        if (args.ExtentDelta.Y == 0) return;
                        this.ClientOutputScroll.Run(this.ClientOutputScroll.ScrollToEnd);
                    };

                    // Copy the output to the view.
                    var output = "";
                    while (!process.StandardOutput.EndOfStream)
                    {
                        var line = await process.StandardOutput.ReadLineAsync();
                        output += (output == "" ? "" : "\n") + line;
                        this.ClientOutput.Set("Text", output);
                    }
                });
            }
        }
    }
}