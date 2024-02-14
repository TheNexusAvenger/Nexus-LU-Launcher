## Client Download Changes
As of October 3rd, 2021, the hosting of the client files was taken down and
there are no plans to set up new mirrors of the client. V.0.3.X and lower no
longer work if the client was not downloaded before, and V.0.4.0 and newer will
require you to find the client archive yourself.

# Nexus LU Launcher
Nexus LU (LEGO Universe) Launcher is a custom, cross-platform
user interface for installing and launching LEGO Universe
for community-run LEGO Universe servers.

![Launcher example](images/launcher.png)

## Running
For all platforms, a LEGO Universe client archive is required.
Instructions on how to get one are not provided.

### Windows
[Download and extract `Nexus-LU-Launcher-Windows-x64.zip` from the releases and run the executable.](https://github.com/TheNexusAvenger/Nexus-LU-Launcher/releases/latest)
Compatibility for Windows 8.1 and older is not guarenteed.

### macOS
[Download and extract `Nexus-LU-Launcher-macOS-x64.zip` from the releases and run the application.](https://github.com/TheNexusAvenger/Nexus-LU-Launcher/releases/latest)
macOS 10.15 or newer is required.

### Linux
Linux releases are provided under the releases, [but the Flatpak is recommended](https://flathub.org/apps/io.thenexusavenger.Nexus-LU-Launcher).
It includes WINE and creates a desktop entry for launching.

```bash
flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo
flatpak install io.thenexusavenger.Nexus-LU-Launcher
```

#### Valve Steam Deck
There is an optional patch named "Steam One-Click" as of version 2.2.0. It
will attempt to:
- Add Nexus LU Launcher to Steam.
- Prompt to change the controller layout.
- Turn off fullscreen, since it can cause the mouse cursor to disappear in game mode.

**A manual restart of Steam (such as changing from desktop to game mode) is
required for the controller layout prompt to appear,** or Nexus LU Launcher
must already be added to Steam. **Turning off fullscreen is also done after the
first launch of the client** due to the settings file not being there. Consider
doing the first launch in desktop mode, and then switch to game mode.

No artwork is currently provided for the launcher, and uninstalling the patch will
have no effect.

## Goals
The goals of the launcher is the following:
* Allow for launching the client on Windows, macOS, and Linux.
* Use a user interface similar to the original LEGO Universe launcher.
* Automate the process of extracting the client, installing patches,
  and launching LEGO Universe.
* Be able to select from a list of servers to connect to.
* Enable the installation of optional patches.

## Non-Goals
The following aren't current goals of the launcher, but could
be made at some point:
* Provide a method to change the locale.
  * Looking for additional translations to be provided before this is implemented.
* Unpacking the client for use with server hosting.
* Automate installing WINE for non-Windows and non-macOS installs.
  * The Flatpak release handles this for Linux.

## Custom Download Location
By default, Nexus LU Launcher will download files to a directory named
`.nlul` in your user directory. This can be changed in the "Settings" tab.

# Building
Nexus LEGO Universe Launcher requires .NET 8.0 to be installed
since it allows packaging as single files without the requirement of
decompressing files. After cloning the repository **with the submodules**,
building can be done with the `dotnet build`:
```bash
dotnet build
```
or `dotnet publish` command:
```bash
dotnet publish
```

For creating the distributables in, there is a Python script that builds the
launcher releases for win-x64, osx-x64, and linux-x64:
```bash
python publish.py
cd bin/ # The ZIP files of the distributables will be in bin/ of the repository.
```

Each release needs to be created for their platform.
- **For macOS**, the version of macOS compiled on is the minimum version
  of macOS the release will work on.
- **For Linux**, the version of `glibc` on the system will become the minimum
  version of `glibc` the release will work on. If you have Docker, `python3 publish-docker.py`
  will perform a build with RedHat UBI8 (uses `glibc` 2.28).

# Additions
## Patches
Patches can be added and will be approved in pull requests if there
is a proper justification to have them. They can including finishing incomplete
features on the client, like guilds, or components that allow the client
to work, like alternative communication mods.

## Servers *(Cancelled)*
Creating servers has been removed from the project as of release 0.3.0.

# Disclaimer
LEGO<sup>Ⓡ</sup> is a trademark of the LEGO Group. The LEGO Group is not
affiliated with this project, has not endorsed or authorized its operation,
and is not liable for any safety issues in relation to its operation.
