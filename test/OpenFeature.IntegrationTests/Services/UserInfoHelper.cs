using Microsoft.AspNetCore.Http;

namespace OpenFeature.IntegrationTests.Services;

public static class UserInfoHelper
{
    /// <summary>
    /// Extracts the user ID from the HTTP request context.
    /// </summary>
    /// <param name="context">The HTTP context containing the request.</param>
    /// <returns>The user ID as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the user ID is not found in the route values.</exception>
    public static string GetUserId(HttpContext context)
    {
        if (context.Request.RouteValues.TryGetValue("userId", out var userId) && userId is string userIdString)
        {
            return userIdString;
        }
        throw new ArgumentNullException(nameof(userId), "User ID not found in route values.");
    }

    /// <summary>
    /// Extracts the feature name from the HTTP request context.
    /// </summary>
    /// <param name="context">The HTTP context containing the request.</param>
    /// <returns>The feature name as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the feature name is not found in the route values.</exception>
    public static string GetFeatureName(HttpContext context)
    {
        if (context.Request.RouteValues.TryGetValue("featureName", out var featureName) && featureName is string featureNameString)
        {
            return featureNameString;
        }
        throw new ArgumentNullException(nameof(featureName), "Feature name not found in route values.");
    }
}
