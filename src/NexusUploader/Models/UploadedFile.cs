using System.Text.Json.Serialization;

namespace NexusUploader.Nexus;

public class UploadedFile
{
    [JsonPropertyName("uuid")]
    public string Id { get; set; } = default!;

    [JsonIgnore]
    public int FileSize { get; set; } = default!;

    [JsonPropertyName("filename")]
    public string FileName { get; set; } = default!;

    [JsonIgnore]
    public string OriginalFile { get; set; } = default!;
}