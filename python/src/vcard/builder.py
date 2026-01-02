"""
Strongly Typed Builder API for vCard
"""

from enum import Enum
from typing import List
from .dom import VCardObject, VCardProperty


# ============================================================================
# Type-Safe Enums for Property Parameters
# ============================================================================

class TelType(Enum):
    """Telephone type parameter values"""
    TEXT = "text"
    VOICE = "voice"
    FAX = "fax"
    CELL = "cell"
    VIDEO = "video"
    PAGER = "pager"
    TEXTPHONE = "textphone"
    WORK = "work"
    HOME = "home"


class EmailType(Enum):
    """Email type parameter values"""
    WORK = "work"
    HOME = "home"
    INTERNET = "internet"


class AdrType(Enum):
    """Address type parameter values"""
    WORK = "work"
    HOME = "home"
    POSTAL = "postal"
    PARCEL = "parcel"
    DOM = "dom"
    INTL = "intl"


# ============================================================================
# Fluent Builder for VCardObject
# ============================================================================

class VCardBuilder:
    """
    Fluent builder for creating vCard objects with a type-safe API

    Example:
        vcard = (VCardBuilder()
            .with_version("4.0")
            .with_formatted_name("John Doe")
            .with_telephone("+1-555-1234", [TelType.WORK, TelType.VOICE])
            .with_email("john@example.com", [EmailType.WORK])
            .build())
    """

    def __init__(self):
        self._vcard = VCardObject()

    def with_version(self, version: str) -> 'VCardBuilder':
        """Set the vCard version (typically "4.0")"""
        self._vcard.version = version
        return self

    def with_formatted_name(self, name: str) -> 'VCardBuilder':
        """Set the formatted name (FN) - required property"""
        self._vcard.formatted_name = name
        return self

    def with_name(self, name: str) -> 'VCardBuilder':
        """Set the structured name (N) as semicolon-separated string"""
        self._vcard.name = name
        return self

    def with_name_parts(
        self,
        family_name: str,
        given_name: str,
        additional_names: str = "",
        honorific_prefixes: str = "",
        honorific_suffixes: str = ""
    ) -> 'VCardBuilder':
        """Set the structured name with separate components"""
        name_value = f"{family_name};{given_name};{additional_names};{honorific_prefixes};{honorific_suffixes}"
        self._vcard.name = name_value
        return self

    def with_telephone(self, number: str, types: List[TelType] = None) -> 'VCardBuilder':
        """Add a telephone number with type-safe parameters"""
        prop = VCardProperty("TEL", number)
        if types:
            for tel_type in types:
                prop.add_parameter("TYPE", tel_type.value)
        self._vcard.add_property(prop)
        return self

    def with_email(self, email: str, types: List[EmailType] = None) -> 'VCardBuilder':
        """Add an email address with type-safe parameters"""
        prop = VCardProperty("EMAIL", email)
        if types:
            for email_type in types:
                prop.add_parameter("TYPE", email_type.value)
        self._vcard.add_property(prop)
        return self

    def with_address(self, address: str, types: List[AdrType] = None) -> 'VCardBuilder':
        """Add a delivery address as semicolon-separated string"""
        prop = VCardProperty("ADR", address)
        if types:
            for adr_type in types:
                prop.add_parameter("TYPE", adr_type.value)
        self._vcard.add_property(prop)
        return self

    def with_address_parts(
        self,
        po_box: str,
        extended_address: str,
        street: str,
        locality: str,
        region: str,
        postal_code: str,
        country: str,
        types: List[AdrType] = None
    ) -> 'VCardBuilder':
        """Add a delivery address with separate components"""
        address_value = f"{po_box};{extended_address};{street};{locality};{region};{postal_code};{country}"
        return self.with_address(address_value, types)

    def with_organization(self, organization: str) -> 'VCardBuilder':
        """Set the organization (ORG)"""
        self._vcard.organization = organization
        return self

    def with_title(self, title: str) -> 'VCardBuilder':
        """Set the job title (TITLE)"""
        self._vcard.title = title
        return self

    def with_role(self, role: str) -> 'VCardBuilder':
        """Set the role (ROLE)"""
        self._vcard.role = role
        return self

    def with_nickname(self, nickname: str) -> 'VCardBuilder':
        """Set the nickname (NICKNAME)"""
        self._vcard.nickname = nickname
        return self

    def with_photo(self, uri: str) -> 'VCardBuilder':
        """Set the photo URI (PHOTO)"""
        self._vcard.photo = uri
        return self

    def with_birthday(self, date: str) -> 'VCardBuilder':
        """Set the birthday (BDAY) - format: YYYYMMDD or YYYY-MM-DD"""
        self._vcard.birthday = date
        return self

    def with_anniversary(self, date: str) -> 'VCardBuilder':
        """Set the anniversary (ANNIVERSARY) - format: YYYYMMDD or YYYY-MM-DD"""
        self._vcard.anniversary = date
        return self

    def with_gender(self, gender: str) -> 'VCardBuilder':
        """Set the gender (GENDER)"""
        self._vcard.gender = gender
        return self

    def with_url(self, url: str) -> 'VCardBuilder':
        """Add a URL"""
        self._vcard.add_property(VCardProperty("URL", url))
        return self

    def with_note(self, note: str) -> 'VCardBuilder':
        """Add a note (NOTE)"""
        self._vcard.add_property(VCardProperty("NOTE", note))
        return self

    def with_uid(self, uid: str) -> 'VCardBuilder':
        """Set the unique identifier (UID)"""
        self._vcard.uid = uid
        return self

    def with_categories(self, categories: str) -> 'VCardBuilder':
        """Add categories (CATEGORIES)"""
        self._vcard.add_property(VCardProperty("CATEGORIES", categories))
        return self

    def with_revision(self, revision: str) -> 'VCardBuilder':
        """Set the revision date/time (REV)"""
        self._vcard.revision = revision
        return self

    def with_time_zone(self, time_zone: str) -> 'VCardBuilder':
        """Set the time zone (TZ)"""
        self._vcard.time_zone = time_zone
        return self

    def with_geo(self, geo: str) -> 'VCardBuilder':
        """Set the geographic position (GEO)"""
        self._vcard.geo = geo
        return self

    def with_custom_property(self, name: str, value: str) -> 'VCardBuilder':
        """Add a custom property (for extension properties not covered by typed methods)"""
        self._vcard.add_property(VCardProperty(name, value))
        return self

    def build(self) -> VCardObject:
        """Build and return the vCard object"""
        return self._vcard
