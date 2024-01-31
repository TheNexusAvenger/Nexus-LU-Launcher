using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nexus.LU.Launcher.State.Util;

public class GitHubTag
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
    
[JsonSerializable(typeof(List<GitHubTag>))]
internal partial class GitHubTagJsonContext : JsonSerializerContext
{
}

public static class GitHubUtil
{
    /// <summary>
    /// Returns the latest tag for a GitHub repository.
    /// Throws an exception when the request fails or a tag does not exist.
    /// </summary>
    /// <param name="repository">GitHub repository to get the latest tag for.</param>
    /// <returns>Latest tag of the repository.</returns>
    public static async Task<string> GetLatestTagAsync(string repository)
    {
        // Fetch the latest tags.
        var url = $"https://api.github.com/repos/{repository}/tags";
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Nexus-LU-Launcher Tag Fetch");
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        // Parse the response and return the latest tag.
        var responseBody = await response.Content.ReadAsStringAsync();
        var tags = JsonSerializer.Deserialize<List<GitHubTag>>(responseBody, GitHubTagJsonContext.Default.ListGitHubTag)!;
        if (tags[0].Name == null)
        {
            throw new HttpRequestException("GitHub API returned tag without a name.");
        }
        return tags[0].Name;
    }
}