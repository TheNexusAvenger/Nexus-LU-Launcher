﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Nexus.LU.Launcher.Gui.Component.Base;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client.Patch;
using Nexus.LU.Launcher.State.Enum;

namespace Nexus.LU.Launcher.Gui.Component.Patches;

public class PatchEntry : Border
{
    /// <summary>
    /// Patch states that show an install button.
    /// </summary>
    public readonly List<ExtendedPatchState> InstallButtonStates = new List<ExtendedPatchState>()
    {
        ExtendedPatchState.NotInstalled,
        ExtendedPatchState.FailedToInstall,
    };
    
    /// <summary>
    /// Patch states that show an uninstall button.
    /// </summary>
    public readonly List<ExtendedPatchState> UninstallButtonStates = new List<ExtendedPatchState>()
    {
        ExtendedPatchState.Installed,
        ExtendedPatchState.UpdateAvailable,
        ExtendedPatchState.FailedToUninstall,
        ExtendedPatchState.CheckingForUpdates,
        ExtendedPatchState.UpdatesCheckFailed,
        ExtendedPatchState.FailedToUpdate,
    };
    
    /// <summary>
    /// Patch states that show an update button.
    /// </summary>
    public readonly List<ExtendedPatchState> UpdateButtonStates = new List<ExtendedPatchState>()
    {
        ExtendedPatchState.UpdateAvailable,
        ExtendedPatchState.FailedToUpdate,
    };

    /// <summary>
    /// Status messages for the states.
    /// </summary>
    public readonly Dictionary<ExtendedPatchState, string> StatusMessages = new Dictionary<ExtendedPatchState, string>()
    {
        { ExtendedPatchState.Installing, "Installing..." },
        { ExtendedPatchState.FailedToInstall, "Failed to install." },
        { ExtendedPatchState.Uninstalling, "Uninstalling..." },
        { ExtendedPatchState.FailedToUninstall, "Failed to uninstall." },
        { ExtendedPatchState.CheckingForUpdates, "Checking for updates..." },
        { ExtendedPatchState.UpdatesCheckFailed, "Updates check failed." },
        { ExtendedPatchState.UpdateAvailable, "Update available." },
        { ExtendedPatchState.Updating, "Updating..." },
        { ExtendedPatchState.FailedToUpdate, "Failed to update." },
    };
    
    /// <summary>
    /// Color of the install button.
    /// </summary>
    private static readonly SolidColorBrush InstallColor = new SolidColorBrush(new Color(255, 0, 205, 0));
        
    /// <summary>
    /// Color of the uninstall button.
    /// </summary>
    private static readonly SolidColorBrush UninstallColor = new SolidColorBrush(new Color(255, 205, 0, 0));
        
    /// <summary>
    /// Data of the patch.
    /// </summary>
    public ExtendedClientPatch PatchData
    {
        get => GetValue(PatchDataProperty);
        set => SetValue(PatchDataProperty, value);
    }
    public static readonly StyledProperty<ExtendedClientPatch> PatchDataProperty = AvaloniaProperty.Register<Window, ExtendedClientPatch>(nameof(PatchData));

    /// <summary>
    /// Name of the patch.
    /// </summary>
    private readonly TextBlock patchName;
        
    /// <summary>
    /// Description of the patch.
    /// </summary>
    private readonly TextBlock patchDescription;
    
    /// <summary>
    /// Button for the install.
    /// </summary>
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
        
        // Connect changing the patch.
        this.PropertyChanged += (sender, args) =>
        {
            if (args.Property != PatchDataProperty) return;
            this.Run(this.ConnectPatch);
        };
        
        // Connect the buttons.
        this.installButton.ButtonPressed += ((sender, args) =>
        {
            if (InstallButtonStates.Contains(this.PatchData.State))
            {
                Task.Run(this.PatchData.InstallAsync);
            }
            else if (UninstallButtonStates.Contains(this.PatchData.State))
            {
                Task.Run(this.PatchData.UninstallAsync);
            }
        });
        this.updateButton.ButtonPressed += ((sender, args) =>
        {
            if (UpdateButtonStates.Contains(this.PatchData.State))
            {
                Task.Run(this.PatchData.InstallAsync);
            }
        });
    }

    /// <summary>
    /// Connects the patch.
    /// </summary>
    private void ConnectPatch()
    {
        var patch = this.PatchData;
        this.patchName.Text = patch.Name;
        this.patchDescription.Text = patch.Description;
        patch.StateChanged += (patchState) =>
        {
            this.Run(this.UpdateButtons);
        };
        this.UpdateButtons();
    }

    /// <summary>
    /// Updates the buttons.
    /// </summary>
    private void UpdateButtons()
    {
        // Update the install button.
        var state = this.PatchData.State;
        if (InstallButtonStates.Contains(state))
        {
            this.installButton.IsVisible = true;
            this.installButton.Color = InstallColor;
            this.installText.Text = "Install";
        }
        else if (UninstallButtonStates.Contains(state))
        {
            this.installButton.IsVisible = true;
            this.installButton.Color = UninstallColor;
            this.installText.Text = "Uninstall";
        }
        else
        {
            this.installButton.IsVisible = false;
        }
        
        // Update the update button.
        this.updateButton.IsVisible = this.UpdateButtonStates.Contains(state);

        // Update the status text.
        if (StatusMessages.TryGetValue(state, out var statusMessage))
        {
            this.statusText.Text = statusMessage;
            this.statusText.IsVisible = true;
        }
        else
        {
            this.statusText.IsVisible = false;
        }
    }
}