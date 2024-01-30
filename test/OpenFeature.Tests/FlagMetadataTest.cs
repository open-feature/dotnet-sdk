using System;
using System.Collections.Generic;
using OpenFeature.Model;
using Xunit;

#nullable enable
namespace OpenFeature.Tests;

public class FlagMetadataTest
{
    [Fact]
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
        var exception = Assert.Throws<InvalidCastException>(() => flagMetadata.GetBool("wrongKey"));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Cannot cast System.String to System.Boolean", exception.Message);
    }

    [Fact]
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
        var exception = Assert.Throws<InvalidCastException>(() => flagMetadata.GetInt("wrongKey"));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Cannot cast System.String to System.Int32", exception.Message);
    }

    [Fact]
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
        var exception = Assert.Throws<InvalidCastException>(() => flagMetadata.GetDouble("wrongKey"));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Cannot cast System.String to System.Double", exception.Message);
    }

    [Fact]
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
        var exception = Assert.Throws<InvalidCastException>(() => flagMetadata.GetString("wrongKey"));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Cannot cast System.Object to System.String", exception.Message);
    }
}
