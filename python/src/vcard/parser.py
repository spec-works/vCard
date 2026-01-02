"""
vCard Parser for RFC 6350
"""

from typing import List
from .dom import VCardObject, VCardProperty


class ParseException(Exception):
    """Exception raised when parsing fails"""
    pass


class VCardParser:
    """Parser for text/vcard format (RFC 6350)"""

    def __init__(self):
        self._lines: List[str] = []
        self._current_line: int = 0

    def parse(self, vcard_text: str) -> List[VCardObject]:
        """Parse vCards from text (returns all vCards found)"""
        vcards = []
        self._lines = self._unfold_lines(vcard_text)
        self._current_line = 0

        while self._current_line < len(self._lines):
            # Skip empty lines
            while self._current_line < len(self._lines) and not self._lines[self._current_line].strip():
                self._current_line += 1

            if self._current_line >= len(self._lines):
                break

            line = self._lines[self._current_line]
            if line.upper() == "BEGIN:VCARD":
                self._current_line += 1
                vcard = VCardObject()
                self._parse_component(vcard)
                vcards.append(vcard)
            else:
                raise ParseException(f"Expected BEGIN:VCARD but got: {line}")

        if not vcards:
            raise ParseException("No vCard data found")

        return vcards

    def parse_file(self, file_path: str) -> List[VCardObject]:
        """Parse vCards from a file"""
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        return self.parse(content)

    def _unfold_lines(self, vcard_text: str) -> List[str]:
        """Unfold long lines according to RFC 6350"""
        unfolded_lines = []
        lines = vcard_text.replace('\r\n', '\n').split('\n')

        current_line = []

        for line in lines:
            if line and (line[0] == ' ' or line[0] == '\t'):
                # Continuation line - remove leading whitespace and append
                current_line.append(line[1:])
            else:
                if current_line:
                    unfolded_line = ''.join(current_line)
                    if unfolded_line.strip():
                        unfolded_lines.append(unfolded_line)
                current_line = [line]

        if current_line:
            unfolded_line = ''.join(current_line)
            if unfolded_line.strip():
                unfolded_lines.append(unfolded_line)

        return unfolded_lines

    def _parse_component(self, component: VCardObject):
        """Parse properties of a component"""
        while self._current_line < len(self._lines):
            line = self._lines[self._current_line]

            if line.upper().startswith("END:"):
                end_component_type = line[4:].upper()
                if end_component_type != component.component_type:
                    raise ParseException(
                        f"Mismatched END tag: expected END:{component.component_type} but got END:{end_component_type}"
                    )
                self._current_line += 1

                # Validate required properties for vCard
                self._validate_vcard(component)

                return
            else:
                prop = self._parse_property(line)
                component.add_property(prop)
                self._current_line += 1

        raise ParseException(f"Unexpected end of input while parsing {component.component_type}")

    def _parse_property(self, line: str) -> VCardProperty:
        """Parse a property line"""
        colon_index = self._find_unquoted_char(line, ':')
        if colon_index == -1:
            raise ParseException(f"Invalid property line (missing colon): {line}")

        name_and_params = line[:colon_index]
        value = line[colon_index + 1:]

        # Unescape value
        value = self._unescape_value(value)

        # Parse name and parameters
        semicolon_index = self._find_unquoted_char(name_and_params, ';')

        if semicolon_index != -1:
            property_name = name_and_params[:semicolon_index].upper()
            params_part = name_and_params[semicolon_index + 1:]
        else:
            property_name = name_and_params.upper()
            params_part = None

        prop = VCardProperty(property_name, value)

        if params_part:
            self._parse_parameters(params_part, prop)

        return prop

    def _parse_parameters(self, params_part: str, prop: VCardProperty):
        """Parse property parameters"""
        parameters = self._split_parameters(params_part)

        for param in parameters:
            equals_index = param.find('=')
            if equals_index == -1:
                raise ParseException(f"Invalid parameter (missing equals): {param}")

            param_name = param[:equals_index].upper()
            param_value = param[equals_index + 1:]

            # Remove quotes if present
            if param_value.startswith('"') and param_value.endswith('"') and len(param_value) >= 2:
                param_value = param_value[1:-1]

            # Handle comma-separated values
            values = self._split_parameter_values(param_value)
            for value in values:
                prop.add_parameter(param_name, value)

    def _split_parameters(self, params_part: str) -> List[str]:
        """Split parameters by semicolon, respecting quotes"""
        parameters = []
        current = []
        in_quotes = False

        for c in params_part:
            if c == '"':
                in_quotes = not in_quotes
                current.append(c)
            elif c == ';' and not in_quotes:
                parameters.append(''.join(current))
                current = []
            else:
                current.append(c)

        if current:
            parameters.append(''.join(current))

        return parameters

    def _split_parameter_values(self, param_value: str) -> List[str]:
        """Split parameter values by comma, respecting quotes"""
        values = []
        current = []
        in_quotes = False

        for c in param_value:
            if c == '"':
                in_quotes = not in_quotes
            elif c == ',' and not in_quotes:
                values.append(''.join(current))
                current = []
            else:
                current.append(c)

        if current:
            values.append(''.join(current))

        return values

    def _find_unquoted_char(self, s: str, target: str) -> int:
        """Find the first occurrence of target not inside quotes"""
        in_quotes = False
        for i, c in enumerate(s):
            if c == '"':
                in_quotes = not in_quotes
            elif c == target and not in_quotes:
                return i
        return -1

    def _unescape_value(self, value: str) -> str:
        """Unescape special characters in value"""
        return (
            value
            .replace('\\n', '\n')
            .replace('\\N', '\n')
            .replace('\\;', ';')
            .replace('\\,', ',')
            .replace('\\\\', '\\')
        )

    def _validate_vcard(self, vcard: VCardObject):
        """Validate required vCard properties"""
        # VERSION is required (RFC 6350 Section 6.7.9)
        if not vcard.version:
            raise ParseException(
                "Missing required VERSION property (RFC 6350 Section 6.7.9). "
                "vCard must include VERSION:4.0"
            )

        # Only version 4.0 is supported (per ADR 0004)
        if vcard.version != "4.0":
            raise ParseException(
                f"Unsupported vCard version: {vcard.version}. "
                "Only version 4.0 is supported (see ADR 0004)."
            )

        # FN (formatted name) is required (RFC 6350 Section 6.2.1)
        if not vcard.formatted_name:
            raise ParseException(
                "Missing required FN (Formatted Name) property (RFC 6350 Section 6.2.1). "
                "vCard must include FN property."
            )
