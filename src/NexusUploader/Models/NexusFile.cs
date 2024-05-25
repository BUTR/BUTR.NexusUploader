using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NexusUploader.Models;

public class NexusFile
{
    [JsonPropertyName("id")]
    public List<int> Ids { get; set; } = default!;

    [JsonPropertyName("file_id")]
    public int FileId { get; set; } = default!;

    [JsonPropertyName("category_name")]
    public string CategoryName { get; set; } = default!;

    [JsonPropertyName("version")]
    public string FileVersion { get; set; } = default!;
}