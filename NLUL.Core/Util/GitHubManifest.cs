/*
 * TheNexusAvenger
 *
 * Manages fetching files from GitHub.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace NLUL.Core.Util
{
    /*
     * Data class for a GitHub tag.
     */
    public class GitHubTag
    {
        public string commit;
        public string name;
    }
    
    /*
     * Class for a Git commit result.
     */
    public class GitCommitResult
    {
        public string sha;
    }
    
    /*
     * Class for a Git tag result.
     */
    public class GitTagResult
    {
        public string name;
        public GitCommitResult commit;
    }
    
    /*
     * Exception for being rate limited.
     */
    public class GitHubRateLimitException : HttpRequestException
    {
        /*
         * Creates the exception.
         */
        public GitHubRateLimitException() : base("GitHub request was rate limited.")
        {
            
        }
    }
    
    /*
     * Class for a specific GitHub remote.
     */
    public class GitHubManifestEntry
    {
        public string repository;
        public string directory;
        public string lastCommit;
        private GitHubManifest manifest;
        
        /*
         * Creates the entry. 
         */
        protected internal GitHubManifestEntry(string repository,string directory,GitHubManifest manifest)
        {
            this.repository = repository;
            this.directory = directory;
            this.manifest = manifest;
            this.lastCommit = null;
        }
        
        /*
         * Creates an entry.
         * Intended only for JSON parsing.
         */
        public GitHubManifestEntry()
        {
            this.repository = null;
            this.directory = null;
            this.manifest = null;
            this.lastCommit = null;
        }
        
        /*
         * Sets the manifest to use.
         */
        protected internal void SetManifest(GitHubManifest manifest)
        {
            this.manifest = manifest;
        }
        
        /*
         * Performs a GET request.
         */
        public string PerformRequest(string url)
        {
            // Send the HTTP request.
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent","NLUL Commits Fetch");
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
        
        /*
         * Returns the latest commit of a branch.
         */
        public string GetLatestCommit(string branch)
        {
            // Send the HTTP request.
            var commitsJson = this.PerformRequest("https://api.github.com/repos/" + this.repository + "/commits/" + branch);
            
            // Parse the JSON and return the last tag.
            var commits = JsonConvert.DeserializeObject<GitCommitResult>(commitsJson);
            return commits.sha;
        }
        
        /*
         * Returns if the fetched branch is the latest.
         */
        public bool IsBranchUpToDate(string branch)
        {
            return this.lastCommit == this.GetLatestCommit(branch);
        }
        
        /*
         * Returns the latest tag.
         */
        public GitHubTag GetLatestTag()
        {
            // Send the HTTP request.
            var tagsJson = this.PerformRequest("https://api.github.com/repos/" + this.repository + "/tags");
            
            // Parse the JSON and return the last tag's commit.
            var tags = JsonConvert.DeserializeObject<List<GitTagResult>>(tagsJson);
            if (tags.Count > 0)
            {
                return new GitHubTag()
                {
                    commit = tags[0].commit.sha,
                    name = tags[0].name,
                };
            }
            return null;
        }
        
        /*
         * Returns if the fetched tag is the latest.
         */
        public bool IsTagUpToDate()
        {
            return this.lastCommit == this.GetLatestTag().commit;
        }
        
        /*
         * Fetches a specified commit.
         */
        public void FetchCommit(string commit,bool force = false)
        {
            // Return if the commit is the same as the latest fetch.
            if (commit == this.lastCommit && force != true)
            {
                return;
            }
            
            // Delete the existing files.
            var parentDirectory = Directory.GetParent(this.directory).FullName;
            var zipDirectory = Path.Combine(parentDirectory,commit + ".zip");
            if (File.Exists(zipDirectory))
            {
                File.Delete(zipDirectory);
            }
            if (Directory.Exists(this.directory))
            {
                Directory.Delete(this.directory,true);
            }
            
            // Download the ZIP of the commit.
            var client = new WebClient();
            client.DownloadFile("https://github.com/" + this.repository + "/archive/" + commit + ".zip",zipDirectory);
            
            // Uncompress the ZIP file to the temporary directory.
            var temporaryDirectory = Path.Combine(parentDirectory,commit);
            ZipFile.ExtractToDirectory(zipDirectory,temporaryDirectory);
            
            // Move the directory.
            Directory.Move(Path.Combine(temporaryDirectory,Directory.GetDirectories(temporaryDirectory)[0]),this.directory);
            
            // Clear the files.
            File.Delete(zipDirectory);
            Directory.Delete(temporaryDirectory,true);
            
            // Save the manifest.
            this.lastCommit = commit;
            this.manifest.Save();
        }
        
        /*
         * Fetches the latest branch.
         */
        public void FetchLatestBranch(string branch,bool force = false)
        {
            this.FetchCommit(this.GetLatestCommit(branch),force);
        }
        
        /*
         * Fetches the latest tag.
         */
        public void FetchLatestTag(bool force = false)
        {
            this.FetchCommit(this.GetLatestTag().commit,force);
        }
    }
    
    /*
     * Class for storing GitHub fetches.
     */
    public class GitHubManifest
    {
        private List<GitHubManifestEntry> manifest;
        private string fileLocation;
        
        /*
         * Creates the manifest.
         */
        public GitHubManifest(string fileLocation)
        {
            this.fileLocation = fileLocation;
            this.manifest = new List<GitHubManifestEntry>();
            
            // Try to parse the file.
            if (File.Exists(fileLocation))
            {
                try
                {
                    this.manifest = JsonConvert.DeserializeObject<List<GitHubManifestEntry>>(File.ReadAllText(fileLocation));
                }
                catch (JsonException)
                {
                    
                }
            }
        }
        
        /*
         * Returns the entry for a remote and file location.
         */
        public GitHubManifestEntry GetEntry(string repository,string directory)
        {
            // Find and return the entry if it exists.
            foreach (var entry in this.manifest)
            {
                if (string.Equals(entry.repository,repository,StringComparison.CurrentCultureIgnoreCase) && string.Equals(entry.directory,directory,StringComparison.CurrentCultureIgnoreCase))
                {
                    entry.SetManifest(this);
                    return entry;
                }
            }
            
            // Create and return a new entry.
            var newEntry = new GitHubManifestEntry(repository,directory,this);
            this.manifest.Add(newEntry);
            return newEntry;
        }
        
        /*
         * Saves the manifest.
         */
        protected internal void Save()
        {
            File.WriteAllText(this.fileLocation,JsonConvert.SerializeObject(this.manifest,Formatting.Indented));
        }
    }
}