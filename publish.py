"""
TheNexusAvenger

Creates the binaries for distribution.
Currently only does x64 builds. The need for ARM64
(mainly macOS) is unknown given the client only runs
on x86.
"""

PROJECTS = [
    "CLI",
    "GUI",
]
PLATFORMS = [
    ["Windows-x64","win-x64"],
    ["macOS-x64","osx-x64"],
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