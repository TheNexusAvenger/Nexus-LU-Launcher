using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nexus.LU.Launcher.State.Model;

public class ArchivePatchJson
{
    /// <summary>
    /// Name of the patch by localization.
    /// </summary>
    [JsonPropertyName("name")]
    public Dictionary<string, string>? Name { get; set; } = null!;
    
    /// <summary>
    /// Description of the patch by localization.
    /// </summary>
    [JsonPropertyName("description")]
    public Dictionary<string, string>? Description { get; set; } = null!;
    
    /// <summary>
    /// Optional requirements in order for the patch to be applied.
    /// </summary>
    [JsonPropertyName("requirements")]
    public List<string>? Requirements { get; set; }
}

[JsonSerializable(typeof(ArchivePatchJson))]
[JsonSourceGenerationOptions(WriteIndented=true, IncludeFields = true)]
internal partial class ArchivePatchJsonJsonContext : JsonSerializerContext
{
}