using System;
using OpenFeature.Constant;
using OpenFeature.Extention;

namespace OpenFeature.Error
{
    public class FeatureProviderException : Exception
    {
        public ErrorType ErrorType { get; }
        public string ErrorTypeDescription { get; }

        public FeatureProviderException(ErrorType errorType, Exception innerException)
            : base(null, innerException)
        {
            this.ErrorType = errorType;
            this.ErrorTypeDescription = errorType.GetDescription();
        }
    }
}
