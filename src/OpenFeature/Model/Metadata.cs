namespace OpenFeature.Model
{
    /// <summary>
    /// <see cref="Api"/> metadata
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Gets name of <see cref="Api"/> instance
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Metadata"/> class.
        /// </summary>
        /// <param name="name">Name of <see cref="Api"/> instance</param>
        public Metadata(string name)
        {
            this.Name = name;
        }
    }
}
