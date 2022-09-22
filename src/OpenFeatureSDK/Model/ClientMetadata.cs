namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// Represents the client metadata
    /// </summary>
    public class ClientMetadata : Metadata
    {
        /// <summary>
        /// Version of the client
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMetadata"/> class
        /// </summary>
        /// <param name="name">Name of client</param>
        /// <param name="version">Version of client</param>
        public ClientMetadata(string name, string version) : base(name)
        {
            this.Version = version;
        }
    }
}
