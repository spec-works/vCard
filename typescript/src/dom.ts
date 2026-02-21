/**
 * Domain Object Model for vCard (RFC 6350)
 */

/**
 * Represents a vCard property with parameters and value
 */
export class VCardProperty {
  public name: string;
  public value: string;
  public parameters: Map<string, string[]> = new Map();

  constructor(name: string, value: string) {
    this.name = name;
    this.value = value;
  }

  addParameter(paramName: string, paramValue: string): void {
    const existing = this.parameters.get(paramName);
    if (existing) {
      existing.push(paramValue);
    } else {
      this.parameters.set(paramName, [paramValue]);
    }
  }

  getParameter(paramName: string): string | undefined {
    const values = this.parameters.get(paramName);
    return values?.[0];
  }

  getParameters(paramName: string): string[] {
    return this.parameters.get(paramName) ?? [];
  }
}

/**
 * Base class for all vCard components
 */
export abstract class VCardComponent {
  public properties: Map<string, VCardProperty[]> = new Map();

  abstract get componentType(): string;

  addProperty(property: VCardProperty): void {
    const existing = this.properties.get(property.name);
    if (existing) {
      existing.push(property);
    } else {
      this.properties.set(property.name, [property]);
    }
  }

  getProperty(name: string): VCardProperty | undefined {
    return this.properties.get(name)?.[0];
  }

  getProperties(name: string): VCardProperty[] {
    return this.properties.get(name) ?? [];
  }
}

/**
 * Root vCard object (VCARD)
 */
export class VCardObject extends VCardComponent {
  private _telephones: Telephone[] = [];
  private _emails: Email[] = [];
  private _addresses: Address[] = [];

  get componentType(): string {
    return 'VCARD';
  }

  // Required properties
  get version(): string | undefined {
    return this.getProperty('VERSION')?.value;
  }
  set version(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('VERSION', value));
  }

  get formattedName(): string | undefined {
    return this.getProperty('FN')?.value;
  }
  set formattedName(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('FN', value));
  }

  // Identification properties
  get name(): string | undefined {
    return this.getProperty('N')?.value;
  }
  set name(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('N', value));
  }

  get nickname(): string | undefined {
    return this.getProperty('NICKNAME')?.value;
  }
  set nickname(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('NICKNAME', value));
  }

  get photo(): string | undefined {
    return this.getProperty('PHOTO')?.value;
  }
  set photo(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('PHOTO', value));
  }

  get birthday(): string | undefined {
    return this.getProperty('BDAY')?.value;
  }
  set birthday(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('BDAY', value));
  }

  get anniversary(): string | undefined {
    return this.getProperty('ANNIVERSARY')?.value;
  }
  set anniversary(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('ANNIVERSARY', value));
  }

  get gender(): string | undefined {
    return this.getProperty('GENDER')?.value;
  }
  set gender(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('GENDER', value));
  }

  // Strongly-typed collections
  get telephones(): Telephone[] {
    return this._telephones;
  }
  set telephones(value: Telephone[]) {
    this._telephones = value;
  }

  get emails(): Email[] {
    return this._emails;
  }
  set emails(value: Email[]) {
    this._emails = value;
  }

  get addresses(): Address[] {
    return this._addresses;
  }
  set addresses(value: Address[]) {
    this._addresses = value;
  }

  // Other communication properties
  get impps(): VCardProperty[] {
    return this.getProperties('IMPP');
  }
  get languages(): VCardProperty[] {
    return this.getProperties('LANG');
  }

  // Geographical properties
  get timeZone(): string | undefined {
    return this.getProperty('TZ')?.value;
  }
  set timeZone(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('TZ', value));
  }

  get geo(): string | undefined {
    return this.getProperty('GEO')?.value;
  }
  set geo(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('GEO', value));
  }

  // Organizational properties
  get title(): string | undefined {
    return this.getProperty('TITLE')?.value;
  }
  set title(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('TITLE', value));
  }

  get role(): string | undefined {
    return this.getProperty('ROLE')?.value;
  }
  set role(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('ROLE', value));
  }

  get logo(): string | undefined {
    return this.getProperty('LOGO')?.value;
  }
  set logo(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('LOGO', value));
  }

  get organization(): string | undefined {
    return this.getProperty('ORG')?.value;
  }
  set organization(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('ORG', value));
  }

  get members(): VCardProperty[] {
    return this.getProperties('MEMBER');
  }
  get related(): VCardProperty[] {
    return this.getProperties('RELATED');
  }

  // Explanatory properties
  get categories(): VCardProperty[] {
    return this.getProperties('CATEGORIES');
  }
  get notes(): VCardProperty[] {
    return this.getProperties('NOTE');
  }

  get productId(): string | undefined {
    return this.getProperty('PRODID')?.value;
  }
  set productId(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('PRODID', value));
  }

  get revision(): string | undefined {
    return this.getProperty('REV')?.value;
  }
  set revision(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('REV', value));
  }

  get sound(): string | undefined {
    return this.getProperty('SOUND')?.value;
  }
  set sound(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('SOUND', value));
  }

  get uid(): string | undefined {
    return this.getProperty('UID')?.value;
  }
  set uid(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('UID', value));
  }

  get clientPidMaps(): VCardProperty[] {
    return this.getProperties('CLIENTPIDMAP');
  }
  get urls(): VCardProperty[] {
    return this.getProperties('URL');
  }
  get keys(): VCardProperty[] {
    return this.getProperties('KEY');
  }

  // Security properties
  get kind(): string | undefined {
    return this.getProperty('KIND')?.value;
  }
  set kind(value: string | undefined) {
    if (value !== undefined) this.addProperty(new VCardProperty('KIND', value));
  }

  // Calendar properties
  get fbUrls(): VCardProperty[] {
    return this.getProperties('FBURL');
  }
  get calAdrs(): VCardProperty[] {
    return this.getProperties('CALADRURI');
  }
  get calUris(): VCardProperty[] {
    return this.getProperties('CALURI');
  }
}

/**
 * Represents a structured name value
 */
export class StructuredName {
  familyName: string = '';
  givenName: string = '';
  additionalNames: string = '';
  honorificPrefixes: string = '';
  honorificSuffixes: string = '';

  static parse(value: string): StructuredName {
    const parts = value.split(';');
    const name = new StructuredName();
    name.familyName = parts[0] ?? '';
    name.givenName = parts[1] ?? '';
    name.additionalNames = parts[2] ?? '';
    name.honorificPrefixes = parts[3] ?? '';
    name.honorificSuffixes = parts[4] ?? '';
    return name;
  }

  toString(): string {
    return `${this.familyName};${this.givenName};${this.additionalNames};${this.honorificPrefixes};${this.honorificSuffixes}`;
  }
}

/**
 * Represents a structured address value
 */
export class StructuredAddress {
  postOfficeBox: string = '';
  extendedAddress: string = '';
  streetAddress: string = '';
  locality: string = '';
  region: string = '';
  postalCode: string = '';
  countryName: string = '';

  static parse(value: string): StructuredAddress {
    const parts = value.split(';');
    const addr = new StructuredAddress();
    addr.postOfficeBox = parts[0] ?? '';
    addr.extendedAddress = parts[1] ?? '';
    addr.streetAddress = parts[2] ?? '';
    addr.locality = parts[3] ?? '';
    addr.region = parts[4] ?? '';
    addr.postalCode = parts[5] ?? '';
    addr.countryName = parts[6] ?? '';
    return addr;
  }

  toString(): string {
    return `${this.postOfficeBox};${this.extendedAddress};${this.streetAddress};${this.locality};${this.region};${this.postalCode};${this.countryName}`;
  }
}

// Import types to make them available
import { Telephone, Email, Address } from './types';
