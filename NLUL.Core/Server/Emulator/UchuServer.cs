/*
 * TheNexusAvenger
 *
 * Sets up and runs an Uchu instance.
 * https://github.com/UchuServer/Uchu
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using NLUL.Core.Server.Prerequisite;
using NLUL.Core.Util;
using Formatting = Newtonsoft.Json.Formatting;

namespace NLUL.Core.Server.Emulator
{
    /*
     * State of the Uchu server.
     */
    public class UchuState
    {
        public int ProcessId = 0;
        public string GitRemote = "UchuServer/Uchu";
        public string GitBranch = "master";
        public string InfectedRoseRemote = "Wincent01/InfectedRose";
        public string InfectedRoseBranch = "master";
        public string RakDotNetRemote = "UchuServer/RakDotNet";
        public string RakDotNetBranch = "uchu-optimized";
        public Dictionary<string,object> ConfigOverrides = new Dictionary<string,object>();
    }

    public class UchuServer : IEmulator
    {
        private const string BUILD_MODE = "Debug";
        private const string DOTNET_APP_VERSION = "netcoreapp3.1";
        
        private ServerInfo serverInfo;
        private UchuState state;
        private GitHubManifest manifest;
        
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
         * Creates an Uchu Server object.
         */
        public UchuServer(ServerInfo info)
        {
            this.serverInfo = info;
            this.ReadState();
            this.manifest = new GitHubManifest(Path.Combine(this.serverInfo.ServerFileLocation,"github.json"));
        }

        /*
         * Reads the current state.
         */
        private void ReadState()
        {
            // Read the state.
            var stateLocation = Path.Combine(this.serverInfo.ServerFileLocation,"state.json");
            if (File.Exists(stateLocation))
            {
                this.state = JsonConvert.DeserializeObject<UchuState>(File.ReadAllText(stateLocation));
            }
            else
            {
                this.state = new UchuState();
            }
        }
        
        /*
         * Saves the current state.
         */
        private void WriteState()
        {
            // Serialize the state.
            var stateLocation = Path.Combine(this.serverInfo.ServerFileLocation,"state.json");
            File.WriteAllText(stateLocation,JsonConvert.SerializeObject(this.state,Formatting.Indented));
        }
        
        /*
         * Returns the server directory.
         * Used since the name isn't guaranteed based on the remote and branch.
         */
        public string GetServerDirectory()
        {
            return Path.Combine(this.serverInfo.ServerFileLocation,"Server");
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
            if (!this.state.ConfigOverrides.ContainsKey("ResourcesConfiguration"))
            {
                this.state.ConfigOverrides["ResourcesConfiguration"] = new Dictionary<string,object>()
                {
                    {"GameResourceFolder",Path.GetFullPath(Path.Combine(this.serverInfo.ClientLocation,"res"))},
                };
            }
            if (!this.state.ConfigOverrides.ContainsKey("DllSource"))
            {
                this.state.ConfigOverrides["DllSource"] = new Dictionary<string,object>()
                {
                    {"DotNetPath",Path.GetFullPath(Path.Combine(this.serverInfo.ServerFileLocation,"Tools","dotnet3.1","dotnet"))},
                    {"Instance","../../../../Uchu.Instance/bin/Debug/netcoreapp3.1/Uchu.Instance.dll"},
                    {"ScriptDllSource","../../../../Uchu.StandardScripts/bin/Debug/netcoreapp3.1/Uchu.StandardScripts.dll"},
                };
            }

            if (!this.state.ConfigOverrides.ContainsKey("Networking"))
            {
                this.state.ConfigOverrides["Networking"] = new Dictionary<string,object>()
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
            MergeXmlWithOverrides(document,document.DocumentElement,this.state.ConfigOverrides);
            document.Save(configLocation);
        }

        /*
         * Returns the prerequisites for the server.
         */
        public List<IPrerequisite> GetPrerequisites()
        {
            return new List<IPrerequisite>()
            {
                new DotNetCore31(Path.Combine(this.serverInfo.ServerFileLocation,"Tools")),
                new UnpackedLegoUniverseClient(this.serverInfo.SystemInfo),
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
                if (this.state.ProcessId != 0)
                {
                    Process.GetProcessById(this.state.ProcessId);
                    return true;
                }
            }
            catch (ArgumentException)
            {
                // Update that the server is not running.
                this.state.ProcessId = 0;
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
            return !this.manifest.GetEntry(this.state.GitRemote,this.GetServerDirectory()).IsBranchUpToDate(this.state.GitBranch);
        }
        
        /*
         * Installs the server. Used for both initializing
         * the first time and updating.
         */
        public void Install()
        {
            // Get the tool locations.
            var toolsLocation = Path.Combine(this.serverInfo.ServerFileLocation,"Tools");
            var dotNetDirectoryLocation = Path.Combine(toolsLocation,"dotnet3.1");
            var dotNetExecutableLocation = Path.Combine(dotNetDirectoryLocation, "dotnet");

            // Remove the previous server.
            Console.WriteLine("Clearing old files");
            if (Directory.Exists(this.GetServerDirectory()))
            {
                Directory.Delete(this.GetServerDirectory(),true);
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
            var targetServerDirectory = this.GetServerDirectory();
            this.manifest.GetEntry(this.state.GitRemote,this.GetServerDirectory()).FetchLatestBranch(this.state.GitBranch,true);
            Directory.Delete(Path.Combine(targetServerDirectory,"InfectedRose"),true);
            Directory.Delete(Path.Combine(targetServerDirectory,"RakDotNet"),true);

            // Download and extract InfectedRose.
            Console.WriteLine("Downloading the latest InfectedRose library from GitHub/" + this.state.InfectedRoseRemote);
            var infectedRoseDirectory = Path.Combine(this.serverInfo.ServerFileLocation,"InfectedRose");
            this.manifest.GetEntry(this.state.InfectedRoseRemote,infectedRoseDirectory).FetchLatestBranch(this.state.InfectedRoseBranch);
            FileSystem.CopyDirectory(infectedRoseDirectory,Path.Combine(targetServerDirectory,"InfectedRose"));
            
            // Download and extract RakDotNet.
            Console.WriteLine("Downloading the latest RakDotNet library from GitHub/" + this.state.RakDotNetRemote);
            var rakDotNetDirectory = Path.Combine(this.serverInfo.ServerFileLocation,"RakDotNet");
            this.manifest.GetEntry(this.state.RakDotNetRemote,rakDotNetDirectory).FetchLatestBranch(this.state.RakDotNetBranch);
            FileSystem.CopyDirectory(rakDotNetDirectory,Path.Combine(targetServerDirectory,"RakDotNet"));

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
            
            // Create the default configuration.
            Console.WriteLine("Creating the default configuration.");
            this.CreateConfig();
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
            var toolsLocation = Path.Combine(this.serverInfo.ServerFileLocation,"Tools");
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
            this.state.ProcessId = uchuProcess.Id;
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
                var uchuProcess = Process.GetProcessById(this.state.ProcessId);
                uchuProcess.Kill(true);
                Console.WriteLine("Stopped server.");
            }
            else
            {
                Console.WriteLine("Server not running.");
            }

            // Store 0 as the process id.
            this.state.ProcessId = 0;
            this.WriteState();
        }
    }
}