# OpenFeature .NET SDK Project Structure

This document describes the additional subproject code repositories produced by the OpenFeature .NET SDK project and compiled into releases, including their status and intent.

## Overview

The OpenFeature .NET SDK project consists of multiple subprojects that are developed within this repository and compiled into separate NuGet packages. Each subproject serves a specific purpose within the OpenFeature ecosystem while maintaining independence and focused functionality.

## Subprojects

### 1. OpenFeature (Core SDK)

-   **Location**: `src/OpenFeature/`
-   **Package**: `OpenFeature`
-   **Status**: ✅ **Active/Stable** - Primary production package
-   **Intent**: Provides the core OpenFeature SDK functionality including the evaluation API, provider interfaces, hooks, and fundamental abstractions. This is the main package that most users will consume.
-   **Target Frameworks**: .NET 8.0, .NET 9.0, .NET Standard 2.0, .NET Framework 4.6.2
-   **Dependencies**: Microsoft.Extensions.Logging.Abstractions, plus framework-specific compatibility packages

### 2. OpenFeature.DependencyInjection

-   **Location**: `src/OpenFeature.DependencyInjection/`
-   **Package**: `OpenFeature.DependencyInjection`
-   **Status**: ⚠️ **Deprecated** - Migrated to Hosting package in v2.9.0
-   **Intent**: Originally provided dependency injection integration for .NET applications. Functionality has been moved to the OpenFeature.Hosting package for better lifecycle management.
-   **Target Frameworks**: .NET 8.0, .NET 9.0, .NET Standard 2.0, .NET Framework 4.6.2
-   **Dependencies**: Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Options

### 3. OpenFeature.Hosting

-   **Location**: `src/OpenFeature.Hosting/`
-   **Package**: `OpenFeature.Hosting`
-   **Status**: ✅ **Active/Stable** - Recommended for DI scenarios
-   **Intent**: Provides integration with .NET's hosting model and dependency injection container. Enables seamless configuration and lifecycle management of OpenFeature providers in hosted applications (ASP.NET Core, Worker Services, etc.).
-   **Target Frameworks**: .NET 8.0, .NET 9.0, .NET Standard 2.0, .NET Framework 4.6.2
-   **Dependencies**: Microsoft.Extensions.Hosting.Abstractions

### 4. OpenFeature.Providers.MultiProvider

-   **Location**: `src/OpenFeature.Providers.MultiProvider/`
-   **Package**: `OpenFeature.Providers.MultiProvider`
-   **Status**: 🔬 **Experimental** - Feature preview
-   **Intent**: Enables the use of multiple underlying feature flag providers simultaneously with configurable evaluation strategies. Supports scenarios like primary/fallback configurations, A/B testing provider comparisons, and migration between providers.
-   **Target Frameworks**: .NET 8.0, .NET 9.0, .NET Standard 2.0, .NET Framework 4.6.2
-   **Dependencies**: OpenFeature (core SDK)

## Supporting Projects

### Test Projects

The following test projects support the main packages but are not compiled into releases:

-   **OpenFeature.Tests** - Unit tests for the core SDK
-   **OpenFeature.DependencyInjection.Tests** - Tests for DI integration
-   **OpenFeature.Hosting.Tests** - Tests for hosting integration
-   **OpenFeature.Providers.MultiProvider.Tests** - Tests for MultiProvider
-   **OpenFeature.E2ETests** - End-to-end specification compliance tests
-   **OpenFeature.IntegrationTests** - Integration testing scenarios
-   **OpenFeature.AotCompatibility** - AOT (Ahead-of-Time) compilation validation
-   **OpenFeature.Benchmarks** - Performance benchmarking

### Sample Projects

-   **Samples.AspNetCore** - Example ASP.NET Core application demonstrating OpenFeature usage

### Specification Assets

-   **spec/** - Git submodule containing the OpenFeature specification and compliance testing assets

## Release Strategy

All production packages are versioned together and released simultaneously through automated release-please workflows. The version is managed centrally and applied across all packages to maintain consistency.

### NuGet Package Distribution

Each subproject (except deprecated ones) produces a separate NuGet package:

-   `OpenFeature` - Core SDK (required by all applications)
-   `OpenFeature.Hosting` - Hosting and DI integration (recommended for modern .NET apps)
-   `OpenFeature.Providers.MultiProvider` - Multi-provider functionality (experimental)

### Compatibility Matrix

All packages maintain compatibility across:

-   **.NET 8.0+** - Latest LTS and current versions
-   **.NET Standard 2.0** - Broad ecosystem compatibility
-   **.NET Framework 4.6.2+** - Legacy application support

## Development Status Legend

-   ✅ **Active/Stable** - Production-ready, actively maintained, recommended for use
-   🔬 **Experimental** - Preview functionality, API may change, use with caution
-   ⚠️ **Deprecated** - Supported but not recommended for new projects, migration path available
-   ❌ **Discontinued** - No longer supported or developed

## Architecture Decisions

The multi-package approach enables:

1. **Minimal Dependencies** - Applications only include needed functionality
2. **Independent Evolution** - Packages can evolve at different rates based on user needs
3. **Ecosystem Growth** - Clear extension points for community contributions
4. **Framework Flexibility** - Support across diverse .NET application types

For detailed usage examples and integration patterns, refer to the main [README.md](README.md) and individual package documentation.
