# Specworks vCard - TypeScript

Parse, validate, and serialize vCard data according to [RFC 6350](https://www.rfc-editor.org/rfc/rfc6350).

## Installation

```bash
npm install @specworks/vcard
```

## Features

- ✅ **RFC 6350 Compliant** - Full implementation of the vCard specification
- ✅ **Parse vCard** - Parse .vcf files and vCard strings
- ✅ **Validate Data** - Comprehensive validation of vCard properties
- ✅ **Serialize to vCard** - Generate valid .vcf files
- ✅ **DOM Support** - Complete Document Object Model for vCard data
- ✅ **Type-Safe API** - Strong typing with TypeScript enums and interfaces
- ✅ **Zero Dependencies** - No runtime dependencies

## Quick Start

### Parsing vCard

```typescript
import { VCardParser } from '@specworks/vcard';

const parser = new VCardParser();
const vcards = parser.parse(vcardString);

for (const vcard of vcards) {
  console.log(`Name: ${vcard.formattedName}`);
  console.log(`Phones: ${vcard.telephones.length}`);
  console.log(`Emails: ${vcard.emails.length}`);
}
```

### Creating vCard

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

### Validating vCard

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
| `VCardParser` | Parses vCard text into `VCardObject[]` |
| `VCardSerializer` | Serializes `VCardObject` to vCard text |
| `VCardValidator` | Validates `VCardObject` against RFC 6350 |
| `VCardObject` | Root vCard DOM object |
| `VCardProperty` | Generic property with parameters and value |
| `Telephone` | Strongly-typed telephone with `TelType` flags |
| `Email` | Strongly-typed email with `EmailType` flags |
| `Address` | Strongly-typed postal address with `AdrType` flags |

### Enums

| Enum | Values |
|------|--------|
| `TelType` | Text, Voice, Fax, Cell, Video, Pager, TextPhone, Work, Home |
| `EmailType` | Work, Home, Internet |
| `AdrType` | Work, Home, Postal, Parcel, Dom, Intl |

## Requirements

- Node.js 18 or later
- TypeScript 5.0 or later

## License

MIT License
