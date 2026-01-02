# vCard Test Cases

This directory contains comprehensive test cases for vCard (RFC 6350) implementations. The test cases cover both normal usage patterns and edge cases.

## Normal Test Cases

### Basic Contacts

- **minimal_vcard.vcf** - Minimal valid vCard with only required properties (VERSION and FN)
- **basic_personal_contact.vcf** - Simple personal contact with common properties
- **business_contact.vcf** - Business contact with organization, title, work contact information
- **contact_with_multiple_phones_emails.vcf** - Contact with multiple telephone numbers and email addresses

### Complete Contacts

- **complete_contact_all_properties.vcf** - Comprehensive vCard demonstrating all major RFC 6350 properties

## Property-Specific Test Cases

### Structured Data

- **complex_structured_name.vcf** - Names with all components (family, given, additional, prefix, suffix)
- **complex_structured_address.vcf** - Addresses with all components (PO box, extended, street, locality, region, postal code, country)
- **empty_structured_components.vcf** - Structured values with empty components

### Telephone Properties

- **phone_multiple_types.vcf** - Phone numbers with various TYPE parameters (work, home, cell, fax, voice, video, pager, textphone)

### Email Properties

- **email_multiple_types.vcf** - Email addresses with various TYPE parameters (work, home, internet)

### Address Properties

- **address_multiple_types.vcf** - Addresses with various TYPE parameters (work, home, postal, parcel, dom, intl)

### Date and Time

- **various_date_formats.vcf** - Different date formats (YYYYMMDD, YYYY-MM-DD, --MMDD, YYYY, YYYY-MM, datetime)
- **timezone_formats.vcf** - Various timezone representations (UTC offset, positive offset, IANA timezone names)

### Geographic

- **geographic_coordinates.vcf** - GEO property with various coordinate formats

### Communication

- **instant_messaging.vcf** - IMPP properties for instant messaging (XMPP, SIP, Skype, AIM)
- **language_properties.vcf** - LANG properties with preference levels

### Relationships

- **related_entities.vcf** - RELATED properties (spouse, child, colleague, friend, parent)

### Media

- **photo_property.vcf** - PHOTO properties with URLs and media types
- **logo_property.vcf** - LOGO properties for organizational logos
- **sound_property.vcf** - SOUND properties for pronunciation guides

### Categories and Labels

- **multiple_categories.vcf** - Multiple CATEGORIES properties
- **multiple_nicknames.vcf** - NICKNAME with comma-separated values
- **multiple_notes.vcf** - Multiple NOTE properties
- **multiple_urls.vcf** - Multiple URL properties

### KIND Property

- **kind_individual.vcf** - KIND:individual (default, represents a person)
- **kind_group.vcf** - KIND:group with MEMBER properties
- **kind_organization.vcf** - KIND:org for organizations
- **kind_location.vcf** - KIND:location for places/rooms

### Calendar Integration

- **calendar_properties.vcf** - FBURL, CALADRURI, CALURI properties

### Security

- **public_key_property.vcf** - KEY properties for public keys

### Advanced

- **clientpidmap_property.vcf** - CLIENTPIDMAP for synchronization
- **pref_parameter.vcf** - PREF parameter for preference ordering
- **value_parameter.vcf** - VALUE parameter demonstrations

## Edge Cases and Special Scenarios

### Line Handling

- **line_folding_long_values.vcf** - Long lines folded at 75 characters with continuation using space/tab
- **whitespace_handling.vcf** - Empty lines and whitespace variations

### Character Encoding

- **escaped_characters.vcf** - Special character escaping (\n, \;, \,, \\)
- **unicode_characters.vcf** - UTF-8 characters (Spanish, Chinese, Japanese, Arabic, emoji)

### File Structure

- **multiple_vcards_in_file.vcf** - Multiple vCard objects in a single file

### Parameter Handling

- **parameters_with_quotes.vcf** - Parameters with quoted values and special characters
- **property_groups.vcf** - Property grouping using item1., item2. prefixes

### Case Sensitivity

- **case_insensitive_properties.vcf** - Mixed case property names and parameters (should be case-insensitive)

### Property Order

- **unusual_property_order.vcf** - Valid vCard with non-standard property ordering (VERSION not first)

### Extensions

- **custom_extension_properties.vcf** - Custom X- prefixed extension properties

## Test Case Organization

Test cases are organized by:

1. **Functionality** - What vCard features they exercise
2. **Complexity** - From minimal to comprehensive
3. **Edge Cases** - Unusual but valid scenarios that test parser robustness

## Usage in Testing

These test cases can be used to:

1. **Parser Testing** - Verify correct parsing of valid vCard data
2. **Serializer Testing** - Generate vCards and compare to expected format
3. **Round-Trip Testing** - Parse, serialize, and parse again to verify data integrity
4. **Validator Testing** - Check proper validation of vCard constraints
5. **Edge Case Handling** - Ensure robust handling of unusual inputs

## RFC 6350 Compliance

All test cases are designed to be compliant with:

- **RFC 6350** - vCard Format Specification (version 4.0)

## Notes

- All vCard 4.0 files should use UTF-8 encoding
- Line endings should be CRLF (\\r\\n) per RFC 6350
- Property names are case-insensitive
- Parameter names are case-insensitive
- Some test cases intentionally include edge cases to test parser robustness
