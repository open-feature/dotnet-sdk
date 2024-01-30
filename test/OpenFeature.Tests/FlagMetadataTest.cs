using System.Collections.Generic;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests
{
    public class FlagMetadataTest
    {
        [Fact]
        public void GetBool_Should_Return_Null_If_Key_Not_Found()
        {
            // Arrange
            var metadata = new Dictionary<string, object>();
            var flagMetadata = new FlagMetadata(metadata);

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
                { "boolKey", true }
            };
            var flagMetadata = new FlagMetadata(metadata);

            // Act
            var result = flagMetadata.GetBool("boolKey");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetInt_Should_Return_Null_If_Key_Not_Found()
        {
            // Arrange
            var metadata = new Dictionary<string, object>();
            var flagMetadata = new FlagMetadata(metadata);

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
        public void GetDouble_Should_Return_Null_If_Key_Not_Found()
        {
            // Arrange
            var metadata = new Dictionary<string, object>();
            var flagMetadata = new FlagMetadata(metadata);

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
        public void GetString_Should_Return_Null_If_Key_Not_Found()
        {
            // Arrange
            var metadata = new Dictionary<string, object>();
            var flagMetadata = new FlagMetadata(metadata);

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
    }
}
