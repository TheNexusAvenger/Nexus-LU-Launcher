using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.Core;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Play
{
    public class PlayView : Panel
    {
        /// <summary>
        /// Container of the server entry list.
        /// </summary>
        private readonly StackPanel serverListContainer;
        
        /// <summary>
        /// List of server entries.
        /// </summary>
        private readonly List<ServerEntry> serverEntries = new List<ServerEntry>();
        
        /// <summary>
        /// Entry for registering new servers.
        /// </summary>
        private readonly NewServerEntry newServerEntry;
        
        /// <summary>
        /// Creates a play view.
        /// </summary>
        public PlayView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.serverListContainer = this.Get<StackPanel>("ServerList");
            this.newServerEntry = new NewServerEntry();
            this.serverListContainer.Children.Add(this.newServerEntry);
            this.Get<PlayPanel>("PlayPanel").ClientOutput = this.Get<TextBox>("ClientOutput");
            this.Get<PlayPanel>("PlayPanel").ClientOutputScroll = this.Get<ScrollViewer>("ClientOutputScroll");
            
            // Connect the events.
            var playContainer = this.Get<StackPanel>("PlayContainer");
            var clientOutputScroll = this.Get<ScrollViewer>("ClientOutputScroll");
            PersistentState.ServerListChanged += this.UpdateServerList;
            PersistentState.SelectedServerChanged += this.UpdateServerList;
            Client.StateChanged += () =>
            {
                var outputVisible = SystemInfo.GetDefault().Settings.LogsEnabled && Client.State == PlayState.Launched;
                playContainer.IsVisible = !outputVisible;
                clientOutputScroll.IsVisible = outputVisible;
            };
            
            // Update the server list initially.
            this.UpdateServerList();
        }
        
        /// <summary>
        /// Updates the server list.
        /// </summary>
        private void UpdateServerList()
        {
            // Create new server entries and update the values.
            var selectedServer = PersistentState.SelectedServer;
            for (var i = 0; i < PersistentState.SystemInfo.Settings.Servers.Count; i++)
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
                entryDisplay.UpdateWidth((PersistentState.SystemInfo.Settings.Servers.Count + 1) >= 4);
                var entry = PersistentState.SystemInfo.Settings.Servers[i];
                entryDisplay.ServerName = entry.ServerName;
                entryDisplay.ServerAddress = entry.ServerAddress;
                entryDisplay.Selected = (selectedServer == entry);
            }
            this.newServerEntry.UpdateWidth((PersistentState.SystemInfo.Settings.Servers.Count + 1) >= 4);
            
            // Remove the old entries.
            for (var i = PersistentState.SystemInfo.Settings.Servers.Count; i < this.serverEntries.Count; i++)
            {
                var oldEntry = this.serverEntries[i];
                this.serverEntries.Remove(oldEntry);
                this.serverListContainer.Children.Remove(oldEntry);
            }
            
            // Move the add server entry to the end.
            this.serverListContainer.Children.Move(this.serverListContainer.Children.IndexOf(this.newServerEntry),this.serverListContainer.Children.Count - 1);
        }
    }
}