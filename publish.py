"""
TheNexusAvenger

Creates the binaries for distribution.
"""

PROJECTS = [
    "GUI",
]
PLATFORMS = [
    ["Windows-x64","win-x64"],
    ["macOS-x64","osx-x64"],
    # ["macOS-ARM64","osx-arm64"], # TODO: LU works on Apple M1. .NET 6 required for macOS ARM64 (still in preview). Also need to find a tester to validate.
    ["Linux-x64","linux-x64"],
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
for project in PROJECTS:
    for platform in PLATFORMS:
        # Compile the project for the platform.
        print("Exporting " + project + " for " + platform[0])
        subprocess.call(["dotnet","publish","-r",platform[1],"-c","Release","NLUL." + project + "/NLUL." + project + ".csproj"],stdout=open(os.devnull,"w"))

        # Clear the unwanted files of the compile.
        dotNetVersion = os.listdir("NLUL." + project + "/bin/Release/")[0]
        outputDirectory = "NLUL." + project + "/bin/Release/" + dotNetVersion + "/" + platform[1] + "/publish"
        for file in os.listdir(outputDirectory):
            if file.endswith(".pdb"):
                os.remove(outputDirectory + "/" + file)

        # Rename the GUI executables.
        if project == "GUI":
            linuxOutputFile = outputDirectory + "/Nexus-LU-Launcher"
            windowsOutputFile = outputDirectory + "/Nexus-LU-Launcher.exe"
            if os.path.exists(linuxOutputFile):
                os.remove(linuxOutputFile)
            elif os.path.exists(windowsOutputFile):
                os.remove(windowsOutputFile)
            if os.path.exists(outputDirectory + "/NLUL.GUI"):
                os.rename(outputDirectory + "/NLUL.GUI",linuxOutputFile)
            elif os.path.exists(outputDirectory + "/NLUL.GUI.exe"):
                os.rename(outputDirectory + "/NLUL.GUI.exe",windowsOutputFile)

        # Create the archive.
        shutil.make_archive("bin/NLUL-" + project + "-" + platform[0],"zip","NLUL." + project + "/bin/Release/" + dotNetVersion + "/" + platform[1] + "/publish")

# Clear the existing macOS GUI release.
if os.path.exists("bin/NLUL-GUI-macOS-x64.zip"):
    print("Clearing the macOS x64 NLUL-GUI release.")
    os.remove("bin/NLUL-GUI-macOS-x64.zip")

# Package the macOS release.
print("Packaging macOS release.")
dotNetVersion = os.listdir("NLUL.GUI/bin/Release/")[0]
shutil.copytree("NLUL.GUI/bin/Release/" + dotNetVersion + "/osx-x64/publish","bin/NLUL-GUI-macOS-x64/Nexus LU Launcher.app/Contents/MacOS")
os.mkdir("bin/NLUL-GUI-macOS-x64/Nexus LU Launcher.app/Contents/Resources")
shutil.copy("packaging/macOS/NexusLULauncherLogo.icns","bin/NLUL-GUI-macOS-x64/Nexus LU Launcher.app/Contents/Resources/NexusLULauncherLogo.icns")
shutil.copy("packaging/macOS/Info.plist","bin/NLUL-GUI-macOS-x64/Nexus LU Launcher.app/Contents/Info.plist")
shutil.make_archive("bin/NLUL-GUI-macOS-x64","zip","bin/NLUL-GUI-macOS-x64")
shutil.rmtree("bin/NLUL-GUI-macOS-x64")

# Rename the GUI releases to be more clear.
# The GUI releases are expected to be used by more users.
for platform in PLATFORMS:
    os.rename("bin/NLUL-GUI-" + platform[0] + ".zip","bin/Nexus-LU-Launcher-" + platform[0] + ".zip")