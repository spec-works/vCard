# ADR 0003: .NET Version Support Strategy

## Status

Accepted

## Context

The vCard .NET library needs to define its target framework support strategy. .NET has evolved significantly with multiple framework versions available:

- **.NET Framework** (4.x) - Windows-only, legacy
- **.NET Standard** - Cross-platform API specification
- **.NET Core / .NET 5+** - Modern, cross-platform runtime
- **.NET 10** - Latest LTS release (November 2025)

Key considerations:
1. **Maximum compatibility** vs. **Modern features**
2. **Cross-platform support** (Windows, Linux, macOS)
3. **Long-term support** requirements
4. **NuGet package compatibility**
5. **Modern C# language features**

Library consumers may be on:
- Modern .NET applications (.NET 6+)
- Legacy .NET Framework applications
- Xamarin/MAUI mobile apps
- Unity game engine projects

## Decision

The vCard .NET library SHALL:

1. **Primary Target: .NET 10**
   - Build and test against .NET 10 as the primary target
   - Take advantage of modern C# 13 features
   - Optimize for latest runtime performance

2. **Compatibility Target: .NET Standard 2.1**
   - Multi-target to support .NET Standard 2.1
   - Enables compatibility with:
     - .NET Core 3.0+
     - .NET 5+
     - Xamarin.iOS 12.16+
     - Xamarin.Android 10.0+
     - Unity 2021.2+

3. **No .NET Framework 4.x Support**
   - Do NOT target .NET Framework 4.x directly
   - .NET Framework 4.8 does NOT support .NET Standard 2.1
   - Users on .NET Framework should migrate to .NET 6+ (LTS)

### Project File Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0;netstandard2.1</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### Version Support Matrix

| Target Framework | Supported | Notes |
|-----------------|-----------|-------|
| .NET 10 | ✅ Primary | Full feature set, latest C# |
| .NET 8 (LTS) | ✅ Via .NET Standard 2.1 | Long-term support |
| .NET 6 (LTS) | ✅ Via .NET Standard 2.1 | Extended support until Nov 2024 |
| .NET Standard 2.1 | ✅ Compatibility | Cross-platform baseline |
| .NET Standard 2.0 | ❌ | Missing required APIs |
| .NET Framework 4.8 | ❌ | Does not support .NET Standard 2.1 |
| .NET Framework 4.7.2 | ❌ | Does not support .NET Standard 2.1 |

## Consequences

### Positive

1. **Modern Features**: Can use C# 8.0+ features (nullable reference types, pattern matching, etc.)
2. **Cross-Platform**: Works on Windows, Linux, macOS via .NET Standard 2.1
3. **Performance**: Optimized for .NET 10 runtime
4. **Simplified Support**: Two clear targets instead of many
5. **Future-Proof**: .NET 10 is current LTS, .NET 12 (next LTS) will support our code
6. **Mobile/Unity**: .NET Standard 2.1 enables Xamarin and Unity support

### Negative

1. **No .NET Framework 4.x**: Users on legacy Framework must upgrade or use older library versions
2. **Two Targets to Test**: Must test both .NET 10 and .NET Standard 2.1 builds
3. **Feature Parity**: Must avoid .NET 10-only APIs in shared code paths

### Migration Path for .NET Framework Users

Users on .NET Framework 4.x have several options:

1. **Upgrade to .NET 8 (Recommended)**:
   - In-place upgrade using .NET Upgrade Assistant
   - Benefits from LTS support until November 2026

2. **Use .NET 6**:
   - Compatible with our .NET Standard 2.1 target
   - LTS support until November 2024

3. **Stay on .NET Framework**:
   - Use version 1.x of the vCard library (if we create a .NET Framework-compatible version)
   - No new features, security updates only

### Development Guidelines

1. **Use .NET 10 for Development**:
   - Primary development IDE should target .NET 10
   - Take advantage of latest C# features

2. **Test on Both Targets**:
   - CI/CD must run tests on both .NET 10 and .NET Standard 2.1
   - Catch compatibility issues early

3. **API Design**:
   - Use `#if` directives only when absolutely necessary
   - Prefer .NET Standard 2.1-compatible APIs in shared code
   - .NET 10-specific optimizations should be isolated

4. **NuGet Package**:
   - Single package with both targets
   - NuGet automatically selects best target for consumer

## Alternatives Considered

### Alternative 1: Target Only .NET 10
- **Rejected**: Would exclude .NET 6/8 users and Xamarin/Unity
- Too restrictive for a library

### Alternative 2: Target .NET Standard 2.0
- **Rejected**: Missing important APIs (Span<T>, ValueTuple, etc.)
- Would limit performance optimizations

### Alternative 3: Support .NET Framework 4.8
- **Rejected**: Would require targeting .NET Standard 2.0
- .NET Framework is in maintenance mode
- Microsoft recommends migration to .NET 6+

### Alternative 4: Multiple Framework Targets (net10.0;net8.0;net6.0;netstandard2.1)
- **Rejected**: Unnecessary complexity
- .NET Standard 2.1 covers .NET 6+ compatibility
- More targets = more testing surface

## References

- [.NET Standard versions](https://learn.microsoft.com/en-us/dotnet/standard/net-standard)
- [.NET Support Policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
- [.NET 10 Release Notes](https://github.com/dotnet/core/tree/main/release-notes/10.0)
- [.NET Framework Lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework)

## Date

2026-01-02
