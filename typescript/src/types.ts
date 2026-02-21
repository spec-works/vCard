/**
 * Strongly typed enums and property classes for vCard (RFC 6350)
 */

import { VCardProperty } from './dom';

// Telephone type flags
export enum TelType {
  None = 0,
  Text = 1 << 0,
  Voice = 1 << 1,
  Fax = 1 << 2,
  Cell = 1 << 3,
  Video = 1 << 4,
  Pager = 1 << 5,
  TextPhone = 1 << 6,
  Work = 1 << 7,
  Home = 1 << 8,
}

// Email type flags
export enum EmailType {
  None = 0,
  Work = 1 << 0,
  Home = 1 << 1,
  Internet = 1 << 2,
}

// Address type flags
export enum AdrType {
  None = 0,
  Work = 1 << 0,
  Home = 1 << 1,
  Postal = 1 << 2,
  Parcel = 1 << 3,
  Dom = 1 << 4,
  Intl = 1 << 5,
}

const TEL_TYPE_MAP: Record<string, TelType> = {
  text: TelType.Text,
  voice: TelType.Voice,
  fax: TelType.Fax,
  cell: TelType.Cell,
  video: TelType.Video,
  pager: TelType.Pager,
  textphone: TelType.TextPhone,
  work: TelType.Work,
  home: TelType.Home,
};

const EMAIL_TYPE_MAP: Record<string, EmailType> = {
  work: EmailType.Work,
  home: EmailType.Home,
  internet: EmailType.Internet,
};

const ADR_TYPE_MAP: Record<string, AdrType> = {
  work: AdrType.Work,
  home: AdrType.Home,
  postal: AdrType.Postal,
  parcel: AdrType.Parcel,
  dom: AdrType.Dom,
  intl: AdrType.Intl,
};

function flagNames<T extends number>(value: T, map: Record<string, T>): string[] {
  const names: string[] = [];
  for (const [name, flag] of Object.entries(map)) {
    if (flag !== 0 && (value & flag) === flag) {
      names.push(name);
    }
  }
  return names;
}

/**
 * Represents a telephone number with type information
 */
export class Telephone {
  value: string = '';
  types: TelType = TelType.None;

  toProperty(): VCardProperty {
    const prop = new VCardProperty('TEL', this.value);
    for (const name of flagNames(this.types, TEL_TYPE_MAP)) {
      prop.addParameter('TYPE', name);
    }
    return prop;
  }

  static fromProperty(property: VCardProperty): Telephone {
    const tel = new Telephone();
    tel.value = property.value;
    for (const type of property.getParameters('TYPE')) {
      const flag = TEL_TYPE_MAP[type.toLowerCase()];
      if (flag !== undefined) {
        tel.types |= flag;
      } else {
        // Handle comma-separated values within a single quoted param
        for (const t of type.split(',')) {
          const f = TEL_TYPE_MAP[t.trim().toLowerCase()];
          if (f !== undefined) tel.types |= f;
        }
      }
    }
    return tel;
  }
}

/**
 * Represents an email address with type information
 */
export class Email {
  value: string = '';
  types: EmailType = EmailType.None;

  toProperty(): VCardProperty {
    const prop = new VCardProperty('EMAIL', this.value);
    for (const name of flagNames(this.types, EMAIL_TYPE_MAP)) {
      prop.addParameter('TYPE', name);
    }
    return prop;
  }

  static fromProperty(property: VCardProperty): Email {
    const email = new Email();
    email.value = property.value;
    for (const type of property.getParameters('TYPE')) {
      const flag = EMAIL_TYPE_MAP[type.toLowerCase()];
      if (flag !== undefined) {
        email.types |= flag;
      } else {
        for (const t of type.split(',')) {
          const f = EMAIL_TYPE_MAP[t.trim().toLowerCase()];
          if (f !== undefined) email.types |= f;
        }
      }
    }
    return email;
  }
}

/**
 * Represents a postal address with type information
 */
export class Address {
  postOfficeBox: string = '';
  extendedAddress: string = '';
  street: string = '';
  city: string = '';
  state: string = '';
  postalCode: string = '';
  country: string = '';
  types: AdrType = AdrType.None;

  toProperty(): VCardProperty {
    const value = `${this.postOfficeBox};${this.extendedAddress};${this.street};${this.city};${this.state};${this.postalCode};${this.country}`;
    const prop = new VCardProperty('ADR', value);
    for (const name of flagNames(this.types, ADR_TYPE_MAP)) {
      prop.addParameter('TYPE', name);
    }
    return prop;
  }

  static fromProperty(property: VCardProperty): Address {
    const parts = property.value.split(';');
    const address = new Address();
    address.postOfficeBox = parts[0] ?? '';
    address.extendedAddress = parts[1] ?? '';
    address.street = parts[2] ?? '';
    address.city = parts[3] ?? '';
    address.state = parts[4] ?? '';
    address.postalCode = parts[5] ?? '';
    address.country = parts[6] ?? '';

    for (const type of property.getParameters('TYPE')) {
      const flag = ADR_TYPE_MAP[type.toLowerCase()];
      if (flag !== undefined) {
        address.types |= flag;
      } else {
        for (const t of type.split(',')) {
          const f = ADR_TYPE_MAP[t.trim().toLowerCase()];
          if (f !== undefined) address.types |= f;
        }
      }
    }
    return address;
  }
}
