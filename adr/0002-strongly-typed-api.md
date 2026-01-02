# ADR 0002: Strongly Typed API for vCard Properties

## Status

Accepted

## Context

The current vCard implementations across all three languages (.NET, Python, Rust) use a generic string-based approach for creating and manipulating vCard properties:

**Current Approach:**
```rust
// Rust example - all languages follow similar pattern
vcard.add_property(VCardProperty::new("VERSION", "4.0"));
vcard.add_property(VCardProperty::new("FN", "John Doe"));
vcard.add_property(VCardProperty::new("EMAIL", "john@example.com"));
```

This approach has several drawbacks:

1. **No compile-time validation**: Property names and values are strings, so typos aren't caught until runtime
2. **Limited discoverability**: IDEs cannot suggest available properties or their expected formats
3. **No type safety**: All values are strings, even for structured data (dates, coordinates, etc.)
4. **Verbose**: Requires manually creating `VCardProperty` objects and managing parameters
5. **Error-prone**: Easy to misspell property names (e.g., "EMAI" instead of "EMAIL")

While the string-based approach provides flexibility for custom/extension properties, the vast majority of use cases involve standard RFC 6350 properties that have well-defined types and structures.

## Decision

All vCard implementations SHALL provide a strongly typed API alongside the existing generic property API. The strongly typed API should:

1. **Provide dedicated methods/properties for standard vCard fields**
   - Common properties: VERSION, FN, N, EMAIL, TEL, ADR, ORG, etc.
   - Type-safe representations where applicable

2. **Support builder pattern for property construction**
   - Fluent API for adding properties with parameters
   - Type-safe parameter values (e.g., enums for TYPE parameter)

3. **Maintain backward compatibility**
   - Existing generic `add_property()` / `AddProperty()` remains available
   - Strongly typed API is additive, not a breaking change

4. **Language-specific implementations**
   - **.NET**: Extension methods, fluent builder, property setters
   - **Python**: Builder pattern or property decorators
   - **Rust**: Builder pattern with type-safe methods

## Implementation Guidelines

### Rust Implementation

```rust
// Enum for TYPE parameters
pub enum TelType {
    Work,
    Home,
    Cell,
    Voice,
    Fax,
    // ...
}

// Builder for VCardObject
impl VCardObject {
    pub fn builder() -> VCardBuilder {
        VCardBuilder::new()
    }
}

pub struct VCardBuilder {
    vcard: VCardObject,
}

impl VCardBuilder {
    pub fn version(mut self, version: &str) -> Self {
        self.vcard.add_property(VCardProperty::new("VERSION", version));
        self
    }

    pub fn formatted_name(mut self, name: &str) -> Self {
        self.vcard.add_property(VCardProperty::new("FN", name));
        self
    }

    pub fn telephone(mut self, number: &str, types: Vec<TelType>) -> Self {
        let mut prop = VCardProperty::new("TEL", number);
        for tel_type in types {
            prop.add_parameter("TYPE", tel_type.as_str());
        }
        self.vcard.add_property(prop);
        self
    }

    pub fn build(self) -> VCardObject {
        self.vcard
    }
}

// Usage:
let vcard = VCardObject::builder()
    .version("4.0")
    .formatted_name("John Doe")
    .telephone("+1-555-555-1234", vec![TelType::Work, TelType::Voice])
    .build();
```

### .NET Implementation

C# has excellent object initialization syntax that handles both simple and complex scenarios. The strongly typed experience in .NET SHOULD primarily rely on object initializers, with builder pattern as an optional convenience.

**Object Initializers (Primary Approach):**

C# object initializers handle all scenarios elegantly, including complex objects:

```csharp
// Simple vCard
var vcard = new VCardObject
{
    Version = "4.0",
    FormattedName = "John Doe",
    Organization = "ACME Corp",
    Title = "Software Engineer"
};

// Complex vCard with properties that have parameters
// The VCardObject exposes properties and methods that work naturally with initializers
var vcard = new VCardObject
{
    Version = "4.0",
    FormattedName = "John Doe",
    Name = "Doe;John;Michael;Mr.;Jr.",
    Organization = "ACME Corp"
};

// For properties with parameters (TEL, EMAIL, ADR), use helper methods or extension methods
vcard.AddTelephone("+1-555-1234", TelType.Work, TelType.Voice);
vcard.AddEmail("john@example.com", EmailType.Work);
```

**Builder Pattern (Optional Convenience):**

The builder pattern MAY be provided as an alternative for developers who prefer fluent APIs, but it is NOT required for handling complex objects. C# object initializers are sufficient for all scenarios.

```csharp
// Optional builder for those who prefer fluent APIs
public class VCardBuilder
{
    private readonly VCardObject _vcard = new VCardObject();

    public VCardBuilder WithVersion(string version)
    {
        _vcard.Version = version;
        return this;
    }

    public VCardBuilder WithTelephone(string number, params TelType[] types)
    {
        _vcard.AddTelephone(number, types);
        return this;
    }

    public VCardObject Build() => _vcard;
}

// Usage (optional style, not required):
var vcard = new VCardBuilder()
    .WithVersion("4.0")
    .WithFormattedName("John Doe")
    .WithTelephone("+1-555-555-1234", TelType.Work, TelType.Voice)
    .Build();
```

**Key Principle:**

There is NO reason to require the builder pattern for "complex objects" in C#. Object initialization can address all scenarios. The builder pattern is purely a stylistic choice, not a technical necessity.

### Python Implementation

```python
# Builder pattern
class VCardBuilder:
    def __init__(self):
        self.vcard = VCardObject()

    def version(self, version: str) -> 'VCardBuilder':
        self.vcard.version = version
        return self

    def formatted_name(self, name: str) -> 'VCardBuilder':
        self.vcard.formatted_name = name
        return self

    def telephone(self, number: str, types: List[TelType]) -> 'VCardBuilder':
        prop = VCardProperty("TEL", number)
        for tel_type in types:
            prop.add_parameter("TYPE", tel_type.value)
        self.vcard.add_property(prop)
        return self

    def build(self) -> VCardObject:
        return self.vcard

# Usage:
vcard = (VCardBuilder()
    .version("4.0")
    .formatted_name("John Doe")
    .telephone("+1-555-555-1234", [TelType.WORK, TelType.VOICE])
    .build())
```

## Consequences

### Positive

1. **Type safety**: Compile-time checking prevents common errors
2. **Discoverability**: IDE autocomplete shows available properties and methods
3. **Cleaner code**: Fluent API reduces boilerplate
4. **Validation**: Can enforce property format requirements at construction time
5. **Documentation**: Type signatures serve as inline documentation
6. **Refactoring safety**: Changes to property names caught by compiler

### Negative

1. **Implementation complexity**: More code to write and maintain
2. **Learning curve**: Two ways to do the same thing (generic vs. typed)
3. **Maintenance burden**: Must update typed API when RFC evolves
4. **Not exhaustive**: Cannot cover all possible vCard extensions

### Mitigation

- Keep generic property API for extension properties
- Document when to use typed vs. generic approach
- Provide migration examples in documentation
- Start with most common properties (FN, N, EMAIL, TEL, ADR, ORG)

## Examples

### Before (Generic API)
```rust
let mut vcard = VCardObject::new();
vcard.add_property(VCardProperty::new("VERSION", "4.0"));
vcard.add_property(VCardProperty::new("FN", "John Doe"));

let mut tel = VCardProperty::new("TEL", "+1-555-555-1234");
tel.add_parameter("TYPE", "work");
tel.add_parameter("TYPE", "voice");
vcard.add_property(tel);
```

### After (Strongly Typed API)
```rust
let vcard = VCardObject::builder()
    .version("4.0")
    .formatted_name("John Doe")
    .telephone("+1-555-555-1234", vec![TelType::Work, TelType::Voice])
    .build();
```

## References

- [RFC 6350 - vCard Format Specification](https://www.rfc-editor.org/rfc/rfc6350.html)
- Builder Pattern: https://rust-lang.github.io/api-guidelines/type-safety.html
- Fluent Interface: https://en.wikipedia.org/wiki/Fluent_interface

## Date

2026-01-02
