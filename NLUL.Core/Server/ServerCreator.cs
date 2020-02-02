/*
 * TheNexusAvenger
 *
 * Creates a server.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NLUL.Core.Server.Emulator;

namespace NLUL.Core.Server
{
    public class ServerCreator
    {
        private SystemInfo SystemInfo;
        private Dictionary<string,ServerInfo> Servers;
        
        /*
         * Creates a Server Creator object.
         */
        public ServerCreator(SystemInfo systemInfo)
        {
            this.SystemInfo = systemInfo;
            this.ReadServers();
        }
        
        /*
         * Reads the current servers.
         */
        private void ReadServers()
        {
            var stateLocation = Path.Combine(this.SystemInfo.SystemFileLocation,"Server","servers.json");
            if (File.Exists(stateLocation))
            {
                this.Servers = JsonConvert.DeserializeObject<Dictionary<string,ServerInfo>>(File.ReadAllText(stateLocation));
            }
            else
            {
                this.Servers = new Dictionary<string,ServerInfo>();
            }
        }
        
        /*
         * Saves the current server.
         */
        private void WriteServers()
        {
            // Serialize the state.
            var stateLocation = Path.Combine(this.SystemInfo.SystemFileLocation,"Server","servers.json");
            File.WriteAllText(stateLocation,JsonConvert.SerializeObject(this.Servers,Formatting.Indented));
        }
        
        /*
         * Returns the server with a given name.
         */
        public IEmulator GetServer(string name)
        {
            // Return the server if it exists.
            if (this.Servers.ContainsKey(name.ToLower()))
            {
                return this.Servers[name.ToLower()].CreateEmulator();
            }
            
            // Return null (not found).
            return null;
        }
        
        /*
         * Creates a server.
         */
        public IEmulator CreateServer(string name,ServerType type)
        {
            // Throw an error if the server name is a duplicate.
            if (this.Servers.ContainsKey(name.ToLower()))
            {
                throw new ArgumentException("Server name already exists.");
            }
            
            // Escape the name for the file name.
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)",invalidChars);
            var escapedName = Regex.Replace(name,invalidRegStr,"_");
            
            // Determine the directory.
            var serverDirectory = Path.Combine(this.SystemInfo.SystemFileLocation,"Server",escapedName);
            while (Directory.Exists(serverDirectory))
            {
                serverDirectory += "_";
            }
            
            // Create the server info and directory.
            Directory.CreateDirectory(serverDirectory);
            var serverInfo = new ServerInfo(this.SystemInfo,serverDirectory,this.SystemInfo.ClientLocation,type);
            
            // Store the server.
            this.Servers[name.ToLower()] = serverInfo;
            this.WriteServers();
            
            // Return the server emulator.
            return serverInfo.CreateEmulator();
        }
        
        /*
         * Deletes a server.
         */
        public void DeleteServer(string name)
        {
            // Throw an error if the server name doesn't exist.
            if (!this.Servers.ContainsKey(name.ToLower()))
            {
                throw new ArgumentException("Server doesn't exist.");
            }
            
            // Get the server data and delete the server files.
            var serverInfo = this.Servers[name.ToLower()];
            Directory.Delete(serverInfo.ServerFileLocation,true);
            
            // Remove the server entry.
            this.Servers.Remove(name.ToLower());
            this.WriteServers();
        }
    }
}