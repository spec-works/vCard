/**
 * Parser for text/vcard format (RFC 6350)
 */

import { VCardComponent, VCardObject, VCardProperty } from './dom';
import { Telephone, Email, Address } from './types';

export class ParseException extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'ParseException';
  }
}

export class VCardParser {
  private lines: string[] = [];
  private currentLine: number = 0;

  /**
   * Parse vCard text content. Always returns a list (per ADR 0001).
   */
  parse(vcardText: string): VCardObject[] {
    const vcards: VCardObject[] = [];
    this.lines = this.unfoldLines(vcardText);
    this.currentLine = 0;

    while (this.currentLine < this.lines.length) {
      // Skip empty lines
      while (this.currentLine < this.lines.length && this.lines[this.currentLine].trim() === '') {
        this.currentLine++;
      }
      if (this.currentLine >= this.lines.length) break;

      const line = this.lines[this.currentLine];
      if (line.toUpperCase() === 'BEGIN:VCARD') {
        this.currentLine++;
        const vcard = new VCardObject();
        this.parseComponent(vcard);
        vcards.push(vcard);
      } else {
        throw new ParseException(`Expected BEGIN:VCARD but got: ${line}`);
      }
    }

    if (vcards.length === 0) {
      throw new ParseException('No vCard data found');
    }

    return vcards;
  }

  /**
   * Parse vCard from file content (convenience wrapper for Node.js usage)
   */
  parseFile(filePath: string): VCardObject[] {
    const fs = require('fs');
    const content = fs.readFileSync(filePath, 'utf-8');
    return this.parse(content);
  }

  private unfoldLines(vcardText: string): string[] {
    const unfoldedLines: string[] = [];
    const rawLines = vcardText.split(/\r\n|\n/);
    let currentLine = '';

    for (const line of rawLines) {
      if (line.length > 0 && (line[0] === ' ' || line[0] === '\t')) {
        // Continuation line
        currentLine += line.substring(1);
      } else {
        if (currentLine.trim() !== '') {
          unfoldedLines.push(currentLine);
        }
        currentLine = line;
      }
    }

    if (currentLine.trim() !== '') {
      unfoldedLines.push(currentLine);
    }

    return unfoldedLines;
  }

  private parseComponent(component: VCardComponent): void {
    while (this.currentLine < this.lines.length) {
      const line = this.lines[this.currentLine];

      if (line.toUpperCase().startsWith('END:')) {
        const endComponentType = line.substring(4).toUpperCase();
        if (endComponentType !== component.componentType) {
          throw new ParseException(
            `Mismatched END tag: expected END:${component.componentType} but got END:${endComponentType}`,
          );
        }
        this.currentLine++;

        if (component instanceof VCardObject) {
          this.validateVCard(component);
        }
        return;
      } else {
        const property = this.parseProperty(line);

        if (component instanceof VCardObject) {
          switch (property.name) {
            case 'TEL':
              component.telephones.push(Telephone.fromProperty(property));
              component.addProperty(property);
              break;
            case 'EMAIL':
              component.emails.push(Email.fromProperty(property));
              component.addProperty(property);
              break;
            case 'ADR':
              component.addresses.push(Address.fromProperty(property));
              component.addProperty(property);
              break;
            default:
              component.addProperty(property);
              break;
          }
        } else {
          component.addProperty(property);
        }

        this.currentLine++;
      }
    }

    throw new ParseException(`Unexpected end of input while parsing ${component.componentType}`);
  }

  private parseProperty(line: string): VCardProperty {
    const colonIndex = this.findUnquotedChar(line, ':');
    if (colonIndex === -1) {
      throw new ParseException(`Invalid property line (missing colon): ${line}`);
    }

    const nameAndParams = line.substring(0, colonIndex);
    let value = line.substring(colonIndex + 1);
    value = this.unescapeValue(value);

    const semicolonIndex = this.findUnquotedChar(nameAndParams, ';');
    let propertyName: string;
    let paramsPart: string | null = null;

    if (semicolonIndex !== -1) {
      propertyName = nameAndParams.substring(0, semicolonIndex).toUpperCase();
      paramsPart = nameAndParams.substring(semicolonIndex + 1);
    } else {
      propertyName = nameAndParams.toUpperCase();
    }

    const property = new VCardProperty(propertyName, value);

    // Validate property name (must be alphanumeric, hyphens, dots for groups)
    // Group names cannot have consecutive dots
    if (!/^[A-Za-z0-9._-]+$/.test(propertyName) || /\.\./.test(propertyName)) {
      throw new ParseException(`Invalid property name: ${propertyName}`);
    }

    if (paramsPart !== null) {
      this.parseParameters(paramsPart, property);
    }

    return property;
  }

  private parseParameters(paramsPart: string, property: VCardProperty): void {
    const parameters = this.splitParameters(paramsPart);

    for (const param of parameters) {
      const equalsIndex = param.indexOf('=');
      if (equalsIndex === -1) {
        throw new ParseException(`Invalid parameter (missing equals): ${param}`);
      }

      const paramName = param.substring(0, equalsIndex).toUpperCase();
      let paramValue = param.substring(equalsIndex + 1);

      // Remove quotes if present; don't split quoted values on commas
      let wasQuoted = false;
      if (paramValue.startsWith('"') && paramValue.endsWith('"') && paramValue.length >= 2) {
        paramValue = paramValue.substring(1, paramValue.length - 1);
        wasQuoted = true;
      }

      if (wasQuoted) {
        property.addParameter(paramName, paramValue);
      } else {
        const values = this.splitParameterValues(paramValue);
        for (const value of values) {
          property.addParameter(paramName, value);
        }
      }
    }
  }

  private splitParameters(paramsPart: string): string[] {
    const parameters: string[] = [];
    let current = '';
    let inQuotes = false;

    for (let i = 0; i < paramsPart.length; i++) {
      const c = paramsPart[i];
      if (c === '"') {
        inQuotes = !inQuotes;
        current += c;
      } else if (c === ';' && !inQuotes) {
        parameters.push(current);
        current = '';
      } else {
        current += c;
      }
    }

    if (current.length > 0) {
      parameters.push(current);
    }

    return parameters;
  }

  private splitParameterValues(paramValue: string): string[] {
    const values: string[] = [];
    let current = '';
    let inQuotes = false;

    for (let i = 0; i < paramValue.length; i++) {
      const c = paramValue[i];
      if (c === '"') {
        inQuotes = !inQuotes;
      } else if (c === ',' && !inQuotes) {
        values.push(current);
        current = '';
      } else {
        current += c;
      }
    }

    if (current.length > 0) {
      values.push(current);
    }

    return values;
  }

  private findUnquotedChar(str: string, target: string): number {
    let inQuotes = false;
    for (let i = 0; i < str.length; i++) {
      if (str[i] === '"') {
        inQuotes = !inQuotes;
      } else if (str[i] === target && !inQuotes) {
        return i;
      }
    }
    return -1;
  }

  private unescapeValue(value: string): string {
    return value
      .replace(/\\n/gi, '\n')
      .replace(/\\;/g, ';')
      .replace(/\\,/g, ',')
      .replace(/\\\\/g, '\\');
  }

  private validateVCard(vcard: VCardObject): void {
    if (!vcard.version) {
      throw new ParseException(
        'Missing required VERSION property (RFC 6350 Section 6.7.9). vCard must include VERSION:4.0',
      );
    }

    if (vcard.version !== '4.0') {
      throw new ParseException(
        `Unsupported vCard version: ${vcard.version}. Only version 4.0 is supported (see ADR 0004).`,
      );
    }

    if (!vcard.formattedName) {
      throw new ParseException(
        'Missing required FN (Formatted Name) property (RFC 6350 Section 6.2.1). vCard must include FN property.',
      );
    }
  }
}
