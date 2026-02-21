# vCard Documentation

Welcome to the vCard documentation. This component provides libraries for parsing, validating, and serializing vCard data according to [RFC 6350](https://www.rfc-editor.org/rfc/rfc6350).

## What is vCard?

vCard is the standard for electronic business cards. RFC 6350 defines version 4.0 of the vCard format, which enables the capture and exchange of information normally found on a business card, including:

- Names and organizations
- Postal addresses
- Phone numbers and email addresses
- URLs and instant messaging handles
- Photos and logos
- Geographical coordinates
- And much more

## Available Implementations

Choose the implementation that matches your technology stack:

### [.NET](dotnet/index.md)

Full-featured .NET library for vCard 4.0 parsing and generation.

- **Package**: [VCard.Net](https://www.nuget.org/packages/VCard.Net) on NuGet
- **Target Frameworks**: .NET 10.0, .NET 8.0
- **Language**: C# with nullable reference types
- [View .NET Documentation →](dotnet/index.md)

### [Python](python/index.md)

Python library for vCard 4.0 support.

- **Package**: [vcard](https://pypi.org/project/vcard/) on PyPI
- **Python Version**: 3.8+
- [View Python Documentation →](python/index.md)

### [Rust](rust/index.md)

Rust library for vCard 4.0 parsing and serialization.

- **Package**: [vcard](https://crates.io/crates/vcard) on crates.io
- **Rust Edition**: 2021
- [View Rust Documentation →](rust/index.md)

### [TypeScript](typescript/index.md)

TypeScript library for vCard 4.0 parsing and serialization.

- **Package**: [@specworks/vcard](https://www.npmjs.com/package/@specworks/vcard) on npm
- **Node.js**: 18+
- **Zero Dependencies**
- [View TypeScript Documentation →](typescript/index.md)

## Quick Start

### .NET

```bash
dotnet add package VCard.Net
```

```csharp
using VCard;

// Parse a vCard
var vcard = VCard.Parse(vcardText);
Console.WriteLine($"Name: {vcard.FormattedName}");

// Create a vCard
var newCard = new VCard
{
    FormattedName = "John Doe",
    Email = "john@example.com"
};
string vcardText = newCard.ToString();
```

### Python

```bash
pip install vcard
```

```python
from vcard import VCard

# Parse a vCard
vcard = VCard.parse(vcard_text)
print(f"Name: {vcard.formatted_name}")

# Create a vCard
new_card = VCard(
    formatted_name="John Doe",
    email="john@example.com"
)
vcard_text = str(new_card)
```

### Rust

```toml
[dependencies]
vcard = "*"
```

```rust
use vcard::VCard;

// Parse a vCard
let vcard = VCard::parse(&vcard_text)?;
println!("Name: {}", vcard.formatted_name);

// Create a vCard
let new_card = VCard::new()
    .formatted_name("John Doe")
    .email("john@example.com");
let vcard_text = new_card.to_string();
```

### TypeScript

```bash
npm install @specworks/vcard
```

```typescript
import { VCardParser, VCardObject, VCardSerializer, Telephone, TelType } from '@specworks/vcard';

// Parse a vCard
const parser = new VCardParser();
const vcards = parser.parse(vcardText);
console.log(`Name: ${vcards[0].formattedName}`);

// Create a vCard
const vcard = new VCardObject();
vcard.version = '4.0';
vcard.formattedName = 'John Doe';
const serializer = new VCardSerializer();
const text = serializer.serialize(vcard);
```

## Specification Compliance

All implementations follow [RFC 6350](https://www.rfc-editor.org/rfc/rfc6350) - vCard Format Specification version 4.0.

Key features implemented:
- ✅ All required properties (VERSION, FN, N)
- ✅ All standard properties (TEL, EMAIL, ADR, etc.)
- ✅ Property parameters (TYPE, VALUE, PREF, etc.)
- ✅ Property grouping
- ✅ Value escaping and encoding
- ✅ Multiple property instances
- ✅ Extensibility (X-properties)

See each implementation's documentation for detailed compliance information.

## Test Cases

All implementations are tested against shared test cases in the [testcases/](https://github.com/spec-works/vCard/tree/main/testcases) directory. This ensures consistency across languages.

## Contributing

Contributions are welcome! See the [GitHub repository](https://github.com/spec-works/vCard) for:

- Issue tracking
- Pull request guidelines
- Architecture Decision Records (ADRs)
- Development setup instructions

## License

All vCard implementations are licensed under the [MIT License](https://github.com/spec-works/vCard/blob/main/LICENSE).

## Links

- **GitHub Repository**: [github.com/spec-works/vCard](https://github.com/spec-works/vCard)
- **RFC 6350 Specification**: [rfc-editor.org/rfc/rfc6350](https://www.rfc-editor.org/rfc/rfc6350)
- **SpecWorks Factory**: [spec-works.github.io](https://spec-works.github.io)
