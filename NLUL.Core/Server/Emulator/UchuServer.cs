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
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using NLUL.Core.Server.Prerequisite;
using NLUL.Core.Server.Util;
using Formatting = Newtonsoft.Json.Formatting;

namespace NLUL.Core.Server.Emulator
{
    /*
     * State of the Uchu server.
     */
    public class UchuState
    {
        public string CurrentVersion;
        public int ProcessId = 0;
        public string GitRemote = "UchuServer/Uchu";
        public string GitBranch = "master";
        public Dictionary<string,object> ConfigOverrides = new Dictionary<string,object>();
    }

    public class UchuServer : IEmulator
    {
        private const string BUILD_MODE = "Debug";
        private const string DOTNET_APP_VERSION = "netcoreapp3.1";
        
        private ServerInfo ServerInfo;
        private UchuState State;
        
        /*
         * Merges an element with a set of overrides.
         */
        public static void MergeXmlWithOverrides(XmlDocument document,XmlElement element,Dictionary<string,object> overrides)
        {
            // Merge the values.
            foreach (var entry in overrides)
            {
                try
                {
                    // Merge the child elements.
                    var subOverrides = JsonConvert.DeserializeObject<Dictionary<string,object>>(entry.Value.ToString());
                    if (element.GetElementsByTagName(entry.Key).Count == 0)
                    {
                        element.AppendChild(document.CreateElement(entry.Key));
                    }
                    MergeXmlWithOverrides(document,(XmlElement) element.GetElementsByTagName(entry.Key)[0], subOverrides);
                }
                catch (JsonException)
                {
                    // Remove the existing nodes of the same name.
                    var elementsToRemove = element.GetElementsByTagName(entry.Key).Cast<XmlElement>().ToList();
                    foreach (var subElement in elementsToRemove)
                    {
                        element.RemoveChild(subElement);
                    }
                    
                    try
                    {
                        // Replace the list of elements.
                        var subOverrides = JsonConvert.DeserializeObject<List<object>>(entry.Value.ToString());
                        foreach (var subOverride in subOverrides)
                        {
                            var newElement = document.CreateElement(entry.Key);
                            newElement.InnerText = subOverride.ToString();
                            element.AppendChild(newElement);
                        }
                    }
                    catch (JsonException)
                    {
                        // Add a new node with the value.
                        var newElement = document.CreateElement(entry.Key);
                        newElement.InnerText = entry.Value.ToString();
                        element.AppendChild(newElement);
                    }
                }
            }
        }
        
        /*
         * Returns the current GitHub commit.
         */
        private string GetCurrentGitHubCommit()
        {
            return GitHub.GetLastCommit(this.State.GitRemote,this.State.GitBranch);
        }
        
        /*
         * Creates an Uchu Server object.
         */
        public UchuServer(ServerInfo info)
        {
            this.ServerInfo = info;
            this.ReadState();
        }
        
        /*
         * Reads the current state.
         */
        private void ReadState()
        {
            // Read the state.
            var stateLocation = Path.Combine(this.ServerInfo.ServerFileLocation,"state.json");
            if (File.Exists(stateLocation))
            {
                this.State = JsonConvert.DeserializeObject<UchuState>(File.ReadAllText(stateLocation));
            }
            else
            {
                this.State = new UchuState();
            }
        }
        
        /*
         * Saves the current state.
         */
        private void WriteState()
        {
            // Serialize the state.
            var stateLocation = Path.Combine(this.ServerInfo.ServerFileLocation,"state.json");
            File.WriteAllText(stateLocation,JsonConvert.SerializeObject(this.State,Formatting.Indented));
        }
        
        /*
         * Returns the server directory.
         * Used since the name isn't guaranteed based on the remote and branch.
         */
        public string GetServerDirectory()
        {
            return Directory.GetDirectories(Path.Combine(this.ServerInfo.ServerFileLocation,"Server"))[0];
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
            // Delete the existing configuration.
            Console.WriteLine("Deleting old configurations.");
            var buildLocation = Path.Combine(this.GetServerDirectory(),"Uchu.Master","bin",BUILD_MODE,DOTNET_APP_VERSION);
            var defaultConfigLocation = Path.Combine(buildLocation,"config.default.xml");
            var configLocation = Path.Combine(buildLocation,"config.xml");
            if (File.Exists(defaultConfigLocation))
            {
                File.Delete(defaultConfigLocation);
            }
            if (File.Exists(configLocation))
            {
                File.Delete(configLocation);
            }
            
            // Run the executable to get the default configuration.
            Console.WriteLine("Getting default configuration.");
            this.Start();
            while (!File.Exists(defaultConfigLocation))
            {
                Thread.Sleep(50);
            }
            this.Stop();
            
            // Add the defaults to the configuration overrides.
            if (!this.State.ConfigOverrides.ContainsKey("ResourcesConfiguration"))
            {
                this.State.ConfigOverrides["ResourcesConfiguration"] = new Dictionary<string,object>()
                {
                    {"GameResourceFolder",Path.GetFullPath(Path.Combine(this.ServerInfo.ClientLocation,"res"))},
                };
            }
            if (!this.State.ConfigOverrides.ContainsKey("DllSource"))
            {
                this.State.ConfigOverrides["DllSource"] = new Dictionary<string,object>()
                {
                    {"DotNetPath",Path.GetFullPath(Path.Combine(this.ServerInfo.ServerFileLocation,"Tools","dotnet3.1","dotnet"))},
                    {"Instance","../../../../Uchu.Instance/bin/Debug/netcoreapp3.1/Uchu.Instance.dll"},
                    {"ScriptDllSource","../../../../Uchu.StandardScripts/bin/Debug/netcoreapp3.1/Uchu.StandardScripts.dll"},
                };
            }

            if (!this.State.ConfigOverrides.ContainsKey("Networking"))
            {
                this.State.ConfigOverrides["Networking"] = new Dictionary<string,object>()
                {
                    {"WorldPort",new List<int>() {2003,2004,2005,2006,2007,2008,2009,2010,2011,2012}},
                };
            }
            
            // Write and re-read the state to ensure the objects are consistent.
            this.WriteState();
            this.ReadState();

            // Read the configuration and apply the overrides.
            Console.WriteLine("Writing the configuration.");
            var document = new XmlDocument();
            document.LoadXml(File.ReadAllText(defaultConfigLocation));
            MergeXmlWithOverrides(document,document.DocumentElement,this.State.ConfigOverrides);
            document.Save(configLocation);
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
         * Returns if the server is running.
         */
        public bool IsRunning()
        {
            // Return true if the process id isn't zero and the process exists.
            try
            {
                if (this.State.ProcessId != 0)
                {
                    Process.GetProcessById(this.State.ProcessId);
                    return true;
                }
            }
            catch (ArgumentException)
            {
                // Update that the server is not running.
                this.State.ProcessId = 0;
                this.WriteState();
            }

            // Return false (not running).
            return false;
        }
        
        /*
         * Returns if an update is available.
         */
        public bool IsUpdateAvailable()
        {
            return this.State.CurrentVersion != this.GetCurrentGitHubCommit();
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
            Console.WriteLine("Downloading the latest server from GitHub/" + this.State.GitRemote);
            var targetServerZip = Path.Combine(this.ServerInfo.ServerFileLocation,"Server.zip");
            client.DownloadFile("https://github.com/" + this.State.GitRemote + "/archive/" + this.State.GitBranch + ".zip",targetServerZip);
            ZipFile.ExtractToDirectory(targetServerZip,Path.Combine(this.ServerInfo.ServerFileLocation,"Server"));
            var targetServerDirectory = this.GetServerDirectory();
            Directory.Delete(Path.Combine(targetServerDirectory,"InfectedRose"),true);
            Directory.Delete(Path.Combine(targetServerDirectory,"RakDotNet"),true);
            
            // Download and extract InfectedRose.
            Console.WriteLine("Downloading the latest InfectedRose library from GitHub/Wincent01/InfectedRose");
            var targetInfectedRoseDownloadZip = Path.Combine(this.ServerInfo.ServerFileLocation,"InfectedRose.zip");
            var targetInfectedRoseDownloadDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"InfectedRose");
            client.DownloadFile("https://github.com/Wincent01/InfectedRose/archive/master.zip",targetInfectedRoseDownloadZip);
            ZipFile.ExtractToDirectory(targetInfectedRoseDownloadZip,targetInfectedRoseDownloadDirectory);
            Directory.Move(Path.Combine(targetInfectedRoseDownloadDirectory,"InfectedRose-master"),Path.Combine(targetServerDirectory,"InfectedRose"));
            
            // Download and extract InfectedRose.
            Console.WriteLine("Downloading the latest InfectedRose library from GitHub/UchuServer/RakDotNet");
            var targetRakDotNetDownloadZip = Path.Combine(this.ServerInfo.ServerFileLocation,"RakDotNet.zip");
            var targetRakDotNetDownloadDirectory = Path.Combine(this.ServerInfo.ServerFileLocation,"RakDotNet");
            client.DownloadFile("https://github.com/yuwui/RakDotNet/archive/3.25/tcpudp.zip",targetRakDotNetDownloadZip);
            ZipFile.ExtractToDirectory(targetRakDotNetDownloadZip,targetRakDotNetDownloadDirectory);
            Directory.Move(Path.Combine(targetRakDotNetDownloadDirectory,"RakDotNet-3.25-tcpudp"),Path.Combine(targetServerDirectory,"RakDotNet"));

            // Compile the server.
            Console.WriteLine("Building the server.");
            var buildProcess = new Process();
            buildProcess.StartInfo.WorkingDirectory = targetServerDirectory;
            buildProcess.StartInfo.FileName = dotNetExecutableLocation;
            buildProcess.StartInfo.CreateNoWindow = true;
            buildProcess.StartInfo.Arguments = "build -c " + BUILD_MODE;
            buildProcess.Start();
            buildProcess.WaitForExit();
            buildProcess.Close();

            // Clean up the download files.
            Console.WriteLine("Cleaning up files.");
            this.CleanupFiles();
            
            // Create the default configuration.
            Console.WriteLine("Creating the default configuration.");
            this.CreateConfig();
            this.State.CurrentVersion = this.GetCurrentGitHubCommit();
            this.WriteState();
        }

        /*
         * Starts the server.
         */
        public void Start()
        {
            // Return if the server is already running.
            if (this.IsRunning())
            {
                Console.WriteLine("Server is already running.");
                return;
            }
            
            // Determine the file locations.
            var toolsLocation = Path.Combine(this.ServerInfo.ServerFileLocation,"Tools");
            var dotNetDirectoryLocation = Path.Combine(toolsLocation,"dotnet3.1");
            var dotNetExecutableLocation = Path.Combine(dotNetDirectoryLocation, "dotnet");
            var masterServerDirectory = Path.Combine(this.GetServerDirectory(),"Uchu.Master","bin",BUILD_MODE,DOTNET_APP_VERSION);
            var masterServerExecutable = Path.Combine(masterServerDirectory,"Uchu.Master.dll");
            
            // Create and start the process.
            var uchuProcess = new Process();
            uchuProcess.StartInfo.FileName = dotNetExecutableLocation;
            uchuProcess.StartInfo.WorkingDirectory = masterServerDirectory;
            uchuProcess.StartInfo.Arguments = masterServerExecutable;
            uchuProcess.StartInfo.CreateNoWindow = true;
            uchuProcess.Start();
            Console.WriteLine("Started server.");
            
            // Start and store the process id.
            this.State.ProcessId = uchuProcess.Id;
            this.WriteState();
        }
        
        /*
         * Stops the server.
         */
        public void Stop()
        {
            // Stop the process.
            if (this.IsRunning())
            {
                var uchuProcess = Process.GetProcessById(this.State.ProcessId);
                uchuProcess.Kill(true);
                Console.WriteLine("Stopped server.");
            }
            else
            {
                Console.WriteLine("Server not running.");
            }

            // Store 0 as the process id.
            this.State.ProcessId = 0;
            this.WriteState();
        }
    }
}