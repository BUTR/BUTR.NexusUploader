namespace NexusUploader
{
    public class GameRef
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public GameRef() { }

        public GameRef(string name, int id)
        {
            Name = name;
            Id = id.ToString();
        }
    }
}