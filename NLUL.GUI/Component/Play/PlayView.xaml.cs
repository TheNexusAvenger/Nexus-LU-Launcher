/*
 * TheNexusAvenger
 *
 * View for the play screen.
 */

using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Play
{
    public class PlayView : Panel
    {
        private StackPanel serverListContainer;
        private List<ServerEntry> serverEntries = new List<ServerEntry>();
        
        /*
        * Creates a play view.
        */
        public PlayView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.serverListContainer = this.Get<StackPanel>("ServerList");
            
            // Connect the events.
            PersistentState.ServerListChanged += this.UpdateServerList;
            PersistentState.SelectedServerChanged += this.UpdateServerList;
            
            // Update the server list initially.
            this.UpdateServerList();
        }
        
        /*
         * Updates the server list.
         */
        private void UpdateServerList()
        {
            // Create new server entries and update the values.
            var selectedServer = PersistentState.GetSelectedServer();
            for (var i = 0; i < PersistentState.State.servers.Count; i++)
            {
                // Create the entry.
                if (this.serverEntries.Count <= i)
                {
                    var newEntry = new ServerEntry();
                    this.serverEntries.Add(newEntry);
                    this.serverListContainer.Children.Add(newEntry);
                }
                
                // Set the entry.
                var entryDisplay = this.serverEntries[i];
                var entry = PersistentState.State.servers[i];
                entryDisplay.ServerName = entry.serverName;
                entryDisplay.ServerAddress = entry.serverAddress;
                entryDisplay.Selected = (selectedServer == entry);
            }
            
            // Remove the old entries.
            for (var i = PersistentState.State.servers.Count; i < this.serverEntries.Count; i++)
            {
                var oldEntry = this.serverEntries[i];
                this.serverEntries.Remove(oldEntry);
                this.serverListContainer.Children.Remove(oldEntry);
            }
        }
    }
}