using Microsoft.Extensions.DependencyInjection;
using OpenFeature.Providers.MultiProvider.Models;
using OpenFeature.Providers.MultiProvider.Strategies;

namespace OpenFeature.Providers.MultiProvider.DependencyInjection;

/// <summary>
/// Builder for configuring a multi-provider with dependency injection.
/// </summary>
public class MultiProviderBuilder
{
    private readonly List<Func<IServiceProvider, ProviderEntry>> _providerFactories = [];
    private Func<IServiceProvider, BaseEvaluationStrategy>? _strategyFactory;

    /// <summary>
    /// Adds a provider to the multi-provider configuration using a factory method.
    /// </summary>
    /// <param name="name">The name for the provider.</param>
    /// <param name="factory">A factory method to create the provider instance.</param>
    /// <returns>The <see cref="MultiProviderBuilder"/> instance for chaining.</returns>
    public MultiProviderBuilder AddProvider(string name, Func<IServiceProvider, FeatureProvider> factory)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Provider name cannot be null or empty.", nameof(name));
        }

        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory), "Provider configuration cannot be null.");
        }

        return AddProvider<FeatureProvider>(name, sp => factory(sp));
    }

    /// <summary>
    /// Adds a provider to the multi-provider configuration using a type.
    /// </summary>
    /// <typeparam name="TProvider">The type of the provider to add.</typeparam>
    /// <param name="name">The name for the provider.</param>
    /// <param name="factory">An optional factory method to create the provider instance. If not provided, the provider will be resolved from the service provider.</param>
    /// <returns>The <see cref="MultiProviderBuilder"/> instance for chaining.</returns>
    public MultiProviderBuilder AddProvider<TProvider>(string name, Func<IServiceProvider, TProvider>? factory = null)
        where TProvider : FeatureProvider
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Provider name cannot be null or empty.", nameof(name));
        }

        this._providerFactories.Add(sp =>
        {
            var provider = factory != null
                ? factory(sp)
                : sp.GetRequiredService<TProvider>();
            return new ProviderEntry(provider, name);
        });

        return this;
    }

    /// <summary>
    /// Adds a provider instance to the multi-provider configuration.
    /// </summary>
    /// <param name="name">The name for the provider.</param>
    /// <param name="provider">The provider instance to add.</param>
    /// <returns>The <see cref="MultiProviderBuilder"/> instance for chaining.</returns>
    public MultiProviderBuilder AddProvider(string name, FeatureProvider provider)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Provider name cannot be null or empty.", nameof(name));
        }

        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider), "Provider configuration cannot be null.");
        }

        return AddProvider<FeatureProvider>(name, _ => provider);
    }

    /// <summary>
    /// Sets the evaluation strategy for the multi-provider.
    /// </summary>
    /// <typeparam name="TStrategy">The type of the evaluation strategy.</typeparam>
    /// <returns>The <see cref="MultiProviderBuilder"/> instance for chaining.</returns>
    public MultiProviderBuilder UseStrategy<TStrategy>()
        where TStrategy : BaseEvaluationStrategy, new()
    {
        return UseStrategy(static _ => new TStrategy());
    }

    /// <summary>
    /// Sets the evaluation strategy for the multi-provider using a factory method.
    /// </summary>
    /// <param name="factory">A factory method to create the strategy instance.</param>
    /// <returns>The <see cref="MultiProviderBuilder"/> instance for chaining.</returns>
    public MultiProviderBuilder UseStrategy(Func<IServiceProvider, BaseEvaluationStrategy> factory)
    {
        this._strategyFactory = factory ?? throw new ArgumentNullException(nameof(factory), "Strategy for multi-provider cannot be null.");
        return this;
    }

    /// <summary>
    /// Sets the evaluation strategy for the multi-provider.
    /// </summary>
    /// <param name="strategy">The strategy instance to use.</param>
    /// <returns>The <see cref="MultiProviderBuilder"/> instance for chaining.</returns>
    public MultiProviderBuilder UseStrategy(BaseEvaluationStrategy strategy)
    {
        if (strategy == null)
        {
            throw new ArgumentNullException(nameof(strategy));
        }

        return UseStrategy(_ => strategy);
    }

    /// <summary>
    /// Builds the provider entries using the service provider.
    /// </summary>
    internal List<ProviderEntry> BuildProviderEntries(IServiceProvider serviceProvider)
    {
        return this._providerFactories.Select(factory => factory(serviceProvider)).ToList();
    }

    /// <summary>
    /// Builds the evaluation strategy using the service provider.
    /// </summary>
    internal BaseEvaluationStrategy? BuildEvaluationStrategy(IServiceProvider serviceProvider)
    {
        return this._strategyFactory?.Invoke(serviceProvider);
    }
}
