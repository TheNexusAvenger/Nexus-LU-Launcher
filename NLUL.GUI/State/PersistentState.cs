/*
 * TheNexusAvenger
 *
 * Stores the persistent state of the launcher.
 */



using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using NLUL.Core;

namespace NLUL.GUI.State
{
    /*
     * Manages the persistent state.
     */
    public class PersistentState
    {
        public delegate void EmptyEventHandler();
        public static event EmptyEventHandler ServerListChanged;
        public static event EmptyEventHandler SelectedServerChanged;

        private static readonly SystemInfo SystemInfo = SystemInfo.GetDefault();
        public static LauncherSettings State => SystemInfo.Settings;

        /*
         * Saves the state.
         */
        public static void Save()
        {
            SystemInfo.SaveSettings();
        }
        
        /*
         * Saves the state in a background thread.
         * Intended for save calls from the UI.
         */
        public static void SaveBackground()
        {
            new Thread(Save).Start();
        }
        
        /*
         * Returns the server object for the given name.
         */
        public static ServerEntry GetServerEntry(string serverName)
        {
            // Iterate over the servers and return if the name matches.
            foreach (var server in State.servers)
            {
                if (server.serverName == serverName)
                {
                    return server;
                }
            }
            
            // Return null (not found).
            return null;
        }
        
        /*
         * Returns the selected server.
         */
        public static ServerEntry GetSelectedServer()
        {
            return GetServerEntry(State.selectedServer);
        }
        
        /*
         * Sets the selected server.
         */
        public static void SetSelectedServer(string serverName)
        {
            // Set the selected server if the entry exists.
            var lastSelectedServer = State.selectedServer;
            if (GetServerEntry(serverName) != null)
            {
                State.selectedServer = serverName;
            }
            else
            {
                State.selectedServer = null;
            }
            SaveBackground();
            
            // Fire the event if the selected server changed.
            if (lastSelectedServer != State.selectedServer)
            {
                SelectedServerChanged?.Invoke();
                Client.UpdateState();
            }
        }
        
        /*
         * Adds a server entry.
         */
        public static void AddServerEntry(string serverName, string serverAddress)
        {
            // Return if the entry exists.
            if (GetServerEntry(serverName) != null)
            {
                return;
            }
            
            // Add the entry.
            State.servers.Add(new ServerEntry()
            {
                serverName = serverName,
                serverAddress = serverAddress,
            });
            SaveBackground();
            ServerListChanged?.Invoke();
        }
        
        /*
         * Removes a server entry.
         */
        public static void RemoveServerEntry(string serverName)
        {
            // Get the server entry and return if it doesn't exist.
            var serverEntry = GetServerEntry(serverName);
            if (serverEntry == null)
            {
                return;
            }
            
            // Remove the entry.
            State.servers.Remove(serverEntry);
            ServerListChanged?.Invoke();
            
            // Update the selected server. Updating also invokes saving.
            SetSelectedServer(State.selectedServer);
        }
    }
}