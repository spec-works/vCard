"""
Document Object Model for vCard
"""

from typing import Dict, List, Optional
from dataclasses import dataclass, field


class VCardProperty:
    """Represents a vCard property with parameters and value"""

    def __init__(self, name: str, value: str):
        self.name = name.upper()
        self.value = value
        self.parameters: Dict[str, List[str]] = {}

    def add_parameter(self, param_name: str, param_value: str):
        """Add a parameter to this property"""
        param_name = param_name.upper()
        if param_name not in self.parameters:
            self.parameters[param_name] = []
        self.parameters[param_name].append(param_value)

    def get_parameter(self, param_name: str) -> Optional[str]:
        """Get the first value of a parameter"""
        param_name = param_name.upper()
        values = self.parameters.get(param_name, [])
        return values[0] if values else None

    def get_parameters(self, param_name: str) -> List[str]:
        """Get all values of a parameter"""
        param_name = param_name.upper()
        return self.parameters.get(param_name, [])


class VCardComponent:
    """Base class for vCard components"""

    def __init__(self):
        self.properties: Dict[str, List[VCardProperty]] = {}

    def add_property(self, prop: VCardProperty):
        """Add a property to this component"""
        if prop.name not in self.properties:
            self.properties[prop.name] = []
        self.properties[prop.name].append(prop)

    def get_property(self, name: str) -> Optional[VCardProperty]:
        """Get the first property with the given name"""
        name = name.upper()
        props = self.properties.get(name, [])
        return props[0] if props else None

    def get_properties(self, name: str) -> List[VCardProperty]:
        """Get all properties with the given name"""
        name = name.upper()
        return self.properties.get(name, [])

    @property
    def component_type(self) -> str:
        """Return the component type"""
        raise NotImplementedError


class VCardObject(VCardComponent):
    """Root vCard object (VCARD)"""

    @property
    def component_type(self) -> str:
        return "VCARD"

    # Required properties
    @property
    def version(self) -> Optional[str]:
        prop = self.get_property("VERSION")
        return prop.value if prop else None

    @version.setter
    def version(self, value: str):
        self.add_property(VCardProperty("VERSION", value))

    @property
    def formatted_name(self) -> Optional[str]:
        prop = self.get_property("FN")
        return prop.value if prop else None

    @formatted_name.setter
    def formatted_name(self, value: str):
        self.add_property(VCardProperty("FN", value))

    # Identification properties
    @property
    def name(self) -> Optional[str]:
        prop = self.get_property("N")
        return prop.value if prop else None

    @name.setter
    def name(self, value: str):
        self.add_property(VCardProperty("N", value))

    @property
    def nickname(self) -> Optional[str]:
        prop = self.get_property("NICKNAME")
        return prop.value if prop else None

    @property
    def photo(self) -> Optional[str]:
        prop = self.get_property("PHOTO")
        return prop.value if prop else None

    @property
    def birthday(self) -> Optional[str]:
        prop = self.get_property("BDAY")
        return prop.value if prop else None

    @property
    def organization(self) -> Optional[str]:
        prop = self.get_property("ORG")
        return prop.value if prop else None

    @property
    def title(self) -> Optional[str]:
        prop = self.get_property("TITLE")
        return prop.value if prop else None

    # Communication properties
    @property
    def telephones(self) -> List[VCardProperty]:
        return self.get_properties("TEL")

    @property
    def emails(self) -> List[VCardProperty]:
        return self.get_properties("EMAIL")

    @property
    def addresses(self) -> List[VCardProperty]:
        return self.get_properties("ADR")

    @property
    def urls(self) -> List[VCardProperty]:
        return self.get_properties("URL")


@dataclass
class StructuredName:
    """Represents a structured name value"""
    family_name: str = ""
    given_name: str = ""
    additional_names: str = ""
    honorific_prefixes: str = ""
    honorific_suffixes: str = ""

    @staticmethod
    def parse(value: str) -> 'StructuredName':
        """Parse a structured name from a semicolon-separated string"""
        parts = value.split(';')
        return StructuredName(
            family_name=parts[0] if len(parts) > 0 else "",
            given_name=parts[1] if len(parts) > 1 else "",
            additional_names=parts[2] if len(parts) > 2 else "",
            honorific_prefixes=parts[3] if len(parts) > 3 else "",
            honorific_suffixes=parts[4] if len(parts) > 4 else "",
        )

    def __str__(self) -> str:
        return f"{self.family_name};{self.given_name};{self.additional_names};{self.honorific_prefixes};{self.honorific_suffixes}"


@dataclass
class StructuredAddress:
    """Represents a structured address value"""
    post_office_box: str = ""
    extended_address: str = ""
    street_address: str = ""
    locality: str = ""
    region: str = ""
    postal_code: str = ""
    country_name: str = ""

    @staticmethod
    def parse(value: str) -> 'StructuredAddress':
        """Parse a structured address from a semicolon-separated string"""
        parts = value.split(';')
        return StructuredAddress(
            post_office_box=parts[0] if len(parts) > 0 else "",
            extended_address=parts[1] if len(parts) > 1 else "",
            street_address=parts[2] if len(parts) > 2 else "",
            locality=parts[3] if len(parts) > 3 else "",
            region=parts[4] if len(parts) > 4 else "",
            postal_code=parts[5] if len(parts) > 5 else "",
            country_name=parts[6] if len(parts) > 6 else "",
        )

    def __str__(self) -> str:
        return f"{self.post_office_box};{self.extended_address};{self.street_address};{self.locality};{self.region};{self.postal_code};{self.country_name}"
