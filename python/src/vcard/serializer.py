"""
vCard Serializer for RFC 6350
"""

from typing import List
from .dom import VCardObject, VCardProperty


class VCardSerializer:
    """Serializer for text/vcard format (RFC 6350)"""

    MAX_LINE_LENGTH = 75

    def serialize(self, vcard: VCardObject) -> str:
        """Serialize a VCardObject to vCard format string"""
        lines = []
        self._serialize_component(vcard, lines)
        return '\r\n'.join(lines) + '\r\n'

    def serialize_multiple(self, vcards: List[VCardObject]) -> str:
        """Serialize multiple VCardObjects to vCard format string"""
        result = []
        for vcard in vcards:
            result.append(self.serialize(vcard))
        return ''.join(result)

    def serialize_to_file(self, vcard: VCardObject, file_path: str):
        """Serialize a VCardObject to a file"""
        content = self.serialize(vcard)
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)

    def serialize_multiple_to_file(self, vcards: List[VCardObject], file_path: str):
        """Serialize multiple VCardObjects to a file"""
        content = self.serialize_multiple(vcards)
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)

    def _serialize_component(self, component: VCardObject, lines: List[str]):
        """Serialize a component to lines"""
        # Write BEGIN
        self._write_line(lines, f"BEGIN:{component.component_type}")

        # Write VERSION first if present
        version_prop = component.get_property("VERSION")
        if version_prop:
            self._serialize_property(version_prop, lines)

        # Write all other properties
        for prop_list in component.properties.values():
            for prop in prop_list:
                # Skip VERSION as it's already written
                if prop.name != "VERSION":
                    self._serialize_property(prop, lines)

        # Write END
        self._write_line(lines, f"END:{component.component_type}")

    def _serialize_property(self, prop: VCardProperty, lines: List[str]):
        """Serialize a property to lines"""
        line_parts = [prop.name]

        # Add parameters
        if prop.parameters:
            for param_name, param_values in prop.parameters.items():
                for param_value in param_values:
                    line_parts.append(';')
                    line_parts.append(param_name)
                    line_parts.append('=')

                    # Quote parameter value if it contains special characters
                    if self._needs_quoting(param_value):
                        line_parts.append('"')
                        line_parts.append(param_value)
                        line_parts.append('"')
                    else:
                        line_parts.append(param_value)

        line_parts.append(':')
        line_parts.append(self._escape_value(prop.value))

        line = ''.join(line_parts)
        self._write_line(lines, line)

    def _write_line(self, lines: List[str], line: str):
        """Write a line, folding if necessary"""
        if len(line) <= self.MAX_LINE_LENGTH:
            lines.append(line)
            return

        # Fold long lines (RFC 6350 Section 3.2)
        first_line = line[:self.MAX_LINE_LENGTH]
        lines.append(first_line)

        remaining = line[self.MAX_LINE_LENGTH:]
        while remaining:
            chunk_length = min(self.MAX_LINE_LENGTH - 1, len(remaining))
            chunk = remaining[:chunk_length]
            lines.append(' ' + chunk)
            remaining = remaining[chunk_length:]

    def _needs_quoting(self, value: str) -> bool:
        """Check if a parameter value needs quoting"""
        return any(c in value for c in ':;, \t')

    def _escape_value(self, value: str) -> str:
        """Escape special characters in value"""
        if not value:
            return value

        return (
            value
            .replace('\\', '\\\\')
            .replace(';', '\\;')
            .replace(',', '\\,')
            .replace('\n', '\\n')
            .replace('\r', '')
        )
