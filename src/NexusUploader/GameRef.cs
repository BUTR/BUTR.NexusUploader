namespace NexusUploader.Models;

public class GameRef
{
    public string Name { get; set; } = default!;
    public string Id { get; set; } = default!;

    public GameRef() { }

    public GameRef(string name, int id)
    {
        Name = name;
        Id = id.ToString();
    }
}