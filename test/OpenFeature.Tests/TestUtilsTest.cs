namespace OpenFeature.Tests;

public class TestUtilsTest
{
    [Fact]
    public async Task Should_Fail_If_Assertion_Fails()
    {
        await Assert.ThrowsAnyAsync<Exception>(() => Utils.AssertUntilAsync(_ => Assert.True(1.Equals(2)), 100, 10));
    }

    [Fact]
    public async Task Should_Pass_If_Assertion_Fails()
    {
        await Utils.AssertUntilAsync(_ => Assert.True(1.Equals(1)));
    }
}
