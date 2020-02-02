/*
 * TheNexusAvenger
 *
 * Helper methods for GitHub.
 */

using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace NLUL.Core.Server.Util
{
    /*
     * Class for a Git commit.
     */
    public class GitCommit
    {
        public string sha;
    }
    
    public class GitHub
    {
        /*
         * Returns the last commit id.
         */
        public static string GetLastCommit(string user,string repository)
        {
            // Get the commits.
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Uchu Commits Fetch");
            var commitsResponse  = client.GetAsync("https://api.github.com/repos/" + user + "/" + repository + "/commits").Result;
            var commitsJson = commitsResponse.Content.ReadAsStringAsync().Result;
            
            // Parse the JSON and return the last commit.
            var commits = JsonConvert.DeserializeObject<List<GitCommit>>(commitsJson);
            return commits[0].sha;
        }
    }
}