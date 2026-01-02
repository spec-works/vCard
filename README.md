# vCard Parser Library

A complete multi-language library for parsing, validating, and serializing vCard (RFC 6350) data.

## Overview

This project provides RFC 6350-compliant vCard parsers in three languages:
- **.NET (C#)** - Production-ready with full serialization, validation, and 42+ tests
- **Python** - Zero-dependency implementation with strongly typed builder API and type hints
- **Rust** - Memory-safe implementation with strongly typed builder API and 12+ tests

## Specifications

This implementation is based on:
- **[RFC 6350](https://www.rfc-editor.org/rfc/rfc6350.html)** - vCard Format Specification (August 2011)
- **[RFC 9554](https://datatracker.ietf.org/doc/rfc9554/)** - vCard Format Extensions for JSContact (May 2024)
- **[RFC 6868](https://www.rfc-editor.org/rfc/rfc6868.html)** - Parameter Value Encoding in iCalendar and vCard
- **[IANA vCard Elements Registry](https://www.iana.org/assignments/vcard-elements/vcard-elements.xhtml)** - Official registry

See [specs.json](specs.json) for the full specification linkset.

## Features

All implementations support:

- **RFC 6350 Compliant Parsing**
  - Line unfolding and folding
  - Property parameter parsing
  - Escape sequence handling
  - Multiple vCard support

- **Complete DOM**
  - Object-oriented representation
  - All standard vCard properties (FN, N, TEL, EMAIL, ADR, ORG, etc.)
  - Property parameter access
  - Multiple values per property

- **Data Validation** (.NET)
  - Required property validation
  - Format validation (EMAIL, URL, GEO, TZ, dates)
  - Warning system for non-critical issues

- **Serialization** (.NET, Python)
  - Write vCard format
  - Automatic line folding
  - Property escaping

## Quick Start

### .NET

```csharp
using VCard;

// Parsing
var parser = new VCardParser();
var vcards = parser.Parse("BEGIN:VCARD\nVERSION:4.0\nFN:John Doe\nEND:VCARD");
var vcard = vcards[0];
Console.WriteLine(vcard.FormattedName); // "John Doe"

// Creating a new vCard with object initializer
var newVcard = new VCardObject
{
    Version = "4.0",
    FormattedName = "John Doe",
    Organization = "ABC Corporation",
    Title = "Senior Software Engineer",
    Telephones = new List<Telephone>
    {
        new Telephone { Value = "+1-555-1234", Types = TelType.Work | TelType.Voice }
    },
    Emails = new List<Email>
    {
        new Email { Value = "john@example.com", Types = EmailType.Work }
    }
};
```

See [dotnet/README.md](dotnet/README.md) for details.

### Python

```python
from vcard import VCardParser, VCardBuilder, TelType, EmailType

# Parsing
parser = VCardParser()
vcards = parser.parse("BEGIN:VCARD\nVERSION:4.0\nFN:John Doe\nEND:VCARD")
vcard = vcards[0]
print(vcard.formatted_name)  # "John Doe"

# Creating with strongly typed builder API
vcard = (VCardBuilder()
    .with_version("4.0")
    .with_formatted_name("John Doe")
    .with_telephone("+1-555-1234", [TelType.WORK])
    .with_email("john@example.com", [EmailType.WORK])
    .build())
```

See [python/README.md](python/README.md) for details.

### Rust

```rust
use vcard::{VCardParser, VCardObject, TelType, EmailType};

// Parsing
let mut parser = VCardParser::new();
let vcards = parser.parse("BEGIN:VCARD\nVERSION:4.0\nFN:John Doe\nEND:VCARD").unwrap();
let vcard = &vcards[0];
println!("{}", vcard.formatted_name().unwrap());  // "John Doe"

// Creating with strongly typed builder API
let vcard = VCardObject::builder()
    .version("4.0")
    .formatted_name("John Doe")
    .telephone("+1-555-1234", vec![TelType::Work])
    .email("john@example.com", vec![EmailType::Work])
    .build();
```

See [rust/README.md](rust/README.md) for details.

## Project Structure

```
vCard/
├── dotnet/                 # .NET implementation
│   ├── src/VCard/          # Library source
│   ├── tests/              # Unit tests (31+ tests)
│   ├── sample.vcf          # Sample vCard file
│   └── README.md           # .NET documentation
├── python/                 # Python implementation
│   ├── src/vcard/          # Library source
│   ├── tests/              # Unit tests
│   └── README.md           # Python documentation
├── rust/                   # Rust implementation
│   ├── src/                # Library source
│   ├── Cargo.toml          # Rust package config
│   └── README.md           # Rust documentation
├── specs.json              # Specification references
└── README.md               # This file
```

## Testing

### .NET
```bash
cd dotnet
dotnet test
# Result: Passed! - 42 tests
```

### Python
```bash
cd python
python -m pytest tests/
```

### Rust
```bash
cd rust
cargo test
# Result: ok. 12 passed
```

## Common vCard Properties

- **VERSION** - vCard version (required, must be "4.0", "3.0", or "2.1")
- **FN** - Formatted Name (required)
- **N** - Structured Name (Family;Given;Additional;Prefix;Suffix)
- **NICKNAME** - Nickname
- **PHOTO** - Photo or avatar URI
- **BDAY** - Birthday
- **ANNIVERSARY** - Anniversary date
- **GENDER** - Gender
- **TEL** - Telephone number
- **EMAIL** - Email address
- **ADR** - Delivery address (POBox;Extended;Street;Locality;Region;PostalCode;Country)
- **GEO** - Geographic position
- **TZ** - Time zone
- **TITLE** - Job title
- **ROLE** - Role or occupation
- **ORG** - Organization
- **URL** - Web address
- **NOTE** - Note or comment
- **UID** - Unique identifier

## Example vCard

```
BEGIN:VCARD
VERSION:4.0
FN:John Michael Doe
N:Doe;John;Michael;Mr.;Jr.
NICKNAME:Johnny
BDAY:19850415
TEL;TYPE=work,voice:+1-555-555-1234
TEL;TYPE=home:555-555-5678
EMAIL;TYPE=work:john.doe@example.com
ADR;TYPE=work:;;123 Main Street;Springfield;IL;62701;USA
ORG:ABC Corporation;Engineering Department
TITLE:Senior Software Engineer
URL:https://www.example.com
NOTE:Important contact
CATEGORIES:Work,VIP
UID:urn:uuid:4fbe8971-0bc3-424c-9c26-36c3e1eff6b1
REV:20240102T120000Z
END:VCARD
```

See [dotnet/sample.vcf](dotnet/sample.vcf) for a complete example.

## RFC Compliance

All implementations follow RFC 6350 requirements:

- **Line Folding** (Section 3.2): Long lines are folded at 75 characters
- **Content Lines** (Section 3.3): Property name, parameters, and value parsing
- **Property Parameters** (Section 5): TYPE, VALUE, PREF, LANGUAGE, etc.
- **Property Value Types** (Section 4): text, uri, date, date-time, etc.
- **Escape Sequences**: `\n`, `\;`, `\,`, `\\`

## Validation (.NET)

The .NET implementation includes comprehensive validation:

- Required properties (VERSION, FN)
- Version compatibility (4.0, 3.0, 2.1)
- Email format validation
- URL format validation
- Geographic coordinate format
- Time zone format
- Date format validation
- Structured name/address validation

## License

MIT License - All implementations are provided for educational and commercial use.

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## References

- [RFC 6350 - vCard Format Specification](https://www.rfc-editor.org/rfc/rfc6350.html)
- [RFC 9554 - vCard Format Extensions for JSContact](https://datatracker.ietf.org/doc/rfc9554/)
- [RFC 6868 - Parameter Value Encoding](https://www.rfc-editor.org/rfc/rfc6868.html)
- [IANA vCard Elements Registry](https://www.iana.org/assignments/vcard-elements/vcard-elements.xhtml)
- [vCard Format (Wikipedia)](https://en.wikipedia.org/wiki/VCard)
- [CalConnect Developer Guide](https://devguide.calconnect.org/vCard/vcard-4/)

## Related Projects

This vCard parser is part of a larger collection of specification-based software components. See the [specification folder](../specification) for the xRegistry catalog.
