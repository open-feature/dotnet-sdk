using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenFeature.DependencyInjection.Providers.Memory;
using OpenFeature.IntegrationTests.Services;
using OpenFeature.Providers.Memory;

namespace OpenFeature.IntegrationTests;

public class FeatureFlagIntegrationTest
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
        using var server = await CreateServerAsync(services =>
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
        });

        var client = server.CreateClient();
        var requestUri = $"/features/{userId}/flags/{FeatureA}";

        // Act
        var response = await client.GetAsync(requestUri);
        var responseContent = await response.Content.ReadFromJsonAsync<FeatureFlagResponse<bool>>();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue("Expected HTTP status code 200 OK.");
        responseContent.Should().NotBeNull("Expected response content to be non-null.");
        responseContent!.FeatureName.Should().Be(FeatureA, "Expected feature name to be 'feature-a'.");
        responseContent.FeatureValue.Should().Be(expectedResult, "Expected feature value to match the expected result.");
    }

    private static async Task<TestServer> CreateServerAsync(Action<IServiceCollection>? configureServices = null)
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
                var flagService = provider.GetRequiredService<IFeatureFlagConfigurationService>();
                return flagService.GetFlags();
            });
        });

        var app = builder.Build();

        app.UseRouting();
        app.Map($"/features/{{userId}}/flags/{{featureName}}", async context =>
        {
            var client = context.RequestServices.GetRequiredService<IFeatureClient>();
            var featureName = UserInfoHelper.GetFeatureName(context);
            var res = await client.GetBooleanValueAsync(featureName, false).ConfigureAwait(false);
            var result = await client.GetBooleanValueAsync(featureName, false).ConfigureAwait(false);

            var response = new FeatureFlagResponse<bool>(featureName, result);

            // Serialize the response object to JSON
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Write the JSON response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonResponse).ConfigureAwait(false);
        });

        await app.StartAsync().ConfigureAwait(false);

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
