using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.Gui.Component.Play;

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
            var clientState = ClientState.Get();
            var playContainer = this.Get<StackPanel>("PlayContainer");
            var clientOutputScroll = this.Get<ScrollViewer>("ClientOutputScroll");
            clientState.ServerList.ServerListChanged += () =>
            {
                this.RunMainThread(this.UpdateServerList);
            };
            clientState.LauncherStateChanged += (state) =>
            {
                this.RunMainThread(() =>
                {
                    var outputVisible = SystemInfo.GetDefault().Settings.LogsEnabled && state == LauncherState.Launched;
                    playContainer.IsVisible = !outputVisible;
                    clientOutputScroll.IsVisible = outputVisible;
                });
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
            var serverList = ClientState.Get().ServerList;
            var selectedServer = serverList.SelectedEntry;
            for (var i = 0; i < serverList.ServerEntries.Count; i++)
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
                entryDisplay.UpdateWidth((serverList.ServerEntries.Count + 1) >= 4);
                var entry = serverList.ServerEntries[i];
                entryDisplay.ServerName = entry.ServerName;
                entryDisplay.ServerAddress = entry.ServerAddress;
                entryDisplay.Selected = (selectedServer == entry);
            }
            this.newServerEntry.UpdateWidth((serverList.ServerEntries.Count + 1) >= 4);
            
            // Remove the old entries.
            for (var i = serverList.ServerEntries.Count; i < this.serverEntries.Count; i++)
            {
                var oldEntry = this.serverEntries[i];
                this.serverEntries.Remove(oldEntry);
                this.serverListContainer.Children.Remove(oldEntry);
            }
            
            // Move the add server entry to the end.
            this.serverListContainer.Children.Move(this.serverListContainer.Children.IndexOf(this.newServerEntry),this.serverListContainer.Children.Count - 1);
        }
}