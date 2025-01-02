using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests;

public class AsyncLocalTransactionContextPropagatorTests
{
    [Fact]
    public void GetTransactionContext_ReturnsEmpty_WhenNoContextIsSet()
    {
        // Arrange
        var propagator = new AsyncLocalTransactionContextPropagator();

        // Act
        var context = propagator.GetTransactionContext();

        // Assert
        Assert.Equal(EvaluationContext.Empty, context);
    }

    [Fact]
    public void SetTransactionContext_SetsAndGetsContextCorrectly()
    {
        // Arrange
        var propagator = new AsyncLocalTransactionContextPropagator();
        var evaluationContext = EvaluationContext.Builder()
            .Set("initial", "yes")
            .Build();

        // Act
        propagator.SetTransactionContext(evaluationContext);
        var context = propagator.GetTransactionContext();

        // Assert
        Assert.Equal(evaluationContext, context);
        Assert.Equal(evaluationContext.GetValue("initial"), context.GetValue("initial"));
    }

    [Fact]
    public void SetTransactionContext_OverridesPreviousContext()
    {
        // Arrange
        var propagator = new AsyncLocalTransactionContextPropagator();

        var initialContext = EvaluationContext.Builder()
            .Set("initial", "yes")
            .Build();
        var newContext = EvaluationContext.Empty;

        // Act
        propagator.SetTransactionContext(initialContext);
        propagator.SetTransactionContext(newContext);
        var context = propagator.GetTransactionContext();

        // Assert
        Assert.Equal(newContext, context);
    }
}
