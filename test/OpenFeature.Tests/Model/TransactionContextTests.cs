using System.Threading.Tasks;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests.Model;

public class TransactionContextTests
{
    [Fact]
    public void SetTransactionContext_ShouldStoreTransactionContext()
    {
        // Arrange
        var api = Api.Instance;
        var transactionContext = TransactionContext.Builder()
            .Set("userId", "12345")
            .Set("userAgent", "Mozilla/5.0")
            .BuildTransactionContext();

        // Act
        api.SetTransactionContext(transactionContext);

        // Assert
        var context = api.GetContext();
        Assert.Equal("12345", context.GetValue("userId").AsString);
        Assert.Equal("Mozilla/5.0", context.GetValue("userAgent").AsString);
    }

    [Fact]
    public async Task EvaluateFlagAsync_ShouldUseTransactionContext()
    {
        // Arrange
        var api = Api.Instance;
        var transactionContext = EvaluationContext.Builder()
            .Set("userId", "12345")
            .Set("userAgent", "Mozilla/5.0")
            .BuildTransactionContext();
        api.SetTransactionContext(transactionContext);

        var client = api.GetClient();
        var flagKey = "test-flag";
        var defaultValue = "default-value";

        // Act
        var result = await client.GetStringValueAsync(flagKey, defaultValue);

        // Assert
        Assert.Equal("default-value", result);
    }
}
