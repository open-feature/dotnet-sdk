using System;
using System.Collections.Generic;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests;

public class TrackingEventDetailsTest
{
    [Fact]
    [Specification("6.2.1", "The `tracking event details` structure MUST define an optional numeric `value`, associating a scalar quality with an `tracking event`.")]
    public void TrackingEventDetails_HasAnOptionalValueProperty()
    {
        var builder = new TrackingEventDetailsBuilder();
        var details = builder.Build();
        Assert.Null(details.Value);
    }

    [Fact]
    [Specification("6.2.1", "The `tracking event details` structure MUST define an optional numeric `value`, associating a scalar quality with an `tracking event`.")]
    public void TrackingEventDetails_HasAValueProperty()
    {
        const double value = 23.5;
        var builder = new TrackingEventDetailsBuilder().SetValue(value);
        var details = builder.Build();
        Assert.Equal(value, details.Value);
    }

    [Fact]
    [Specification("6.2.2", "The `tracking event details` MUST support the inclusion of custom fields, having keys of type `string`, and values of type `boolean | string | number | structure`.")]
    public void TrackingEventDetails_CanTakeValues()
    {
        var structure = new Structure(new Dictionary<string, Value> { { "key", new Value("value") } });
        var dateTimeValue = new Value(DateTime.Now);
        var builder = TrackingEventDetails.Builder()
            .Set("boolean", true)
            .Set("string", "some string")
            .Set("double", 123.3)
            .Set("structure", structure)
            .Set("value", dateTimeValue);
        var details = builder.Build();
        Assert.Equal(5, details.Count);
        Assert.Equal(true, details.GetValue("boolean").AsBoolean);
        Assert.Equal("some string", details.GetValue("string").AsString);
        Assert.Equal(123.3, details.GetValue("double").AsDouble);
        Assert.Equal(structure, details.GetValue("structure").AsStructure);
        Assert.Equal(dateTimeValue, details.GetValue("value"));
    }
}
