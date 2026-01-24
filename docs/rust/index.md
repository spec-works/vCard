# vCard for Rust

Rust library for parsing, validating, and serializing vCard 4.0 data according to [RFC 6350](https://www.rfc-editor.org/rfc/rfc6350).

## Installation

Add to your `Cargo.toml`:

```toml
[dependencies]
vcard = "*"
```

## Quick Start

### Parsing a vCard

```rust
use vcard::VCard;

fn main() -> Result<(), Box<dyn std::error::Error>> {
    let vcard_text = r#"BEGIN:VCARD
VERSION:4.0
FN:John Doe
N:Doe;John;;;
EMAIL:john.doe@example.com
TEL;TYPE=work:+1-555-555-5555
END:VCARD"#;

    let card = VCard::parse(vcard_text)?;

    println!("Name: {}", card.formatted_name());
    println!("Email: {}", card.email().unwrap());
    println!("Phone: {}", card.telephone().unwrap());

    Ok(())
}
```

### Creating a vCard

```rust
use vcard::VCard;

fn main() {
    let card = VCard::new()
        .formatted_name("Jane Smith")
        .given_name("Jane")
        .family_name("Smith")
        .email("jane.smith@example.com")
        .telephone("+1-555-123-4567")
        .organization("Acme Corporation");

    let vcard_text = card.to_string();
    println!("{}", vcard_text);
}
```

## Features

- ✅ **RFC 6350 Compliant** - Full implementation of vCard 4.0 specification
- ✅ **Safe and Fast** - Leveraging Rust's safety guarantees and performance
- ✅ **Parse and Generate** - Read existing vCards and create new ones
- ✅ **Property Validation** - Compile-time and runtime validation
- ✅ **Zero-Copy Parsing** - Efficient parsing with minimal allocations
- ✅ **Comprehensive Testing** - Extensive test coverage

## Requirements

- Rust Edition 2021

## Documentation

For detailed API documentation, see [docs.rs/vcard](https://docs.rs/vcard).

For implementation details and examples, see the [Rust implementation README](https://github.com/spec-works/vCard/blob/main/rust/README.md).

## Source Code

View the source code on [GitHub](https://github.com/spec-works/vCard/tree/main/rust).

## License

MIT License - see [LICENSE](https://github.com/spec-works/vCard/blob/main/LICENSE) for details.
