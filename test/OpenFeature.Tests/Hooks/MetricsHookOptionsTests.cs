using OpenFeature.Hooks;
using OpenFeature.Model;

namespace OpenFeature.Tests.Hooks;

public class MetricsHookOptionsTests
{
    [Fact]
    public void Default_Options_Should_Be_Initialized_Correctly()
    {
        // Arrange & Act
        var options = MetricsHookOptions.Default;

        // Assert
        Assert.NotNull(options);
        Assert.Empty(options.CustomDimensions);
        Assert.Empty(options.FlagMetadataCallbacks);
    }

    [Fact]
    public void CreateBuilder_Should_Return_New_Builder_Instance()
    {
        // Arrange & Act
        var builder = MetricsHookOptions.CreateBuilder();

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<MetricsHookOptions.MetricsHookOptionsBuilder>(builder);
    }

    [Fact]
    public void Build_Should_Return_Options()
    {
        // Arrange
        var builder = MetricsHookOptions.CreateBuilder();

        // Act
        var options = builder.Build();

        // Assert
        Assert.NotNull(options);
        Assert.IsType<MetricsHookOptions>(options);
    }

    [Theory]
    [InlineData("custom_dimension_value")]
    [InlineData(1.0)]
    [InlineData(2025)]
    [InlineData(null)]
    [InlineData(true)]
    public void Builder_Should_Allow_Adding_Custom_Dimensions(object? value)
    {
        // Arrange
        var builder = MetricsHookOptions.CreateBuilder();
        var key = "custom_dimension_key";

        // Act
        builder.WithCustomDimension(key, value);
        var options = builder.Build();

        // Assert
        Assert.Single(options.CustomDimensions);
        Assert.Equal(key, options.CustomDimensions.First().Key);
        Assert.Equal(value, options.CustomDimensions.First().Value);
    }

    [Fact]
    public void Builder_Should_Allow_Adding_Flag_Metadata_Expressions()
    {
        // Arrange
        var builder = MetricsHookOptions.CreateBuilder();
        var key = "flag_metadata_key";
        static object? expression(ImmutableMetadata m) => m.GetString("flag_metadata_key");

        // Act
        builder.WithFlagEvaluationMetadata(key, expression);
        var options = builder.Build();

        // Assert
        Assert.Single(options.FlagMetadataCallbacks);
        Assert.Equal(key, options.FlagMetadataCallbacks.First().Key);
        Assert.Equal(expression, options.FlagMetadataCallbacks.First().Value);
    }
}
