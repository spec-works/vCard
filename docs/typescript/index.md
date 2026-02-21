# vCard for TypeScript

TypeScript library for parsing, validating, and serializing vCard 4.0 data according to [RFC 6350](https://www.rfc-editor.org/rfc/rfc6350).

## Installation

```bash
npm install @specworks/vcard
```

## Quick Start

### Parsing a vCard

```typescript
import { VCardParser } from '@specworks/vcard';

const parser = new VCardParser();
const vcards = parser.parse(vcardString);

for (const vcard of vcards) {
  console.log(`Name: ${vcard.formattedName}`);
  for (const tel of vcard.telephones) {
    console.log(`Phone: ${tel.value}`);
  }
  for (const email of vcard.emails) {
    console.log(`Email: ${email.value}`);
  }
}
```

### Creating a vCard

```typescript
import { VCardObject, VCardSerializer, Telephone, Email, TelType, EmailType } from '@specworks/vcard';

const vcard = new VCardObject();
vcard.version = '4.0';
vcard.formattedName = 'John Doe';
vcard.name = 'Doe;John;;;';

const tel = new Telephone();
tel.value = '+1-555-1234';
tel.types = TelType.Work | TelType.Voice;
vcard.telephones.push(tel);

const email = new Email();
email.value = 'john@example.com';
email.types = EmailType.Work;
vcard.emails.push(email);

const serializer = new VCardSerializer();
console.log(serializer.serialize(vcard));
```

### Validating a vCard

```typescript
import { VCardValidator } from '@specworks/vcard';

const validator = new VCardValidator();
const result = validator.validate(vcard);

if (result.isValid) {
  console.log('vCard is valid!');
} else {
  for (const error of result.errors) {
    console.log(`Error: ${error}`);
  }
}
```

## API Reference

### Classes

| Class | Description |
|-------|-------------|
| `VCardParser` | Parses vCard text into `VCardObject[]`. Always returns a list (per ADR 0001). |
| `VCardSerializer` | Serializes `VCardObject` to vCard text with line folding and escaping. |
| `VCardValidator` | Validates `VCardObject` against RFC 6350 rules. |
| `VCardObject` | Root vCard DOM object with typed property accessors. |
| `VCardProperty` | Generic property with name, value, and parameters. |
| `Telephone` | Strongly-typed telephone with `TelType` flags. |
| `Email` | Strongly-typed email with `EmailType` flags. |
| `Address` | Strongly-typed postal address with `AdrType` flags. |
| `StructuredName` | Helper for parsing/formatting N property components. |
| `StructuredAddress` | Helper for parsing/formatting ADR property components. |

### Enums

| Enum | Values |
|------|--------|
| `TelType` | Text, Voice, Fax, Cell, Video, Pager, TextPhone, Work, Home |
| `EmailType` | Work, Home, Internet |
| `AdrType` | Work, Home, Postal, Parcel, Dom, Intl |

Enums support bitwise OR for combining types: `TelType.Work | TelType.Voice`.

## Features

- ✅ RFC 6350 compliant parser with line unfolding and escape handling
- ✅ Serializer with automatic line folding (75-char limit) and escaping
- ✅ Comprehensive validator for all standard properties
- ✅ Strongly-typed API with TypeScript enums for TEL, EMAIL, and ADR types
- ✅ Parse always returns a list (prevents silent data loss per ADR 0001)
- ✅ vCard 4.0 only (per ADR 0004)
- ✅ Zero runtime dependencies
- ✅ Tested against 74 shared test cases (43 positive + 31 negative)

## Requirements

- Node.js 18 or later
- TypeScript 5.0 or later

## Source Code

View the source code on [GitHub](https://github.com/spec-works/vCard/tree/main/typescript).
