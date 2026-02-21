/**
 * Integration tests that validate vCard parser against shared test case files.
 * Each .vcf file has a corresponding .json file with expected data.
 */

import { describe, it, expect } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';
import { VCardParser } from '../src/parser';
import { VCardValidator } from '../src/validator';
import { VCardObject } from '../src/dom';

interface NameData {
  family: string;
  given: string;
  additional: string;
  prefix: string;
  suffix: string;
}

interface TelephoneData {
  value: string;
  types?: string[];
}

interface EmailData {
  value: string;
  types?: string[];
}

interface AddressData {
  pobox: string;
  extended: string;
  street: string;
  locality: string;
  region: string;
  postalcode: string;
  country: string;
  types?: string[];
}

interface VCardData {
  version?: string;
  fn?: string;
  n?: NameData;
  nickname?: string;
  gender?: string;
  bday?: string;
  org?: string;
  title?: string;
  telephones?: TelephoneData[];
  emails?: EmailData[];
  addresses?: AddressData[];
}

interface TestCaseData {
  vcards: VCardData[];
}

function getTestcasesDir(): string {
  let current = __dirname;
  while (true) {
    const testcases = path.join(current, 'testcases');
    if (fs.existsSync(testcases) && fs.statSync(testcases).isDirectory()) {
      return testcases;
    }
    const parent = path.dirname(current);
    if (parent === current) break;
    current = parent;
  }
  throw new Error('Could not find testcases directory');
}

function getTestFiles(): Array<{ name: string; vcfPath: string; jsonPath: string }> {
  const testcasesDir = getTestcasesDir();
  const files = fs.readdirSync(testcasesDir).filter((f) => f.endsWith('.vcf')).sort();

  const testFiles: Array<{ name: string; vcfPath: string; jsonPath: string }> = [];
  for (const vcfFile of files) {
    const jsonFile = vcfFile.replace('.vcf', '.json');
    const jsonPath = path.join(testcasesDir, jsonFile);
    if (fs.existsSync(jsonPath)) {
      testFiles.push({
        name: vcfFile.replace('.vcf', ''),
        vcfPath: path.join(testcasesDir, vcfFile),
        jsonPath,
      });
    }
  }
  return testFiles;
}

function getNegativeTestFiles(): string[] {
  const testcasesDir = getTestcasesDir();
  const negativeDir = path.join(testcasesDir, 'negative');
  if (!fs.existsSync(negativeDir)) return [];
  return fs.readdirSync(negativeDir).filter((f) => f.endsWith('.vcf')).sort().map((f) => path.join(negativeDir, f));
}

describe('vCard Parser - Shared Test Cases', () => {
  const testFiles = getTestFiles();

  it.each(testFiles)('$name: parse VCF and compare to JSON', ({ name, vcfPath, jsonPath }) => {
    const parser = new VCardParser();
    const vcfText = fs.readFileSync(vcfPath, 'utf-8');
    const vcards = parser.parse(vcfText);

    const expectedData: TestCaseData = JSON.parse(fs.readFileSync(jsonPath, 'utf-8'));
    const expectedVCards = expectedData.vcards;

    expect(vcards.length).toBe(expectedVCards.length);

    for (let i = 0; i < vcards.length; i++) {
      const vcard = vcards[i];
      const expected = expectedVCards[i];

      // Compare basic properties
      if (expected.version !== undefined) {
        expect(vcard.version).toBe(expected.version);
      }
      if (expected.fn !== undefined) {
        expect(vcard.formattedName).toBe(expected.fn);
      }

      // Compare structured name
      if (expected.n) {
        const actualN = vcard.name;
        expect(actualN).toBeDefined();
        const parts = actualN!.split(';');
        expect(parts[0] ?? '').toBe(expected.n.family);
        expect(parts[1] ?? '').toBe(expected.n.given);
        expect(parts[2] ?? '').toBe(expected.n.additional);
        expect(parts[3] ?? '').toBe(expected.n.prefix);
        expect(parts[4] ?? '').toBe(expected.n.suffix);
      }

      // Compare optional simple properties
      if (expected.nickname !== undefined) {
        expect(vcard.getProperty('NICKNAME')?.value).toBe(expected.nickname);
      }
      if (expected.gender !== undefined) {
        expect(vcard.getProperty('GENDER')?.value).toBe(expected.gender);
      }
      if (expected.bday !== undefined) {
        expect(vcard.getProperty('BDAY')?.value).toBe(expected.bday);
      }
      if (expected.org !== undefined) {
        expect(vcard.getProperty('ORG')?.value).toBe(expected.org);
      }
      if (expected.title !== undefined) {
        expect(vcard.getProperty('TITLE')?.value).toBe(expected.title);
      }

      // Compare telephones
      if (expected.telephones) {
        const actualTels = vcard.getProperties('TEL');
        expect(actualTels.length).toBe(expected.telephones.length);

        for (let j = 0; j < expected.telephones.length; j++) {
          const expectedTel = expected.telephones[j];
          const actualTel = actualTels[j];
          expect(actualTel.value).toBe(expectedTel.value);

          if (expectedTel.types && expectedTel.types.length > 0) {
            // Flatten comma-separated values for comparison (handles quoted params)
            const actualTypes = actualTel.getParameters('TYPE').flatMap((t) => t.toLowerCase().split(','));
            for (const expectedType of expectedTel.types) {
              // Check both as-is and comma-split
              const expLower = expectedType.toLowerCase();
              const matched = actualTypes.includes(expLower) ||
                actualTypes.some((a) => expLower.split(',').every((e) => actualTypes.includes(e)));
              expect(matched).toBe(true);
            }
          }
        }
      }

      // Compare emails
      if (expected.emails) {
        const actualEmails = vcard.getProperties('EMAIL');
        expect(actualEmails.length).toBe(expected.emails.length);

        for (let j = 0; j < expected.emails.length; j++) {
          const expectedEmail = expected.emails[j];
          const actualEmail = actualEmails[j];
          expect(actualEmail.value).toBe(expectedEmail.value);

          if (expectedEmail.types && expectedEmail.types.length > 0) {
            const actualTypes = actualEmail.getParameters('TYPE').flatMap((t) => t.toLowerCase().split(','));
            for (const expectedType of expectedEmail.types) {
              expect(actualTypes).toContain(expectedType.toLowerCase());
            }
          }
        }
      }

      // Compare addresses
      if (expected.addresses) {
        const actualAddrs = vcard.getProperties('ADR');
        expect(actualAddrs.length).toBe(expected.addresses.length);

        for (let j = 0; j < expected.addresses.length; j++) {
          const expectedAdr = expected.addresses[j];
          const actualAdr = actualAddrs[j];
          const parts = actualAdr.value.split(';');

          expect(parts[0] ?? '').toBe(expectedAdr.pobox);
          expect(parts[1] ?? '').toBe(expectedAdr.extended);
          expect(parts[2] ?? '').toBe(expectedAdr.street);
          expect(parts[3] ?? '').toBe(expectedAdr.locality);
          expect(parts[4] ?? '').toBe(expectedAdr.region);
          expect(parts[5] ?? '').toBe(expectedAdr.postalcode);
          expect(parts[6] ?? '').toBe(expectedAdr.country);

          if (expectedAdr.types && expectedAdr.types.length > 0) {
            const actualTypes = actualAdr.getParameters('TYPE').flatMap((t) => t.toLowerCase().split(','));
            for (const expectedType of expectedAdr.types) {
              expect(actualTypes).toContain(expectedType.toLowerCase());
            }
          }
        }
      }
    }
  });
});

describe('vCard Parser - Negative Test Cases', () => {
  const negativeFiles = getNegativeTestFiles();

  it.each(negativeFiles.map((f) => ({ name: path.basename(f, '.vcf'), path: f })))(
    '$name: should reject invalid vCard',
    ({ path: filePath }) => {
      const parser = new VCardParser();
      const validator = new VCardValidator();
      const vcfText = fs.readFileSync(filePath, 'utf-8');

      let parseError = false;
      let validationError = false;

      try {
        const vcards = parser.parse(vcfText);
        // If parse succeeds, validation should fail
        for (const vcard of vcards) {
          const result = validator.validate(vcard);
          if (!result.isValid) {
            validationError = true;
          }
        }
      } catch {
        parseError = true;
      }

      expect(parseError || validationError).toBe(true);
    },
  );
});
