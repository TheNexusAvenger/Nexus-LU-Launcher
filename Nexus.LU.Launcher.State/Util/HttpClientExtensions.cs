using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nexus.LU.Launcher.State.Util;

public static class HttpClientExtensions
{
    /// <summary>
    /// Downloads a file.
    /// </summary>
    /// <param name="this">HttpClient to use to download.</param>
    /// <param name="address">Address to download from.</param>
    /// <param name="fileName">File to download to.</param>
    public static async Task DownloadFileAsync(this HttpClient @this, string address, string fileName)
    {
        await using var stream = await @this.GetStreamAsync(address);
        await using var fileStream = new FileStream(fileName, FileMode.CreateNew);
        await stream.CopyToAsync(fileStream);
    }
}