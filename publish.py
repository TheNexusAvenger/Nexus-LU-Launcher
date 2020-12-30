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
for project in PROJECTS:
    for platform in PLATFORMS:
        # Compile the project for the platform.
        print("Exporting " + project + " for " + platform[0])
        subprocess.call(["dotnet","publish","-r",platform[1],"-c","Release","NLUL." + project + "/NLUL." + project + ".csproj"])

        # Clear the unwanted files of the compile.
        dotNetVersion = os.listdir("NLUL." + project + "/bin/Release/")[0]
        outputDirectory = "NLUL." + project + "/bin/Release/" + dotNetVersion + "/" + platform[1] + "/publish"
        for file in os.listdir(outputDirectory):
            if file.endswith(".pdb"):
                os.remove(outputDirectory + "/" + file)

        # Create the archive.
        shutil.make_archive("bin/NLUL-" + project + "-" + platform[0],"zip","NLUL." + project + "/bin/Release/" + dotNetVersion + "/" + platform[1] + "/publish")