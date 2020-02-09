/*
 * TheNexusAvenger
 *
 * Helper methods for GitHub.
 */

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
        public static string GetLastCommit(string remote,string branch)
        {
            // Get the commits.
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Uchu Commits Fetch");
            var commitsResponse  = client.GetAsync("https://api.github.com/repos/" + remote + "/commits/" + branch).Result;
            var commitsJson = commitsResponse.Content.ReadAsStringAsync().Result;
            
            // Parse the JSON and return the last commit.
            var commits = JsonConvert.DeserializeObject<GitCommit>(commitsJson);
            return commits.sha;
        }
    }
}