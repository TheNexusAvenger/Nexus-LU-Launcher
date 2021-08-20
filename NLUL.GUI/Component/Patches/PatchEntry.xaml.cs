using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using NLUL.Core.Client.Patch;
using NLUL.GUI.Component.Base;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Patches
{
    public enum PatchState
    {
        NotInstalled,
        Installing,
        Installed,
        FailedToInstall,
        Uninstalling,
        FailedToUninstall,
        CheckingForUpdates,
        UpdatesCheckFailed,
        UpdateAvailable,
        Updating,
        FailedToUpdate,
    }
    
    public class PatchEntry : Border
    {
        /// <summary>
        /// Color of the install button.
        /// </summary>
        private static readonly SolidColorBrush InstallColor = new SolidColorBrush(new Color(255,0,205,0));
        
        /// <summary>
        /// Color of the uninstall button.
        /// </summary>
        private static readonly SolidColorBrush UninstallColor = new SolidColorBrush(new Color(255, 205, 0, 0));
        
        /// <summary>
        /// Data of the patch.
        /// </summary>
        public IPatch PatchData
        {
            get => GetValue(PatchDataProperty);
            set => SetValue(PatchDataProperty, value);
        }
        public static readonly StyledProperty<IPatch> PatchDataProperty = AvaloniaProperty.Register<Window, IPatch>(nameof(PatchData));
        
        /// <summary>
        /// Patcher of the client.
        /// </summary>
        public ClientPatcher Patcher
        {
            get => GetValue(PatcherProperty);
            set => SetValue(PatcherProperty, value);
        }
        public static readonly StyledProperty<ClientPatcher> PatcherProperty = AvaloniaProperty.Register<Window, ClientPatcher>(nameof(Patcher));

        /// <summary>
        /// State of the patch.
        /// </summary>
        private PatchState patchState = PatchState.NotInstalled;
        
        /// <summary>
        /// Name of the patch.
        /// </summary>
        private readonly TextBlock patchName;
        
        /// <summary>
        /// Description of the patch.
        /// </summary>
        private readonly TextBlock patchDescription;
        private readonly RoundedButton installButton;
        
        /// <summary>
        /// Install text of the patch.
        /// </summary>
        private readonly TextBlock installText;
        
        /// <summary>
        /// Update button of the patch.
        /// </summary>
        private readonly RoundedButton updateButton;
        
        /// <summary>
        /// Status text of the patch.
        /// </summary>
        private readonly TextBlock statusText;
        
        /// <summary>
        /// Creates a patch entry.
        /// </summary>
        public PatchEntry()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.patchName = this.Get<TextBlock>("PatchName");
            this.patchDescription = this.Get<TextBlock>("PatchDescription");
            this.installButton = this.Get<RoundedButton>("InstallButton");
            this.installText = this.Get<TextBlock>("InstallText");
            this.updateButton = this.Get<RoundedButton>("UpdateButton");
            this.statusText = this.Get<TextBlock>("StatusText");

            // Connect the events.
            this.PropertyChanged += (sender, args) =>
            {
                if ((args.Property == PatchDataProperty || args.Property == PatcherProperty) && this.Patcher != null && this.PatchData != null)
                {
                    this.CheckForUpdates();
                }
            };
            this.installButton.ButtonPressed += ((sender, args) =>
            {
                if (this.Patcher.IsInstalled(this.PatchData.PatchEnum))
                {
                    this.Uninstall();
                }
                else
                {
                    this.Install();
                }
            });
            this.updateButton.ButtonPressed += ((sender, args) =>
            {
                this.Update();
            });
        }
        
        /// <summary>
        /// Checks for updates.
        /// </summary>
        private void CheckForUpdates()
        {
            // Set the state as uninstalled if the mod isn't installed.
            if (!this.Patcher.IsInstalled(this.PatchData.PatchEnum))
            {
                // Connect refreshing the state when the client finishes patching.
                var patchingStarted = false;
                Client.StateChanged += () =>
                {
                    var state = Client.State;
                    if (state == PlayState.DownloadingClient || state == PlayState.PatchingClient)
                    {
                        // Prepare to update the state after the download/patching ends.
                        patchingStarted = true;
                    }
                    else if (patchingStarted && (state == PlayState.NoSelectedServer || state == PlayState.Ready || state == PlayState.Launching))
                    {
                        // Update the state.
                        patchingStarted = false;
                        CheckForUpdates();
                    }
                };
                
                // Set the initial state as uninstalled.
                this.SetState(PatchState.NotInstalled);
                return;
            }
            
            // Set the initial state for checking for updates.
            this.SetState(PatchState.CheckingForUpdates);
            
            // Start checking for updates.
            // Fetching is done outside the thread to prevent "Call from invalid thread" exceptions.
            var patcher = this.Patcher;
            var patchData = this.PatchData;
            Task.Run(() =>
            {
                try
                {
                    // Set the state based on if there is an update.
                    if (patcher.IsUpdateAvailable(patchData.PatchEnum))
                    {
                        this.SetStateConditionally(PatchState.UpdateAvailable, PatchState.CheckingForUpdates);
                    }
                    else
                    {
                        this.SetStateConditionally(PatchState.Installed, PatchState.CheckingForUpdates);
                    }
                }
                catch (Exception e)
                {
                    // Set as no updates (failed to fetch).
                    this.SetStateConditionally(PatchState.UpdatesCheckFailed, PatchState.CheckingForUpdates);
                    this.Run(() =>
                    {
                        Debug.WriteLine("Failed to fetch update for " + patchData.Name + " because: " + e);
                    });
                }
            });
        }
        
        /// <summary>
        /// Attempts to install the mod.
        /// </summary>
        private void Install()
        {
            // Set the initial state.
            this.SetState(PatchState.Installing);
            
            // Attempt to install.
            // Fetching is done outside the thread to prevent "Call from invalid thread" exceptions.
            var patcher = this.Patcher;
            var patchData = this.PatchData;
            Task.Run(() =>
            {
                try
                {
                    // Install the patch.
                    patcher.Install(patchData.PatchEnum);
                    this.SetStateConditionally(PatchState.Installed, PatchState.Installing);
                }
                catch (Exception e)
                {
                    // Display the install failed.
                    this.SetStateConditionally(PatchState.FailedToInstall, PatchState.Installing);
                    this.Run(() =>
                    {
                        Debug.WriteLine("Failed to install for " + patchData.Name + " because: " + e);
                    });
                }
            });
        }
        
        /// <summary>
        /// Attempts to uninstall the mod.
        /// </summary>
        private void Uninstall()
        {
            // Set the initial state.
            this.SetState(PatchState.Uninstalling);
            
            // Attempt to uninstall.
            // Fetching is done outside the thread to prevent "Call from invalid thread" exceptions.
            var patcher = this.Patcher;
            var patchData = this.PatchData;
            Task.Run(() =>
            {
                try
                {
                    // Uninstall the patch.
                    patcher.Uninstall(patchData.PatchEnum);
                    this.SetStateConditionally(PatchState.NotInstalled, PatchState.Uninstalling);
                }
                catch (Exception e)
                {
                    // Display the uninstall failed.
                    this.SetStateConditionally(PatchState.FailedToUninstall, PatchState.Uninstalling);
                    this.Run(() =>
                    {
                        Debug.WriteLine("Failed to uninstall for " + patchData.Name + " because: " + e);
                    });
                }
            });
        }
        
        /// <summary>
        /// Attempts to update the mod.
        /// </summary>
        private void Update()
        {
            // Set the initial state.
            this.SetState(PatchState.Updating);
            
            // Attempt to update.
            // Fetching is done outside the thread to prevent "Call from invalid thread" exceptions.
            var patcher = this.Patcher;
            var patchData = this.PatchData;
            Task.Run(() =>
            {
                try
                {
                    // Install the patch.
                    patcher.Install(patchData.PatchEnum);
                    this.SetStateConditionally(PatchState.Installed, PatchState.Updating);
                }
                catch (Exception e)
                {
                    // Display the install failed.
                    this.SetStateConditionally(PatchState.FailedToUpdate, PatchState.Updating);
                    this.Run(() =>
                    {
                        Debug.WriteLine("Failed to update for " + patchData.Name + " because: " + e);
                    });
                }
            });
        }
        
        /// <summary>
        /// Sets the state.
        /// </summary>
        /// <param name="state">State to set.</param>
        private void SetState(PatchState state)
        {
            this.patchState = state;
            this.Run(this.UpdatePatch);
        }
        
        /// <summary>
        /// Sets the state if the current state matches.
        /// </summary>
        /// <param name="state">State to set.</param>
        /// <param name="currentState">State required in order to set.</param>
        private void SetStateConditionally(PatchState state,PatchState currentState)
        {
            if (this.patchState == currentState)
            {
                this.SetState(state);
            }
        }

        /// <summary>
        /// Updates the patch entry.
        /// </summary>
        private void UpdatePatch()
        {
            // Return if the patch or patcher is not defined.
            if (this.PatchData == null || this.Patcher == null)
            {
                return;
            }
            
            // Update the button display.
            if (this.patchState == PatchState.NotInstalled)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = InstallColor;
                this.installText.Text = "Install";
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = false;
            }
            else if (this.patchState == PatchState.Installing)
            {
                this.installButton.IsVisible = false;
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Installing...";
            }
            else if (this.patchState == PatchState.Installed)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = UninstallColor;
                this.installText.Text = "Uninstall";
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = false;
            }
            else if (this.patchState == PatchState.FailedToInstall)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = InstallColor;
                this.installText.Text = "Install";
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Failed to install.";
            }
            else if (this.patchState == PatchState.Uninstalling)
            {
                this.installButton.IsVisible = false;
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Uninstalling...";
            }
            else if (this.patchState == PatchState.FailedToUninstall)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = UninstallColor;
                this.installText.Text = "Uninstall";
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Failed to uninstall.";
            }
            else if (this.patchState == PatchState.CheckingForUpdates)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = UninstallColor;
                this.installText.Text = "Uninstall";
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Checking for updates.";
            }
            else if (this.patchState == PatchState.UpdatesCheckFailed)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = UninstallColor;
                this.installText.Text = "Uninstall";
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Updates check failed.";
            }
            else if (this.patchState == PatchState.UpdateAvailable)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = UninstallColor;
                this.installText.Text = "Uninstall";
                this.updateButton.IsVisible = true;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Update available.";
            }
            else if (this.patchState == PatchState.Updating)
            {
                this.installButton.IsVisible = false;
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Updating...";
            }
            else if (this.patchState == PatchState.FailedToUpdate)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = UninstallColor;
                this.installText.Text = "Uninstall";
                this.updateButton.IsVisible = true;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Failed to update.";
            }
            
            // Update the data.
            this.patchName.Text = this.PatchData.Name;
            this.patchDescription.Text = this.PatchData.Description;
        }
    }
}