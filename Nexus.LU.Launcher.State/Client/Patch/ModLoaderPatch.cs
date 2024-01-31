using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;
using Nexus.LU.Launcher.State.Util;

namespace Nexus.LU.Launcher.State.Client.Patch;

public class ModLoaderPatch : IClientPatch
{
    /// <summary>
    /// Name of the patch.
    /// </summary>
    public string Name => "Mod Loader";
        
    /// <summary>
    /// Description of the patch.
    /// </summary>
    public string Description => "Allows the installation of client mods.";

    /// <summary>
    /// Whether to apply the patch by default.
    /// </summary>
    public bool ApplyByDefault => false;

    /// <summary>
    /// State of the patch.
    /// </summary>
    public PatchState State { get; private set; } = PatchState.Loading;

    /// <summary>
    /// Event for the state changing.
    /// </summary>
    public event Action<PatchState>? StateChanged;
    
    /// <summary>
    /// System info of the client.
    /// </summary>
    private readonly SystemInfo systemInfo;

    /// <summary>
    /// Latest tag of the mod loaders in GitHub.
    /// </summary>
    private string? latestTag;

    /// <summary>
    /// Whether the update check failed.
    /// </summary>
    private bool latestTagCheckFailed = false;
    
    /// <summary>
    /// Creates the patch.
    /// </summary>
    /// <param name="systemInfo">System info of the client.</param>
    public ModLoaderPatch(SystemInfo systemInfo)
    {
        this.systemInfo = systemInfo;
        Task.Run(async () =>
        {
            try
            {
                this.latestTag = await GitHubUtil.GetLatestTagAsync("lcdr/raknet_shim_dll");
            }
            catch (Exception e)
            {
                this.latestTagCheckFailed = true;
            }
            await this.RefreshAsync();
        });
    }
    
    /// <summary>
    /// Refreshes the patch state.
    /// </summary>
    public Task RefreshAsync()
    {
        if (this.latestTag == null && !this.latestTagCheckFailed)
        {
            this.State = PatchState.Loading;
        }
        else if (!File.Exists(Path.Join(this.systemInfo.ClientLocation, "dinput8.dll")))
        {
            this.State = PatchState.NotInstalled;
        }
        else if (this.latestTagCheckFailed)
        {
            this.State = PatchState.UpdatesCheckFailed;
        }
        else if (this.latestTag != this.systemInfo.GetPatchStore("ModLoader", "InstalledVersion"))
        {
            this.State = PatchState.UpdateAvailable;
        }
        else
        {
            this.State = PatchState.Installed;
        }
        this.StateChanged?.Invoke(this.State);
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Installs the patch.
    /// </summary>
    public async Task InstallAsync()
    {
        // Throw an exception if there is no latest tag.
        if (this.latestTag == null)
        {
            throw new InvalidOperationException("Latest tag does not exist.");
        }
        
        // Download the mod loader ZIP.
        var client = new HttpClient();
        var modDownloadDirectory = Path.Combine(this.systemInfo.SystemFileLocation, "modloader.zip"); 
        var modUncompressedDirectory = Path.Combine(this.systemInfo.SystemFileLocation, "modloader");
        await client.DownloadFileAsync("https://github.com/lcdr/raknet_shim_dll/releases/download/" + this.latestTag + "/mod.zip", modDownloadDirectory);
        
        // Decompress the mod loader.
        if (Directory.Exists(modUncompressedDirectory))
        {
            Directory.Delete(modUncompressedDirectory, true);
        }
        ZipFile.ExtractToDirectory(modDownloadDirectory, modUncompressedDirectory);
        
        // Remove the existing dinput8.dll.
        var dinput8Location = Path.Join(this.systemInfo.ClientLocation, "dinput8.dll");
        if (File.Exists(dinput8Location))
        {
            File.Delete(dinput8Location);
        }

        // Replace the dinput8.dll file.
        var dinput8DownloadLocation = Directory.GetFiles(modUncompressedDirectory, "dinput8.dll", SearchOption.AllDirectories)[0];
        File.Move(dinput8DownloadLocation, dinput8Location);
        
        // Create the mods directory if it doesn't exist.
        var modsDirectory = Path.Join(this.systemInfo.ClientLocation, "mods");
        if (!Directory.Exists(modsDirectory))
        {
            Directory.CreateDirectory(modsDirectory);
        }
        
        // Save installed version.
        this.systemInfo.SetPatchStore("ModLoader", "InstalledVersion", this.latestTag);
        this.systemInfo.SaveSettings();
        
        // Clear the downloaded files.
        File.Delete(modDownloadDirectory);
        Directory.Delete(modUncompressedDirectory, true);
        await this.RefreshAsync();
    }
    
    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public async Task UninstallAsync()
    {
        // Remove the mod loader DLL.
        File.Delete(Path.Combine(this.systemInfo.ClientLocation, "dinput8.dll"));
            
        // Remove the mods directory if it is empty.
        var modsDirectory = Path.Join(this.systemInfo.ClientLocation, "mods");
        if (Directory.GetDirectories(modsDirectory).Length == 0 && Directory.GetFiles(modsDirectory).Length == 0)
        {
            Directory.Delete(modsDirectory);
        }
        
        // Save that no version is installed.
        this.systemInfo.SetPatchStore("ModLoader", "InstalledVersion", null);
        this.systemInfo.SaveSettings();
        await this.RefreshAsync();
    }
}