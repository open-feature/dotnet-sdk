using OpenFeature.Model;
using OpenFeature.Tests.Internal;

namespace OpenFeature.Tests;

public class DomainScopedProviderTests : ClearOpenFeatureInstanceFixture
{
    [Fact]
    [Specification("2.4.1", "The `provider` MAY define an initialization function which accepts the global `evaluation context` and an optional bound `domain`, which performs initialization logic relevant to the provider.")]
    public async Task Default_Provider_Is_Initialized_With_A_Null_Domain()
    {
        var provider = new DomainRecordingProvider();

        await Api.Instance.SetProviderAsync(provider, TestContext.Current.CancellationToken);

        Assert.Equal(1, provider.InitializeCount);
        Assert.Null(provider.LastDomain);
    }

    [Fact]
    [Specification("1.1.2.2", "The `provider mutator` function MUST invoke the `initialize` function on the newly registered provider before using it to resolve flag values, supplying the bound `domain`, if any.")]
    [Specification("2.4.1", "The `provider` MAY define an initialization function which accepts the global `evaluation context` and an optional bound `domain`, which performs initialization logic relevant to the provider.")]
    [Specification("2.4.4", "A `provider` that declares itself `domain-scoped` MUST accept the bound `domain` during initialization.")]
    public async Task Named_Provider_Is_Initialized_With_Its_Bound_Domain()
    {
        var provider = new DomainRecordingProvider { DomainScoped = true };

        await Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);

        Assert.Equal(1, provider.InitializeCount);
        Assert.Equal("domain-a", provider.LastDomain);
    }

    [Fact]
    [Specification("1.1.8.1", "The `provider mutator` MUST NOT bind a `domain-scoped` provider instance to more than one `domain`, rejecting any attempt to bind an already-bound instance to an additional `domain`.")]
    public async Task Domain_Scoped_Provider_Cannot_Be_Bound_To_A_Second_Domain()
    {
        var provider = new DomainRecordingProvider { DomainScoped = true };

        await Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Api.Instance.SetProviderAsync("domain-b", provider, TestContext.Current.CancellationToken));
    }

    [Fact]
    [Specification("1.1.8.1", "The `provider mutator` MUST NOT bind a `domain-scoped` provider instance to more than one `domain`, rejecting any attempt to bind an already-bound instance to an additional `domain`.")]
    public async Task Domain_Scoped_Named_Provider_Cannot_Be_Bound_As_The_Default_Provider()
    {
        var provider = new DomainRecordingProvider { DomainScoped = true };

        await Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Api.Instance.SetProviderAsync(provider, TestContext.Current.CancellationToken));
    }

    [Fact]
    [Specification("1.1.8.1", "The `provider mutator` MUST NOT bind a `domain-scoped` provider instance to more than one `domain`, rejecting any attempt to bind an already-bound instance to an additional `domain`.")]
    public async Task Domain_Scoped_Default_Provider_Cannot_Be_Bound_To_A_Domain()
    {
        var provider = new DomainRecordingProvider { DomainScoped = true };

        await Api.Instance.SetProviderAsync(provider, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken));
    }

    [Fact]
    [Specification("1.1.8.1", "The `provider mutator` MUST NOT bind a `domain-scoped` provider instance to more than one `domain`, rejecting any attempt to bind an already-bound instance to an additional `domain`.")]
    public async Task Rejected_Binding_Leaves_The_Existing_Binding_Intact()
    {
        var provider = new DomainRecordingProvider { DomainScoped = true };

        await Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Api.Instance.SetProviderAsync("domain-b", provider, TestContext.Current.CancellationToken));

        Assert.Equal(1, provider.InitializeCount);
        Assert.Equal("domain-a", provider.LastDomain);
        Assert.Same(provider, Api.Instance.GetProvider("domain-a"));
        Assert.NotSame(provider, Api.Instance.GetProvider("domain-b"));
        Assert.Equal(0, provider.ShutdownCount);
    }

    [Fact]
    [Specification("1.1.8.1", "The `provider mutator` MUST NOT bind a `domain-scoped` provider instance to more than one `domain`, rejecting any attempt to bind an already-bound instance to an additional `domain`.")]
    public async Task Domain_Scoped_Provider_Can_Be_Set_Again_For_The_Same_Domain()
    {
        var provider = new DomainRecordingProvider { DomainScoped = true };

        await Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);
        await Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);

        Assert.Equal(1, provider.InitializeCount);
        Assert.Equal("domain-a", provider.LastDomain);
        Assert.Equal(0, provider.ShutdownCount);
    }

    [Fact]
    [Specification("2.4.1", "The `provider` MAY define an initialization function which accepts the global `evaluation context` and an optional bound `domain`, which performs initialization logic relevant to the provider.")]
    public async Task Non_Domain_Scoped_Provider_Can_Back_Multiple_Domains()
    {
        var provider = new DomainRecordingProvider();

        await Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);
        await Api.Instance.SetProviderAsync("domain-b", provider, TestContext.Current.CancellationToken);

        Assert.Equal(1, provider.InitializeCount);
        Assert.Equal("domain-a", provider.LastDomain);
        Assert.Same(provider, Api.Instance.GetProvider("domain-a"));
        Assert.Same(provider, Api.Instance.GetProvider("domain-b"));
    }

    [Fact]
    [Specification("1.1.8.1", "The `provider mutator` MUST NOT bind a `domain-scoped` provider instance to more than one `domain`, rejecting any attempt to bind an already-bound instance to an additional `domain`.")]
    public async Task Domain_Scoped_Provider_Can_Be_Bound_To_A_New_Domain_After_It_Is_Replaced()
    {
        var provider = new DomainRecordingProvider { DomainScoped = true };

        await Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);
        await Api.Instance.SetProviderAsync("domain-a", new DomainRecordingProvider(), TestContext.Current.CancellationToken);

        Assert.Equal(1, provider.ShutdownCount);

        // Releasing the provider clears its binding, so the instance is free to back a different domain.
        await Api.Instance.SetProviderAsync("domain-b", provider, TestContext.Current.CancellationToken);

        Assert.Same(provider, Api.Instance.GetProvider("domain-b"));

        // The instance is bound to the new domain, but it is not initialized again. Initialization state lives
        // on the instance, so the domain it was initialized with is still the first one.
        Assert.Equal(1, provider.InitializeCount);
        Assert.Equal("domain-a", provider.LastDomain);
    }

    [Fact]
    [Specification("1.1.8.1", "The `provider mutator` MUST NOT bind a `domain-scoped` provider instance to more than one `domain`, rejecting any attempt to bind an already-bound instance to an additional `domain`.")]
    public async Task Domain_Scoped_Provider_Can_Be_Bound_To_A_New_Domain_After_Api_Shutdown()
    {
        var provider = new DomainRecordingProvider { DomainScoped = true };

        await Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);
        await Api.Instance.ShutdownAsync();

        await Api.Instance.SetProviderAsync("domain-b", provider, TestContext.Current.CancellationToken);

        Assert.Same(provider, Api.Instance.GetProvider("domain-b"));

        // The instance is bound to the new domain, but it is not initialized again. Initialization state lives
        // on the instance, so the domain it was initialized with is still the first one.
        Assert.Equal(1, provider.InitializeCount);
        Assert.Equal("domain-a", provider.LastDomain);
    }

    [Fact]
    [Specification("1.1.8.1", "The `provider mutator` MUST NOT bind a `domain-scoped` provider instance to more than one `domain`, rejecting any attempt to bind an already-bound instance to an additional `domain`.")]
    public async Task Repository_Establishes_The_Domain_Binding_As_It_Installs_A_Provider()
    {
        var repository = new ProviderRepository();
        var provider = new DomainRecordingProvider { DomainScoped = true };
        var context = new EvaluationContextBuilder().Build();

        await repository.SetProviderAsync("domain-a", provider, context, cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(provider.TryBindDomain("domain-b"));
        Assert.True(provider.TryBindDomain("domain-a"));
    }

    [Fact]
    [Specification("1.1.8.1", "The `provider mutator` MUST NOT bind a `domain-scoped` provider instance to more than one `domain`, rejecting any attempt to bind an already-bound instance to an additional `domain`.")]
    public async Task Repository_Rejects_A_Domain_Scoped_Provider_Bound_After_The_Api_Validated_It()
    {
        // The Api's check is advisory and not atomic with the install, so the repository has to make the
        // decision itself. Binding the instance elsewhere first stands in for a concurrent registration
        // slipping in between the two.
        var repository = new ProviderRepository();
        var incumbent = new DomainRecordingProvider();
        var provider = new DomainRecordingProvider { DomainScoped = true };
        var context = new EvaluationContextBuilder().Build();

        await repository.SetProviderAsync("domain-a", incumbent, context, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(provider.TryBindDomain("domain-b"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.SetProviderAsync("domain-a", provider, context, cancellationToken: TestContext.Current.CancellationToken));

        // The rejection leaves the existing registration and the existing binding intact.
        Assert.Same(incumbent, repository.GetProvider("domain-a"));
        Assert.Empty(provider.Domains);
        Assert.True(provider.TryBindDomain("domain-b"));
    }

    [Fact]
    public async Task Repository_Does_Not_Bind_A_Provider_That_Is_Not_Domain_Scoped()
    {
        var repository = new ProviderRepository();
        var provider = new DomainRecordingProvider();
        var context = new EvaluationContextBuilder().Build();

        await repository.SetProviderAsync("domain-a", provider, context, cancellationToken: TestContext.Current.CancellationToken);

        // Nothing was recorded, so the instance remains free to back other domains.
        Assert.True(provider.TryBindDomain("domain-b"));
    }

    [Fact]
    public async Task Non_Domain_Scoped_Provider_Is_Never_Rejected_For_A_Second_Domain()
    {
        var provider = new DomainRecordingProvider();

        await Api.Instance.SetProviderAsync(provider, TestContext.Current.CancellationToken);
        await Api.Instance.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);

        Assert.Same(provider, Api.Instance.GetProvider());
        Assert.Same(provider, Api.Instance.GetProvider("domain-a"));
    }

    [Fact]
    [Specification("1.1.8.1", "The `provider mutator` MUST NOT bind a `domain-scoped` provider instance to more than one `domain`, rejecting any attempt to bind an already-bound instance to an additional `domain`.")]
    public async Task Domain_Scoped_Provider_Can_Move_To_Another_Api_Only_After_The_First_Api_Releases_It()
    {
        var first = new Api();
        var second = new Api();
        var provider = new DomainRecordingProvider { DomainScoped = true };

        await first.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken);

        // While the first Api owns the instance, the second Api cannot bind it, even to the same domain.
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            second.SetProviderAsync("domain-a", provider, TestContext.Current.CancellationToken));

        // A full release clears both the domain binding and the API ownership.
        await first.ShutdownAsync();

        await second.SetProviderAsync("domain-b", provider, TestContext.Current.CancellationToken);

        Assert.Same(provider, second.GetProvider("domain-b"));
        Assert.False(provider.TryBindDomain("domain-c"));

        await second.ShutdownAsync();
    }

    private class DomainRecordingProvider : FeatureProvider
    {
        private readonly Metadata _metadata = new(nameof(DomainRecordingProvider));

        public bool DomainScoped { get; set; }

        public override bool IsDomainScoped => this.DomainScoped;

        public int InitializeCount => this.Domains.Count;

        public int ShutdownCount { get; private set; }

        public List<string?> Domains { get; } = [];

        public string? LastDomain => this.Domains.Count == 0 ? null : this.Domains[this.Domains.Count - 1];

        public override Metadata GetMetadata() => this._metadata;

        public override Task InitializeAsync(EvaluationContext context, string? domain, CancellationToken cancellationToken = default)
        {
            this.Domains.Add(domain);
            return Task.CompletedTask;
        }

        public override Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            this.ShutdownCount++;
            return Task.CompletedTask;
        }

        public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new ResolutionDetails<bool>(flagKey, defaultValue));

        public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new ResolutionDetails<string>(flagKey, defaultValue));

        public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new ResolutionDetails<int>(flagKey, defaultValue));

        public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new ResolutionDetails<double>(flagKey, defaultValue));

        public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new ResolutionDetails<Value>(flagKey, defaultValue));
    }
}
