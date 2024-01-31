using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;
using Nexus.LU.Launcher.State.Util;

namespace Nexus.LU.Launcher.State.Client.Patch;

public class TcpUdpPatch : IClientPatch
{
    /// <summary>
    /// Name of the patch.
    /// </summary>
    public string Name => "TCP/UDP Shim";
        
    /// <summary>
    /// Description of the patch.
    /// </summary>
    public string Description => "Enables connecting to community-run LEGO Universe servers that use TCP/UDP, like Uchu. This mod prevents connecting to normal RakNet servers, like Darkflame Universe. Requires the Mod Loader to be installed. Can't be installed with Auto TCP/UDP Shim.";
    
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
    /// Location of the mod folder.
    /// </summary>
    private string ModFolderLocation => Path.Join(this.systemInfo.ClientLocation, "mods", "raknet_replacer");
    
    /// <summary>
    /// Location of the mod.
    /// </summary>
    private string ModLocation => Path.Join(ModFolderLocation, "mod.dll");

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
    public TcpUdpPatch(SystemInfo systemInfo)
    {
        this.systemInfo = systemInfo;
        Task.Run(async () =>
        {
            try
            {
                this.latestTag = await GitHubUtil.GetLatestTagAsync("lcdr/raknet_shim_dll");
            }
            catch (Exception)
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
        else if (!File.Exists(this.ModLocation) || this.systemInfo.GetPatchStore("AutoTcpUdp", "InstalledVersion") != null)
        {
            this.State = PatchState.NotInstalled;
        }
        else if (this.latestTagCheckFailed)
        {
            this.State = PatchState.UpdatesCheckFailed;
        }
        else if (this.latestTag != this.systemInfo.GetPatchStore("TcpUdp", "InstalledVersion"))
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
        
        // Throw an exception if AutoTcpUdp is installed.
        if (this.systemInfo.GetPatchStore("AutoTcpUdp", "InstalledVersion") != null)
        {
            throw new InvalidOperationException("AutoTcpUdp already installed.");
        }
        
        // Create the mod directory.
        if (!Directory.Exists(this.ModFolderLocation))
        {
            Directory.CreateDirectory(this.ModFolderLocation);
        }
        
        // Remove the existing mod.dll.
        if (File.Exists(this.ModLocation))
        {
            File.Delete(this.ModLocation);
        }
        
        // Download the mod.
        var client = new HttpClient();
        await client.DownloadFileAsync("https://github.com/lcdr/raknet_shim_dll/releases/download/" + this.latestTag + "/mod.dll", this.ModLocation);

        // Save installed version.
        this.systemInfo.SetPatchStore("TcpUdp", "InstalledVersion", this.latestTag);
        this.systemInfo.SaveSettings();
        await this.RefreshAsync();
    }

    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public async Task UninstallAsync()
    {
        // Remove the mod directory.
        Directory.Delete(this.ModFolderLocation, true);
        
        // Save that no version is installed.
        this.systemInfo.SetPatchStore("TcpUdp", "InstalledVersion", null);
        this.systemInfo.SaveSettings();
        await this.RefreshAsync();
    }
}