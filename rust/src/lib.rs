//! # vCard Parser Library
//!
//! A Rust library for parsing and generating vCard (RFC 6350) data.
//!
//! ## Example
//!
//! ```
//! use vcard::{VCardParser, VCardObject, VCardProperty};
//!
//! let vcard_data = "BEGIN:VCARD\nVERSION:4.0\nFN:John Doe\nEND:VCARD";
//! let mut parser = VCardParser::new();
//! let vcards = parser.parse(vcard_data).unwrap();
//! let vcard = &vcards[0];
//!
//! assert_eq!(vcard.get_property("FN").unwrap().value, "John Doe");
//! ```

use std::collections::HashMap;
use std::error::Error;
use std::fmt;

/// Parse error type
#[derive(Debug, Clone)]
pub struct ParseError {
    message: String,
}

impl fmt::Display for ParseError {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(f, "Parse error: {}", self.message)
    }
}

impl Error for ParseError {}

impl ParseError {
    fn new(message: impl Into<String>) -> Self {
        ParseError {
            message: message.into(),
        }
    }
}

/// Represents a vCard property with parameters and value
#[derive(Debug, Clone, PartialEq)]
pub struct VCardProperty {
    pub name: String,
    pub value: String,
    pub parameters: HashMap<String, Vec<String>>,
}

impl VCardProperty {
    /// Create a new VCardProperty
    pub fn new(name: impl Into<String>, value: impl Into<String>) -> Self {
        VCardProperty {
            name: name.into().to_uppercase(),
            value: value.into(),
            parameters: HashMap::new(),
        }
    }

    /// Add a parameter to this property
    pub fn add_parameter(&mut self, param_name: impl Into<String>, param_value: impl Into<String>) {
        let param_name = param_name.into().to_uppercase();
        self.parameters
            .entry(param_name)
            .or_insert_with(Vec::new)
            .push(param_value.into());
    }

    /// Get the first value of a parameter
    pub fn get_parameter(&self, param_name: &str) -> Option<&String> {
        self.parameters
            .get(&param_name.to_uppercase())
            .and_then(|v| v.first())
    }

    /// Get all values of a parameter
    pub fn get_parameters(&self, param_name: &str) -> Option<&Vec<String>> {
        self.parameters.get(&param_name.to_uppercase())
    }
}

/// Root vCard object
#[derive(Debug, Clone, PartialEq)]
pub struct VCardObject {
    pub properties: HashMap<String, Vec<VCardProperty>>,
}

impl VCardObject {
    /// Create a new VCardObject
    pub fn new() -> Self {
        VCardObject {
            properties: HashMap::new(),
        }
    }

    /// Add a property to this vCard
    pub fn add_property(&mut self, property: VCardProperty) {
        self.properties
            .entry(property.name.clone())
            .or_insert_with(Vec::new)
            .push(property);
    }

    /// Get the first property with the given name
    pub fn get_property(&self, name: &str) -> Option<&VCardProperty> {
        self.properties
            .get(&name.to_uppercase())
            .and_then(|v| v.first())
    }

    /// Get all properties with the given name
    pub fn get_properties(&self, name: &str) -> Option<&Vec<VCardProperty>> {
        self.properties.get(&name.to_uppercase())
    }

    /// Get the version
    pub fn version(&self) -> Option<&str> {
        self.get_property("VERSION").map(|p| p.value.as_str())
    }

    /// Get the formatted name
    pub fn formatted_name(&self) -> Option<&str> {
        self.get_property("FN").map(|p| p.value.as_str())
    }

    /// Get the structured name
    pub fn name(&self) -> Option<&str> {
        self.get_property("N").map(|p| p.value.as_str())
    }

    /// Get the organization
    pub fn organization(&self) -> Option<&str> {
        self.get_property("ORG").map(|p| p.value.as_str())
    }

    /// Get the title
    pub fn title(&self) -> Option<&str> {
        self.get_property("TITLE").map(|p| p.value.as_str())
    }

    /// Get all telephone properties
    pub fn telephones(&self) -> Option<&Vec<VCardProperty>> {
        self.get_properties("TEL")
    }

    /// Get all email properties
    pub fn emails(&self) -> Option<&Vec<VCardProperty>> {
        self.get_properties("EMAIL")
    }

    /// Get all address properties
    pub fn addresses(&self) -> Option<&Vec<VCardProperty>> {
        self.get_properties("ADR")
    }
}

impl Default for VCardObject {
    fn default() -> Self {
        Self::new()
    }
}

/// Parser for vCard format
pub struct VCardParser {
    lines: Vec<String>,
    current_line: usize,
}

impl VCardParser {
    /// Create a new VCardParser
    pub fn new() -> Self {
        VCardParser {
            lines: Vec::new(),
            current_line: 0,
        }
    }

    /// Parse vCards from a string (returns all vCards found)
    pub fn parse(&mut self, vcard_text: &str) -> Result<Vec<VCardObject>, ParseError> {
        self.lines = self.unfold_lines(vcard_text);
        self.current_line = 0;

        let mut vcards = Vec::new();

        while self.current_line < self.lines.len() {
            // Skip empty lines
            while self.current_line < self.lines.len() {
                let line = &self.lines[self.current_line];
                if line.trim().is_empty() {
                    self.current_line += 1;
                } else {
                    break;
                }
            }

            if self.current_line >= self.lines.len() {
                break;
            }

            let line = &self.lines[self.current_line];
            if line.eq_ignore_ascii_case("BEGIN:VCARD") {
                self.current_line += 1;
                let mut vcard = VCardObject::new();
                self.parse_component(&mut vcard)?;
                vcards.push(vcard);
            } else {
                return Err(ParseError::new(format!(
                    "Expected BEGIN:VCARD but got: {}",
                    line
                )));
            }
        }

        if vcards.is_empty() {
            return Err(ParseError::new("No vCard data found"));
        }

        Ok(vcards)
    }

    fn unfold_lines(&self, vcard_text: &str) -> Vec<String> {
        let mut unfolded_lines = Vec::new();
        let lines: Vec<&str> = vcard_text.lines().collect();

        let mut current_line = String::new();

        for line in lines {
            if line.starts_with(' ') || line.starts_with('\t') {
                // Continuation line - remove leading whitespace and append
                current_line.push_str(&line[1..]);
            } else {
                if !current_line.is_empty() {
                    let trimmed = current_line.trim();
                    if !trimmed.is_empty() {
                        unfolded_lines.push(trimmed.to_string());
                    }
                }
                current_line = line.to_string();
            }
        }

        if !current_line.is_empty() {
            let trimmed = current_line.trim();
            if !trimmed.is_empty() {
                unfolded_lines.push(trimmed.to_string());
            }
        }

        unfolded_lines
    }

    fn parse_component(&mut self, vcard: &mut VCardObject) -> Result<(), ParseError> {
        while self.current_line < self.lines.len() {
            let line = &self.lines[self.current_line].clone();

            if line.to_uppercase().starts_with("END:") {
                let end_type = &line[4..].to_uppercase();
                if end_type != "VCARD" {
                    return Err(ParseError::new(format!(
                        "Mismatched END tag: expected END:VCARD but got END:{}",
                        end_type
                    )));
                }
                self.current_line += 1;

                // Validate required properties
                self.validate_vcard(vcard)?;

                return Ok(());
            } else {
                let property = self.parse_property(line)?;
                vcard.add_property(property);
                self.current_line += 1;
            }
        }

        Err(ParseError::new("Unexpected end of input while parsing VCARD"))
    }

    fn parse_property(&self, line: &str) -> Result<VCardProperty, ParseError> {
        let colon_index = self.find_unquoted_char(line, ':')
            .ok_or_else(|| ParseError::new(format!("Invalid property line (missing colon): {}", line)))?;

        let name_and_params = &line[..colon_index];
        let value = &line[colon_index + 1..];

        // Unescape value
        let value = self.unescape_value(value);

        // Parse name and parameters
        let (property_name, params_part) = if let Some(semicolon_index) = self.find_unquoted_char(name_and_params, ';') {
            (
                name_and_params[..semicolon_index].to_uppercase(),
                Some(&name_and_params[semicolon_index + 1..]),
            )
        } else {
            (name_and_params.to_uppercase(), None)
        };

        let mut property = VCardProperty::new(property_name, value);

        if let Some(params) = params_part {
            self.parse_parameters(params, &mut property)?;
        }

        Ok(property)
    }

    fn parse_parameters(&self, params_part: &str, property: &mut VCardProperty) -> Result<(), ParseError> {
        let parameters = self.split_parameters(params_part);

        for param in parameters {
            let equals_index = param.find('=')
                .ok_or_else(|| ParseError::new(format!("Invalid parameter (missing equals): {}", param)))?;

            let param_name = param[..equals_index].to_uppercase();
            let mut param_value = param[equals_index + 1..].to_string();

            // Remove quotes if present
            if param_value.starts_with('"') && param_value.ends_with('"') && param_value.len() >= 2 {
                param_value = param_value[1..param_value.len() - 1].to_string();
            }

            // Handle comma-separated values
            let values = self.split_parameter_values(&param_value);
            for value in values {
                property.add_parameter(param_name.clone(), value);
            }
        }

        Ok(())
    }

    fn split_parameters(&self, params_part: &str) -> Vec<String> {
        let mut parameters = Vec::new();
        let mut current = String::new();
        let mut in_quotes = false;

        for c in params_part.chars() {
            if c == '"' {
                in_quotes = !in_quotes;
                current.push(c);
            } else if c == ';' && !in_quotes {
                parameters.push(current.clone());
                current.clear();
            } else {
                current.push(c);
            }
        }

        if !current.is_empty() {
            parameters.push(current);
        }

        parameters
    }

    fn split_parameter_values(&self, param_value: &str) -> Vec<String> {
        let mut values = Vec::new();
        let mut current = String::new();
        let mut in_quotes = false;

        for c in param_value.chars() {
            if c == '"' {
                in_quotes = !in_quotes;
            } else if c == ',' && !in_quotes {
                values.push(current.clone());
                current.clear();
            } else {
                current.push(c);
            }
        }

        if !current.is_empty() {
            values.push(current);
        }

        values
    }

    fn find_unquoted_char(&self, s: &str, target: char) -> Option<usize> {
        let mut in_quotes = false;
        for (i, c) in s.chars().enumerate() {
            if c == '"' {
                in_quotes = !in_quotes;
            } else if c == target && !in_quotes {
                return Some(i);
            }
        }
        None
    }

    fn unescape_value(&self, value: &str) -> String {
        value
            .replace("\\n", "\n")
            .replace("\\N", "\n")
            .replace("\\;", ";")
            .replace("\\,", ",")
            .replace("\\\\", "\\")
    }

    fn validate_vcard(&self, vcard: &VCardObject) -> Result<(), ParseError> {
        // VERSION is required (RFC 6350 Section 6.7.9)
        if vcard.version().is_none() {
            return Err(ParseError::new(
                "Missing required VERSION property (RFC 6350 Section 6.7.9). vCard must include VERSION:4.0".to_string()
            ));
        }

        // Only version 4.0 is supported (per ADR 0004)
        if let Some(version) = vcard.version() {
            if version != "4.0" {
                return Err(ParseError::new(
                    format!("Unsupported vCard version: {}. Only version 4.0 is supported (see ADR 0004).", version)
                ));
            }
        }

        // FN (formatted name) is required (RFC 6350 Section 6.2.1)
        if vcard.formatted_name().is_none() {
            return Err(ParseError::new(
                "Missing required FN (Formatted Name) property (RFC 6350 Section 6.2.1). vCard must include FN property.".to_string()
            ));
        }

        Ok(())
    }
}

impl Default for VCardParser {
    fn default() -> Self {
        Self::new()
    }
}

// ============================================================================
// Strongly Typed API
// ============================================================================

/// Telephone type parameter values
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum TelType {
    /// Text telephone
    Text,
    /// Voice telephone
    Voice,
    /// Fax number
    Fax,
    /// Cell phone
    Cell,
    /// Video conference
    Video,
    /// Pager
    Pager,
    /// Text phone (TTY)
    TextPhone,
    /// Work telephone
    Work,
    /// Home telephone
    Home,
}

impl TelType {
    /// Convert to string representation for vCard
    pub fn as_str(&self) -> &str {
        match self {
            TelType::Text => "text",
            TelType::Voice => "voice",
            TelType::Fax => "fax",
            TelType::Cell => "cell",
            TelType::Video => "video",
            TelType::Pager => "pager",
            TelType::TextPhone => "textphone",
            TelType::Work => "work",
            TelType::Home => "home",
        }
    }
}

/// Email type parameter values
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum EmailType {
    /// Work email
    Work,
    /// Home email
    Home,
    /// Internet email
    Internet,
}

impl EmailType {
    /// Convert to string representation for vCard
    pub fn as_str(&self) -> &str {
        match self {
            EmailType::Work => "work",
            EmailType::Home => "home",
            EmailType::Internet => "internet",
        }
    }
}

/// Address type parameter values
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum AdrType {
    /// Work address
    Work,
    /// Home address
    Home,
    /// Postal address
    Postal,
    /// Parcel delivery address
    Parcel,
    /// Domestic address
    Dom,
    /// International address
    Intl,
}

impl AdrType {
    /// Convert to string representation for vCard
    pub fn as_str(&self) -> &str {
        match self {
            AdrType::Work => "work",
            AdrType::Home => "home",
            AdrType::Postal => "postal",
            AdrType::Parcel => "parcel",
            AdrType::Dom => "dom",
            AdrType::Intl => "intl",
        }
    }
}

/// Builder for creating vCard objects with a fluent, type-safe API
pub struct VCardBuilder {
    vcard: VCardObject,
}

impl VCardBuilder {
    /// Create a new VCardBuilder
    pub fn new() -> Self {
        VCardBuilder {
            vcard: VCardObject::new(),
        }
    }

    /// Set the vCard version (typically "4.0")
    pub fn version(mut self, version: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("VERSION", version));
        self
    }

    /// Set the formatted name (FN) - required property
    pub fn formatted_name(mut self, name: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("FN", name));
        self
    }

    /// Set the structured name (N) - family;given;additional;prefix;suffix
    pub fn name(mut self, name: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("N", name));
        self
    }

    /// Set the structured name with separate components
    pub fn name_parts(
        mut self,
        family: &str,
        given: &str,
        additional: &str,
        prefix: &str,
        suffix: &str,
    ) -> Self {
        let name_value = format!("{};{};{};{};{}", family, given, additional, prefix, suffix);
        self.vcard.add_property(VCardProperty::new("N", name_value));
        self
    }

    /// Add a telephone number with type parameters
    pub fn telephone(mut self, number: impl Into<String>, types: Vec<TelType>) -> Self {
        let mut prop = VCardProperty::new("TEL", number);
        for tel_type in types {
            prop.add_parameter("TYPE", tel_type.as_str());
        }
        self.vcard.add_property(prop);
        self
    }

    /// Add an email address with type parameters
    pub fn email(mut self, email: impl Into<String>, types: Vec<EmailType>) -> Self {
        let mut prop = VCardProperty::new("EMAIL", email);
        for email_type in types {
            prop.add_parameter("TYPE", email_type.as_str());
        }
        self.vcard.add_property(prop);
        self
    }

    /// Add a delivery address with type parameters
    /// Components: po_box;extended;street;locality;region;postal_code;country
    pub fn address(mut self, address: impl Into<String>, types: Vec<AdrType>) -> Self {
        let mut prop = VCardProperty::new("ADR", address);
        for adr_type in types {
            prop.add_parameter("TYPE", adr_type.as_str());
        }
        self.vcard.add_property(prop);
        self
    }

    /// Add a delivery address with separate components
    pub fn address_parts(
        mut self,
        po_box: &str,
        extended: &str,
        street: &str,
        locality: &str,
        region: &str,
        postal_code: &str,
        country: &str,
        types: Vec<AdrType>,
    ) -> Self {
        let address_value = format!(
            "{};{};{};{};{};{};{}",
            po_box, extended, street, locality, region, postal_code, country
        );
        let mut prop = VCardProperty::new("ADR", address_value);
        for adr_type in types {
            prop.add_parameter("TYPE", adr_type.as_str());
        }
        self.vcard.add_property(prop);
        self
    }

    /// Set the organization (ORG)
    pub fn organization(mut self, org: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("ORG", org));
        self
    }

    /// Set the job title (TITLE)
    pub fn title(mut self, title: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("TITLE", title));
        self
    }

    /// Set the role (ROLE)
    pub fn role(mut self, role: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("ROLE", role));
        self
    }

    /// Set the nickname (NICKNAME)
    pub fn nickname(mut self, nickname: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("NICKNAME", nickname));
        self
    }

    /// Set the photo URI (PHOTO)
    pub fn photo(mut self, uri: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("PHOTO", uri));
        self
    }

    /// Set the birthday (BDAY) - format: YYYYMMDD or YYYY-MM-DD
    pub fn birthday(mut self, date: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("BDAY", date));
        self
    }

    /// Set the anniversary (ANNIVERSARY) - format: YYYYMMDD or YYYY-MM-DD
    pub fn anniversary(mut self, date: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("ANNIVERSARY", date));
        self
    }

    /// Set the gender (GENDER)
    pub fn gender(mut self, gender: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("GENDER", gender));
        self
    }

    /// Set the URL (URL)
    pub fn url(mut self, url: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("URL", url));
        self
    }

    /// Set the note (NOTE)
    pub fn note(mut self, note: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("NOTE", note));
        self
    }

    /// Set the unique identifier (UID)
    pub fn uid(mut self, uid: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("UID", uid));
        self
    }

    /// Set the categories (CATEGORIES)
    pub fn categories(mut self, categories: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("CATEGORIES", categories));
        self
    }

    /// Set the revision date/time (REV)
    pub fn revision(mut self, rev: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new("REV", rev));
        self
    }

    /// Add a custom property (for extension properties not covered by typed methods)
    pub fn custom_property(mut self, name: impl Into<String>, value: impl Into<String>) -> Self {
        self.vcard.add_property(VCardProperty::new(name, value));
        self
    }

    /// Build and return the vCard object
    pub fn build(self) -> VCardObject {
        self.vcard
    }
}

impl Default for VCardBuilder {
    fn default() -> Self {
        Self::new()
    }
}

impl VCardObject {
    /// Create a builder for constructing a vCard with a fluent API
    pub fn builder() -> VCardBuilder {
        VCardBuilder::new()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_parse_simple_vcard() {
        let vcard_data = "BEGIN:VCARD\nVERSION:4.0\nFN:John Doe\nEND:VCARD";
        let mut parser = VCardParser::new();
        let vcards = parser.parse(vcard_data).unwrap();

        assert_eq!(vcards.len(), 1);
        let vcard = &vcards[0];
        assert_eq!(vcard.version(), Some("4.0"));
        assert_eq!(vcard.formatted_name(), Some("John Doe"));
    }

    #[test]
    fn test_parse_vcard_with_properties() {
        let vcard_data = "BEGIN:VCARD\nVERSION:4.0\nFN:John Doe\nN:Doe;John;;;\nORG:ABC Corp\nTITLE:Engineer\nEND:VCARD";
        let mut parser = VCardParser::new();
        let vcards = parser.parse(vcard_data).unwrap();

        assert_eq!(vcards.len(), 1);
        let vcard = &vcards[0];
        assert_eq!(vcard.formatted_name(), Some("John Doe"));
        assert_eq!(vcard.name(), Some("Doe;John;;;"));
        assert_eq!(vcard.organization(), Some("ABC Corp"));
        assert_eq!(vcard.title(), Some("Engineer"));
    }

    #[test]
    fn test_parse_vcard_with_parameters() {
        let vcard_data = "BEGIN:VCARD\nVERSION:4.0\nFN:John Doe\nTEL;TYPE=work:+1-555-555-1234\nEND:VCARD";
        let mut parser = VCardParser::new();
        let vcards = parser.parse(vcard_data).unwrap();

        assert_eq!(vcards.len(), 1);
        let vcard = &vcards[0];
        let tel = vcard.get_property("TEL").unwrap();
        assert_eq!(tel.value, "+1-555-555-1234");
        assert_eq!(tel.get_parameter("TYPE"), Some(&"work".to_string()));
    }

    #[test]
    fn test_parse_vcard_with_multiple_properties() {
        let vcard_data = "BEGIN:VCARD\nVERSION:4.0\nFN:John Doe\nTEL;TYPE=work:+1-555-555-1234\nTEL;TYPE=home:555-555-5678\nEND:VCARD";
        let mut parser = VCardParser::new();
        let vcards = parser.parse(vcard_data).unwrap();

        assert_eq!(vcards.len(), 1);
        let vcard = &vcards[0];
        let telephones = vcard.telephones().unwrap();
        assert_eq!(telephones.len(), 2);
        assert_eq!(telephones[0].value, "+1-555-555-1234");
        assert_eq!(telephones[1].value, "555-555-5678");
    }

    #[test]
    fn test_new_vcard_object() {
        let mut vcard = VCardObject::new();
        vcard.add_property(VCardProperty::new("VERSION", "4.0"));
        vcard.add_property(VCardProperty::new("FN", "Jane Smith"));

        assert_eq!(vcard.version(), Some("4.0"));
        assert_eq!(vcard.formatted_name(), Some("Jane Smith"));
    }

    #[test]
    fn test_parse_multiple_vcards() {
        let vcard_data = "BEGIN:VCARD\nVERSION:4.0\nFN:John Doe\nEND:VCARD\n\nBEGIN:VCARD\nVERSION:4.0\nFN:Jane Smith\nEND:VCARD";
        let mut parser = VCardParser::new();
        let vcards = parser.parse(vcard_data).unwrap();

        assert_eq!(vcards.len(), 2);
        assert_eq!(vcards[0].formatted_name(), Some("John Doe"));
        assert_eq!(vcards[1].formatted_name(), Some("Jane Smith"));
    }

    // Tests for strongly typed API

    #[test]
    fn test_builder_basic() {
        let vcard = VCardObject::builder()
            .version("4.0")
            .formatted_name("John Doe")
            .build();

        assert_eq!(vcard.version(), Some("4.0"));
        assert_eq!(vcard.formatted_name(), Some("John Doe"));
    }

    #[test]
    fn test_builder_with_telephone() {
        let vcard = VCardObject::builder()
            .version("4.0")
            .formatted_name("John Doe")
            .telephone("+1-555-555-1234", vec![TelType::Work, TelType::Voice])
            .build();

        let tel = vcard.get_property("TEL").unwrap();
        assert_eq!(tel.value, "+1-555-555-1234");

        let types = tel.get_parameters("TYPE").unwrap();
        assert_eq!(types.len(), 2);
        assert!(types.contains(&"work".to_string()));
        assert!(types.contains(&"voice".to_string()));
    }

    #[test]
    fn test_builder_with_email() {
        let vcard = VCardObject::builder()
            .version("4.0")
            .formatted_name("John Doe")
            .email("john@example.com", vec![EmailType::Work])
            .build();

        let email = vcard.get_property("EMAIL").unwrap();
        assert_eq!(email.value, "john@example.com");
        assert_eq!(email.get_parameter("TYPE"), Some(&"work".to_string()));
    }

    #[test]
    fn test_builder_with_address_parts() {
        let vcard = VCardObject::builder()
            .version("4.0")
            .formatted_name("John Doe")
            .address_parts(
                "",                     // PO Box
                "",                     // Extended
                "123 Main Street",      // Street
                "Springfield",          // Locality
                "IL",                   // Region
                "62701",                // Postal Code
                "USA",                  // Country
                vec![AdrType::Work],
            )
            .build();

        let adr = vcard.get_property("ADR").unwrap();
        assert_eq!(adr.value, ";;123 Main Street;Springfield;IL;62701;USA");
        assert_eq!(adr.get_parameter("TYPE"), Some(&"work".to_string()));
    }

    #[test]
    fn test_builder_complete_vcard() {
        let vcard = VCardObject::builder()
            .version("4.0")
            .formatted_name("John Michael Doe")
            .name_parts("Doe", "John", "Michael", "Mr.", "Jr.")
            .nickname("Johnny")
            .organization("ABC Corporation")
            .title("Software Engineer")
            .telephone("+1-555-555-1234", vec![TelType::Work, TelType::Voice])
            .telephone("555-555-5678", vec![TelType::Home])
            .email("john@example.com", vec![EmailType::Work])
            .url("https://www.example.com")
            .birthday("19850415")
            .note("Important contact")
            .categories("Work,VIP")
            .build();

        assert_eq!(vcard.formatted_name(), Some("John Michael Doe"));
        assert_eq!(vcard.name(), Some("Doe;John;Michael;Mr.;Jr."));
        assert_eq!(vcard.organization(), Some("ABC Corporation"));
        assert_eq!(vcard.title(), Some("Software Engineer"));

        let telephones = vcard.telephones().unwrap();
        assert_eq!(telephones.len(), 2);
        assert_eq!(telephones[0].value, "+1-555-555-1234");
        assert_eq!(telephones[1].value, "555-555-5678");
    }

    #[test]
    fn test_builder_with_custom_property() {
        let vcard = VCardObject::builder()
            .version("4.0")
            .formatted_name("John Doe")
            .custom_property("X-CUSTOM", "Custom Value")
            .build();

        let custom = vcard.get_property("X-CUSTOM").unwrap();
        assert_eq!(custom.value, "Custom Value");
    }
}
