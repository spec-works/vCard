# vCard Rust Library

A complete Rust library for parsing and generating vCard (RFC 6350) data.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Complete DOM**: Object-oriented representation of all vCard properties
- **RFC 6350 Compliant Parser**: Line unfolding, property parameters, escape sequences
- **Strongly Typed Builder API**: Fluent, type-safe API for creating vCards
- **Type-Safe Enums**: Compile-time checking for property parameters (TelType, EmailType, AdrType)
- **Type Safety**: Leverages Rust's type system for safe parsing and construction
- **Zero Dependencies**: Pure Rust implementation
- **Comprehensive Tests**: Full test coverage with 12+ unit tests

## Installation

Add this to your `Cargo.toml`:

```toml
[dependencies]
vcard = "1.0.0"
```

## Usage

### Parsing a vCard String

```rust
use vcard::{VCardParser, VCardObject};

let vcard_data = "BEGIN:VCARD
VERSION:4.0
FN:John Doe
N:Doe;John;Michael;Mr.;Jr.
TEL;TYPE=work:+1-555-555-1234
EMAIL;TYPE=work:john@example.com
ORG:ABC Corporation
TITLE:Software Engineer
END:VCARD";

let mut parser = VCardParser::new();
let vcards = parser.parse(vcard_data).unwrap();
let vcard = &vcards[0]; // or vcards.first().unwrap()

println!("Name: {}", vcard.formatted_name().unwrap());
println!("Organization: {}", vcard.organization().unwrap());
println!("Title: {}", vcard.title().unwrap());
```

### Parsing Multiple vCards

The `parse()` method always returns a `Vec<VCardObject>`, whether the input contains one or multiple vCards. This prevents silent data loss when a stream contains multiple vCards.

```rust
let vcard_data = "BEGIN:VCARD
VERSION:4.0
FN:John Doe
END:VCARD

BEGIN:VCARD
VERSION:4.0
FN:Jane Smith
END:VCARD";

let mut parser = VCardParser::new();
let vcards = parser.parse(vcard_data).unwrap(); // Returns all vCards found

for vcard in &vcards {
    println!("Contact: {}", vcard.formatted_name().unwrap());
}
```

### Creating a vCard with the Strongly Typed API (Recommended)

The library provides a type-safe builder API for creating vCards:

```rust
use vcard::{VCardObject, TelType, EmailType, AdrType};

let vcard = VCardObject::builder()
    .version("4.0")
    .formatted_name("John Michael Doe")
    .name_parts("Doe", "John", "Michael", "Mr.", "Jr.")
    .organization("ABC Corporation")
    .title("Software Engineer")
    .telephone("+1-555-555-1234", vec![TelType::Work, TelType::Voice])
    .telephone("555-555-5678", vec![TelType::Home])
    .email("john@example.com", vec![EmailType::Work])
    .address_parts(
        "",                    // PO Box
        "",                    // Extended
        "123 Main Street",     // Street
        "Springfield",         // Locality
        "IL",                  // Region
        "62701",               // Postal Code
        "USA",                 // Country
        vec![AdrType::Work]
    )
    .url("https://www.example.com")
    .birthday("19850415")
    .note("Important contact")
    .build();

assert_eq!(vcard.version(), Some("4.0"));
assert_eq!(vcard.formatted_name(), Some("John Michael Doe"));
```

### Creating a vCard with the Generic API

For extension properties or when you need more control:

```rust
use vcard::{VCardObject, VCardProperty};

let mut vcard = VCardObject::new();

vcard.add_property(VCardProperty::new("VERSION", "4.0"));
vcard.add_property(VCardProperty::new("FN", "John Doe"));
vcard.add_property(VCardProperty::new("N", "Doe;John;Michael;Mr.;Jr."));
vcard.add_property(VCardProperty::new("ORG", "ABC Corporation"));
vcard.add_property(VCardProperty::new("TITLE", "Software Engineer"));

let mut tel_prop = VCardProperty::new("TEL", "+1-555-555-1234");
tel_prop.add_parameter("TYPE", "work");
tel_prop.add_parameter("TYPE", "voice");
vcard.add_property(tel_prop);

let mut email_prop = VCardProperty::new("EMAIL", "john@example.com");
email_prop.add_parameter("TYPE", "work");
vcard.add_property(email_prop);

// Add custom extension property
vcard.add_property(VCardProperty::new("X-CUSTOM", "Custom Value"));
```

### Working with Properties

```rust
// Get a single property
if let Some(tel) = vcard.get_property("TEL") {
    println!("Phone: {}", tel.value);

    // Access property parameters
    if let Some(type_param) = tel.get_parameter("TYPE") {
        println!("Type: {}", type_param);
    }
}

// Get all properties with the same name
if let Some(telephones) = vcard.telephones() {
    for tel in telephones {
        println!("Phone: {}", tel.value);
        if let Some(types) = tel.get_parameters("TYPE") {
            println!("Types: {:?}", types);
        }
    }
}
```

### Error Handling

```rust
use vcard::{VCardParser, ParseError};

let invalid_data = "VERSION:4.0\nFN:John Doe\nEND:VCARD";

let mut parser = VCardParser::new();
match parser.parse(invalid_data) {
    Ok(vcards) => println!("Parsed {} vCard(s) successfully", vcards.len()),
    Err(e) => println!("Parse error: {}", e),
}
```

## API Documentation

### Strongly Typed API

#### `VCardBuilder`

Fluent builder for creating vCards with type safety:

**Basic Properties:**
- `version(version)` - Set VERSION (typically "4.0")
- `formatted_name(name)` - Set FN (required)
- `name(name)` - Set N as semicolon-separated string
- `name_parts(family, given, additional, prefix, suffix)` - Set N with separate components
- `organization(org)` - Set ORG
- `title(title)` - Set TITLE
- `role(role)` - Set ROLE
- `nickname(nickname)` - Set NICKNAME
- `note(note)` - Set NOTE
- `uid(uid)` - Set UID
- `categories(categories)` - Set CATEGORIES
- `revision(rev)` - Set REV

**Communication Properties:**
- `telephone(number, types: Vec<TelType>)` - Add TEL with type-safe parameters
- `email(email, types: Vec<EmailType>)` - Add EMAIL with type-safe parameters
- `url(url)` - Set URL

**Address Properties:**
- `address(address, types: Vec<AdrType>)` - Add ADR as semicolon-separated string
- `address_parts(po_box, extended, street, locality, region, postal_code, country, types)` - Add ADR with separate components

**Personal Properties:**
- `birthday(date)` - Set BDAY (format: YYYYMMDD)
- `anniversary(date)` - Set ANNIVERSARY (format: YYYYMMDD)
- `gender(gender)` - Set GENDER
- `photo(uri)` - Set PHOTO

**Extension:**
- `custom_property(name, value)` - Add custom/extension property
- `build()` - Build and return the VCardObject

#### Type-Safe Enums

**`TelType`** - Telephone types:
- `Text`, `Voice`, `Fax`, `Cell`, `Video`, `Pager`, `TextPhone`, `Work`, `Home`

**`EmailType`** - Email types:
- `Work`, `Home`, `Internet`

**`AdrType`** - Address types:
- `Work`, `Home`, `Postal`, `Parcel`, `Dom`, `Intl`

### Generic API

#### `VCardObject`

The main vCard container with the following methods:

- `new()` - Create a new empty vCard
- `builder()` - Create a VCardBuilder for fluent API
- `add_property(property)` - Add a property to the vCard
- `get_property(name)` - Get the first property with the given name
- `get_properties(name)` - Get all properties with the given name
- `version()` - Get the VERSION property value
- `formatted_name()` - Get the FN property value
- `name()` - Get the N property value
- `organization()` - Get the ORG property value
- `title()` - Get the TITLE property value
- `telephones()` - Get all TEL properties
- `emails()` - Get all EMAIL properties
- `addresses()` - Get all ADR properties

#### `VCardProperty`

Represents a vCard property with:

- `name` - Property name (uppercase)
- `value` - Property value
- `parameters` - HashMap of property parameters
- `new(name, value)` - Create a new property
- `add_parameter(param_name, param_value)` - Add a parameter
- `get_parameter(param_name)` - Get first parameter value
- `get_parameters(param_name)` - Get all parameter values

#### `VCardParser`

Parser for vCard text:

- `new()` - Create a new parser
- `parse(text)` - Parse vCards from text, returns `Result<Vec<VCardObject>, ParseError>` containing all vCards found

## Building

```bash
cargo build
```

## Testing

```bash
cargo test
```

All tests should pass:

```
running 12 tests
test tests::test_builder_basic ... ok
test tests::test_builder_complete_vcard ... ok
test tests::test_builder_with_address_parts ... ok
test tests::test_builder_with_custom_property ... ok
test tests::test_builder_with_email ... ok
test tests::test_builder_with_telephone ... ok
test tests::test_new_vcard_object ... ok
test tests::test_parse_multiple_vcards ... ok
test tests::test_parse_simple_vcard ... ok
test tests::test_parse_vcard_with_multiple_properties ... ok
test tests::test_parse_vcard_with_parameters ... ok
test tests::test_parse_vcard_with_properties ... ok

test result: ok. 12 passed; 0 failed; 0 ignored; 0 measured
```

## Project Structure

```
vcard-rust/
├── src/
│   └── lib.rs              # Main library implementation
├── Cargo.toml              # Package configuration
└── README.md               # This file
```

## RFC 6350 Compliance

This implementation follows RFC 6350 (vCard Format Specification):

- Line folding/unfolding (Section 3.2)
- Content lines (Section 3.3)
- Property parameters (Section 5)
- Property value data types (Section 4)
- vCard properties (Section 6)
- Escape sequences for special characters

## License

MIT License - This implementation is provided for educational and commercial use.

## References

- [RFC 6350 - vCard Format Specification](https://www.rfc-editor.org/rfc/rfc6350.html)
- [RFC 9554 - vCard Format Extensions for JSContact](https://datatracker.ietf.org/doc/rfc9554/)
- [IANA vCard Elements Registry](https://www.iana.org/assignments/vcard-elements/vcard-elements.xhtml)

## Contributing

Contributions are welcome! Please ensure all tests pass before submitting pull requests.

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass: `cargo test`
6. Submit a pull request
