using System.Linq;
using System.Threading.Tasks;
using NLUL.Core;

namespace NLUL.GUI.State
{
    public class PersistentState
    {
        /// <summary>
        /// Delegate for an event handler with no parameters.
        /// </summary>
        public delegate void EmptyEventHandler();
        
        /// <summary>
        /// Event for the server list changing.
        /// </summary>
        public static event EmptyEventHandler ServerListChanged;
        
        /// <summary>
        /// Event for the selected server changing.
        /// </summary>
        public static event EmptyEventHandler SelectedServerChanged;

        /// <summary>
        /// Information about the system.
        /// </summary>
        public static readonly SystemInfo SystemInfo = SystemInfo.GetDefault();
        
        /// <summary>
        /// The current selected server. May be null.
        /// </summary>
        public static ServerEntry SelectedServer => GetServerEntry(SystemInfo.Settings.SelectedServer);
        
        /// <summary>
        /// Saves the state.
        /// </summary>
        public static void Save()
        {
            SystemInfo.SaveSettings();
        }
        
        /// <summary>
        /// Saves the state in a background thread.
        /// Intended for save calls from the UI.
        /// </summary>
        public static void SaveBackground()
        {
            Task.Run(Save);
        }
        
        /// <summary>
        /// Gets the server entry for the given name.
        /// </summary>
        /// <param name="serverName">Server name to check for.</param>
        /// <returns>The server object for the given name.</returns>
        public static ServerEntry GetServerEntry(string serverName)
        {
            return SystemInfo.Settings.Servers.FirstOrDefault(server => server.ServerName == serverName);
        }

        /// <summary>
        /// Sets the selected server.
        /// </summary>
        /// <param name="serverName">Server name to select.</param>
        public static void SetSelectedServer(string serverName)
        {
            // Set the selected server if the entry exists.
            var lastSelectedServer = SystemInfo.Settings.SelectedServer;
            SystemInfo.Settings.SelectedServer = GetServerEntry(serverName) != null ? serverName : null;
            SaveBackground();
            
            // Fire the event if the selected server changed.
            if (lastSelectedServer == SystemInfo.Settings.SelectedServer) return;
            SelectedServerChanged?.Invoke();
            Client.UpdateState();
        }
        
        /// <summary>
        /// Adds a server entry.
        /// </summary>
        /// <param name="serverName">Display name of the server.</param>
        /// <param name="serverAddress">Address of the server.</param>
        public static void AddServerEntry(string serverName, string serverAddress)
        {
            // Return if the entry exists.
            if (GetServerEntry(serverName) != null)
            {
                return;
            }
            
            // Add the entry.
            SystemInfo.Settings.Servers.Add(new ServerEntry()
            {
                ServerName = serverName,
                ServerAddress = serverAddress,
            });
            SaveBackground();
            ServerListChanged?.Invoke();
        }
        
        /// <summary>
        /// Removes a server entry.
        /// </summary>
        /// <param name="serverName">Display name of the server.</param>
        public static void RemoveServerEntry(string serverName)
        {
            // Get the server entry and return if it doesn't exist.
            var serverEntry = GetServerEntry(serverName);
            if (serverEntry == null)
            {
                return;
            }
            
            // Remove the entry.
            SystemInfo.Settings.Servers.Remove(serverEntry);
            ServerListChanged?.Invoke();
            
            // Update the selected server. Updating also invokes saving.
            SetSelectedServer(SystemInfo.Settings.SelectedServer);
        }
    }
}