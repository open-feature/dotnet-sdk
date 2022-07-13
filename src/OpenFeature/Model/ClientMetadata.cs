namespace OpenFeature.Model
{
    public class ClientMetadata : Metadata
    {
        public string Version { get; }

        public ClientMetadata(string name, string version) : base(name)
        {
            this.Version = version;
        }
    }
}
