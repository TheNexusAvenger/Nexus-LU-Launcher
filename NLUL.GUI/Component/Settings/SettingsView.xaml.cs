using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.Core;
using NLUL.GUI.Component.Base;
using NLUL.GUI.Component.Prompt;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Settings
{
    public class SettingsView : Panel
    {
        /// <summary>
        /// Information about the system.
        /// </summary>
        private readonly SystemInfo systemInfo = SystemInfo.GetDefault();

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
            
            // Connect the events.
            this.logsToggle.ButtonPressed += (sender, args) =>
            {
                this.systemInfo.Settings.LogsEnabled = !this.systemInfo.Settings.LogsEnabled;
                this.systemInfo.SaveSettings();
                this.UpdateSettings();
            };
            this.Get<RoundedImageButton>("ChangeClientParentDirectory").ButtonPressed += (sender, args) =>
            {
                // Prompt for the directory.
                var dialog = new OpenFolderDialog();
                dialog.Directory = this.CurrentParentDirectory;
                var newDirectoryTask = dialog.ShowAsync(this.GetWindow());

                Task.Run(async () =>
                {
                    // Get the new directory.
                    // Can't be awaited directly with ShowAsync because of a multithreading crash on macOS.
                    var newDirectory = await newDirectoryTask;
                    if (string.IsNullOrEmpty(newDirectory) || newDirectory == this.CurrentParentDirectory) return;

                    // Move the clients.
                    ConfirmPrompt.OpenPrompt(
                        "Changing install locations will move any clients you have downloaded to it. Continue?", () =>
                        {
                            this.parentDirectoryDisplay.Text = newDirectory.Replace(
                                Path.DirectorySeparatorChar == '/' ? '\\' : '/', Path.DirectorySeparatorChar);
                            Client.ChangeParentDirectory(newDirectory);
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
            if (this.systemInfo.Settings.LogsEnabled)
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
}