# vCard for .NET

Full-featured .NET library for parsing, validating, and serializing vCard 4.0 data according to [RFC 6350](https://www.rfc-editor.org/rfc/rfc6350).

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package VCard.Net
```

Or using Package Manager Console:

```powershell
Install-Package VCard.Net
```

## Features

- ✅ **RFC 6350 Compliant** - Full implementation of vCard 4.0 specification
- ✅ **Type-Safe API** - Strong typing with nullable reference types
- ✅ **Parse and Generate** - Read existing vCards and create new ones
- ✅ **Property Validation** - Automatic validation of required properties
- ✅ **Property Parameters** - Full support for TYPE, VALUE, PREF, and custom parameters
- ✅ **Property Grouping** - Support for grouped properties
- ✅ **Comprehensive Testing** - 20+ tests covering specification requirements
- ✅ **Multi-Target** - Supports .NET 10.0 and .NET 8.0 (LTS)

## Quick Start

### Parsing a vCard

```csharp
using VCard;

string vcardText = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
N:Doe;John;;;
EMAIL:john.doe@example.com
TEL;TYPE=work:+1-555-555-5555
END:VCARD";

VCard card = VCard.Parse(vcardText);

Console.WriteLine($"Name: {card.FormattedName}");
Console.WriteLine($"Email: {card.Email}");
Console.WriteLine($"Phone: {card.Telephone}");
```

### Creating a vCard

```csharp
using VCard;

var card = new VCard
{
    FormattedName = "Jane Smith",
    GivenName = "Jane",
    FamilyName = "Smith",
    Email = "jane.smith@example.com",
    Telephone = "+1-555-123-4567",
    Organization = "Acme Corporation"
};

string vcardText = card.ToString();
Console.WriteLine(vcardText);
```

### Working with Multiple Properties

```csharp
using VCard;

var card = new VCard
{
    FormattedName = "John Doe"
};

// Add multiple email addresses
card.AddEmail("work@example.com", type: "work");
card.AddEmail("personal@example.com", type: "home");

// Add multiple phone numbers
card.AddTelephone("+1-555-555-5555", type: "work");
card.AddTelephone("+1-555-123-4567", type: "cell");
```

## API Reference

Browse the complete API documentation:

- [API Reference](api/VCard.html) - Detailed API documentation generated from XML comments

## Specification Compliance

This library implements [RFC 6350](https://www.rfc-editor.org/rfc/rfc6350) - vCard Format Specification version 4.0.

### Supported Properties

| Property | RFC Section | Status |
|----------|-------------|--------|
| VERSION | 6.7.9 | ✅ Required |
| FN | 6.2.1 | ✅ Required |
| N | 6.2.2 | ✅ Supported |
| NICKNAME | 6.2.3 | ✅ Supported |
| PHOTO | 6.2.4 | ✅ Supported |
| BDAY | 6.2.5 | ✅ Supported |
| ANNIVERSARY | 6.2.6 | ✅ Supported |
| GENDER | 6.2.7 | ✅ Supported |
| ADR | 6.3.1 | ✅ Supported |
| TEL | 6.4.1 | ✅ Supported |
| EMAIL | 6.4.2 | ✅ Supported |
| IMPP | 6.4.3 | ✅ Supported |
| LANG | 6.4.4 | ✅ Supported |
| TZ | 6.5.1 | ✅ Supported |
| GEO | 6.5.2 | ✅ Supported |
| TITLE | 6.6.1 | ✅ Supported |
| ROLE | 6.6.2 | ✅ Supported |
| LOGO | 6.6.3 | ✅ Supported |
| ORG | 6.6.4 | ✅ Supported |
| MEMBER | 6.6.5 | ✅ Supported |
| RELATED | 6.6.6 | ✅ Supported |
| CATEGORIES | 6.7.1 | ✅ Supported |
| NOTE | 6.7.2 | ✅ Supported |
| PRODID | 6.7.3 | ✅ Supported |
| REV | 6.7.4 | ✅ Supported |
| SOUND | 6.7.5 | ✅ Supported |
| UID | 6.7.6 | ✅ Supported |
| CLIENTPIDMAP | 6.7.7 | ✅ Supported |
| URL | 6.7.8 | ✅ Supported |
| KEY | 6.8.1 | ✅ Supported |
| FBURL | 6.9.1 | ✅ Supported |
| CALADRURI | 6.9.2 | ✅ Supported |
| CALURI | 6.9.3 | ✅ Supported |

### Property Parameters

All standard property parameters are supported:
- LANGUAGE
- VALUE
- PREF
- ALTID
- PID
- TYPE
- MEDIATYPE
- CALSCALE
- SORT-AS
- GEO
- TZ

### Extensions

The library supports custom X-properties for extensibility.

## Testing

The library includes comprehensive tests:
- **Unit Tests** - Property parsing, validation, and serialization
- **Integration Tests** - Complete vCard parsing and generation
- **Specification Tests** - Compliance with RFC 6350 examples

Run tests:
```bash
cd dotnet
dotnet test
```

## Requirements

- .NET 10.0 or .NET 8.0 (LTS)
- C# 10.0 or later

## Source Code

View the source code on [GitHub](https://github.com/spec-works/vCard/tree/main/dotnet).

## Contributing

Contributions welcome! Please:
1. Check existing issues
2. Follow the [SpecWorks Conventions](https://spec-works.github.io/specification/conventions.html)
3. Add tests for new features
4. Update documentation

## License

MIT License - see [LICENSE](https://github.com/spec-works/vCard/blob/main/LICENSE) for details.
