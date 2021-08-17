/*
 * TheNexusAvenger
 *
 * Lower panel for playing.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using NLUL.Core;
using NLUL.GUI.Component.Base;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Play
{
    public class PlayPanel : DockPanel
    {
        public static readonly SolidColorBrush ButtonNormalColor = new SolidColorBrush(new Color(255,0,170,255));
        public static readonly SolidColorBrush ButtonDisabledColor = new SolidColorBrush(new Color(255,44,44,50));
        
        private List<LoadingDot> loadingDots = new List<LoadingDot>();
        private TextBlock loadingText;
        private RoundedButton playButton;
        private TextBlock playText;
        
        /*
         * Creates a play panel.
         */
        public PlayPanel()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.loadingText = this.Get<TextBlock>("LoadingText");
            this.playButton = this.Get<RoundedButton>("PlayButton");
            this.playText = this.Get<TextBlock>("PlayText");

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
        
        /*
         * Sets the loading bar percentage.
         */
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
                    dot.FillPercent = 0;
                }
                else if (endValue < percent)
                {
                    // Set the dot as filled if the percent is after the dot's range.
                    dot.FillPercent = 1;
                }
                else
                {
                    // Fill the dot based on the percentage between the ranges.
                    dot.FillPercent = (percent - startValue) / (endValue - startValue);
                }
            }
        }
        
        /*
         * Sets the loading bar animation point.
         * Used during the (slow) extracting phase to show
         * that the system is responsive.
         */
        private void SetLoadingAnimation(double percent)
        {
            for (var i = 0; i < this.loadingDots.Count; i++)
            {
                // Get the dot and the start and end value for the dot.
                var dot = this.loadingDots[i];
                var value = (i + 0.5) / this.loadingDots.Count;
                var distanceToValue = Math.Abs(percent - value);

                // Set the fill of the dot.
                dot.FillPercent = Math.Clamp(1 - (distanceToValue * 5),0,1);
            }
        }
        
        /*
         * Displays a loading bar animation until
         * the state is no longer the same.
         */
        private void DisplayLoadingBarAnimation(PlayState state)
        {
            new Thread(() =>
            {
                var animationPercent = -0.25;
                while (Client.state == state)
                {
                    // Update the animation.
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        this.SetLoadingAnimation(animationPercent);
                        animationPercent += 0.025;
                        if (animationPercent > 1.25)
                        {
                            animationPercent = -0.25;
                        }
                    });

                    // Wait to update the animation.
                    Thread.Sleep(25);
                }
            }).Start();
        }
        
        /*
         * Invokes when the state changes.
         */
        private void OnStateChanged()
        {
            // Update the text and button based on the state.
            var state = Client.state;
            if (state == PlayState.Uninitialized)
            {
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Loading...";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.DownloadClient)
            {
                this.playButton.Color = ButtonNormalColor;
                this.playButton.Active = true;
                this.loadingText.Text = "Pending client download.";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.DownloadFailed)
            {
                this.playButton.Color = ButtonNormalColor;
                this.playButton.Active = true;
                this.loadingText.Text = "Client download failed. Retry required.";
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
                this.loadingText.Text = "Pending " + Client.GetRuntimeName() + " download.";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.DownloadRuntimeAndClient)
            {
                this.playButton.Color = ButtonNormalColor;
                this.playButton.Active = true;
                this.loadingText.Text = "Pending " + Client.GetRuntimeName() + " and client download.";
                this.SetLoadingBar(0);
            }
            else if (state == PlayState.DownloadingRuntime)
            {
                // Set the button and loading text.
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Downloading and installing " + Client.GetRuntimeName() + ".";

                // Run the animation in a thread.
                this.DisplayLoadingBarAnimation(PlayState.DownloadingRuntime);
            }
            else if (state == PlayState.DownloadingClient)
            {
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
            }
            else if (state == PlayState.ExtractingClient)
            {
                // Set the button and loading text.
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Extracting client.";
                
                // Run the animation in a thread.
                this.DisplayLoadingBarAnimation(PlayState.ExtractingClient);
            }
            else if (state == PlayState.VerifyingClient)
            {
                // Set the button and loading text.
                this.playButton.Color = ButtonDisabledColor;
                this.playButton.Active = false;
                this.loadingText.Text = "Verifying client.";
                
                // Run the animation in a thread.
                this.DisplayLoadingBarAnimation(PlayState.ExtractingClient);
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
                var selectedServer = PersistentState.GetSelectedServer();
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
                this.loadingText.Text = Client.GetManualRuntimeInstallMessage();
                this.SetLoadingBar(0);
            }
        }
        
        /*
         * Invoked when the button is pressed.
         */
        private void OnButtonPressed()
        {
            var state = Client.state;
            if (state == PlayState.DownloadRuntime || state == PlayState.DownloadRuntimeAndClient)
            {
                // Start the download in a thread.
                Client.SetState(PlayState.DownloadingRuntime);
                new Thread(() =>
                {
                    // Start the runtime download.
                    Client.DownloadRuntime(() =>
                    {
                        // Start the download if the client is required.
                        if (Client.state == PlayState.DownloadClient || Client.state == PlayState.VerifyFailed || Client.state == PlayState.DownloadFailed)
                        {
                            this.OnButtonPressed();
                        }
                    });
                }).Start();
            }
            else if (state == PlayState.DownloadClient || state == PlayState.VerifyFailed || state == PlayState.DownloadFailed)
            {
                // Start the download in a thread.
                Client.SetState(PlayState.DownloadingClient);
                new Thread(() =>
                {
                    // Start the client download.
                    Client.RunDownload((message, percent) =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            this.loadingText.Text = message;
                            this.SetLoadingBar(percent);
                        });
                    });
                }).Start();
            }
            else if (state == PlayState.Ready)
            {
                // Launch the client.
                Client.SetState(PlayState.Launching);
                
                // Launch the client.
                new Thread(() =>
                {
                    Client.Launch();
                
                    // Close the window after the launch is complete.
                    // The launch may get delayed by pre-launch patches.
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        IControl currentWindow = this;
                        while (currentWindow != null && !(currentWindow is Window))
                        {
                            currentWindow = currentWindow.Parent;
                        }

                        if (!SystemInfo.GetDefault().Settings.LogsEnabled)
                        {
                            ((Window) currentWindow)?.Close();
                        }
                        Client.SetState(PlayState.Launched);
                    });
                }).Start();
            }
        }
    }
}