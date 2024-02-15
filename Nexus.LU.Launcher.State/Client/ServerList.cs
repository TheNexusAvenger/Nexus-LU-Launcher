﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client;

public class ServerList
{
    /// <summary>
    /// All the stored entries for the server.
    /// </summary>
    public readonly List<ServerEntry> ServerEntries;

    /// <summary>
    /// Selected entry for the server list.
    /// </summary>
    public ServerEntry? SelectedEntry;
    
    /// <summary>
    /// Event for when the server list changes.
    /// </summary>
    public event Action? ServerListChanged;

    /// <summary>
    /// System information that stores the server list.
    /// </summary>
    private readonly SystemInfo systemInfo;

    /// <summary>
    /// Creates a server list instance.
    /// </summary>
    public ServerList()
    {
        this.systemInfo = SystemInfo.GetDefault();
        this.ServerEntries = this.systemInfo.Settings.Servers;
        this.SelectedEntry = this.ServerEntries.FirstOrDefault(entry => entry.ServerName == this.systemInfo.Settings.SelectedServer);
    }

    /// <summary>
    /// Finds a server entry for a name.
    /// </summary>
    /// <param name="serverName">Name of the server to find.</param>
    /// <returns>Server entry for the name if it exists.</returns>
    public ServerEntry? GetServerEntry(string serverName)
    {
        return this.ServerEntries.FirstOrDefault(entry => entry.ServerName == serverName);
    }

    /// <summary>
    /// Adds a server entry.
    /// </summary>
    /// <param name="entry">Entry to add.</param>
    public void AddEntry(ServerEntry entry)
    {
        // Update the entry if it exists already.
        var existingEntry = this.GetServerEntry(entry.ServerName);
        if (existingEntry != null)
        {
            existingEntry.ServerAddress = entry.ServerAddress;
            this.systemInfo.SaveSettings();
            this.ServerListChanged?.Invoke();
            return;
        }
        
        // Add the entry and set it as active if no entry is active.
        this.ServerEntries.Add(entry);
        if (this.SelectedEntry == null)
        {
            this.SelectedEntry = entry;
            this.systemInfo.Settings.SelectedServer = this.SelectedEntry?.ServerName;
        }
        
        // Save the settings.
        this.systemInfo.SaveSettings();
        this.ServerListChanged?.Invoke();
    }

    /// <summary>
    /// Removes a server entry.
    /// </summary>
    /// <param name="serverName">Name of the server to remove.</param>
    public void RemoveEntry(string serverName)
    {
        // Return if the entry does not exist.
        var entry = this.GetServerEntry(serverName);
        if (entry == null)
        {
            return;
        }
        
        // Remove the entry and clear the current setting if it was selected.
        this.ServerEntries.Remove(entry);
        if (entry == this.SelectedEntry)
        {
            this.SelectedEntry = this.ServerEntries.FirstOrDefault();
            this.systemInfo.Settings.SelectedServer = this.SelectedEntry?.ServerName;
        }
        
        // Save the settings.
        this.systemInfo.SaveSettings();
        this.ServerListChanged?.Invoke();
    }

    /// <summary>
    /// Sets a server entry as the active server.
    /// </summary>
    /// <param name="serverName">Name of the server set as active.</param>
    public void SetServerActive(string serverName)
    {
        // Return if the entry does not exist.
        var entry = this.GetServerEntry(serverName);
        if (entry == null)
        {
            return;
        }
        
        // Set the entry as active and save the settings.
        this.SelectedEntry = entry;
        this.systemInfo.Settings.SelectedServer = serverName;
        this.systemInfo.SaveSettings();
        this.ServerListChanged?.Invoke();
    }
}