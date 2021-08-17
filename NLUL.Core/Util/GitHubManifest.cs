using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace NLUL.Core.Util
{
    public class GitHubTag
    {
        /// <summary>
        /// Commit id of the tag.
        /// </summary>
        public string commit { get; set; }
        
        /// <summary>
        /// Name of the tag.
        /// </summary>
        public string name { get; set; }
    }
    
    public class GitCommitResult
    {
        /// <summary>
        /// Hash of the commit.
        /// </summary>
        public string sha;
    }
    
    public class GitTagResult
    {
        /// <summary>
        /// Name of the tag.
        /// </summary>
        public string name;
        
        /// <summary>
        /// Commit of the tag.
        /// </summary>
        public GitCommitResult commit;
    }
    
    public class GitHubRateLimitException : HttpRequestException
    {
        /// <summary>
        /// Creates the rate limit exception.
        /// </summary>
        public GitHubRateLimitException() : base("GitHub request was rate limited.")
        {
            
        }
    }
    
    public class GitHubManifestEntry
    {
        /// <summary>
        /// Repository of the entry.
        /// </summary>
        public string Repository { get; set; }
        
        /// <summary>
        /// Directory of the entry.
        /// </summary>
        public string EntryDirectory { get; set; }
        
        /// <summary>
        /// Last commit of the entry.
        /// </summary>
        public string LastCommit { get; set; }
        
        /// <summary>
        /// Manifest of the entry.
        /// </summary>
        public GitHubManifest Manifest { get; internal set; }
        
        /// <summary>
        /// Creates the entry.
        /// </summary>
        /// <param name="repository">Repository of the entry.</param>
        /// <param name="entryDirectory">Directory of the entry.</param>
        /// <param name="manifest">Manifest of the entry.</param>
        protected internal GitHubManifestEntry(string repository,string entryDirectory,GitHubManifest manifest)
        {
            this.Repository = repository;
            this.EntryDirectory = entryDirectory;
            this.Manifest = manifest;
        }
        
        /// <summary>
        /// Creates the entry.
        /// Intended only for JSON parsing.
        /// </summary>
        public GitHubManifestEntry()
        {
            
        }
        
        /// <summary>
        /// Performs a GET request.
        /// </summary>
        /// <param name="url">URL to fetch.</param>
        /// <returns>The result of the request.</returns>
        /// <exception cref="GitHubRateLimitException">GitHub rate limit was reached.</exception>
        public string PerformRequest(string url)
        {
            // Send the HTTP request.
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "NLUL Commits Fetch");
            var response = client.GetAsync(url).Result;
            var stringResponse = response.Content.ReadAsStringAsync().Result;
            
            // Throw a rate limit error if found.
            if (response.ReasonPhrase == "rate limit exceeded")
            {
                throw new GitHubRateLimitException();
            }
            
            // Return the result.
            return stringResponse;
        }
        
        /// <summary>
        /// Returns the latest commit of a branch.
        /// </summary>
        /// <param name="branch">Branch to fetch.</param>
        /// <returns>The latest commit of a branch.</returns>
        public string GetLatestCommit(string branch)
        {
            // Send the HTTP request.
            var commitsJson = this.PerformRequest("https://api.github.com/repos/" + this.Repository + "/commits/" + branch);
            
            // Parse the JSON and return the last tag.
            var commits = JsonConvert.DeserializeObject<GitCommitResult>(commitsJson);
            return commits?.sha;
        }
        
        /// <summary>
        /// Returns if the fetched branch is the latest.
        /// </summary>
        /// <param name="branch">Branch to check</param>
        /// <returns>Whether the fetched branch is the latest.</returns>
        public bool IsBranchUpToDate(string branch)
        {
            return this.LastCommit == this.GetLatestCommit(branch);
        }
        
        /// <summary>
        /// Returns the latest tag.
        /// </summary>
        /// <returns>The latest tag.</returns>
        public GitHubTag GetLatestTag()
        {
            // Send the HTTP request.
            var tagsJson = this.PerformRequest("https://api.github.com/repos/" + this.Repository + "/tags");
            
            // Parse the JSON and return the last tag's commit.
            var tags = JsonConvert.DeserializeObject<List<GitTagResult>>(tagsJson);
            if (tags?.Count > 0)
            {
                return new GitHubTag()
                {
                    commit = tags[0].commit.sha,
                    name = tags[0].name,
                };
            }
            return null;
        }
        
        /// <summary>
        /// Returns if the fetched tag is the latest.
        /// </summary>
        /// <returns>Whether the fetched tag is the latest.</returns>
        public bool IsTagUpToDate()
        {
            return this.LastCommit == this.GetLatestTag().commit;
        }
        
        /// <summary>
        /// Fetches a specified commit.
        /// </summary>
        /// <param name="commit">Commit to fetch.</param>
        /// <param name="force">Whether to force download.</param>
        public void FetchCommit(string commit, bool force = false)
        {
            // Return if the commit is the same as the latest fetch.
            if (commit == this.LastCommit && force != true)
            {
                return;
            }
            
            // Delete the existing files.
            var parentDirectory = Directory.GetParent(this.EntryDirectory).FullName;
            var zipDirectory = Path.Combine(parentDirectory, commit + ".zip");
            if (File.Exists(zipDirectory))
            {
                File.Delete(zipDirectory);
            }
            if (Directory.Exists(this.EntryDirectory))
            {
                Directory.Delete(this.EntryDirectory, true);
            }
            
            // Download the ZIP of the commit.
            var client = new WebClient();
            client.DownloadFile("https://github.com/" + this.Repository + "/archive/" + commit + ".zip", zipDirectory);
            
            // Un-compress the ZIP file to the temporary directory.
            var temporaryDirectory = Path.Combine(parentDirectory, commit);
            ZipFile.ExtractToDirectory(zipDirectory, temporaryDirectory);
            
            // Move the directory.
            Directory.Move(Path.Combine(temporaryDirectory, Directory.GetDirectories(temporaryDirectory)[0]), this.EntryDirectory);
            
            // Clear the files.
            File.Delete(zipDirectory);
            Directory.Delete(temporaryDirectory, true);
            
            // Save the manifest.
            this.LastCommit = commit;
            this.Manifest.Save();
        }
        
        /// <summary>
        /// Fetches the latest branch.
        /// </summary>
        /// <param name="branch">Branch to download.</param>
        /// <param name="force">Whether to force download.</param>
        public void FetchLatestBranch(string branch, bool force = false)
        {
            this.FetchCommit(this.GetLatestCommit(branch),force);
        }
        
        /// <summary>
        /// Fetches the latest tag.
        /// </summary>
        /// <param name="force">Whether to force download.</param>
        public void FetchLatestTag(bool force = false)
        {
            this.FetchCommit(this.GetLatestTag().commit,force);
        }
    }
    
    public class GitHubManifest
    {
        /// <summary>
        /// Entries in the manifest.
        /// </summary>
        private readonly List<GitHubManifestEntry> manifest = new List<GitHubManifestEntry>();
        
        /// <summary>
        /// File location of the manifest.
        /// </summary>
        private readonly string fileLocation;
        
        /// <summary>
        /// Creates the manifest.
        /// </summary>
        /// <param name="fileLocation">File location of the manifest.</param>
        public GitHubManifest(string fileLocation)
        {
            this.fileLocation = fileLocation;
            
            // Try to parse the file.
            if (!File.Exists(fileLocation)) return;
            try
            {
                this.manifest = JsonConvert.DeserializeObject<List<GitHubManifestEntry>>(File.ReadAllText(fileLocation));
            }
            catch (JsonException)
            {
                    
            }
        }
        
        /// <summary>
        /// Returns the entry for a remote and file location.
        /// </summary>
        /// <param name="repository">Repository to get.</param>
        /// <param name="directory">Directory to manage.</param>
        /// <returns>Whether the entry for a remote and file location.</returns>
        public GitHubManifestEntry GetEntry(string repository,string directory)
        {
            // Find and return the entry if it exists.
            foreach (var entry in this.manifest)
            {
                if (string.Equals(entry.Repository, repository, StringComparison.CurrentCultureIgnoreCase) && string.Equals(entry.EntryDirectory, directory, StringComparison.CurrentCultureIgnoreCase))
                {
                    entry.Manifest = this;
                    return entry;
                }
            }
            
            // Create and return a new entry.
            var newEntry = new GitHubManifestEntry(repository,directory, this);
            this.manifest.Add(newEntry);
            return newEntry;
        }
        
        /// <summary>
        /// Saves the manifest.
        /// </summary>
        protected internal void Save()
        {
            File.WriteAllText(this.fileLocation,JsonConvert.SerializeObject(this.manifest,Formatting.Indented));
        }
    }
}