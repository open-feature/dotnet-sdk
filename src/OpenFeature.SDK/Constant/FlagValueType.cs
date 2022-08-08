namespace OpenFeature.SDK.Constant
{
    /// <summary>
    ///  Used to identity what object type of flag being evaluated
    /// </summary>
    public enum FlagValueType
    {
        /// <summary>
        /// Flag is a boolean value
        /// </summary>
        Boolean,

        /// <summary>
        /// Flag is a string value
        /// </summary>
        String,

        /// <summary>
        /// Flag is a numeric value
        /// </summary>
        Number,

        /// <summary>
        /// Flag is a structured value
        /// </summary>
        Object
    }
}
