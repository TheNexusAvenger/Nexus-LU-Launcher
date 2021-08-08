using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace NLUL.Core.Client.Source
{
    public class ClientPatchEntry
    {
        /// <summary>
        /// Name of the patch that can be applied.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Whether to set up the patch by default.
        /// </summary>
        public bool Default { get; set; }
    }
    
    public class ClientSourceEntry
    {
        /// <summary>
        /// Name of the client source.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Type of the client source.
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// URL to download the client.
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Method for downloading the client.
        /// </summary>
        public string Method { get; set; }
        
        /// <summary>
        /// List of patches for the client.
        /// </summary>
        public List<ClientPatchEntry> Patches { get; set; }
    }
    
    public class SourceList : List<ClientSourceEntry>
    {
        /// <summary>
        /// Returns the list of client sources to use.
        /// </summary>
        public static SourceList GetSources()
        {
            // Get the sources JSON.
            string sourcesJson = null;
            try
            {
                // Fetch the sources from online.
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "NLUL Sources Fetch");
                var response = client.GetAsync("https://raw.githubusercontent.com/TheNexusAvenger/Nexus-LU-Launcher/master/NLUL.Core/Client/Sources.json").Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    sourcesJson = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception)
            {
                // Ignore exceptions.
            }
            
            // Load the sources from the executable as a backup.
            if (sourcesJson == null)
            {
                var sourcesStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NLUL.Core.Client.Sources.json");
                if (sourcesStream == null) return null;
                var reader = new StreamReader(sourcesStream);
                sourcesJson = reader.ReadToEnd();
            }
            
            // Return the list.
            Console.Write(sourcesJson);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SourceList>(sourcesJson);
        }
    }
}