"""
TheNexusAvenger

Creates the binaries for distribution.
"""

PLATFORMS = [
    ["Windows-x64", "win-x64"],
    ["macOS-x64", "osx-x64"],
    ["macOS-ARM64", "osx-arm64"],
    ["Linux-x64", "linux-x64"],
]
MACOS_PACKAGE_BUILDS = [
    ["macOS-x64", "osx-x64"],
    ["macOS-ARM64", "osx-arm64"],
]

import os
import shutil
import subprocess
import sys


"""
Clears a publish directory of unwanted files.
"""
def cleanDirectory(directory):
    for file in os.listdir(directory):
        if file.endswith(".pdb"):
            os.remove(directory + "/" + file)



# Display a warning for Windows runs.
if os.name == "nt":
    sys.stderr.write("Windows was detected. Linux and macOS binaries will be missing the permissions to run.\n")
else:
    sys.stderr.write("Windows was not detected. Windows binaries will create a command prompt window when opening.\n")

# Create the directory.
if os.path.exists("bin"):
    shutil.rmtree("bin")
os.mkdir("bin")

# Compile the releases.
for platform in PLATFORMS:
    # Compile the project for the platform.
    print("Exporting for " + platform[0])
    subprocess.call(["dotnet", "publish", "-r", platform[1], "-c", "Release", "Nexus.LU.Launcher.Gui/Nexus.LU.Launcher.Gui.csproj"])

    # Clear the unwanted files of the compile.
    dotNetVersion = os.listdir("Nexus.LU.Launcher.Gui/bin/Release/")[0]
    outputDirectory = "Nexus.LU.Launcher.Gui/bin/Release/" + dotNetVersion + "/" + platform[1] + "/publish"
    for file in os.listdir(outputDirectory):
        if file.endswith(".pdb"):
            os.remove(outputDirectory + "/" + file)
    linuxOutputFile = outputDirectory + "/Nexus-LU-Launcher"
    windowsOutputFile = outputDirectory + "/Nexus-LU-Launcher.exe"
    if len(os.listdir(outputDirectory)) == 0 or (not os.path.exists(linuxOutputFile) and not os.path.exists(windowsOutputFile)):
        print("Build for " + platform[0] + " failed and will not be created.")
        continue

    # Rename the GUI executables.
    if os.path.exists(linuxOutputFile):
        os.remove(linuxOutputFile)
    elif os.path.exists(windowsOutputFile):
        os.remove(windowsOutputFile)
    if os.path.exists(outputDirectory + "/Nexus.LU.Launcher.Gui"):
        os.rename(outputDirectory + "/Nexus.LU.Launcher.Gui", linuxOutputFile)
    elif os.path.exists(outputDirectory + "/Nexus.LU.Launcher.Gui.exe"):
        os.rename(outputDirectory + "/Nexus.LU.Launcher.Gui.exe", windowsOutputFile)

    # Create the archive.
    shutil.make_archive("bin/Nexus-LU-Launcher-" + platform[0], "zip", "Nexus.LU.Launcher.Gui/bin/Release/" + dotNetVersion + "/" + platform[1] + "/publish")

# Package the macOS release.
dotNetVersion = os.listdir("Nexus.LU.Launcher.Gui/bin/Release/")[0]
for macOsBuild in MACOS_PACKAGE_BUILDS:
    if not os.path.exists("Nexus.LU.Launcher.Gui/bin/Release/" + dotNetVersion + "/" + macOsBuild[1] + "/publish/Nexus-LU-Launcher"):
        continue
    print("Packaging macOS release for " + macOsBuild[0] + ".")
    shutil.copytree("Nexus.LU.Launcher.Gui/bin/Release/" + dotNetVersion + "/" + macOsBuild[1] + "/publish", "bin/Nexus-LU-Launcher-" + macOsBuild[0] + "/Nexus LU Launcher.app/Contents/MacOS")
    os.mkdir("bin/Nexus-LU-Launcher-" + macOsBuild[0] + "/Nexus LU Launcher.app/Contents/Resources")
    shutil.copy("packaging/macOS/NexusLULauncherLogo.icns", "bin/Nexus-LU-Launcher-" + macOsBuild[0] + "/Nexus LU Launcher.app/Contents/Resources/NexusLULauncherLogo.icns")
    shutil.copy("packaging/macOS/Info.plist", "bin/Nexus-LU-Launcher-" + macOsBuild[0] + "/Nexus LU Launcher.app/Contents/Info.plist")
    shutil.make_archive("bin/Nexus-LU-Launcher-" + macOsBuild[0],"zip","bin/Nexus-LU-Launcher-" + macOsBuild[0])
    shutil.rmtree("bin/Nexus-LU-Launcher-" + macOsBuild[0])