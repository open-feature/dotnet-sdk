using OpenFeature.Constant;

namespace OpenFeature.Model
{
    public class ResolutionDetails<T>
    {
        public T Value { get; }
        public string FlagKey { get; }
        public ErrorType ErrorType { get; }
        public string Reason { get; }
        public string Variant { get; }

        public ResolutionDetails(string flagKey, T value, ErrorType errorType = ErrorType.None, string reason = null, string variant = null)
        {
            this.Value = value;
            this.FlagKey = flagKey;
            this.ErrorType = errorType;
            this.Reason = reason;
            this.Variant = variant;
        }
    }
}
