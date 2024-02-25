using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Client.Archive;
using Nexus.LU.Launcher.State.Client.Patch;
using Nexus.LU.Launcher.State.Client.Runtime;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;
using Nexus.LU.Launcher.State.Util;

namespace Nexus.LU.Launcher.State.Client;

public class ClientState
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly List<string> KnownGoodSignatures = new List<string>()
    {
        // Packed client, rar compressed.
        "8f6c7e84eca3bab93232132a88c4ae6f8367227d7eafeaa0ef9c40e86c14edf5",
        // Packed client, includes extra locales, rar compressed.
        "c1531bf9401426042e8bab2de04ba1b723042dc01d9907c2635033d417de9e05",
        // Unpacked client, includes extra locales, rar compressed.
        "0d862f71eedcadc4494c4358261669721b40b2131101cbd6ef476c5a6ec6775b",
        // Unpacked client, zip compressed.
        // This is not documented by Darkflame Universe as it realistically should not be findable.
        // It is kept around for testing purposes, but it is the reason why the RemoveDluAdPatch exists.
        "f0a67205f82165c27c673b5ba9472536d48a3697ae016889ab891b68922947ef",
    };
    
    /// <summary>
    /// Current state of the launcher.
    /// </summary>
    public LauncherState CurrentLauncherState => this.CurrentLauncherProgress.LauncherState;

    /// <summary>
    /// Current progress of the launcher.
    /// </summary>
    public LauncherProgress CurrentLauncherProgress { get; private set; } = new LauncherProgress()
    {
        LauncherState = LauncherState.Uninitialized,
        ProgressBarState = ProgressBarState.Inactive
    };

    /// <summary>
    /// Event for when the launcher state changes.
    /// </summary>
    public event Action<LauncherState>? LauncherStateChanged;

    /// <summary>
    /// Event for when the launcher progress changes.
    /// This event is fired with every progress bar change. LauncherStateChanged is recommended.
    /// </summary>
    public event Action<LauncherProgress>? LauncherProgressChanged;

    /// <summary>
    /// Event for when a patch is added.
    /// </summary>
    public event Action<ExtendedClientPatch>? PatchAdded;
    
    /// <summary>
    /// List of patches for the launcher.
    /// </summary>
    public readonly List<ExtendedClientPatch> Patches;
    
    /// <summary>
    /// List of runtimes for the client.
    /// </summary>
    public readonly List<IRuntime> Runtimes;

    /// <summary>
    /// List of server entries.
    /// </summary>
    public readonly ServerList ServerList;

    /// <summary>
    /// Static instance of the client state.
    /// </summary>
    private static ClientState? _clientState;

    /// <summary>
    /// Creates the client state.
    /// </summary>
    private ClientState() {
        // Create the server list.
        this.ServerList = new ServerList();
        this.ServerList.ServerListChanged += () =>
        {
            if (this.CurrentLauncherState != LauncherState.NoSelectedServer && this.CurrentLauncherState != LauncherState.ReadyToLaunch) return;
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = (this.ServerList.SelectedEntry == null ? LauncherState.NoSelectedServer : LauncherState.ReadyToLaunch),
            });
        };
        
        // Build the patch list.
        var systemInfo = SystemInfo.GetDefault();
        this.Patches = new List<ExtendedClientPatch>()
        {
            new ExtendedClientPatch(new SteamOneClickPatch(systemInfo)),
            new ExtendedClientPatch(new ModLoaderPatch(systemInfo)),
            new ExtendedClientPatch(new AutoTcpUdpPatch(systemInfo)),
            new ExtendedClientPatch(new TcpUdpPatch(systemInfo)),
            new ExtendedClientPatch(new FixAssemblyVendorHologramPatch(systemInfo)),
            new ExtendedClientPatch(new FixAvantGardensSurvivalCrashPatch(systemInfo)),
            new ExtendedClientPatch(new RemoveDluPatchAd(systemInfo)),
        };
        foreach (var archivePatch in systemInfo.Settings.ArchivePatches)
        {
            this.Patches.Add(new ExtendedClientPatch(new LocalArchivePatch(systemInfo, this.ServerList, archivePatch)));
        }
        
        // Build the runtimes list.
        this.Runtimes = new List<IRuntime>()
        {
            new NativeWindowsRuntime(),
            new MacOsWineRuntime(),
            new UserInstalledWineRuntime(),
        };
        
        // Initialize the state.
        this.Initialize();
    }

    /// <summary>
    /// Returns a static instance of ClientState.
    /// </summary>
    /// <returns>Static instance of the client state.</returns>
    public static ClientState Get()
    {
        if (_clientState == null)
        {
            _clientState = new ClientState();
        }
        return _clientState;
    }

    /// <summary>
    /// Returns the runtime to use for the client based on the current system.
    /// </summary>
    /// <returns>Runtime to use for the client.</returns>
    public IRuntime GetRuntime()
    {
        // Return the first installed runtime, if one exists.
        var installedRuntime = this.Runtimes.FirstOrDefault(runtime => runtime.RuntimeState == RuntimeState.Installed);
        if (installedRuntime != null)
        {
            return installedRuntime;
        }
        
        // Return the first supported runtime.
        return this.Runtimes.First(runtime => runtime.RuntimeState != RuntimeState.Unsupported);
    }

    /// <summary>
    /// Sets the launcher progress.
    /// </summary>
    /// <param name="progress">New progress of the launcher.</param>
    private void SetLauncherProgress(LauncherProgress progress)
    {
        var previousState = this.CurrentLauncherState;
        this.CurrentLauncherProgress = progress;
        this.LauncherProgressChanged?.Invoke(progress);
        if (previousState != progress.LauncherState)
        {
            this.LauncherStateChanged?.Invoke(progress.LauncherState);
        }
    }

    /// <summary>
    /// Initializes the client state from the file system.
    /// </summary>
    private void Initialize() {
        // Set the initial state if the runtime is not downloaded (mainly Linux with WINE).
        var runtime = this.GetRuntime();
        if (runtime.RuntimeState == RuntimeState.ManualInstallRequired)
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.ManualRuntimeNotInstalled,
                AdditionalData = runtime.GetType().Name,
            });
            return;
        }

        // Set the initial state if no client is extracted.
        // Due to legal reasons, an automatic download is not provided.
        var systemInfo = SystemInfo.GetDefault();
        if (!File.Exists(Path.Combine(systemInfo.ClientLocation, "legouniverse.exe")))
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.PendingExtractSelection,
            });
            return;
        }

        // Set the initial state if an automated runtime needs to be installed (mainly macOS with WINE).
        // The user extracting the client will automatically handle this state afterwards.
        if (runtime.RuntimeState == RuntimeState.NotInstalled)
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.RuntimeNotInstalled,
                AdditionalData = runtime.GetType().Name,
            });
            return;
        }

        // Prepare the client to play.
        if (this.ServerList.SelectedEntry == null)
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.NoSelectedServer,
            });
        }
        else
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.ReadyToLaunch,
            });
        }
    }

    /// <summary>
    /// Checks the signature of an archive.
    /// </summary>
    /// <param name="archivePath">Path of the archive to check.</param>
    /// <returns>Whether the signature is valid or not.</returns>
    public async Task<bool> CheckSignatureAsync(string archivePath)
    {
        // Set the state as checking the signature.
        Logger.Info($"Check signature of \"{archivePath}\".");
        this.SetLauncherProgress(new LauncherProgress()
        {
            LauncherState = LauncherState.CheckingSignature,
            ProgressBarState = ProgressBarState.Progressing,
        });
        
        // Get the signature.
        var sha256 = SHA256.Create();
        var checksum = await sha256.ComputeHashAsync(File.OpenRead(archivePath));
        var hash = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
        Logger.Info($"Signature is \"{hash}\".");
        if (!KnownGoodSignatures.Contains(hash))
        {
            Logger.Warn("Signature is not known.");
        }
        
        // Reset the state and return the result.
        this.SetLauncherProgress(new LauncherProgress()
        {
            LauncherState = LauncherState.PendingExtractSelection,
        });
        return KnownGoodSignatures.Contains(hash);
    }
    
    /// <summary>
    /// Extracts the client.
    /// </summary>
    /// <param name="archivePath">Path to the archive to extract from.</param>
    public async Task ExtractAsync(string archivePath)
    {
        // Set the state as archiving.
        Logger.Info($"Extracting client from \"{archivePath}\".");
        this.SetLauncherProgress(new LauncherProgress()
        {
            LauncherState = LauncherState.ExtractingClient,
        });
        
        // Get the archive.
        var archive = ClientArchive.GetArchive(archivePath);
        if (archive == null)
        {
            Logger.Error($"Archive \"{archivePath}\" is not a supported archive or does not contain LEGO Universe.");
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.ExtractFailed,
                AdditionalData = "InvalidArchive",
            });
            return;
        }
        
        // Extract the files.
        Logger.Info($"Extracting client using {archive.GetType().Name}.");
        var clientLocation = SystemInfo.GetDefault().SystemFileLocation;
        archive.ExtractProgress += (progress) =>
        {
            if (this.CurrentLauncherState != LauncherState.ExtractingClient) return;
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.ExtractingClient,
                ProgressBarState = ProgressBarState.PercentFill,
                ProgressBarFill = progress,
            });
        };
        try
        {
            archive.ExtractTo(clientLocation);
        }
        catch (Exception e)
        {
            Logger.Error($"An error occured extracting the files. Make sure you have enough space and try again.\n{e}");
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.ExtractFailed,
                AdditionalData = "ExceptionWhileExtracting",
            });
            return;
        }
        
        // Verify the files.
        Logger.Info("Verifying client.");
        this.SetLauncherProgress(new LauncherProgress()
        {
            LauncherState = LauncherState.VerifyingClient,
            ProgressBarState = ProgressBarState.Progressing,
        });
        if (!archive.Verify(clientLocation))
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.VerifyFailed,
            });
            return;
        }
        
        // Download the runtime.
        var runtime = this.GetRuntime();
        if (runtime.RuntimeState == RuntimeState.NotInstalled)
        {
            Logger.Debug($"Installing runtime {runtime.GetType().Name}.");
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.InstallingRuntime,
                ProgressBarState = ProgressBarState.Progressing,
                AdditionalData = runtime.GetType().Name,
            });
            await runtime.InstallAsync();
            Logger.Info($"Installed runtime {runtime.GetType().Name}.");
        }
        
        // Apply the default patches.
        Logger.Info("Applying patches.");
        this.SetLauncherProgress(new LauncherProgress()
        {
            LauncherState = LauncherState.PatchingClient,
            ProgressBarState = ProgressBarState.Progressing,
        });
        foreach (var patch in this.Patches)
        {
            await patch.RefreshAsync();
            if (!patch.ApplyByDefault)
            {
                Logger.Debug($"Patch {patch.Name} is ignored since it is not applied by default.");
                continue;
            }
            if (patch.State != ExtendedPatchState.NotInstalled)
            {
                Logger.Debug($"Patch {patch.Name} is ignored because it is {patch.State}.");
            }
            await patch.InstallAsync();
        }
        
        // Re-initialize the state.
        this.Initialize();
        Logger.Info($"Client is now {this.CurrentLauncherState}.");
    }
    
    /// <summary>
    /// Moves the client parent directory.
    /// </summary>
    /// <param name="destination">Destination directory of the clients.</param>
    public async Task MoveClientParentDirectoryAsync(string destination)
    {
        // Create the parent directory.
        this.SetLauncherProgress(new LauncherProgress()
        {
            LauncherState = LauncherState.MovingClient,
            ProgressBarState = ProgressBarState.Progressing,
        });
        var systemInfo = SystemInfo.GetDefault();
        var existingParentDirectory = systemInfo.SystemFileLocation;
        Directory.CreateDirectory(destination);

        // Set the parent directory.
        systemInfo.Settings.ClientParentLocation = destination;
        systemInfo.SaveSettings();

        // Move the clients.
        foreach (var clientDirectory in Directory.GetDirectories(existingParentDirectory))
        {
            var clientDirectoryName = new DirectoryInfo(clientDirectory).Name;
            if (File.Exists(Path.Combine(clientDirectory, "legouniverse.exe")))
            {
                DirectoryExtensions.Move(clientDirectory, Path.Combine(destination, clientDirectoryName));
            }
        }

        // Reload the patches and state.
        foreach (var patch in this.Patches)
        {
            await patch.RefreshAsync();
        }
        this.Initialize();
    }

    /// <summary>
    /// Adds an archive patch.
    /// An exception is thrown if the patch can't be added.
    /// </summary>
    /// <param name="archivePath">Path of the archive.</param>
    /// <returns>The patch created from the archive.</returns>
    public async Task<ExtendedClientPatch> AddArchivePatchAsync(string archivePath)
    {
        // Throw an exception a patch with the same archive name exists.
        var archiveFileName = Path.GetFileName(archivePath);
        var systemInfo = SystemInfo.GetDefault();
        if (systemInfo.Settings.ArchivePatches.FirstOrDefault(patch => patch.ArchiveName == archiveFileName) != null)
        {
            Logger.Error($"Archive {archiveFileName} already exists.");
            throw new InvalidOperationException("Patch_LocalArchivePatch_AlreadyExistsPrompt");
        }
        
        // Read the patch file.
        string? patchJson = null;
        try
        {
            using var archiveFile = ZipFile.OpenRead(archivePath);
            var patchJsonEntry = archiveFile.Entries.FirstOrDefault(entry => entry.FullName == "patch.json");
            if (patchJsonEntry != null)
            {
                patchJson = await (new StreamReader(patchJsonEntry.Open())).ReadToEndAsync();
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to read archive {archiveFileName}.\n{e}");
            throw new InvalidOperationException("Patch_LocalArchivePatch_ArchiveFileNotReadablePrompt");
        }
        if (patchJson == null)
        {
            Logger.Error($"Archive {archiveFileName} does not have patch.json.");
            throw new InvalidOperationException("Patch_LocalArchivePatch_PatchFileNotFoundPrompt");
        }
        
        // Parse the archive file.
        ArchivePatchJson? data ;
        try
        {
            data = JsonSerializer.Deserialize<ArchivePatchJson>(patchJson, ArchivePatchJsonJsonContext.Default.ArchivePatchJson);
        }
        catch (Exception e)
        {
            Logger.Error($"Archive {archiveFileName} is unreadable.\n{e}");
            throw new InvalidOperationException("Patch_LocalArchivePatch_PatchFileNotReadablePrompt");
        }
        if (data == null)
        {
            Logger.Error($"Archive {archiveFileName} is unreadable.");
            throw new InvalidOperationException("Patch_LocalArchivePatch_PatchFileNotReadablePrompt");
        }
        if (data.Name == null || data.Description == null)
        {
            Logger.Error($"Archive {archiveFileName} patch.json file is invalid.");
            throw new InvalidOperationException("Patch_LocalArchivePatch_PatchFileInvalidPrompt");
        }
        
        // Create the patch and verify it can be applied.
        var archivePatchData = new ArchivePatch()
        {
            ArchiveName = archiveFileName,
            Name = data.Name,
            Description = data.Description,
            Requirements = data.Requirements,
        };
        var archivePatch = new LocalArchivePatch(systemInfo, this.ServerList, archivePatchData);
        archivePatch.VerifyRequirements();
        
        // Add and return the patch.
        var extendedClientPatch = new ExtendedClientPatch(archivePatch);
        var archiveParentDirectory = Path.GetDirectoryName(archivePatch.ArchivePath)!;
        if (!Directory.Exists(archiveParentDirectory))
        {
            Directory.CreateDirectory(archiveParentDirectory);
        }
        File.Copy(archivePath, archivePatch.ArchivePath, true);
        systemInfo.Settings.ArchivePatches.Add(archivePatchData);
        systemInfo.SaveSettings();
        this.Patches.Add(extendedClientPatch);
        this.PatchAdded?.Invoke(extendedClientPatch);
        return extendedClientPatch;
    }
    
    /// <summary>
    /// Launches the client.
    /// </summary>
    /// <param name="host">Host to launch.</param>
    /// <returns>Process that was started.</returns>
    public async Task<Process?> LaunchAsync(ServerEntry host)
    {
        // Set up the runtime if it isn't installed.
        var runtime = this.GetRuntime();
        if (runtime.RuntimeState == RuntimeState.NotInstalled)
        {
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.InstallingRuntime,
                ProgressBarState = ProgressBarState.Progressing,
                AdditionalData = runtime.GetType().Name,
            });
            await runtime.InstallAsync();
        }
        else if (runtime.RuntimeState != RuntimeState.Installed)
        {
            // Stop the launch if a valid runtime isn't set up.
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.ManualRuntimeNotInstalled,
                AdditionalData = runtime.GetType().Name,
            });
            return null;
        }
        
        // Modify the boot file.
        var systemInfo = SystemInfo.GetDefault();
        var bootConfigLocation = Path.Combine(systemInfo.ClientLocation, "boot.cfg");
        LegoDataDictionary bootConfig = null!;
        try
        {
            bootConfig = await LegoDataDictionary.FromFileAsync(bootConfigLocation);
        }
        catch (FormatException)
        {
            bootConfig = await LegoDataDictionary.FromFileAsync(Path.Combine(systemInfo.ClientLocation, "boot_backup.cfg"));
        }
        bootConfig.Set("SERVERNAME", host.ServerName);
        bootConfig.Set("AUTHSERVERIP", host.ServerAddress);
        bootConfig.Set("LOCALE", systemInfo.Settings.Locale);
        await File.WriteAllTextAsync(bootConfigLocation,bootConfig.ToString());
        
        // Apply any pre-launch patches.
        Logger.Info("Preparing pre-launch patches.");
        this.SetLauncherProgress(new LauncherProgress()
        {
            LauncherState = LauncherState.Launching,
            ProgressBarState = ProgressBarState.Progressing,
        });
        foreach (var patch in this.Patches)
        {
            await patch.OnClientRequestLaunchAsync();
        }
        
        // Launch the client.
        Logger.Info($"Launching with {host.ServerName} ({host.ServerAddress})");
        try
        {
            var clientProcess = runtime.RunApplication(Path.Combine(systemInfo.ClientLocation, "legouniverse.exe"), systemInfo.ClientLocation);
            clientProcess.Start();
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.Launched,
            });
            return clientProcess;
        }
        catch (Exception e)
        {
            Logger.Error($"Launching the client failed.\n{e}");
            this.SetLauncherProgress(new LauncherProgress()
            {
                LauncherState = LauncherState.Launched,
            });
            return null;
        }
    }

    /// <summary>
    /// Launches the selected client.
    /// </summary>
    /// <returns>Process that was started.</returns>
    public async Task<Process?> LaunchAsync()
    {
        return await this.LaunchAsync(this.ServerList.SelectedEntry!);
    }
}