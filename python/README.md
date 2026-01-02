# vCard Python Library

A complete Python library for parsing, validating, and serializing vCard (text/vcard) data according to RFC 6350.

## Features

- **Complete DOM**: Object-oriented representation of all vCard properties
- **Strongly Typed Builder API**: Fluent, type-safe API for creating vCards
- **Type-Safe Enums**: Runtime checking for property parameters (TelType, EmailType, AdrType)
- **RFC 6350 Compliant Parser**: Line unfolding, property parameters, escape sequences
- **vCard Serializer**: Write vCard format with automatic line folding
- **Comprehensive Validator**: Required property validation and format validation
- **Type Hints**: Full type annotations for better IDE support
- **Zero Dependencies**: Pure Python implementation

## Installation

```bash
# From source
git clone https://github.com/yourusername/vcard-python.git
cd vcard-python/python
pip install -e .
```

## Usage

### Parsing a vCard String

```python
from vcard import VCardParser

vcard_data = """BEGIN:VCARD
VERSION:4.0
FN:John Doe
N:Doe;John;Michael;Mr.;Jr.
TEL;TYPE=work:+1-555-555-1234
EMAIL;TYPE=work:john@example.com
ORG:ABC Corporation
TITLE:Software Engineer
END:VCARD"""

parser = VCardParser()
vcards = parser.parse(vcard_data)
vcard = vcards[0]  # Get the first vCard

print(f"Name: {vcard.formatted_name}")
print(f"Organization: {vcard.organization}")
print(f"Title: {vcard.title}")
```

### Parsing from a File

```python
parser = VCardParser()
vcards = parser.parse_file("contact.vcf")
vcard = vcards[0]  # Get the first vCard
```

### Parsing Multiple vCards

The `parse()` method always returns a list of vCards, whether the input contains one or multiple vCards. This prevents silent data loss when a file contains multiple vCards.

```python
parser = VCardParser()
vcards = parser.parse(vcard_data)  # Returns all vCards found

for vcard in vcards:
    print(f"Contact: {vcard.formatted_name}")
```

### Creating a vCard with the Strongly Typed API (Recommended)

The library provides a type-safe builder API for creating vCards:

```python
from vcard import VCardBuilder, TelType, EmailType, AdrType

vcard = (VCardBuilder()
    .with_version("4.0")
    .with_formatted_name("John Michael Doe")
    .with_name_parts("Doe", "John", "Michael", "Mr.", "Jr.")
    .with_organization("ABC Corporation")
    .with_title("Software Engineer")
    .with_telephone("+1-555-555-1234", [TelType.WORK, TelType.VOICE])
    .with_telephone("555-555-5678", [TelType.HOME])
    .with_email("john@example.com", [EmailType.WORK])
    .with_address_parts("", "", "123 Main Street", "Springfield", "IL", "62701", "USA", [AdrType.WORK])
    .with_url("https://www.example.com")
    .with_birthday("19850415")
    .with_note("Important contact")
    .build())

print(f"Name: {vcard.formatted_name}")
```

### Creating a vCard with the Generic API

For extension properties or when you need more control:

```python
from vcard import VCardObject, VCardProperty

vcard = VCardObject()
vcard.version = "4.0"
vcard.formatted_name = "John Doe"
vcard.name = "Doe;John;Michael;Mr.;Jr."

tel_prop = VCardProperty("TEL", "+1-555-555-1234")
tel_prop.add_parameter("TYPE", "work")
tel_prop.add_parameter("TYPE", "voice")
vcard.add_property(tel_prop)

email_prop = VCardProperty("EMAIL", "john@example.com")
email_prop.add_parameter("TYPE", "work")
vcard.add_property(email_prop)

# Add custom extension property
vcard.add_property(VCardProperty("X-CUSTOM", "Custom Value"))
```

### Serializing to vCard Format

```python
from vcard import VCardSerializer

serializer = VCardSerializer()
vcard_text = serializer.serialize(vcard)
print(vcard_text)

# Save to file
serializer.serialize_to_file(vcard, "contact.vcf")
```

### Validating a vCard

```python
from vcard import VCardValidator

validator = VCardValidator()
result = validator.validate(vcard)

if result.is_valid:
    print("vCard is valid!")
else:
    print("Validation errors:")
    for error in result.errors:
        print(f"  - {error}")

# Get detailed summary
print(result.get_summary())
```

### Working with Structured Data

```python
from vcard import StructuredName, StructuredAddress

# Parse structured name
name_prop = vcard.get_property("N")
if name_prop:
    structured_name = StructuredName.parse(name_prop.value)
    print(f"Family Name: {structured_name.family_name}")
    print(f"Given Name: {structured_name.given_name}")

# Parse structured address
address_prop = vcard.get_property("ADR")
if address_prop:
    structured_address = StructuredAddress.parse(address_prop.value)
    print(f"Street: {structured_address.street_address}")
    print(f"City: {structured_address.locality}")
    print(f"Postal Code: {structured_address.postal_code}")
```

## Project Structure

```
vcard-python/
├── src/
│   └── vcard/
│       ├── __init__.py       # Package exports
│       ├── dom.py            # Document Object Model
│       ├── parser.py         # RFC 6350 parser
│       ├── serializer.py     # vCard serializer
│       └── validator.py      # Validation logic
├── tests/
│   └── test_vcard.py         # Unit tests
├── README.md                 # This file
└── setup.py                  # Package setup
```

## Requirements

- Python 3.7+
- No external dependencies

## Testing

```bash
python -m pytest tests/
```

## License

MIT License - This implementation is provided for educational and commercial use.

## References

- [RFC 6350 - vCard Format Specification](https://www.rfc-editor.org/rfc/rfc6350.html)
- [RFC 9554 - vCard Format Extensions for JSContact](https://datatracker.ietf.org/doc/rfc9554/)
- [IANA vCard Elements Registry](https://www.iana.org/assignments/vcard-elements/vcard-elements.xhtml)

## Contributing

Contributions are welcome! Please ensure all tests pass before submitting pull requests.
