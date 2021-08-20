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
        /// List of the sources.
        /// </summary>
        private readonly ComboBox sourcesList;

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
            this.sourcesList = this.Get<ComboBox>("SourcesList");
            this.parentDirectoryDisplay = this.Get<TextBlock>("ClientParentDirectory");
            this.UpdateSettings();
            
            // Connect the events.
            this.logsToggle.ButtonPressed += (sender, args) =>
            {
                this.systemInfo.Settings.LogsEnabled = !this.systemInfo.Settings.LogsEnabled;
                this.systemInfo.SaveSettings();
                this.UpdateSettings();
            };
            this.sourcesList.SelectionChanged += (sender, args) =>
            {
                // Get the new source.
                var newSource = Client.ClientSourcesList.First(source => ("(" + source.Type + ") " + source.Name) == (string) sourcesList.SelectedItem);
                if (newSource == Client.ClientSource) return;

                // Set the source.
                // If the client isn't download, ignore warning the player.
                if (Client.State == PlayState.DownloadClient || Client.State == PlayState.DownloadRuntime || Client.State == PlayState.DownloadRuntimeAndClient)
                {
                    Client.ChangeSource(newSource);
                }
                else
                {
                    ConfirmPrompt.OpenPrompt("Changing client sources will delete you existing client and require a re-download. Continue?", () =>
                    {
                        Client.ChangeSource(newSource);
                    }, () =>
                    {
                        sourcesList.SelectedItem = "(" + Client.ClientSource.Type + ") " + Client.ClientSource.Name;
                    });
                }
            };
            this.Get<RoundedImageButton>("ChangeClientParentDirectory").ButtonPressed += (sender, args) =>
            {
                Task.Run(async () =>
                {
                    // Prompt for the directory.
                    var dialog = new OpenFolderDialog();
                    dialog.Directory = this.CurrentParentDirectory;
                    var newDirectory = await dialog.ShowAsync(this.GetWindow());
                    if (newDirectory == null) return;
                    
                    // Move the clients.
                    ConfirmPrompt.OpenPrompt("Changing install locations will move any clients you have downloaded to it. Continue?", () =>
                    {
                        Client.ChangeParentDirectory(newDirectory);
                        this.systemInfo.Settings.ClientParentLocation = newDirectory;
                        this.UpdateSettings();
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
            
            // Update the sources list.
            var sources = Client.ClientSourcesList.Select(source => "(" + source.Type + ") " + source.Name).ToList();
            this.sourcesList.Items = sources;
            this.sourcesList.PlaceholderText = "(" + Client.ClientSource.Type + ") " + Client.ClientSource.Name;
            
            // Update the parent directory.
            this.parentDirectoryDisplay.Text = this.CurrentParentDirectory;
        }
    }
}