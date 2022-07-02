namespace OpenFeature.Model
{
    public class ResolutionDetails<T>
    {
        public T Value { get; }
        public string FlagKey { get; }
        public string ErrorCode { get; }
        public string Reason { get; }
        public string Variant { get; }

        public ResolutionDetails(string flagKey, T value, string errorCode = null, string reason = null, string variant = null)
        {
            Value = value;
            FlagKey = flagKey;
            ErrorCode = errorCode;
            Reason = reason;
            Variant = variant;
        }
    }
}
