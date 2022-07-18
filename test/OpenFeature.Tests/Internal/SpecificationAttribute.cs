using System;

namespace OpenFeature.SDK.Tests.Internal
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class SpecificationAttribute : Attribute
    {
        public string Code { get; }
        public string Description { get; }

        public SpecificationAttribute(string code, string description)
        {
            this.Code = code;
            this.Description = description;
        }
    }
}
