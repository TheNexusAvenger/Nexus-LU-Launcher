/*
 * TheNexusAvenger
 *
 * Sets up and runs an Uchu instance.
 * https://github.com/yuwui/Uchu
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using NLUL.Core.Server.Prerequisite;

namespace NLUL.Core.Server.Emulator
{
    public class UchuServer : IEmulator
    {
        private ServerInfo ServerInfo;
        
        /*
         * Creates an Uchu Server object.
         */
        public UchuServer(ServerInfo info)
        {
            this.ServerInfo = info;
        }
        
        /*
         * Returns the prerequisites for the server.
         */
        public List<IPrerequisite> GetPrerequisites()
        {
            return new List<IPrerequisite>()
            {
                new DotNetCore31(Path.Combine(this.ServerInfo.ServerFileLocation,"Tools"))
            };
            
            // TODO: Detect PostgresSQL install
            // TODO: Redis support?
        }
        
        /*
         * Cleans up files from installs.
         */
        private void CleanupFiles()
        {
            // Clean up the zip files.
            File.Delete(Path.Combine(this.ServerInfo.ServerFileLocation,"InfectedRose.zip"));
            File.Delete(Path.Combine(this.ServerInfo.ServerFileLocation,"RakDotNet.zip"));
            File.Delete(Path.Combine(this.ServerInfo.ServerFileLocation,"ServerDownload.zip"));
            
            // Clean up the extracted directories.
            var infectedRoseDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"InfectedRose");
            if (Directory.Exists(infectedRoseDirectory))
            {
                Directory.Delete(infectedRoseDirectory,true);
            }
            var rakDotNetDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"RakDotNet");
            if (Directory.Exists(rakDotNetDirectory))
            {
                Directory.Delete(rakDotNetDirectory,true);
            }
            var serverDownloadDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"ServerDownload");
            if (Directory.Exists(serverDownloadDirectory))
            {
                Directory.Delete(serverDownloadDirectory,true);
            }
        }
        
        /*
         * Installs the server. Used for both initializing
         * the first time and updating.
         */
        public void Install()
        {
            // Get the tool locations.
            var toolsLocation = Path.Combine(this.ServerInfo.ServerFileLocation,"Tools");
            var dotNetDirectoryLocation = Path.Combine(toolsLocation,"dotnet3.1");
            var dotNetExecutableLocation = Path.Combine(dotNetDirectoryLocation, "dotnet");
            
            // Clear previous files.
            Console.WriteLine("Clearing old files");
            this.CleanupFiles();
            
            // Install additional tools.
            Console.WriteLine("Installing .NET Entity Framework Command Line Interface");
            var entityFrameworkInstallProcess = new Process();
            entityFrameworkInstallProcess.StartInfo.WorkingDirectory = dotNetDirectoryLocation;
            entityFrameworkInstallProcess.StartInfo.FileName = dotNetExecutableLocation;
            entityFrameworkInstallProcess.StartInfo.CreateNoWindow = true;
            entityFrameworkInstallProcess.StartInfo.Arguments = "tool install --global dotnet-ef";
            entityFrameworkInstallProcess.Start();
            entityFrameworkInstallProcess.WaitForExit();
            entityFrameworkInstallProcess.Close();
            
            // Download and extract the server files from master.
            var client = new WebClient();
            Console.WriteLine("Downloading the latest server from GitHub/yuwui/Uchu");
            var targetServerDownloadZip = Path.Combine(this.ServerInfo.ServerFileLocation,"ServerDownload.zip");
            var targetServerDownloadDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"ServerDownload");
            client.DownloadFile("https://github.com/yuwui/Uchu/archive/master.zip",targetServerDownloadZip);
            ZipFile.ExtractToDirectory(targetServerDownloadZip,targetServerDownloadDirectory);
            Directory.Delete(Path.Combine(targetServerDownloadDirectory,"Uchu-master","InfectedRose"),true);
            Directory.Delete(Path.Combine(targetServerDownloadDirectory,"Uchu-master","RakDotNet"),true);
            
            // Download and extract InfectedRose.
            Console.WriteLine("Downloading the latest InfectedRose library from GitHub/Wincent01/InfectedRose");
            var targetInfectedRoseDownloadZip = Path.Combine(this.ServerInfo.ServerFileLocation,"InfectedRose.zip");
            var targetInfectedRoseDownloadDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"InfectedRose");
            client.DownloadFile("https://github.com/Wincent01/InfectedRose/archive/master.zip",targetInfectedRoseDownloadZip);
            ZipFile.ExtractToDirectory(targetInfectedRoseDownloadZip,targetInfectedRoseDownloadDirectory);
            Directory.Move(Path.Combine(targetInfectedRoseDownloadDirectory,"InfectedRose-master"),Path.Combine(targetServerDownloadDirectory,"Uchu-master","InfectedRose"));
            
            // Download and extract InfectedRose.
            Console.WriteLine("Downloading the latest InfectedRose library from GitHub/yuwui/RakDotNet");
            var targetRakDotNetDownloadZip = Path.Combine(this.ServerInfo.ServerFileLocation,"RakDotNet.zip");
            var targetRakDotNetDownloadDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"RakDotNet");
            client.DownloadFile("https://github.com/yuwui/RakDotNet/archive/3.25/tcpudp.zip",targetRakDotNetDownloadZip);
            ZipFile.ExtractToDirectory(targetRakDotNetDownloadZip,targetRakDotNetDownloadDirectory);
            Directory.Move(Path.Combine(targetRakDotNetDownloadDirectory,"RakDotNet-3.25-tcpudp"),Path.Combine(targetServerDownloadDirectory,"Uchu-master","RakDotNet"));

            // Compile the server.
            Console.WriteLine("Building the server.");
            var buildDirectory = Path.Combine(targetServerDownloadDirectory,"Uchu-master");
            var buildProcess = new Process();
            buildProcess.StartInfo.WorkingDirectory = buildDirectory;
            buildProcess.StartInfo.FileName = dotNetExecutableLocation;
            buildProcess.StartInfo.CreateNoWindow = true;
            buildProcess.StartInfo.Arguments = "publish -c Debug";
            buildProcess.Start();
            buildProcess.WaitForExit();
            buildProcess.Close();
            
            // Copy the files to the server directory.
            Console.WriteLine("Creating the server files.");
            var targetServerDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"Server");
            if (Directory.Exists(targetServerDirectory))
            {
                Directory.Delete(targetServerDirectory,true);
            }
            Directory.Move(Path.Combine(targetServerDownloadDirectory,"Uchu-master","Uchu.Master","bin","Debug","netcoreapp3.1","publish"),targetServerDirectory);
            
            // Clean up the download files.
            Console.WriteLine("Cleaning up files.");
            this.CleanupFiles();
        }

        /*
         * Starts the server.
         */
        public void Start()
        {
            
        }
        
        /*
         * Stops the server.
         */
        public void Stop()
        {
            
        }
    }
}