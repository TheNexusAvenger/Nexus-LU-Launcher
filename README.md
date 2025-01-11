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
Compatibility for Windows 8.1 and older is not guaranteed.

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
* Unpacking the client for use with server hosting.
* Automate installing WINE for non-Windows and non-macOS installs.
  * The Flatpak release handles this for Linux.

## Custom Download Location (Non-Flatpak)
By default, Nexus LU Launcher will download files to a directory named
`.nlul` in your user directory. This can be changed in the "Settings" tab.

# Building
Nexus LEGO Universe Launcher requires .NET 9.0 to be installed
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

# Patches
Patches can be added and will be approved in pull requests if there
is a proper justification to have them. They can including finishing incomplete
features on the client, like guilds, or components that allow the client
to work, like alternative communication mods.

## Archive Patches
As of version 2.2.0, patches can be added to the client using ZIP archives.
There are 4 requirements:
1. It must be a ZIP archive (rar, tar.gz, and other formats are not supported).
2. The archive can't only contain a folder containing the all files, typically
   done by selecting "Send To" > "Compressed (zipped) folder" on the folder containing
   the patch files.
3. A valid `patch.json` file exists in the top level of the archive.
4. If a `boot.cfg` is included, make sure the server name and address are correct.

### File Structure
When using "Send To" > "Compressed (zipped) folder" on a folder, it will make
it so only that folder is the top-level item of the archive. For example:
```
MyBadPatch.zip
|-MyBadPatch    <--When the zip is opened, you will see "MyBadPatch" instead of "res" [!]
|---res
|-----myfile1.something
|-----myfile2.something
|---patch.json  <--patch.json is not at the top level [!]
```

Instead, use "Send To" > "Compressed (zipped) folder" on the patch files
directly (NOT the parent folder of them).
```
MyGoodPatch.zip
|-res         <--When the zip is opened, you will see "res" and "patch.json"
|---myfile1.something
|---myfile2.something
|-patch.json  <--patch.json is at the top level
```

### `patch.json`
A `patch.json` file is required in the archive. It can contain the following:
- `name` (required) - Dictionary of names for the locales the user may have.
- `description` (required) - Dictionary of descriptions for the locales the
  user may have.
- `requirements` (optional) - Optional list of requirements the client must
  meet to install. Supported options include:
  - `packed-client` - Requires that the client must be packed.
  - `unpacked-client` - Requires that the client must be unpacked.

LEGO Universe supports `en_US`, `de_DE`, and `en_GB` locales. If a locale is
not provided, a placeholder text will be used.

This is an example `patch.json` with all supported locales and requiring an
unpacked client.
```json
{
  "name": {
    "en_US": "My Cool Patch",
    "de_DE": "My Cool Patch, But German",
    "en_GB": "My Cool Patch, But British"
  },
  "description": {
    "en_US": "My description.",
    "de_DE": "My description, but German.",
    "en_GB": "My description, but British."
  },
  "requirements": [
    "unpacked-client"
  ]
}
```

This is the minimum recommended `patch.json`. It has no requirements.
```json
{
  "name": {
    "en_US": "My Cool Patch"
  },
  "description": {
    "en_US": "My description."
  }
}
```

### `boot.cfg`
Patches can include `boot.cfg`, but it will not replace the stored copy.
Because this file is managed, instead of using the file directly, `SERVERNAME`
and `AUTHSERVERIP` from the file will be added as a server list entry.
Updating addresses for a given name with an updated patch is supported as
well. **Make sure to make `SERVERNAME` (likely) unique, and avoid the default
`Overbuild Universe (US)`.**

# Disclaimer
LEGO<sup>Ⓡ</sup> is a trademark of the LEGO Group. The LEGO Group is not
affiliated with this project, has not endorsed or authorized its operation,
and is not liable for any safety issues in relation to its operation.
