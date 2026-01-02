# VCard.Net

A complete .NET library for parsing, validating, and serializing vCard (text/vcard) data according to RFC 6350.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Complete DOM**: Object-oriented representation of all vCard properties
  - FN (Formatted Name)
  - N (Structured Name)
  - TEL (Telephone)
  - EMAIL (Email Address)
  - ADR (Address)
  - ORG (Organization)
  - And many more...

- **Strongly Typed Object Initializer API**
  - Idiomatic C# object initializers for creating vCards
  - Type-safe flagged enums for property parameters (TelType, EmailType, AdrType)
  - Strongly-typed classes (Telephone, Email, Address) with bitwise flag support
  - Compile-time checking prevents errors
  - Excellent IDE autocomplete support

- **RFC 6350 Compliant Parser**
  - Line unfolding support
  - Property parameter parsing
  - Escape sequence handling
  - Multiple vCard support

- **vCard Serializer**
  - Write vCard format
  - Automatic line folding
  - Property escaping
  - Extension methods for easy serialization

- **Comprehensive Validator**
  - Required property validation (VERSION, FN)
  - Property format validation (EMAIL, URL, GEO, TZ)
  - Component-specific rules
  - Warning system for non-critical issues

- **Extensive Test Coverage**
  - 42+ unit tests
  - Parser tests
  - Serializer tests
  - Validator tests
  - Object Initializer API tests
  - Flagged enum tests
  - Round-trip tests

## Installation

### Building from Source

```bash
git clone https://github.com/yourusername/vcard-net.git
cd vcard-net/dotnet
dotnet build
```

Run the tests:

```bash
dotnet test
```

## Usage

### Parsing a vCard String

```csharp
using VCard;

var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
N:Doe;John;Michael;Mr.;Jr.
TEL;TYPE=work:+1-555-555-1234
EMAIL;TYPE=work:john@example.com
ORG:ABC Corporation
TITLE:Software Engineer
END:VCARD";

var parser = new VCardParser();
var vcards = parser.Parse(vcardData);
var vcard = vcards[0]; // or vcards.First()

Console.WriteLine($"Name: {vcard.FormattedName}");
Console.WriteLine($"Organization: {vcard.Organization}");
Console.WriteLine($"Title: {vcard.Title}");
```

### Parsing from a File

```csharp
var parser = new VCardParser();
var vcards = parser.ParseFile("contact.vcf");
var vcard = vcards[0]; // Get the first vCard
```

### Parsing Multiple vCards

The `Parse()` method always returns a list of vCards, whether the input contains one or multiple vCards. This prevents silent data loss when a file contains multiple vCards.

```csharp
var parser = new VCardParser();
var vcards = parser.Parse(vcardData); // Returns all vCards found

foreach (var vcard in vcards)
{
    Console.WriteLine($"Contact: {vcard.FormattedName}");
}
```

### Creating a vCard with Object Initializers (Recommended)

The library provides idiomatic C# object initializers with strongly-typed classes:

```csharp
using VCard;

var vcard = new VCardObject
{
    Version = "4.0",
    FormattedName = "John Michael Doe",
    Name = "Doe;John;Michael;Mr.;Jr.",
    Organization = "ABC Corporation",
    Title = "Software Engineer",
    Birthday = "19850415",
    Telephones = new List<Telephone>
    {
        new Telephone
        {
            Value = "+1-555-555-1234",
            Types = TelType.Work | TelType.Voice
        },
        new Telephone
        {
            Value = "555-555-5678",
            Types = TelType.Home
        }
    },
    Emails = new List<Email>
    {
        new Email
        {
            Value = "john@example.com",
            Types = EmailType.Work
        }
    },
    Addresses = new List<Address>
    {
        new Address
        {
            Street = "123 Main Street",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            Types = AdrType.Work
        }
    }
};

// Add other properties
vcard.AddProperty(new VCardProperty("URL", "https://www.example.com"));
vcard.AddProperty(new VCardProperty("NOTE", "Important contact"));

Console.WriteLine(vcard.FormattedName); // "John Michael Doe"
```

### Using Flagged Enums for Multiple Types

The strongly-typed classes use flagged enums, allowing multiple types via bitwise operations:

```csharp
// Telephone with multiple types
var workPhone = new Telephone
{
    Value = "+1-555-1234",
    Types = TelType.Work | TelType.Voice | TelType.Cell
};

// Check for specific types
if (workPhone.Types.HasFlag(TelType.Work))
{
    Console.WriteLine("This is a work phone");
}

// Email with multiple types
var email = new Email
{
    Value = "john@work.com",
    Types = EmailType.Work | EmailType.Internet
};

// Address with multiple types
var address = new Address
{
    Street = "123 Main St",
    City = "Springfield",
    Types = AdrType.Work | AdrType.Postal
};
```

### Adding Custom Extension Properties

For extension properties not in the strongly-typed API:

```csharp
var vcard = new VCardObject
{
    Version = "4.0",
    FormattedName = "John Doe"
};

// Add custom extension property
vcard.AddProperty(new VCardProperty("X-CUSTOM", "Custom Value"));
```

### Serializing to vCard Format

```csharp
// Using the serializer
var serializer = new VCardSerializer();
string vcardText = serializer.Serialize(vcard);

// Or using extension method
string vcardText = vcard.ToVCard();

// Save to file
vcard.SaveToFile("contact.vcf");

// Or using serializer
serializer.SerializeToFile(vcard, "contact.vcf");
```

### Validating a vCard

```csharp
var validator = new VCardValidator();
var result = validator.Validate(vcard);

if (result.IsValid)
{
    Console.WriteLine("vCard is valid!");
}
else
{
    Console.WriteLine("Validation errors:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
}

// Get detailed summary
Console.WriteLine(result.GetSummary());
```

### Working with Strongly-Typed Collections

```csharp
// Access telephones
foreach (var telephone in vcard.Telephones)
{
    Console.WriteLine($"Phone: {telephone.Value}");

    // Check for specific types using flagged enums
    if (telephone.Types.HasFlag(TelType.Work))
    {
        Console.WriteLine("  - Work phone");
    }
    if (telephone.Types.HasFlag(TelType.Cell))
    {
        Console.WriteLine("  - Cell phone");
    }
}

// Access emails
foreach (var email in vcard.Emails)
{
    Console.WriteLine($"Email: {email.Value}");
    if (email.Types.HasFlag(EmailType.Work))
    {
        Console.WriteLine("  - Work email");
    }
}

// Access addresses
foreach (var address in vcard.Addresses)
{
    Console.WriteLine($"Address:");
    Console.WriteLine($"  Street: {address.Street}");
    Console.WriteLine($"  City: {address.City}");
    Console.WriteLine($"  State: {address.State}");
    Console.WriteLine($"  Postal Code: {address.PostalCode}");
    Console.WriteLine($"  Country: {address.Country}");

    if (address.Types.HasFlag(AdrType.Work))
    {
        Console.WriteLine("  - Work address");
    }
}

// Access generic properties (for properties not yet strongly-typed)
var urls = vcard.Urls;
foreach (var url in urls)
{
    Console.WriteLine($"URL: {url.Value}");
}
```

### Working with Structured Data

```csharp
// Parse structured name
var name = vcard.GetProperty("N");
var structuredName = StructuredName.Parse(name.Value);
Console.WriteLine($"Family Name: {structuredName.FamilyName}");
Console.WriteLine($"Given Name: {structuredName.GivenName}");
Console.WriteLine($"Honorific Prefix: {structuredName.HonorificPrefixes}");

// Parse structured address
var address = vcard.GetProperty("ADR");
var structuredAddress = StructuredAddress.Parse(address.Value);
Console.WriteLine($"Street: {structuredAddress.StreetAddress}");
Console.WriteLine($"City: {structuredAddress.Locality}");
Console.WriteLine($"State: {structuredAddress.Region}");
Console.WriteLine($"Postal Code: {structuredAddress.PostalCode}");
Console.WriteLine($"Country: {structuredAddress.CountryName}");
```

## vCard Structure

### Common Properties

- **VERSION**: vCard version (required, must be "4.0", "3.0", or "2.1")
- **FN**: Formatted Name (required)
- **N**: Structured Name (Family;Given;Additional;Prefix;Suffix)
- **NICKNAME**: Nickname
- **PHOTO**: Photo or avatar URI
- **BDAY**: Birthday
- **ANNIVERSARY**: Anniversary date
- **GENDER**: Gender
- **TEL**: Telephone number
- **EMAIL**: Email address
- **IMPP**: Instant messaging address
- **LANG**: Language
- **ADR**: Delivery address (POBox;Extended;Street;Locality;Region;PostalCode;Country)
- **GEO**: Geographic position
- **TZ**: Time zone
- **TITLE**: Job title
- **ROLE**: Role or occupation
- **LOGO**: Logo URI
- **ORG**: Organization
- **MEMBER**: Group member
- **RELATED**: Related entity
- **CATEGORIES**: Categories or tags
- **NOTE**: Note or comment
- **PRODID**: Product identifier
- **REV**: Revision date/time
- **SOUND**: Pronunciation sound URI
- **UID**: Unique identifier
- **URL**: Web address
- **KEY**: Public key or authentication certificate

## Validation Rules

The validator checks for:

### Required Properties
- **VERSION**: Must be present and be "4.0", "3.0", or "2.1"
- **FN**: Must be present and non-empty

### Property Format Validation
- **EMAIL**: Basic email format validation (warning only)
- **URL**: Valid URI format
- **GEO**: Geographic coordinates format (geo:lat,long)
- **TZ**: Time zone format (UTC offset or timezone ID)
- **BDAY/ANNIVERSARY**: Date format validation
- **N**: Structured name component validation
- **ADR**: Structured address component validation

### Property Constraints
- **TEL**: Cannot be empty
- **EMAIL**: Cannot be empty
- **UID**: Cannot be empty if present

## RFC 6350 Compliance

This implementation follows RFC 6350 (vCard Format Specification):

- Line folding/unfolding (Section 3.2)
- Content lines (Section 3.3)
- Property parameters (Section 5)
- Property value data types (Section 4)
- vCard properties (Section 6)
- Escape sequences for special characters

## Testing

The test suite includes:

- **Parser Tests**: 15+ tests covering various parsing scenarios
- **Validator Tests**: 8+ tests for validation rules
- **Serializer Tests**: 5+ tests for serialization
- **Object Initializer API Tests**: 11+ tests for strongly typed object initializers
- **Flagged Enum Tests**: 3+ tests for bitwise enum operations
- **Round-Trip Tests**: 2+ tests for parse-serialize-parse cycles

Run all tests:

```bash
dotnet test
# Result: Passed! - 42 tests
```

## Project Structure

```
VCard.Net/
├── src/
│   └── VCard/                     # Main library
│       ├── VCard.DOM.cs           # Document Object Model
│       ├── VCard.StrongTypes.cs   # Strongly-typed classes (Telephone, Email, Address)
│       ├── VCard.Parser.cs        # RFC 6350 parser
│       ├── VCard.Serializer.cs    # vCard serializer
│       ├── VCard.Validator.cs     # Validation logic
│       └── VCard.csproj           # Library project file
├── tests/
│   └── VCard.Tests/               # Test project
│       ├── VCard.Tests.cs         # Unit tests
│       └── VCard.Tests.csproj     # Test project file
├── VCard.sln                      # Solution file
└── README.md                      # This file
```

## Architecture

### Parser
1. **Line Unfolding**: Handles multi-line property values
2. **Tokenization**: Splits content lines into name, parameters, and value
3. **Property Parsing**: Extracts properties with parameters
4. **Value Unescaping**: Handles escape sequences (\n, \;, \\, \,)
5. **Multiple vCard Support**: Can parse files containing multiple vCards

### Serializer
1. **Component Serialization**: Serializes vCard properties in correct order
2. **Property Formatting**: Formats properties with parameters
3. **Line Folding**: Automatically folds lines longer than 75 characters
4. **Value Escaping**: Escapes special characters (\n, \;, \\, \,)

### Validator
1. **Required Property Validation**: Checks for VERSION and FN
2. **Format Validation**: Validates email, URL, date, geo, and timezone formats
3. **Property Constraints**: Enforces property-specific rules
4. **Result Reporting**: Provides detailed error and warning messages

## License

MIT License - This implementation is provided for educational and commercial use.

## References

- [RFC 6350 - vCard Format Specification](https://www.rfc-editor.org/rfc/rfc6350.html)
- [RFC 9554 - vCard Format Extensions for JSContact](https://datatracker.ietf.org/doc/rfc9554/)
- [RFC 6868 - Parameter Value Encoding in iCalendar and vCard](https://www.rfc-editor.org/rfc/rfc6868.html)
- [IANA vCard Elements Registry](https://www.iana.org/assignments/vcard-elements/vcard-elements.xhtml)

## Contributing

Contributions are welcome! Please ensure all tests pass before submitting pull requests.

### Development

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass: `dotnet test`
6. Submit a pull request
