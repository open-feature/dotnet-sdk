# OpenFeature Dotnet Web Sample

This sample demonstrates how to use the OpenFeature .NET Web Application. It includes a simple .NET 9 web application that retrieves and evaluates feature flags using the OpenFeature client. The sample is set up with the `InMemoryProvider` and a relatively simple boolean `welcome-message` feature flag.

The sample can easily extended with alternative providers, which you can find in the [dotnet-sdk-contrib](https://github.com/open-feature/dotnet-sdk-contrib) repository.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed on your machine.

## Setup

1. Clone the repository:

   ```shell
   git clone https://github.com/open-feature/dotnet-sdk.git openfeature-dotnet-sdk
   ```

1. Navigate to the Web sample project directory:

   ```shell
   cd openfeature-dotnet-sdk/samples/AspNetCore
   ```

1. Run the following command to start the application:

   ```shell
   dotnet run
   ```

1. Open your web browser and navigate to `http://localhost:5412/welcome` to see the application in action.

### Enable OpenFeature debug logging

You can enable OpenFeature debug logging by setting the `Logging:LogLevel:OpenFeature.*` setting in [appsettings.Development.json](appsettings.Development.json) to `Debug`. This will provide detailed logs of the OpenFeature SDK's operations, which can be helpful for troubleshooting and understanding how feature flags are being evaluated.
