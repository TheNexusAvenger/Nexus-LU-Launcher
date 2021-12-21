## No Longer Functional
As of October 3rd, 2021, the hosting of the client files was taken down and
there are no plans to set up new mirrors of the client. V.0.3.X and lower no
longer work if the client was not downloaded before, and V.0.4.0 and newer will
require you to find the client archive yourself.

# Nexus LU Launcher
Nexus LU (LEGO Universe) Launcher is a custom, cross-platform
user interface for installing and launching LEGO Universe
for community-run LEGO Universe servers.

![Launcher example](images/launcher.png)

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
  * This feature could be worked on but requires work on the client to enable
    localization. The primary blocker for this is translations for the client.
* Unpacking the client for use with server hosting.
* Automate installing WINE for non-Windows and non-macOS installs.

## Custom Download Location
By default, Nexus LU Launcher will download files to a directory named
`.nlul` in your user directory. This can be changed in the "Settings" tab.

# Building
Nexus LEGO Universe Launcher requires .NET 5.0 to be installed
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
CLI and GUI projects for win-x64, osx-x64, and linux-x64:
```bash
python publish.py
cd bin/ # The ZIP files of the distributables will be in bin/ of the repository.
```

**For distributing non-Windows builds, make sure to run the script on
macOS or Linux.** Otherwise, the executable permissions will be missing.

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
