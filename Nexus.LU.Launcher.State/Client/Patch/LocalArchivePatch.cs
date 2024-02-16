using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client.Patch;

public class LocalArchivePatch : IClientPatch
{
    /// <summary>
    /// Whether to apply the patch by default.
    /// </summary>
    public bool ApplyByDefault => false;

    /// <summary>
    /// State of the patch.
    /// </summary>
    public PatchState State { get; private set; }

    /// <summary>
    /// Event for the state changing.
    /// </summary>
    public event Action<PatchState>? StateChanged;

    /// <summary>
    /// Archive patch data.
    /// </summary>
    public readonly ArchivePatch ArchivePatch;
    
    /// <summary>
    /// Path of the archive file.
    /// </summary>
    public string ArchivePath => Path.Combine(this.systemInfo.SystemFileLocation, "PatchArchives", this.ArchivePatch.ArchiveName);

    /// <summary>
    /// System info of the client.
    /// </summary>
    private readonly SystemInfo systemInfo;
    
    /// <summary>
    /// List of the servers.
    /// </summary>
    private readonly ServerList serverList;
    
    /// <summary>
    /// Creates the patch.
    /// </summary>
    /// <param name="systemInfo">System info of the client.</param>
    /// <param name="serverList">List of servers.</param>
    /// <param name="archivePatch">Archive patch data to use.</param>
    public LocalArchivePatch(SystemInfo systemInfo, ServerList serverList, ArchivePatch archivePatch)
    {
        this.systemInfo = systemInfo;
        this.serverList = serverList;
        this.ArchivePatch = archivePatch;
        this.State = this.ArchivePatch.Installed ? PatchState.Installed : PatchState.NotInstalled;
    }

    /// <summary>
    /// Verifies the requirements of the patch are met. Throws an exception if they aren't.
    /// </summary>
    public void VerifyRequirements()
    {
        if (this.ArchivePatch.Requirements == null) return;
        
        // Verify the client is packed.
        var packedClientFiles = Path.Combine(this.systemInfo.ClientLocation, "res", "pack");
        if (this.ArchivePatch.Requirements.Contains("packed-client") && !Directory.Exists(packedClientFiles))
        {
            throw new InvalidOperationException("Prompt_LocalArchivePatch_PackedClientRequired");
        }
        
        // Verify the client is unpacked.
        if (this.ArchivePatch.Requirements.Contains("unpacked-client") && Directory.Exists(packedClientFiles))
        {
            throw new InvalidOperationException("Prompt_LocalArchivePatch_UnpackedClientRequired");
        }
    }

    /// <summary>
    /// Refreshes the patch state.
    /// </summary>
    public Task RefreshAsync()
    {
        if (!this.systemInfo.Settings.ArchivePatches.Contains(this.ArchivePatch))
        {
            // Set the state as incompatible if the patch was removed.
            this.State = PatchState.Incompatible;
        }
        else
        {
            // Read the state from the installed state.
            // Reading the contents of files is not done to save I/O operations.
            this.State = this.ArchivePatch.Installed ? PatchState.Installed : PatchState.NotInstalled;
        }
        this.StateChanged?.Invoke(this.State);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Installs the patch.
    /// </summary>
    public async Task InstallAsync()
    {
        // Verify the patch can be applied.
        this.VerifyRequirements();
        
        // Back up the files and replace with the new version.
        var originalFilesPath = Path.Combine(this.systemInfo.ClientLocation, "originalFiles");
        if (!Directory.Exists(originalFilesPath))
        {
            Directory.CreateDirectory(originalFilesPath);
        }
        using var archive = ZipFile.OpenRead(this.ArchivePath);
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith('/')) continue;
            if (entry.FullName == "patch.json") continue;
            
            // Add the server list entry if boot.cfg is included.
            if (entry.FullName == "boot.cfg")
            {
                try
                {
                    var bootConfig = LegoDataDictionary.FromString(await new StreamReader(entry.Open()).ReadToEndAsync());
                    this.serverList.AddEntry(new ServerEntry()
                    {
                        ServerName = bootConfig.Get<string>("SERVERNAME"),
                        ServerAddress = bootConfig.Get<string>("AUTHSERVERIP"),
                    });
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to add server list entry.\n{e}");
                }
                continue;
            }
            
            // Back up the file.
            var targetFilePath = Path.Combine(this.systemInfo.ClientLocation, entry.FullName);
            var backupFilePath = Path.Combine(originalFilesPath, entry.FullName);
            if (File.Exists(targetFilePath) && !File.Exists(backupFilePath))
            {
                Logger.Debug($"Backing up {entry.FullName}");
                var backupFileDirectory = Directory.GetParent(backupFilePath)!.FullName;
                if (!Directory.Exists(backupFileDirectory))
                {
                    Directory.CreateDirectory(backupFileDirectory);
                }
                File.Copy(targetFilePath, backupFilePath);
            }
            
            // Replace the file.
            Logger.Debug($"Replacing {entry.FullName}");
            var targetFileDirectory = Directory.GetParent(targetFilePath)!.FullName;
            if (!Directory.Exists(targetFileDirectory))
            {
                Directory.CreateDirectory(targetFileDirectory);
            }
            entry.ExtractToFile(targetFilePath, true);
        }
        
        // Set the patch as installed.
        this.ArchivePatch.Installed = true;
        this.systemInfo.SaveSettings();
        this.StateChanged?.Invoke(PatchState.Installed);
    }

    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public Task UninstallAsync()
    {
        // Revert the files.
        var originalFilesPath = Path.Combine(this.systemInfo.ClientLocation, "originalFiles");
        using var archive = ZipFile.OpenRead(this.ArchivePath);
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith('/')) continue;
            if (entry.FullName == "patch.json" || entry.FullName == "boot.cfg") continue;

            var targetFilePath = Path.Combine(this.systemInfo.ClientLocation, entry.FullName);
            var backupFilePath = Path.Combine(originalFilesPath, entry.FullName);
            if (File.Exists(targetFilePath) && File.Exists(backupFilePath))
            {
                Logger.Debug($"Reverting {entry.FullName}");
                File.Copy(backupFilePath, targetFilePath, true);
            } else if (File.Exists(targetFilePath))
            {
                Logger.Debug($"Deleting {entry.FullName}");
                File.Delete(targetFilePath);
            }
        }

        // Set the patch as not installed.
        this.ArchivePatch.Installed = false;
        this.systemInfo.SaveSettings();
        this.StateChanged?.Invoke(PatchState.NotInstalled);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes the patch.
    /// </summary>
    public void Remove()
    {
        this.systemInfo.Settings.ArchivePatches.Remove(this.ArchivePatch);
        if (File.Exists(this.ArchivePath))
        {
            File.Delete(this.ArchivePath);
        }
        this.systemInfo.SaveSettings();
        this.RefreshAsync().Wait();
    }
}