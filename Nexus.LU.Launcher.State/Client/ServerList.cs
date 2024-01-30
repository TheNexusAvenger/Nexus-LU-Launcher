using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client;

public class ServerList
{
    /// <summary>
    /// All the stored entries for the server.
    /// </summary>
    public List<ServerEntry> ServerEntries;

    /// <summary>
    /// Selected entry for the server list.
    /// </summary>
    public ServerEntry SelectedEntry;
    
    /// <summary>
    /// Event for when the server list changes.
    /// </summary>
    public event Action ServerListChanged;

    /// <summary>
    /// System information that stores the server list.
    /// </summary>
    private readonly SystemInfo _systemInfo;

    /// <summary>
    /// Creates a server list instance.
    /// </summary>
    public ServerList()
    {
        this._systemInfo = SystemInfo.GetDefault();
        this.ServerEntries = this._systemInfo.Settings.Servers;
        this.SelectedEntry = this.ServerEntries.FirstOrDefault(entry => entry.ServerName == this._systemInfo.Settings.SelectedServer);
    }

    /// <summary>
    /// Adds a server entry.
    /// </summary>
    /// <param name="entry">Entry to add.</param>
    public void AddEntry(ServerEntry entry)
    {
        // Add the entry and set it as active if no entry is active.
        this.ServerEntries.Add(entry);
        if (this.SelectedEntry == null)
        {
            this.SelectedEntry = entry;
            this._systemInfo.Settings.SelectedServer = this.SelectedEntry?.ServerName;
        }
        
        // Save the settings.
        this._systemInfo.SaveSettings();
        this.ServerListChanged?.Invoke();
    }

    /// <summary>
    /// Removes a server entry.
    /// </summary>
    /// <param name="serverName">Name of the server to remove.</param>
    public void RemoveEntry(string serverName)
    {
        // Return if the entry does not exist.
        var entry = this.ServerEntries.FirstOrDefault(entry => entry.ServerName == serverName);
        if (entry == null)
        {
            return;
        }
        
        // Remove the entry and clear the current setting if it was selected.
        this.ServerEntries.Remove(entry);
        if (entry == this.SelectedEntry)
        {
            this.SelectedEntry = this.ServerEntries.FirstOrDefault();
            this._systemInfo.Settings.SelectedServer = this.SelectedEntry?.ServerName;
        }
        
        // Save the settings.
        this._systemInfo.SaveSettings();
        this.ServerListChanged?.Invoke();
    }

    /// <summary>
    /// Sets a server entry as the active server.
    /// </summary>
    /// <param name="serverName">Name of the server set as active.</param>
    public void SetServerActive(string serverName)
    {
        // Return if the entry does not exist.
        var entry = this.ServerEntries.FirstOrDefault(entry => entry.ServerName == serverName);
        if (entry == null)
        {
            return;
        }
        
        // Set the entry as active and save the settings.
        this.SelectedEntry = entry;
        this._systemInfo.Settings.SelectedServer = serverName;
        this._systemInfo.SaveSettings();
        this.ServerListChanged?.Invoke();
    }
}