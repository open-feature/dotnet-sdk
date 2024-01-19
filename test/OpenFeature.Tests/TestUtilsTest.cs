using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests
{
    public class TestUtilsTest
    {
        [Fact]
        public async void Should_Fail_If_Assertion_Fails()
        {
            await Assert.ThrowsAnyAsync<Exception>(() => Utils.AssertUntilAsync(() => Assert.True(1.Equals(2)))).ConfigureAwait(false);
        }

        [Fact]
        public async void Should_Pass_If_Assertion_Fails()
        {
            await Utils.AssertUntilAsync(() => Assert.True(1.Equals(1))).ConfigureAwait(false);
        }
    }
}
