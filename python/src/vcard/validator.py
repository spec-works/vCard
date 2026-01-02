"""
vCard Validator for RFC 6350
"""

import re
from typing import List
from .dom import VCardObject


class ValidationResult:
    """Result of validation operation"""

    def __init__(self):
        self.errors: List[str] = []
        self.warnings: List[str] = []

    @property
    def is_valid(self) -> bool:
        """Check if validation passed (no errors)"""
        return len(self.errors) == 0

    def add_error(self, error: str):
        """Add an error message"""
        self.errors.append(error)

    def add_warning(self, warning: str):
        """Add a warning message"""
        self.warnings.append(warning)

    def get_summary(self) -> str:
        """Get a summary of validation results"""
        summary = f"Validation Result: {'VALID' if self.is_valid else 'INVALID'}\n"
        summary += f"Errors: {len(self.errors)}\n"
        summary += f"Warnings: {len(self.warnings)}\n"

        if self.errors:
            summary += "\nErrors:\n"
            for error in self.errors:
                summary += f"  - {error}\n"

        if self.warnings:
            summary += "\nWarnings:\n"
            for warning in self.warnings:
                summary += f"  - {warning}\n"

        return summary


class VCardValidator:
    """Validator for vCard components according to RFC 6350"""

    def validate(self, vcard: VCardObject) -> ValidationResult:
        """Validate a vCard object"""
        result = ValidationResult()
        self._validate_vcard(vcard, result)
        return result

    def _validate_vcard(self, vcard: VCardObject, result: ValidationResult):
        """Validate vCard properties"""
        # VCARD MUST have VERSION and FN
        self._validate_required_property(vcard, "VERSION", result)
        self._validate_required_property(vcard, "FN", result)

        # VERSION must be 4.0 (or 3.0, 2.1 for backward compatibility)
        version = vcard.get_property("VERSION")
        if version:
            valid_versions = ["4.0", "3.0", "2.1"]
            if version.value not in valid_versions:
                result.add_error(
                    f"VERSION must be one of: {', '.join(valid_versions)}, found: {version.value}"
                )
            elif version.value != "4.0":
                result.add_warning(
                    f"VERSION {version.value} is supported but deprecated. Consider upgrading to 4.0"
                )

        # FN (Formatted Name) must not be empty
        fn = vcard.get_property("FN")
        if fn and not fn.value.strip():
            result.add_error("FN (Formatted Name) cannot be empty")

        # Validate telephones
        for tel in vcard.telephones:
            self._validate_telephone(tel, result)

        # Validate emails
        for email in vcard.emails:
            self._validate_email(email, result)

        # Validate addresses
        for adr in vcard.addresses:
            self._validate_address(adr, result)

        # Validate URLs
        for url in vcard.urls:
            self._validate_url(url, result)

    def _validate_required_property(self, vcard: VCardObject, property_name: str, result: ValidationResult):
        """Validate that a required property is present"""
        prop = vcard.get_property(property_name)
        if not prop:
            result.add_error(f"Required property {property_name} is missing")

    def _validate_telephone(self, prop, result: ValidationResult):
        """Validate a telephone property"""
        if not prop.value.strip():
            result.add_error("TEL property cannot be empty")

        # Validate TYPE parameter if present
        types = prop.get_parameters("TYPE")
        valid_types = ["work", "home", "text", "voice", "fax", "cell", "video", "pager", "textphone"]
        for type_val in types:
            if type_val.lower() not in valid_types:
                result.add_warning(f"TEL TYPE parameter has non-standard value: {type_val}")

    def _validate_email(self, prop, result: ValidationResult):
        """Validate an email property"""
        if not prop.value.strip():
            result.add_error("EMAIL property cannot be empty")
            return

        # Basic email validation
        email_pattern = r'^[^@\s]+@[^@\s]+\.[^@\s]+$'
        if not re.match(email_pattern, prop.value):
            result.add_warning(f"EMAIL property may not be a valid email address: {prop.value}")

    def _validate_address(self, prop, result: ValidationResult):
        """Validate an address property"""
        # ADR property format: POBox;Extended;Street;Locality;Region;PostalCode;Country
        parts = prop.value.split(';')
        if len(parts) != 7:
            result.add_warning(
                f"ADR property should have exactly 7 components, found {len(parts)}: {prop.value}"
            )

    def _validate_url(self, prop, result: ValidationResult):
        """Validate a URL property"""
        if not prop.value.strip():
            result.add_error("URL property cannot be empty")
            return

        # Basic URL validation
        if not prop.value.startswith(('http://', 'https://', 'ftp://')):
            result.add_warning(f"URL property may not be a valid URL: {prop.value}")
