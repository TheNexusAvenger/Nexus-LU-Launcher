using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.Core;
using NLUL.GUI.Component.Base;

namespace NLUL.GUI.Component.Settings
{
    public class SettingsView : Panel
    {
        /// <summary>
        /// Information about the system.
        /// </summary>
        private SystemInfo systemInfo = SystemInfo.GetDefault();

        /// <summary>
        /// Button for toggling the logs.
        /// </summary>
        private RoundedImageButton logsToggle;
        
        /// <summary>
        /// Creates a settings view.
        /// </summary>
        public SettingsView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.logsToggle = this.Get<RoundedImageButton>("LogsToggle");
            this.UpdateSettings();
            
            // Connect the events.
            this.logsToggle.ButtonPressed += (sender, args) =>
            {
                this.systemInfo.Settings.LogsEnabled = !this.systemInfo.Settings.LogsEnabled;
                this.systemInfo.SaveSettings();
                this.UpdateSettings();
            };
        }

        /// <summary>
        /// Updates the displayed settings.
        /// </summary>
        public void UpdateSettings()
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
        }
    }
}