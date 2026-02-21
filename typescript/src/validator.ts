/**
 * Validator for vCard components according to RFC 6350
 */

import { VCardComponent, VCardObject, VCardProperty } from './dom';

export class ValidationResult {
  errors: string[] = [];
  warnings: string[] = [];

  get isValid(): boolean {
    return this.errors.length === 0;
  }

  addError(error: string): void {
    this.errors.push(error);
  }

  addWarning(warning: string): void {
    this.warnings.push(warning);
  }

  getSummary(): string {
    let summary = `Validation Result: ${this.isValid ? 'VALID' : 'INVALID'}\n`;
    summary += `Errors: ${this.errors.length}\n`;
    summary += `Warnings: ${this.warnings.length}\n`;

    if (this.errors.length > 0) {
      summary += '\nErrors:\n';
      for (const error of this.errors) {
        summary += `  - ${error}\n`;
      }
    }

    if (this.warnings.length > 0) {
      summary += '\nWarnings:\n';
      for (const warning of this.warnings) {
        summary += `  - ${warning}\n`;
      }
    }

    return summary;
  }
}

export class VCardValidator {
  validate(vcard: VCardObject): ValidationResult {
    const result = new ValidationResult();
    this.validateVCard(vcard, result);
    return result;
  }

  private validateVCard(vcard: VCardObject, result: ValidationResult): void {
    // Required properties
    this.validateRequiredProperty(vcard, 'VERSION', result);
    this.validateRequiredProperty(vcard, 'FN', result);

    // VERSION must be 4.0 (or 3.0, 2.1 for backward compatibility)
    const versionProps = vcard.getProperties('VERSION');
    if (versionProps.length > 1) {
      result.addError('Duplicate VERSION property');
    }
    const version = vcard.getProperty('VERSION');
    if (version) {
      const validVersions = ['4.0', '3.0', '2.1'];
      if (!validVersions.includes(version.value)) {
        result.addError(`VERSION must be one of: ${validVersions.join(', ')}, found: ${version.value}`);
      } else if (version.value !== '4.0') {
        result.addWarning(`VERSION ${version.value} is supported but deprecated. Consider upgrading to 4.0`);
      }
    }

    // FN must not be empty; duplicate FN is an error
    const fnProps = vcard.getProperties('FN');
    if (fnProps.length > 1) {
      result.addError('Duplicate FN property');
    }
    const fn = vcard.getProperty('FN');
    if (fn && !fn.value.trim()) {
      result.addError('FN (Formatted Name) cannot be empty');
    }

    // Validate N format if present
    const name = vcard.getProperty('N');
    if (name) {
      this.validateStructuredName(name, result);
    }

    // Validate telephones
    for (const tel of vcard.telephones) {
      this.validateTelephone(tel, result);
    }
    for (const telProp of vcard.getProperties('TEL')) {
      this.validateTelProperty(telProp, result);
    }

    // Validate emails
    for (const email of vcard.emails) {
      this.validateEmail(email, result);
    }

    // Validate addresses
    for (const adr of vcard.addresses) {
      this.validateAddress(adr, result);
    }

    // Validate BDAY
    const bday = vcard.getProperty('BDAY');
    if (bday) {
      this.validateDateFormat(bday, result, 'BDAY');
    }

    // Validate ANNIVERSARY
    const anniversary = vcard.getProperty('ANNIVERSARY');
    if (anniversary) {
      this.validateDateFormat(anniversary, result, 'ANNIVERSARY');
    }

    // Validate GEO
    const geo = vcard.getProperty('GEO');
    if (geo) {
      this.validateGeoFormat(geo, result);
    }

    // Validate TZ
    const tz = vcard.getProperty('TZ');
    if (tz) {
      this.validateTimeZoneFormat(tz, result);
    }

    // Validate UID
    const uid = vcard.getProperty('UID');
    if (uid && !uid.value.trim()) {
      result.addError('UID cannot be empty if present');
    }

    // Validate MEMBER references (KIND:group requires UID for member resolution)
    const kind = vcard.getProperty('KIND');
    if (kind && kind.value.toLowerCase() === 'group') {
      const members = vcard.getProperties('MEMBER');
      const uid = vcard.getProperty('UID');
      if (members.length > 0 && !uid) {
        result.addError('KIND:group vCard with MEMBER properties must have a UID');
      }
      if (uid) {
        for (const member of members) {
          if (member.value === `urn:uuid:${uid.value}`) {
            result.addError('MEMBER property cannot reference the vCard itself (circular reference)');
          }
        }
      }
    }

    // Validate all properties for valid names and parameters
    for (const [name, props] of vcard.properties) {
      // Property names must be alphanumeric, hyphens, or dots (for groups)
      if (!/^[A-Za-z0-9._-]+$/.test(name)) {
        result.addError(`Invalid property name: ${name}`);
      }
      for (const prop of props) {
        // Check for invalid ENCODING parameter
        const encoding = prop.getParameter('ENCODING');
        if (encoding) {
          const validEncodings = ['B', 'BASE64', 'QUOTED-PRINTABLE', '8BIT'];
          if (validEncodings.includes(encoding.toUpperCase())) {
            result.addError(`ENCODING parameter is not valid in vCard 4.0: ${encoding}`);
          }
        }
      }
    }

    // Validate URLs
    for (const url of vcard.urls) {
      this.validateUrlFormat(url, result);
    }
  }

  private validateRequiredProperty(component: VCardComponent, propertyName: string, result: ValidationResult): void {
    if (!component.getProperty(propertyName)) {
      result.addError(`Required property ${propertyName} is missing`);
    }
  }

  private validateStructuredName(property: VCardProperty, result: ValidationResult): void {
    const parts = property.value.split(';');
    if (parts.length > 5) {
      result.addError(`N property has more than 5 components: ${property.value}`);
    }
    if (parts.every((p) => !p.trim())) {
      result.addError('N property must have at least one non-empty component');
    }
  }

  private validateTelephone(telephone: { value: string; types: number }, result: ValidationResult): void {
    if (!telephone.value.trim()) {
      result.addError('TEL property cannot be empty');
    }
  }

  private validateTelProperty(property: VCardProperty, result: ValidationResult): void {
    const validTelTypes = ['text', 'voice', 'fax', 'cell', 'video', 'pager', 'textphone', 'work', 'home'];
    for (const type of property.getParameters('TYPE')) {
      for (const t of type.split(',')) {
        if (!validTelTypes.includes(t.trim().toLowerCase())) {
          result.addError(`TEL TYPE parameter has invalid value: ${t}`);
        }
      }
    }
  }

  private validateEmail(email: { value: string; types: number }, result: ValidationResult): void {
    if (!email.value.trim()) {
      result.addError('EMAIL property cannot be empty');
      return;
    }
    const emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
    if (!emailPattern.test(email.value)) {
      result.addWarning(`EMAIL property may not be a valid email address: ${email.value}`);
    }
  }

  private validateAddress(_address: { types: number }, _result: ValidationResult): void {
    // All address components are optional
  }

  private validateDateFormat(property: VCardProperty, result: ValidationResult, propertyName: string): void {
    const value = property.value;
    if (!value.trim()) {
      result.addError(`${propertyName} cannot be empty`);
      return;
    }

    const patterns = [
      /^\d{8}$/,                              // YYYYMMDD
      /^\d{4}-\d{2}-\d{2}$/,                  // YYYY-MM-DD
      /^--\d{4}$/,                            // --MMDD
      /^--\d{2}-\d{2}$/,                      // --MM-DD
      /^\d{4}-\d{2}$/,                        // YYYY-MM
      /^\d{4}$/,                              // YYYY
      /^\d{8}T\d{6}$/,                        // YYYYMMDDTHHMMSS
      /^\d{8}T\d{6}Z$/,                       // YYYYMMDDTHHMMSSZ
      /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$/, // YYYY-MM-DDTHH:MM:SS
    ];

    if (!patterns.some((p) => p.test(value))) {
      result.addError(`${propertyName} has non-standard date format: ${value}`);
    }
  }

  private validateGeoFormat(property: VCardProperty, result: ValidationResult): void {
    const value = property.value;
    if (!value.trim()) {
      result.addError('GEO property cannot be empty');
      return;
    }
    const geoPattern = /^geo:[-+]?\d+\.?\d*,[-+]?\d+\.?\d*$/;
    if (!geoPattern.test(value)) {
      result.addWarning(`GEO property may not be in correct format (expected geo:lat,long): ${value}`);
    }
  }

  private validateTimeZoneFormat(property: VCardProperty, result: ValidationResult): void {
    const value = property.value;
    if (!value.trim()) {
      result.addError('TZ property cannot be empty');
      return;
    }
    const utcOffsetPattern = /^[+-]\d{2}:?\d{2}$/;
    if (!utcOffsetPattern.test(value) && !value.includes('/')) {
      result.addWarning(`TZ property may not be in correct format: ${value}`);
    }
  }

  private validateUrlFormat(property: VCardProperty, result: ValidationResult): void {
    const value = property.value;
    if (!value.trim()) {
      result.addError('URL property cannot be empty');
      return;
    }
    try {
      new URL(value);
    } catch {
      result.addError(`URL property is not a valid URL: ${value}`);
    }
  }
}
