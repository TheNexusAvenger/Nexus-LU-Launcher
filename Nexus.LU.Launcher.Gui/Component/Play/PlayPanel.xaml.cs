using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Nexus.LU.Launcher.Gui.Component.Base;
using Nexus.LU.Launcher.Gui.Component.Prompt;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.Gui.Component.Play;

public class PlayPanel : DockPanel
{
    /// <summary>
    /// Launcher states that the play button is active.
    /// </summary>
    public List<LauncherState> PlayButtonActiveStates = new List<LauncherState>()
    {
        LauncherState.PendingExtractSelection,
        LauncherState.ExtractFailed,
        LauncherState.VerifyFailed,
        LauncherState.RuntimeNotInstalled,
        LauncherState.ReadyToLaunch,
    };

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
    public ScrollViewer? ClientOutputScroll { get; set; }

    /// <summary>
    /// Text container of the client output.
    /// </summary>
    public TextBox? ClientOutput { get; set; }

    /// <summary>
    /// Creates a play panel.
    /// </summary>
    public PlayPanel()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);
        this.loadingText = this.Get<TextBlock>("LoadingText");
        this.playButton = this.Get<RoundedButton>("PlayButton");
        
        // Apply the text.
        var localization = Localization.Get();
        localization.LocalizeText(this.Get<TextBlock>("PlayText"));

        // Create the dots.
        var loadingBarContainer = this.Get<DockPanel>("LoadingDotsContainer");
        for (var i = 0; i < 20; i++)
        {
            var dot = new LoadingDot();
            loadingBarContainer.Children.Add(dot);
            this.loadingDots.Add(dot);
        }

        // Connect the events.
        var clientState = ClientState.Get();
        clientState.LauncherProgressChanged += (progress) =>
        {
            this.RunMainThread(() =>
            {
                this.OnLauncherProgress(progress);
            });
        };
        this.playButton.ButtonPressed += (sender, args) => this.OnButtonPressed();

        // Set up the initial state.
        this.OnLauncherProgress(clientState.CurrentLauncherProgress);
    }

    /// <summary>
    /// Sets the loading bar percentage.
    /// </summary>
    /// <param name="percent">Percent to fill.</param>
    private void SetLoadingBar(double percent)
    {
        this.RunMainThread(() =>
        {
            for (var i = 0; i < this.loadingDots.Count; i++)
            {
                // Get the dot and the start and end value for the dot.
                var dot = this.loadingDots[i];
                var startValue = (double)i / this.loadingDots.Count;
                var endValue = (double)(i + 1) / this.loadingDots.Count;

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
        });
    }

    /// <summary>
    /// Sets the loading bar animation point.
    /// Used during the (slow) extracting phase to show
    /// that the system is responsive.
    /// </summary>
    /// <param name="percent">Percent to fill.</param>
    private void SetLoadingAnimation(double percent)
    {
        this.RunMainThread(() =>
        {
            for (var i = 0; i < this.loadingDots.Count; i++)
            {
                // Get the dot and the start and end value for the dot.
                var dot = this.loadingDots[i];
                var value = (i + 0.5) / this.loadingDots.Count;
                var distanceToValue = Math.Abs(percent - value);

                // Set the fill of the dot.
                dot.FillPercent = Math.Clamp(1 - (distanceToValue * 5), 0, 1);
            }
        });
    }

    /// <summary>
    /// Prompts to extract an archive.
    /// </summary>
    private void PromptExtract()
    {
        // Prompt for the file.
        var localization = Localization.Get();
        var window = this.GetWindow()!;
        var openFileTask =  window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = localization.GetLocalizedString("Client_ArchiveFilePickerTitle"),
            FileTypeFilter = new List<FilePickerFileType>() { new FilePickerFileType(localization.GetLocalizedString("Client_ArchiveFilePickerFileType"))
                {
                    Patterns = new List<string>() { "*.zip", "*.rar" },
                },
            },
        });

        Task.Run(async () =>
        {
            // Get the archive location.
            // Can't be awaited directly with ShowAsync because of a multithreading crash on macOS.
            var archiveLocations = await openFileTask;
            if (archiveLocations.Count == 0) return;
            var archiveLocation = archiveLocations[0];

            // Start the extract.
            await ClientState.Get().ExtractAsync(archiveLocation.Path.LocalPath);
        });
    }

    /// <summary>
    /// Displays a loading bar animation until
    /// the state is no longer the same.
    /// </summary>
    /// <param name="state">State to check for.</param>
    private void DisplayLoadingBarAnimation(LauncherState state)
    {
        var clientState = ClientState.Get();
        Task.Run(async () =>
        {
            var animationPercent = -0.25;
            while (clientState.CurrentLauncherState == state)
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
    /// Invokes when the launcher makes progress.
    /// </summary>
    /// <param name="launcherProgress">Progress made by the launch.</param>
    private void OnLauncherProgress(LauncherProgress launcherProgress)
    {
        // Update the bar.
        if (launcherProgress.ProgressBarState == ProgressBarState.Inactive)
        {
            this.SetLoadingBar(0);
        }
        else if (launcherProgress.ProgressBarState == ProgressBarState.PercentFill)
        {
            this.SetLoadingBar(launcherProgress.ProgressBarFill!.Value);
        }
        else if (launcherProgress.ProgressBarState == ProgressBarState.Progressing)
        {
            this.DisplayLoadingBarAnimation(launcherProgress.LauncherState);
        }

        // Update the text.
        var localization = Localization.Get();
        var clientState = ClientState.Get();
        var stateLoadingText = launcherProgress.LauncherState.ToString();
        if (launcherProgress.AdditionalData != null)
        {
            stateLoadingText += $"_{launcherProgress.AdditionalData}";
        }
        stateLoadingText = localization.GetLocalizedString($"Client_Status_{stateLoadingText}");
        if (launcherProgress.LauncherState == LauncherState.ReadyToLaunch)
        {
            var selectedServer = clientState.ServerList.SelectedEntry!;
            stateLoadingText = string.Format(stateLoadingText, selectedServer.ServerName, selectedServer.ServerAddress);
        }
        this.loadingText.Text = stateLoadingText;

        // Update the play button.
        if (PlayButtonActiveStates.Contains(launcherProgress.LauncherState))
        {
            this.playButton.Color = ButtonNormalColor;
            this.playButton.Active = true;
        }
        else
        {
            this.playButton.Color = ButtonDisabledColor;
            this.playButton.Active = false;
        }
        
        // Prompt to re-try extracting if it failed.
        if (launcherProgress.LauncherState == LauncherState.ExtractFailed || launcherProgress.LauncherState == LauncherState.VerifyFailed)
        {
            var promptMessage = string.Format(localization.GetLocalizedString("Client_RetrySelectArchivePrompt"), stateLoadingText);
            ConfirmPrompt.OpenPrompt( promptMessage, this.PromptExtract);
        }
    }

    /// <summary>
    /// Invoked when the play button is pressed.
    /// </summary>
    private void OnButtonPressed()
    {
        var clientState = ClientState.Get();
        var launcherState = clientState.CurrentLauncherState;
        if (launcherState == LauncherState.PendingExtractSelection || launcherState == LauncherState.ExtractFailed || launcherState == LauncherState.VerifyFailed)
        {
            this.PromptExtract();
        }
        else if (launcherState == LauncherState.RuntimeNotInstalled || launcherState == LauncherState.ReadyToLaunch)
        {
            Task.Run(async () =>
            {
                // Start the process.
                var process = await clientState.LaunchAsync();
                if (process == null) return;
                
                // Close the window if the output is not enabled.
                // Close the window after the launch is complete.
                // The launch may get delayed by pre-launch patches.
                if (!SystemInfo.GetDefault().Settings.LogsEnabled)
                {
                    this.RunMainThread(() =>
                    {
                        this.GetWindow()?.Close();
                    });
                    return;
                }
                
                // Set up the displaying the output logs.
                this.ClientOutputScroll!.ScrollChanged += (sender, args) =>
                {
                    if (args.ExtentDelta.Y == 0) return;
                    this.ClientOutputScroll.RunMainThread(this.ClientOutputScroll.ScrollToEnd);
                };

                // Copy the output to the view.
                var output = "";
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = await process.StandardOutput.ReadLineAsync();
                    output += (output == "" ? "" : "\n") + line;
                    var currentOutput = output;
                    this.RunMainThread(() =>
                    {
                        this.ClientOutput!.Text = currentOutput;
                    });
                }
            });
        }
    }
}