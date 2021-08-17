using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using NLUL.Core.Client.Patch;

namespace NLUL.Core.Client.Source
{
    public class ClientPatchEntry
    {
        /// <summary>
        /// Name of the patch that can be applied.
        /// </summary>
        public ClientPatchName Name { get; set; }
        
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
        /// Returns the local sources to use if the HTTP fetch failed.
        /// </summary>
        /// <returns>The local sources to use</returns>
        public static string GetLocalSources()
        {
            var sourcesStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NLUL.Core.Client.Sources.json");
            if (sourcesStream == null) return null;
            var reader = new StreamReader(sourcesStream);
            return reader.ReadToEnd();
        }
        
        /// <summary>
        /// Returns the list of client sources to use.
        /// </summary>
        /// <param name="json">JSON data to use.</param>
        public static SourceList GetSources(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SourceList>(json);
        }
        
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

            // Return the list.
            sourcesJson ??= GetLocalSources();
            return GetSources(sourcesJson);
        }
    }
}