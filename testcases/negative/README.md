# Negative Test Cases for vCard Parser

This directory contains malformed and invalid vCard files designed to test error handling and validation in the parser implementations. Each test case represents a specific violation of RFC 6350 or common parsing errors.

## Purpose

These negative test cases ensure that the parser:
- Detects and reports errors appropriately
- Fails gracefully on invalid input
- Provides meaningful error messages
- Does not crash or produce incorrect results silently

## Test Categories

### 1. Structural Errors

#### `missing_begin.vcf`
**Error:** Missing `BEGIN:VCARD` marker
**Expected:** Parser should reject - vCard must start with BEGIN:VCARD

#### `missing_end.vcf`
**Error:** Missing `END:VCARD` marker
**Expected:** Parser should reject - vCard must end with END:VCARD

#### `incomplete_vcard.vcf`
**Error:** vCard starts but has no content or END marker
**Expected:** Parser should reject - incomplete structure

#### `nested_vcard.vcf`
**Error:** vCard nested inside another vCard
**Expected:** Parser should reject - nesting not allowed per RFC 6350

#### `mismatched_begin_end.vcf`
**Error:** BEGIN:VCARD but END:VCALENDAR
**Expected:** Parser should reject - component type mismatch

#### `wrong_component_type.vcf`
**Error:** Uses VCALENDAR instead of VCARD
**Expected:** Parser should reject - wrong component type

#### `property_after_end.vcf`
**Error:** Properties appear after END:VCARD
**Expected:** Parser should reject or ignore - content after END is invalid

### 2. Required Property Violations

#### `missing_version.vcf`
**Error:** No VERSION property (required per RFC 6350 Section 6.7.9)
**Expected:** Parser should reject - VERSION is mandatory

#### `missing_fn.vcf`
**Error:** No FN (formatted name) property (required per RFC 6350 Section 6.2.1)
**Expected:** Parser should reject - FN is mandatory

#### `duplicate_version.vcf`
**Error:** Multiple VERSION properties (only one allowed per RFC 6350)
**Expected:** Parser should reject or use first - cardinality violation

#### `duplicate_fn.vcf`
**Error:** Multiple FN properties (only one allowed per RFC 6350)
**Expected:** Parser may accept (RFC allows multiple in some contexts) or reject

### 3. Version Support

#### `unsupported_version_2_1.vcf`
**Error:** vCard version 2.1 (not supported - only 4.0)
**Expected:** Parser should reject with "unsupported version" error
**Rationale:** ADR 0004 - only vCard 4.0 is supported

#### `unsupported_version_3_0.vcf`
**Error:** vCard version 3.0 (not supported - only 4.0)
**Expected:** Parser should reject with "unsupported version" error
**Rationale:** ADR 0004 - only vCard 4.0 is supported

#### `unsupported_version_1_0.vcf`
**Error:** vCard version 1.0 (never existed)
**Expected:** Parser should reject - invalid version

#### `invalid_version_format.vcf`
**Error:** VERSION has text instead of numeric format
**Expected:** Parser should reject - invalid version format

### 4. Syntax Errors

#### `malformed_property_no_colon.vcf`
**Error:** Property line missing colon separator
**Expected:** Parser should reject - invalid property syntax

#### `invalid_property_name.vcf`
**Error:** Property name contains spaces (invalid per RFC 6350 Section 3.3)
**Expected:** Parser should reject - property names must be valid tokens

#### `malformed_parameter_syntax.vcf`
**Error:** Parameter without value (TYPE: instead of TYPE=value)
**Expected:** Parser should reject - parameters must have format NAME=VALUE

#### `invalid_group_syntax.vcf`
**Error:** Multiple dots in group syntax (invalid per RFC 6350 Section 3.3)
**Expected:** Parser should reject - group.property format only

### 5. Line Folding Errors

#### `invalid_line_folding.vcf`
**Error:** Continuation line not indented with space or tab
**Expected:** Parser may misinterpret as separate property - RFC 6350 Section 3.2

### 6. Escape Sequence Errors

#### `unescaped_special_characters.vcf`
**Error:** Literal newlines and semicolons not escaped
**Expected:** Parser may misinterpret structure - should escape \n, \;, \,, \\

### 7. Structured Value Errors

#### `invalid_structured_value.vcf`
**Error:** N property has too many components (should be 5: Family;Given;Additional;Prefix;Suffix)
**Expected:** Parser should reject or ignore extra fields - RFC 6350 Section 6.2.2

#### `missing_required_structured_components.vcf`
**Error:** N property with only empty components
**Expected:** Parser may accept (empty is valid) or reject if implementation requires values

### 8. Data Format Errors

#### `invalid_date_format.vcf`
**Error:** BDAY with text format instead of ISO 8601
**Expected:** Parser should reject - dates must be YYYYMMDD or full ISO 8601

#### `invalid_encoding.vcf`
**Error:** Invalid base64 in PHOTO property
**Expected:** Parser should reject - encoding violations

#### `invalid_quoted_printable.vcf`
**Error:** Invalid quoted-printable encoding
**Expected:** Parser should reject - encoding format violations

#### `invalid_uri_scheme.vcf`
**Error:** URL property with invalid URI
**Expected:** Parser may accept (lenient) or reject (strict URI validation)

### 9. Parameter Errors

#### `invalid_parameter_value.vcf`
**Error:** TYPE parameter with non-standard value
**Expected:** Parser may accept (extensible) or reject if strict validation enabled

### 10. Empty and Whitespace Files

#### `empty_file.vcf`
**Error:** Completely empty file
**Expected:** Parser should reject - no content to parse

#### `only_whitespace.vcf`
**Error:** File contains only whitespace
**Expected:** Parser should reject - no valid vCard content

### 11. Semantic Errors

#### `circular_reference.vcf`
**Error:** MEMBER reference that could create circular dependency
**Expected:** Parser may accept (circular references need runtime detection)

## Usage in Tests

### Expected Test Behavior

Negative test cases should:
1. **Throw exceptions** - Parser raises appropriate error (ParseException, ValidationException, etc.)
2. **Return error codes** - Parser returns failure status with error details
3. **Produce empty results** - Parser returns empty list or null with logged errors

### Example Test Implementation

**.NET:**
```csharp
[Theory]
[InlineData("missing_begin.vcf")]
[InlineData("missing_version.vcf")]
public void NegativeTests_ShouldThrowException(string filename)
{
    var parser = new VCardParser();
    var content = File.ReadAllText($"testcases/negative/{filename}");

    Assert.Throws<ParseException>(() => parser.Parse(content));
}
```

**Python:**
```python
@pytest.mark.parametrize("filename", [
    "missing_begin.vcf",
    "missing_version.vcf",
])
def test_negative_cases_should_raise_error(filename):
    parser = VCardParser()
    with open(f"testcases/negative/{filename}") as f:
        content = f.read()

    with pytest.raises(ParseException):
        parser.parse(content)
```

**Rust:**
```rust
#[test]
fn test_negative_missing_begin() {
    let content = fs::read_to_string("testcases/negative/missing_begin.vcf").unwrap();
    let mut parser = VCardParser::new();
    let result = parser.parse(&content);

    assert!(result.is_err());
}
```

## Test Statistics

- **Total negative test cases:** 31
- **Structural errors:** 7
- **Required property violations:** 4
- **Version errors:** 4
- **Syntax errors:** 4
- **Data format errors:** 6
- **Other errors:** 6

## References

- [RFC 6350 - vCard Format Specification](https://www.rfc-editor.org/rfc/rfc6350.html)
- Section 3.3: Property and Parameter Syntax
- Section 6.7.9: VERSION Property (Required)
- Section 6.2.1: FN Property (Required)
- [ADR 0004: Support Only vCard Version 4.0](../../adr/0004-vcard-version-4-only.md)

## Notes

- Some parsers may be more lenient than others - this is acceptable
- The key is that parsers should not silently accept and misinterpret invalid data
- Error messages should be clear and actionable for developers
- Some test cases may pass on lenient parsers (e.g., duplicate properties, extra fields)
- Version 2.1 and 3.0 test cases MUST fail per ADR 0004
