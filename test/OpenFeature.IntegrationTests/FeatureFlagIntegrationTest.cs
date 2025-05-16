using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OpenFeature.Constant;
using OpenFeature.DependencyInjection;
using OpenFeature.DependencyInjection.Providers.Memory;
using OpenFeature.Hooks;
using OpenFeature.IntegrationTests.Services;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using Xunit.Abstractions;

namespace OpenFeature.IntegrationTests;

public class FeatureFlagIntegrationTest(ITestOutputHelper outputHelper)
{
    // TestUserId is "off", other users are "on"
    private const string FeatureA = "feature-a";
    private const string TestUserId = "123";

    [Theory]
    [InlineData(TestUserId, false, ServiceLifetime.Singleton)]
    [InlineData(TestUserId, false, ServiceLifetime.Scoped)]
    [InlineData(TestUserId, false, ServiceLifetime.Transient)]
    [InlineData("SomeOtherId", true, ServiceLifetime.Singleton)]
    [InlineData("SomeOtherId", true, ServiceLifetime.Scoped)]
    [InlineData("SomeOtherId", true, ServiceLifetime.Transient)]
    public async Task VerifyFeatureFlagBehaviorAcrossServiceLifetimesAsync(string userId, bool expectedResult, ServiceLifetime serviceLifetime)
    {
        // Arrange
        using var server = await CreateServerAsync(serviceLifetime, services =>
        {
            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton<IFeatureFlagConfigurationService, FlagConfigurationService>();
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped<IFeatureFlagConfigurationService, FlagConfigurationService>();
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<IFeatureFlagConfigurationService, FlagConfigurationService>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
            }
        }).ConfigureAwait(true);

        var client = server.CreateClient();
        var requestUri = $"/features/{userId}/flags/{FeatureA}";

        // Act
        var response = await client.GetAsync(requestUri).ConfigureAwait(true);
        var responseContent = await response.Content.ReadFromJsonAsync<FeatureFlagResponse<bool>>().ConfigureAwait(true);

        // Assert
        Assert.True(response.IsSuccessStatusCode, "Expected HTTP status code 200 OK.");
        Assert.NotNull(responseContent);
        Assert.Equal(FeatureA, responseContent!.FeatureName);
        Assert.Equal(expectedResult, responseContent.FeatureValue);
    }

    [Fact]
    public async Task VerifyLoggingHookIsRegisteredAsync()
    {
        // Arrange
        var logger = new FakeLogger();
        Action<IServiceCollection> configureServices = services =>
        {
            services.AddTransient<IFeatureFlagConfigurationService, FlagConfigurationService>();
        };

        Action<OpenFeatureBuilder> openFeatureBuilder = cfg =>
        {
            cfg.AddHook(_ => new LoggingHook(logger));
        };

        using var server = await CreateServerAsync(ServiceLifetime.Transient, configureServices, openFeatureBuilder).ConfigureAwait(true);

        var client = server.CreateClient();
        var requestUri = $"/features/{TestUserId}/flags/{FeatureA}";

        // Act
        var response = await client.GetAsync(requestUri).ConfigureAwait(true);
        var logs = logger.Collector.GetSnapshot();

        // Assert
        Assert.True(response.IsSuccessStatusCode, "Expected HTTP status code 200 OK.");
        Assert.Equal(4, logs.Count);
        Assert.Multiple(() =>
        {
            Assert.Contains("Before Flag Evaluation", logs[0].Message);
            Assert.Contains("After Flag Evaluation", logs[1].Message);
        });
    }

    [Fact]
    public async Task VerifyHandlerIsRegisteredAsync()
    {
        // Arrange
        Action<IServiceCollection> configureServices = services =>
        {
            services.AddTransient<IFeatureFlagConfigurationService, FlagConfigurationService>();
        };

        var handlerSuccess = false;
        Action<OpenFeatureBuilder> openFeatureBuilder = cfg =>
        {
            cfg.AddHandler(ProviderEventTypes.ProviderReady, (_) => { handlerSuccess = true; });
        };

        using var server = await CreateServerAsync(ServiceLifetime.Transient, configureServices, openFeatureBuilder)
            .ConfigureAwait(true);

        var client = server.CreateClient();
        var requestUri = $"/features/{TestUserId}/flags/{FeatureA}";

        // Act
        var response = await client.GetAsync(requestUri).ConfigureAwait(true);

        // Assert
        Assert.True(response.IsSuccessStatusCode, "Expected HTTP status code 200 OK.");
        Assert.True(handlerSuccess);
    }

    [Fact]
    public async Task VerifyMultipleHandlersAreRegisteredAsync()
    {
        // Arrange
        Action<IServiceCollection> configureServices = services =>
        {
            services.AddTransient<IFeatureFlagConfigurationService, FlagConfigurationService>();
        };

        var counter = 0;
        var handler1Success = false;
        var handler2Success = false;
        Action<OpenFeatureBuilder> openFeatureBuilder = cfg =>
        {
            cfg.AddHandler(ProviderEventTypes.ProviderReady, sp => { Interlocked.Increment(ref counter); return _ => { handler1Success = true; }; });
            cfg.AddHandler(ProviderEventTypes.ProviderReady, sp => { Interlocked.Increment(ref counter); return _ => { handler2Success = true; }; });
        };

        using var server = await CreateServerAsync(ServiceLifetime.Transient, configureServices, openFeatureBuilder)
            .ConfigureAwait(true);

        var client = server.CreateClient();
        var requestUri = $"/features/{TestUserId}/flags/{FeatureA}";

        // Act
        var response = await client.GetAsync(requestUri).ConfigureAwait(true);

        // Assert
        Assert.True(response.IsSuccessStatusCode, "Expected HTTP status code 200 OK.");
        Assert.Multiple(() =>
        {
            Assert.Equal(2, counter);
            Assert.True(handler1Success);
            Assert.True(handler2Success);
        });
    }

    [Fact]
    public async Task VerifyHandlersAreRegisteredWithServiceProviderAsync()
    {
        // Arrange
        var logs = string.Empty;
        Action<IServiceCollection> configureServices = services =>
        {
            services.AddFakeLogging(a => a.OutputSink = log => logs = string.Join('|', logs, log));
            services.AddTransient<IFeatureFlagConfigurationService, FlagConfigurationService>();
        };

        Action<OpenFeatureBuilder> openFeatureBuilder = cfg =>
        {
            cfg.AddHandler(ProviderEventTypes.ProviderReady, sp => (@event) =>
            {
                var innerLoger = sp.GetService<ILogger<FeatureFlagIntegrationTest>>();
                innerLoger!.LogInformation("Handler invoked from builder!");
            });
        };

        using var server = await CreateServerAsync(ServiceLifetime.Transient, configureServices, openFeatureBuilder)
            .ConfigureAwait(true);

        var client = server.CreateClient();
        var requestUri = $"/features/{TestUserId}/flags/{FeatureA}";

        // Act
        var response = await client.GetAsync(requestUri).ConfigureAwait(true);

        // Assert
        Assert.True(response.IsSuccessStatusCode, "Expected HTTP status code 200 OK.");
        Assert.Contains("Handler invoked from builder!", logs);
    }

    private static async Task<TestServer> CreateServerAsync(ServiceLifetime serviceLifetime,
        Action<IServiceCollection>? configureServices = null,
        Action<OpenFeatureBuilder>? openFeatureBuilder = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        configureServices?.Invoke(builder.Services);
        builder.Services.TryAddSingleton<IFeatureFlagConfigurationService, FlagConfigurationService>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddOpenFeature(cfg =>
        {
            cfg.AddHostedFeatureLifecycle();
            cfg.AddContext((builder, provider) =>
            {
                // Retrieve the HttpContext from IHttpContextAccessor, ensuring it's not null.
                var context = provider.GetRequiredService<IHttpContextAccessor>().HttpContext
                    ?? throw new InvalidOperationException("HttpContext is not available.");

                var userId = UserInfoHelper.GetUserId(context);
                builder.Set("user", userId);
            });
            cfg.AddInMemoryProvider(provider =>
            {
                if (serviceLifetime == ServiceLifetime.Scoped)
                {
                    using var scoped = provider.CreateScope();
                    var flagService = scoped.ServiceProvider.GetRequiredService<IFeatureFlagConfigurationService>();
                    return flagService.GetFlags();
                }
                else
                {
                    var flagService = provider.GetRequiredService<IFeatureFlagConfigurationService>();
                    return flagService.GetFlags();
                }
            });

            if (openFeatureBuilder is not null)
            {
                openFeatureBuilder(cfg);
            }
        });

        var app = builder.Build();

        app.UseRouting();
        app.Map($"/features/{{userId}}/flags/{{featureName}}", async context =>
        {
            var client = context.RequestServices.GetRequiredService<IFeatureClient>();
            var featureName = UserInfoHelper.GetFeatureName(context);
            var res = await client.GetBooleanValueAsync(featureName, false).ConfigureAwait(true);
            var result = await client.GetBooleanValueAsync(featureName, false).ConfigureAwait(true);

            var response = new FeatureFlagResponse<bool>(featureName, result);

            // Serialize the response object to JSON
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Write the JSON response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonResponse).ConfigureAwait(true);
        });

        await app.StartAsync().ConfigureAwait(true);

        return app.GetTestServer();
    }

    public class FlagConfigurationService : IFeatureFlagConfigurationService
    {
        private readonly IDictionary<string, Flag> _flags;
        public FlagConfigurationService()
        {
            _flags = new Dictionary<string, Flag>
            {
                {
                    "feature-a", new Flag<bool>(
                        variants: new Dictionary<string, bool>()
                        {
                            { "on", true },
                            { "off", false }
                        },
                    defaultVariant: "on", context => {
                        var id = context.GetValue("user").AsString;
                        if(id == null)
                        {
                            return "on"; // default variant
                        }

                        return id == TestUserId ? "off" : "on";
                    })
                }
            };
        }
        public Dictionary<string, Flag> GetFlags() => _flags.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
