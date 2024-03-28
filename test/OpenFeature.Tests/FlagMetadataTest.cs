using System.Collections.Generic;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests;

public class FlagMetadataTest
{
    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    public void GetBool_Should_Return_Null_If_Key_Not_Found()
    {
        // Arrange
        var flagMetadata = new FlagMetadata();

        // Act
        var result = flagMetadata.GetBool("nonexistentKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    [Specification("1.4.14.1", "Condition: `Flag metadata` MUST be immutable.")]
    public void GetBool_Should_Return_Value_If_Key_Found()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            {
                "boolKey", true
            }
        };
        var flagMetadata = new FlagMetadata(metadata);

        // Act
        var result = flagMetadata.GetBool("boolKey");

        // Assert
        Assert.True(result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    public void GetBool_Should_Throw_Value_Is_Invalid()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            {
                "wrongKey", "11a"
            }
        };
        var flagMetadata = new FlagMetadata(metadata);

        // Act
        var result = flagMetadata.GetBool("wrongKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    public void GetInt_Should_Return_Null_If_Key_Not_Found()
    {
        // Arrange
        var flagMetadata = new FlagMetadata();

        // Act
        var result = flagMetadata.GetInt("nonexistentKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    [Specification("1.4.14.1", "Condition: `Flag metadata` MUST be immutable.")]
    public void GetInt_Should_Return_Value_If_Key_Found()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            {
                "intKey", 1
            }
        };
        var flagMetadata = new FlagMetadata(metadata);

        // Act
        var result = flagMetadata.GetInt("intKey");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    public void GetInt_Should_Throw_Value_Is_Invalid()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            {
                "wrongKey", "11a"
            }
        };
        var flagMetadata = new FlagMetadata(metadata);

        // Act
        var result = flagMetadata.GetInt("wrongKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    public void GetDouble_Should_Return_Null_If_Key_Not_Found()
    {
        // Arrange
        var flagMetadata = new FlagMetadata();

        // Act
        var result = flagMetadata.GetDouble("nonexistentKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    [Specification("1.4.14.1", "Condition: `Flag metadata` MUST be immutable.")]
    public void GetDouble_Should_Return_Value_If_Key_Found()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            {
                "doubleKey", 1.2
            }
        };
        var flagMetadata = new FlagMetadata(metadata);

        // Act
        var result = flagMetadata.GetDouble("doubleKey");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.2, result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    public void GetDouble_Should_Throw_Value_Is_Invalid()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            {
                "wrongKey", "11a"
            }
        };
        var flagMetadata = new FlagMetadata(metadata);

        // Act
        var result = flagMetadata.GetDouble("wrongKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    public void GetString_Should_Return_Null_If_Key_Not_Found()
    {
        // Arrange
        var flagMetadata = new FlagMetadata();

        // Act
        var result = flagMetadata.GetString("nonexistentKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    [Specification("1.4.14.1", "Condition: `Flag metadata` MUST be immutable.")]
    public void GetString_Should_Return_Value_If_Key_Found()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            {
                "stringKey", "11"
            }
        };
        var flagMetadata = new FlagMetadata(metadata);

        // Act
        var result = flagMetadata.GetString("stringKey");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("11", result);
    }

    [Fact]
    [Specification("1.4.14",
        "If the `flag metadata` field in the `flag resolution` structure returned by the configured `provider` is set, the `evaluation details` structure's `flag metadata` field MUST contain that value. Otherwise, it MUST contain an empty record.")]
    public void GetString_Should_Throw_Value_Is_Invalid()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            {
                "wrongKey", new object()
            }
        };
        var flagMetadata = new FlagMetadata(metadata);

        // Act
        var result = flagMetadata.GetString("wrongKey");

        // Assert
        Assert.Null(result);
    }
}
