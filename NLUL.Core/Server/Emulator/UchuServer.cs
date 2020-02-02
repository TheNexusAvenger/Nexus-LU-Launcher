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
using System.Xml.Serialization;
using NLUL.Core.Server.Prerequisite;

namespace NLUL.Core.Server.Emulator
{
    /*
     * Database section of the Uchu config.
     */
    public class UchuDatabase
    {
        public string Provider = "postgres";
        public string Database = "uchu";
        public string Host = "localhost";
        public string Username = "postgres";
        public string Password = "postgres";
    }
    
    /*
     * ConsoleLogging section of the Uchu config.
     */
    public class UchuConsoleLogging
    {
        public string Level = "Debug";
    }
    
    /*
     * FileLogging section of the Uchu config.
     */
    public class UchuFileLogging
    {
        public string Level = "None";
        public string Logfile = "uchu.log";
    }
    
    /*
     * DllSource section of the Uchu config.
     */
    public class UchuDllSource
    {
        public string ServerDllSourcePath = "../../../../";
        public string DotNetPath = "dotnet";
        public string ScriptDllSource = "Uchu.StandardScripts";
    }
    
    /*
     * ManagedScriptSources section of the Uchu config.
     */
    public class UchuManagedScriptSources
    {
        
    }
    
    /*
     * ResourcesConfiguration section of the Uchu config.
     */
    public class UchuResourcesConfiguration
    {
        public string GameResourceFolder = "/res";
    }
    
    /*
     * UchuNetworking section of the Uchu config.
     */
    public class UchuNetworking
    {
        public string Certificate;
        public string Hostname;
        public string CharacterPort = "2002";
    }
    
    /*
     * Uchu XML config structure.
     */
    [XmlRoot(ElementName = "Uchu")]
    public class UchuConfig
    {
        public UchuDatabase Database = new UchuDatabase();
        public UchuConsoleLogging ConsoleLogging = new UchuConsoleLogging();
        public UchuFileLogging FileLogging = new UchuFileLogging();
        public UchuDllSource DllSource = new UchuDllSource();
        public UchuManagedScriptSources ManagedScriptSources = new UchuManagedScriptSources();
        public UchuResourcesConfiguration ResourcesConfiguration = new UchuResourcesConfiguration();
        public UchuNetworking Networking = new UchuNetworking();
    }
    
    public class UchuServer : IEmulator
    {
        private const string BUILD_MODE = "Debug";
        private const string DOTNET_APP_VERSION = "netcoreapp3.1";
        
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
            File.Delete(Path.Combine(this.ServerInfo.ServerFileLocation,"Server.zip"));
            
            // Clean up the extracted directories.
            var infectedRoseDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"InfectedRose");
            if (Directory.Exists(infectedRoseDirectory))
            {
                Directory.Delete(infectedRoseDirectory,true);
            }
            var rakDotNetDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"RakDotNet");
            if (Directory.Exists(rakDotNetDirectory))
            {
                Directory.Delete(rakDotNetDirectory, true);
            }
        }
        
        /*
         * Creates the initial server configuration.
         */
        private void CreateConfig()
        {
            // Create the base config.
            var config = new UchuConfig();
            
            // Set the values that are specific to the install.
            config.DllSource.DotNetPath = Path.GetFullPath(Path.Combine(this.ServerInfo.ServerFileLocation,"Tools","dotnet3.1","dotnet"));
            config.ResourcesConfiguration.GameResourceFolder = Path.GetFullPath(Path.Combine(this.ServerInfo.ClientLocation,"res"));
            
            // Write the config.
            var configLocation = Path.Combine(this.ServerInfo.ServerFileLocation,"Server","Uchu-master","Uchu.Master","bin",BUILD_MODE,DOTNET_APP_VERSION,"config.xml");
            var configSerializer = new XmlSerializer(typeof(UchuConfig));  
            var configWriter = new StreamWriter(configLocation);  
            configSerializer.Serialize(configWriter,config);  
            configWriter.Close();  
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
            
            // TODO: Verify client is unpacked.
            
            // Clear previous files.
            Console.WriteLine("Clearing old files");
            this.CleanupFiles();
            
            // Remove the previous server.
            var serverOldDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"Server");
            if (Directory.Exists(serverOldDirectory))
            {
                Directory.Delete(serverOldDirectory,true);
            }
            
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
            var targetServerZip = Path.Combine(this.ServerInfo.ServerFileLocation,"Server.zip");
            var targetServerDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"Server");
            client.DownloadFile("https://github.com/yuwui/Uchu/archive/master.zip",targetServerZip);
            ZipFile.ExtractToDirectory(targetServerZip,targetServerDirectory);
            Directory.Delete(Path.Combine(targetServerDirectory,"Uchu-master","InfectedRose"),true);
            Directory.Delete(Path.Combine(targetServerDirectory,"Uchu-master","RakDotNet"),true);
            
            // Download and extract InfectedRose.
            Console.WriteLine("Downloading the latest InfectedRose library from GitHub/Wincent01/InfectedRose");
            var targetInfectedRoseDownloadZip = Path.Combine(this.ServerInfo.ServerFileLocation,"InfectedRose.zip");
            var targetInfectedRoseDownloadDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"InfectedRose");
            client.DownloadFile("https://github.com/Wincent01/InfectedRose/archive/master.zip",targetInfectedRoseDownloadZip);
            ZipFile.ExtractToDirectory(targetInfectedRoseDownloadZip,targetInfectedRoseDownloadDirectory);
            Directory.Move(Path.Combine(targetInfectedRoseDownloadDirectory,"InfectedRose-master"),Path.Combine(targetServerDirectory,"Uchu-master","InfectedRose"));
            
            // Download and extract InfectedRose.
            Console.WriteLine("Downloading the latest InfectedRose library from GitHub/yuwui/RakDotNet");
            var targetRakDotNetDownloadZip = Path.Combine(this.ServerInfo.ServerFileLocation,"RakDotNet.zip");
            var targetRakDotNetDownloadDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"RakDotNet");
            client.DownloadFile("https://github.com/yuwui/RakDotNet/archive/3.25/tcpudp.zip",targetRakDotNetDownloadZip);
            ZipFile.ExtractToDirectory(targetRakDotNetDownloadZip,targetRakDotNetDownloadDirectory);
            Directory.Move(Path.Combine(targetRakDotNetDownloadDirectory,"RakDotNet-3.25-tcpudp"),Path.Combine(targetServerDirectory,"Uchu-master","RakDotNet"));

            // Compile the server.
            Console.WriteLine("Building the server.");
            var buildDirectory = Path.Combine(targetServerDirectory,"Uchu-master");
            var buildProcess = new Process();
            buildProcess.StartInfo.WorkingDirectory = buildDirectory;
            buildProcess.StartInfo.FileName = dotNetExecutableLocation;
            buildProcess.StartInfo.CreateNoWindow = true;
            buildProcess.StartInfo.Arguments = "build -c " + BUILD_MODE;
            buildProcess.Start();
            buildProcess.WaitForExit();
            buildProcess.Close();

            // Clean up the download files.
            Console.WriteLine("Cleaning up files.");
            // this.CleanupFiles();
            
            // Create the default configuration.
            Console.WriteLine("Creating the default configuration.");
            this.CreateConfig();
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