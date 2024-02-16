using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Nexus.LU.Launcher.Gui.Component.Base;
using Nexus.LU.Launcher.Gui.Component.Prompt;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.Gui.Component.Settings;

public class SettingsView : Panel
{
    /// <summary>
    /// Button for toggling the logs.
    /// </summary>
    private readonly RoundedImageButton logsToggle;

    /// <summary>
    /// Display of the parent directory.
    /// </summary>
    private readonly TextBlock parentDirectoryDisplay;
    
    /// <summary>
    /// Parent directory of the clients.
    /// </summary>
    private string CurrentParentDirectory => SystemInfo.GetDefault().SystemFileLocation.Replace(Path.DirectorySeparatorChar == '/' ? '\\' : '/', Path.DirectorySeparatorChar);
    
    /// <summary>
    /// Creates a settings view.
    /// </summary>
    public SettingsView()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);
        this.logsToggle = this.Get<RoundedImageButton>("LogsToggle");
        this.parentDirectoryDisplay = this.Get<TextBlock>("ClientParentDirectory");
        this.UpdateSettings();
        
        // Apply the text.
        var localization = Localization.Get();
        localization.LocalizeText(this.Get<TextBlock>("ShowClientLogsLabel"));
        localization.LocalizeText(this.Get<TextBlock>("LauncherFilesLabel"));
        
        // Connect the events.
        this.logsToggle.ButtonPressed += (sender, args) =>
        {
            var systemInfo = SystemInfo.GetDefault();
            systemInfo.Settings.LogsEnabled = !systemInfo.Settings.LogsEnabled;
            systemInfo.SaveSettings();
            this.UpdateSettings();
        };
        this.Get<RoundedImageButton>("ChangeClientParentDirectory").ButtonPressed += (sender, args) =>
        {
            // Display a prompt to the user that changing client directories is not supported for the Flatpak.
            // The Flatpak sandbox prevents reads/writes outside of allowed files and portals.
            if (Environment.GetEnvironmentVariable("FLATPAK_ID") != null)
            {
                NotificationPrompt.OpenPrompt(localization.GetLocalizedString("Settings_ChangeLocationBlockedFlatpak"));
                return;
            }
            
            // Prompt for the directory.
            var window = this.GetWindow()!;
            var newDirectoryTask =  window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = Localization.Get().GetLocalizedString("Settings_ChangeLocationFolderPickerTitle"),
                SuggestedStartLocation = window.StorageProvider.TryGetFolderFromPathAsync(this.CurrentParentDirectory).Result,
            });

            Task.Run(async () =>
            {
                // Get the new directory.
                // Can't be awaited directly with ShowAsync because of a multithreading crash on macOS.
                var newDirectoryList = await newDirectoryTask;
                if (newDirectoryList.Count == 0 || newDirectoryList[0].Path.LocalPath == this.CurrentParentDirectory) return;
                var newDirectory = newDirectoryList[0].Path.LocalPath;

                // Move the clients.
                ConfirmPrompt.OpenPrompt(Localization.Get().GetLocalizedString("Settings_ChangeLocationPrompt"), () =>
                    {
                        this.parentDirectoryDisplay.Text = newDirectory.Replace(
                            Path.DirectorySeparatorChar == '/' ? '\\' : '/', Path.DirectorySeparatorChar);
                        Task.Run(async () => await ClientState.Get().MoveClientParentDirectoryAsync(newDirectory));
                    });
            });
        };
    }

    /// <summary>
    /// Updates the displayed settings.
    /// </summary>
    private void UpdateSettings()
    {
        // Update the logs toggle.
        if (SystemInfo.GetDefault().Settings.LogsEnabled)
        {
            this.logsToggle.BaseSource = "/Assets/Images/Prompt/Confirm.png";
            this.logsToggle.HoverSource = "/Assets/Images/Prompt/Confirm.png";
            this.logsToggle.PressSource = "/Assets/Images/Prompt/ConfirmPress.png";
        }
        else
        {
            this.logsToggle.BaseSource = "/Assets/Images/Prompt/Cancel.png";
            this.logsToggle.HoverSource = "/Assets/Images/Prompt/Cancel.png";
            this.logsToggle.PressSource = "/Assets/Images/Prompt/CancelPress.png";
        }
        this.logsToggle.UpdateSource();
        
        // Update the parent directory.
        this.parentDirectoryDisplay.Text = this.CurrentParentDirectory;
    }
}