/**
 * Serializer for text/vcard format (RFC 6350)
 */

import { VCardComponent, VCardObject, VCardProperty } from './dom';

const MAX_LINE_LENGTH = 75;

export class VCardSerializer {
  /**
   * Serialize a VCardObject to vCard format string
   */
  serialize(vcard: VCardObject): string {
    return this.serializeComponent(vcard);
  }

  /**
   * Serialize multiple VCardObjects to vCard format string
   */
  serializeMultiple(vcards: VCardObject[]): string {
    return vcards.map((v) => this.serializeComponent(v)).join('');
  }

  private serializeComponent(component: VCardComponent): string {
    const lines: string[] = [];

    lines.push(...this.writeLine(`BEGIN:${component.componentType}`));

    // VERSION must come first (after BEGIN)
    const versionProp = component.getProperty('VERSION');
    if (versionProp) {
      lines.push(...this.serializeProperty(versionProp));
    }

    // Write all other generic properties
    for (const [, propertyList] of component.properties) {
      for (const property of propertyList) {
        if (property.name !== 'VERSION') {
          lines.push(...this.serializeProperty(property));
        }
      }
    }

    // Write strongly-typed properties for VCardObject
    if (component instanceof VCardObject) {
      for (const telephone of component.telephones) {
        lines.push(...this.serializeProperty(telephone.toProperty()));
      }
      for (const email of component.emails) {
        lines.push(...this.serializeProperty(email.toProperty()));
      }
      for (const address of component.addresses) {
        lines.push(...this.serializeProperty(address.toProperty()));
      }
    }

    lines.push(...this.writeLine(`END:${component.componentType}`));

    return lines.join('\r\n') + '\r\n';
  }

  private serializeProperty(property: VCardProperty): string[] {
    let line = property.name;

    // Add parameters
    for (const [paramName, paramValues] of property.parameters) {
      for (const paramValue of paramValues) {
        line += ';';
        line += paramName;
        line += '=';
        if (this.needsQuoting(paramValue)) {
          line += `"${paramValue}"`;
        } else {
          line += paramValue;
        }
      }
    }

    line += ':';
    line += this.escapeValue(property.value);

    return this.writeLine(line);
  }

  private writeLine(line: string): string[] {
    if (line.length <= MAX_LINE_LENGTH) {
      return [line];
    }

    // Fold long lines (RFC 6350 Section 3.2)
    const result: string[] = [];
    result.push(line.substring(0, MAX_LINE_LENGTH));

    let remaining = line.substring(MAX_LINE_LENGTH);
    while (remaining.length > 0) {
      const chunkLength = Math.min(MAX_LINE_LENGTH - 1, remaining.length);
      result.push(' ' + remaining.substring(0, chunkLength));
      remaining = remaining.substring(chunkLength);
    }

    return result;
  }

  private needsQuoting(value: string): boolean {
    return value.includes(':') || value.includes(';') || value.includes(',') || value.includes(' ') || value.includes('\t');
  }

  private escapeValue(value: string): string {
    if (!value) return value;
    return value
      .replace(/\\/g, '\\\\')
      .replace(/;/g, '\\;')
      .replace(/,/g, '\\,')
      .replace(/\n/g, '\\n')
      .replace(/\r/g, '');
  }
}
