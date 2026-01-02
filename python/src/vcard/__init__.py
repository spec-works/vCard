"""
vCard Parser Library
A Python library for parsing and generating vCard (RFC 6350) data.
"""

from .parser import VCardParser, ParseException
from .dom import VCardObject, VCardProperty, StructuredName, StructuredAddress
from .serializer import VCardSerializer
from .validator import VCardValidator, ValidationResult
from .builder import VCardBuilder, TelType, EmailType, AdrType

__version__ = "1.0.0"
__all__ = [
    "VCardParser",
    "ParseException",
    "VCardObject",
    "VCardProperty",
    "StructuredName",
    "StructuredAddress",
    "VCardSerializer",
    "VCardValidator",
    "ValidationResult",
    "VCardBuilder",
    "TelType",
    "EmailType",
    "AdrType",
]
