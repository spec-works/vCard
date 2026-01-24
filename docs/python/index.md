# vCard for Python

Python library for parsing, validating, and serializing vCard 4.0 data according to [RFC 6350](https://www.rfc-editor.org/rfc/rfc6350).

## Installation

Install via pip:

```bash
pip install vcard
```

## Quick Start

### Parsing a vCard

```python
from vcard import VCard

vcard_text = """BEGIN:VCARD
VERSION:4.0
FN:John Doe
N:Doe;John;;;
EMAIL:john.doe@example.com
TEL;TYPE=work:+1-555-555-5555
END:VCARD"""

card = VCard.parse(vcard_text)

print(f"Name: {card.formatted_name}")
print(f"Email: {card.email}")
print(f"Phone: {card.telephone}")
```

### Creating a vCard

```python
from vcard import VCard

card = VCard(
    formatted_name="Jane Smith",
    given_name="Jane",
    family_name="Smith",
    email="jane.smith@example.com",
    telephone="+1-555-123-4567",
    organization="Acme Corporation"
)

vcard_text = str(card)
print(vcard_text)
```

## Features

- ✅ **RFC 6350 Compliant** - Full implementation of vCard 4.0 specification
- ✅ **Pythonic API** - Idiomatic Python design
- ✅ **Parse and Generate** - Read existing vCards and create new ones
- ✅ **Property Validation** - Automatic validation of required properties
- ✅ **Type Hints** - Full type annotation support
- ✅ **Comprehensive Testing** - pytest test suite

## Requirements

- Python 3.8+

## Documentation

For detailed API documentation and examples, see the [Python implementation README](https://github.com/spec-works/vCard/blob/main/python/README.md).

## Source Code

View the source code on [GitHub](https://github.com/spec-works/vCard/tree/main/python).

## License

MIT License - see [LICENSE](https://github.com/spec-works/vCard/blob/main/LICENSE) for details.
