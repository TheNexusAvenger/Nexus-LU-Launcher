/*
 * TheNexusAvenger
 *
 * Displays information about a patch.
 */

using System;
using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using NLUL.Core.Client.Patch;
using NLUL.GUI.Component.Base;

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
        public PatchData PatchData
        {
            get { return GetValue(PatchDataProperty); }
            set { SetValue(PatchDataProperty,value); }
        }
        public static readonly StyledProperty<PatchData> PatchDataProperty = AvaloniaProperty.Register<Window,PatchData>(nameof(PatchData));
        
        public ClientPatcher Patcher
        {
            get { return GetValue(PatcherProperty); }
            set { SetValue(PatcherProperty,value); }
        }
        public static readonly StyledProperty<ClientPatcher> PatcherProperty = AvaloniaProperty.Register<Window,ClientPatcher>(nameof(Patcher));

        public static readonly SolidColorBrush InstallColor = new SolidColorBrush(new Color(255,0,205,0));
        public static readonly SolidColorBrush RemoveColor = new SolidColorBrush(new Color(255,205,0,0));
        
        private PatchState patchState = PatchState.NotInstalled;
        private TextBlock patchName;
        private TextBlock patchDescription;
        private RoundedButton installButton;
        private TextBlock installText;
        private RoundedButton updateButton;
        private TextBlock statusText;
        
        /*
         * Creates a patch entry.
         */
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
        
        /*
         * Checks for updates.
         */
        private void CheckForUpdates()
        {
            // Set the state as uninstalled if the mod isn't installed.
            if (!this.Patcher.IsInstalled(this.PatchData.PatchEnum))
            {
                this.SetState(PatchState.NotInstalled);
                return;
            }
            
            // Set the initial state for checking for updates.
            this.SetState(PatchState.CheckingForUpdates);
            
            // Start checking for updates.
            // Fetching is done outside the thread to prevent "Call from invalid thread" exceptions.
            var patcher = this.Patcher;
            var patchData = this.PatchData;
            new Thread(() =>
            {
                try
                {
                    // Set the state based on if there is an update.
                    if (patcher.IsUpdateAvailable(patchData.PatchEnum))
                    {
                        this.SetStateConditionally(PatchState.UpdateAvailable,PatchState.CheckingForUpdates);
                    }
                    else
                    {
                        this.SetStateConditionally(PatchState.Installed,PatchState.CheckingForUpdates);
                    }
                }
                catch (Exception e)
                {
                    // Set as no updates (failed to fetch).
                    this.SetStateConditionally(PatchState.UpdatesCheckFailed,PatchState.CheckingForUpdates);
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Debug.WriteLine("Failed to fetch update for " + patchData.PatchName + " because: " + e);
                    });
                }
            }).Start();
        }
        
        /*
         * Attempts to install the mod.
         */
        private void Install()
        {
            // Set the initial state.
            this.SetState(PatchState.Installing);
            
            // Attempt to install.
            // Fetching is done outside the thread to prevent "Call from invalid thread" exceptions.
            var patcher = this.Patcher;
            var patchData = this.PatchData;
            new Thread(() =>
            {
                try
                {
                    // Install the patch.
                    patcher.Install(patchData.PatchEnum);
                    this.SetStateConditionally(PatchState.Installed,PatchState.Installing);
                }
                catch (Exception e)
                {
                    // Display the install failed.
                    this.SetStateConditionally(PatchState.FailedToInstall,PatchState.Installing);
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Debug.WriteLine("Failed to install for " + patchData.PatchName + " because: " + e);
                    });
                }
            }).Start();
        }
        
        /*
         * Attempts to uninstall the mod.
         */
        private void Uninstall()
        {
            // Set the initial state.
            this.SetState(PatchState.Uninstalling);
            
            // Attempt to uninstall.
            // Fetching is done outside the thread to prevent "Call from invalid thread" exceptions.
            var patcher = this.Patcher;
            var patchData = this.PatchData;
            new Thread(() =>
            {
                try
                {
                    // Uninstall the patch.
                    patcher.Uninstall(patchData.PatchEnum);
                    this.SetStateConditionally(PatchState.NotInstalled,PatchState.Uninstalling);
                }
                catch (Exception e)
                {
                    // Display the uninstall failed.
                    this.SetStateConditionally(PatchState.FailedToUninstall,PatchState.Uninstalling);
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Debug.WriteLine("Failed to uninstall for " + patchData.PatchName + " because: " + e);
                    });
                }
            }).Start();
        }
        
        /*
         * Attempts to update the mod.
         */
        private void Update()
        {
            // Set the initial state.
            this.SetState(PatchState.Updating);
            
            // Attempt to update.
            // Fetching is done outside the thread to prevent "Call from invalid thread" exceptions.
            var patcher = this.Patcher;
            var patchData = this.PatchData;
            new Thread(() =>
            {
                try
                {
                    // Install the patch.
                    patcher.Install(patchData.PatchEnum);
                    this.SetStateConditionally(PatchState.Installed,PatchState.Updating);
                }
                catch (Exception e)
                {
                    // Display the install failed.
                    this.SetStateConditionally(PatchState.FailedToUpdate,PatchState.Updating);
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Debug.WriteLine("Failed to update for " + patchData.PatchName + " because: " + e);
                    });
                }
            }).Start();
        }
        
        /*
         * Sets the state.
         */
        private void SetState(PatchState state)
        {
            this.patchState = state;
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.UpdatePatch();
            });
        }
        
        /*
         * Sets the state if the current state matches.
         */
        private void SetStateConditionally(PatchState state,PatchState currentState)
        {
            if (this.patchState == currentState)
            {
                this.SetState(state);
            }
        }

        /*
         * Updates the patch entry.
         */
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
                this.installButton.Color = RemoveColor;
                this.installText.Text = "Remove";
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
                this.installButton.Color = RemoveColor;
                this.installText.Text = "Uninstall";
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Failed to uninstall.";
            }
            else if (this.patchState == PatchState.CheckingForUpdates)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = RemoveColor;
                this.installText.Text = "Uninstall";
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Checking for updates.";
            }
            else if (this.patchState == PatchState.UpdatesCheckFailed)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = RemoveColor;
                this.installText.Text = "Uninstall";
                this.updateButton.IsVisible = false;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Updates check failed.";
            }
            else if (this.patchState == PatchState.UpdateAvailable)
            {
                this.installButton.IsVisible = true;
                this.installButton.Color = RemoveColor;
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
                this.installButton.Color = RemoveColor;
                this.installText.Text = "Uninstall";
                this.updateButton.IsVisible = true;
                this.statusText.IsVisible = true;
                this.statusText.Text = "Failed to update.";
            }
            
            // Update the data.
            this.patchName.Text = this.PatchData.PatchName;
            this.patchDescription.Text = this.PatchData.PatchDescription;
        }
    }
}