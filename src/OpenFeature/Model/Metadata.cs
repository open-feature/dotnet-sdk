namespace OpenFeature.SDK.Model
{
    /// <summary>
    /// <see cref="OpenFeature"/> metadata
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Gets name of <see cref="OpenFeature"/> instance
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metadata"/> class.
        /// </summary>
        /// <param name="name">Name of <see cref="OpenFeature"/> instance</param>
        public Metadata(string name)
        {
            this.Name = name;
        }
    }
}
