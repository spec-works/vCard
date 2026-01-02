# ADR 0001: Parse() Method Always Returns List

## Status

Accepted

## Context

RFC 6350 (vCard Format Specification) explicitly allows both single and multiple vCard objects to be contained within a single text stream. Section 3.6.1 states:

> "A text/vcard MIME entity can contain one or more vCards."

The initial API design included two separate methods:
- `Parse(string)` - returned a single `VCardObject`
- `ParseMultiple(string)` - returned `List<VCardObject>`

This design had several problems:

1. **User confusion**: Users had to inspect the content beforehand to know which method to call
2. **Silent data loss**: If a file contained multiple vCards but `Parse()` was called, all vCards after the first would be silently discarded
3. **API inconsistency**: The method name didn't indicate whether the input was expected to contain one or many vCards
4. **Violation of principle of least surprise**: Users would not expect data to be silently discarded

## Decision

Change the `Parse()` method signature to always return `List<VCardObject>`, regardless of whether the input contains one or multiple vCards.

**Before:**
```csharp
public VCardObject Parse(string vcardText)
public List<VCardObject> ParseMultiple(string vcardText)
```

**After:**
```csharp
public List<VCardObject> Parse(string vcardText)
```

The `ParseMultiple()` method is removed as it becomes redundant.

## Consequences

### Positive

1. **No silent data loss**: All vCards in the input are always returned
2. **Consistent API**: Users don't need to guess which method to call
3. **RFC compliance**: Properly handles the RFC 6350 allowance for multiple vCards
4. **Predictable behavior**: The same method works for both single and multiple vCards
5. **Simpler API surface**: One method instead of two

### Negative

1. **Breaking change**: Existing code using `Parse()` must be updated
   - Migration path is straightforward: `var vcard = parser.Parse(data)` becomes `var vcard = parser.Parse(data)[0]`
2. **Slightly more verbose**: Single vCard use cases require indexing or `.First()`
   - This is acceptable tradeoff for correctness and consistency

### Migration Example

**Before:**
```csharp
var parser = new VCardParser();
var vcard = parser.Parse(vcardData);
Console.WriteLine(vcard.FormattedName);
```

**After:**
```csharp
var parser = new VCardParser();
var vcards = parser.Parse(vcardData);
var vcard = vcards[0]; // or vcards.First()
Console.WriteLine(vcard.FormattedName);
```

## References

- [RFC 6350 - vCard Format Specification](https://www.rfc-editor.org/rfc/rfc6350.html)
- Section 3.6.1: MIME Content Type Definition
- User feedback: "A caller should not have to look at the content first to know which method to call."

## Date

2026-01-02
