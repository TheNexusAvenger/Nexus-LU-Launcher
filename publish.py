"""
TheNexusAvenger

Creates the binaries for distribution.
"""

import os
import platform
import shutil
import subprocess

MACOS_PACKAGE_BUILDS = [
    ["macOS-x64", "osx-x64"],
    ["macOS-ARM64", "osx-arm64"],
]

# Set the platforms.
if platform.system() == "Windows":
    print("Building for Windows.")
    buildMode = "Windows"
    PLATFORMS = [
        ["Windows-x64", "win-x64"],
    ]
elif platform.system() == "Darwin":
    print("Building for macOS.")
    buildMode = "macOS"
    PLATFORMS = [
        ["macOS-x64", "osx-x64"],
        ["macOS-ARM64", "osx-arm64"],
    ]
elif platform.system() == "Linux":
    print("Building for Linux.")
    buildMode = "Linux"
    PLATFORMS = [
        ["Linux-x64", "linux-x64"],
    ]
else:
    print("Unsupported platform: " + platform.system())
    exit(1)
print("")


"""
Clears a publish directory of unwanted files.
"""
def cleanDirectory(directory):
    for file in os.listdir(directory):
        if file.endswith(".pdb"):
            os.remove(directory + "/" + file)


# Create the directory.
if os.path.exists("bin"):
    shutil.rmtree("bin")
os.mkdir("bin")

# Compile the releases.
for platformData in PLATFORMS:
    # Compile the project for the platform.
    print("Exporting for " + platformData[0])
    subprocess.call(["dotnet", "publish", "-r", platformData[1], "-c", "Release", "Nexus.LU.Launcher.Gui/Nexus.LU.Launcher.Gui.csproj"])

    # Clear the unwanted files of the compile.
    dotNetVersion = os.listdir("Nexus.LU.Launcher.Gui/bin/Release/")[0]
    outputDirectory = "Nexus.LU.Launcher.Gui/bin/Release/" + dotNetVersion + "/" + platformData[1] + "/publish"
    for file in os.listdir(outputDirectory):
        if file.endswith(".pdb"):
            os.remove(outputDirectory + "/" + file)
    if len(os.listdir(outputDirectory)) == 0 or (not os.path.exists(outputDirectory + "/Nexus.LU.Launcher.Gui") and not os.path.exists(outputDirectory + "/Nexus.LU.Launcher.Gui.exe")):
        print("Build for " + platformData[0] + " failed and will not be created.")
        continue

    # Rename the GUI executables.
    linuxOutputFile = outputDirectory + "/Nexus-LU-Launcher"
    windowsOutputFile = outputDirectory + "/Nexus-LU-Launcher.exe"
    if os.path.exists(linuxOutputFile):
        os.remove(linuxOutputFile)
    elif os.path.exists(windowsOutputFile):
        os.remove(windowsOutputFile)
    if os.path.exists(outputDirectory + "/Nexus.LU.Launcher.Gui"):
        os.rename(outputDirectory + "/Nexus.LU.Launcher.Gui", linuxOutputFile)
    elif os.path.exists(outputDirectory + "/Nexus.LU.Launcher.Gui.exe"):
        os.rename(outputDirectory + "/Nexus.LU.Launcher.Gui.exe", windowsOutputFile)

    # Create the archive.
    shutil.make_archive("bin/Nexus-LU-Launcher-" + platformData[0], "zip", "Nexus.LU.Launcher.Gui/bin/Release/" + dotNetVersion + "/" + platformData[1] + "/publish")

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

# Write any files about limitations.
if buildMode == "macOS":
    with open("bin/requirements-maxos.txt", "w") as file:
        file.write("The following macOS version was used and will be the minimum version that will work with this relase:\n")
        file.write(platform.mac_ver()[0])
elif buildMode == "Linux":
    with open("bin/requirements-linux.txt", "w") as file:
        file.write("The following glibc version was used and will be the minimum version that will work with this relase:\n")
        file.write(subprocess.check_output(["ldd",  "--version"]).decode())
