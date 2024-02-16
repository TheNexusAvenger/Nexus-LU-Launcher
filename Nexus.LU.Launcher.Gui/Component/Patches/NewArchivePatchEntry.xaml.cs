using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Nexus.LU.Launcher.Gui.Component.Base;
using Nexus.LU.Launcher.Gui.Component.Prompt;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client;

namespace Nexus.LU.Launcher.Gui.Component.Patches;

public class NewArchivePatchEntry : Border
{
    /// <summary>
    /// Creates a new archive patch entry.
    /// </summary>
    public NewArchivePatchEntry()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);
        
        // Apply the text.
        var localization = Localization.Get();
        var addButton = this.Get<RoundedButton>("AddArchivePatchButton");
        localization.LocalizeText(this.Get<TextBlock>("AddArchivePatchButtonText"));
        localization.LocalizeWidth(addButton, "Patch_AddButton");
        
        // Connect the button.
        addButton.ButtonPressed += (sender, args) =>
        {
            // Prompt for the file.
            var window = this.GetWindow()!;
            var openFileTask =  window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = localization.GetLocalizedString("Patch_ArchiveFilePickerTitle"),
                FileTypeFilter = new List<FilePickerFileType>() { new FilePickerFileType(localization.GetLocalizedString("Patch_ArchiveFilePickerFileType"))
                    {
                        Patterns = new List<string>() { "*.zip" },
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
                
                // Add and install the archive.
                try
                {
                    // Add the patch.
                    var newPatch = await ClientState.Get().AddArchivePatchAsync(archiveLocation.Path.LocalPath);
                    
                    // Prompt applying the patch.
                    ConfirmPrompt.OpenPrompt(localization.GetLocalizedString("Patch_LocalArchivePatch_ConfirmInstallOnAddPrompt"), () => Task.Run(newPatch.InstallAsync));
                }
                catch (Exception e)
                {
                    NotificationPrompt.OpenPrompt(localization.GetLocalizedString(e.Message));
                }
            });
        };
    }
}