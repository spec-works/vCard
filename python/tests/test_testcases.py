"""
Integration tests that validate vCard parser and serializer against test case files.
Each .vcf file has a corresponding .json file with expected data.
"""

import json
import os
from pathlib import Path
from typing import List, Dict, Any, Optional
import pytest

from vcard.parser import VCardParser
from vcard.serializer import VCardSerializer
from vcard.dom import VCardObject, VCardProperty
from vcard.builder import VCardBuilder, TelType, EmailType, AdrType


def get_testcases_dir() -> Path:
    """Find the testcases directory."""
    current = Path(__file__).parent
    while current.parent != current:
        testcases = current / "testcases"
        if testcases.exists() and testcases.is_dir():
            return testcases
        current = current.parent
    raise FileNotFoundError("Could not find testcases directory")


def get_test_files() -> List[tuple]:
    """Get all .vcf/.json test file pairs."""
    testcases_dir = get_testcases_dir()
    vcf_files = sorted(testcases_dir.glob("*.vcf"))

    test_files = []
    for vcf_file in vcf_files:
        json_file = vcf_file.with_suffix(".json")
        if json_file.exists():
            test_files.append((vcf_file.stem, str(vcf_file), str(json_file)))

    return test_files


def assert_property(test_name: str, property_path: str, expected: Optional[str], actual: Optional[str]):
    """Assert that a property value matches expected value."""
    if expected is None:
        return
    actual_value = actual or ""
    assert expected == actual_value, \
        f"{test_name}: Property {property_path} mismatch. Expected '{expected}', got '{actual_value}'"


def parse_tel_type(type_str: str) -> Optional[TelType]:
    """Parse a string into TelType enum."""
    type_map = {
        'text': TelType.TEXT,
        'voice': TelType.VOICE,
        'fax': TelType.FAX,
        'cell': TelType.CELL,
        'video': TelType.VIDEO,
        'pager': TelType.PAGER,
        'textphone': TelType.TEXTPHONE,
        'work': TelType.WORK,
        'home': TelType.HOME,
    }
    return type_map.get(type_str.lower())


def parse_email_type(type_str: str) -> Optional[EmailType]:
    """Parse a string into EmailType enum."""
    type_map = {
        'work': EmailType.WORK,
        'home': EmailType.HOME,
        'internet': EmailType.INTERNET,
    }
    return type_map.get(type_str.lower())


def parse_adr_type(type_str: str) -> Optional[AdrType]:
    """Parse a string into AdrType enum."""
    type_map = {
        'work': AdrType.WORK,
        'home': AdrType.HOME,
        'postal': AdrType.POSTAL,
        'parcel': AdrType.PARCEL,
        'dom': AdrType.DOM,
        'intl': AdrType.INTL,
    }
    return type_map.get(type_str.lower())


@pytest.mark.parametrize("test_name,vcf_path,json_path", get_test_files())
def test_parse_vcf_compare_to_json(test_name: str, vcf_path: str, json_path: str):
    """
    Parse VCF file and compare the result to the expected JSON data.

    Args:
        test_name: Name of the test case
        vcf_path: Path to the .vcf file
        json_path: Path to the .json file
    """
    # Parse VCF file
    parser = VCardParser()
    with open(vcf_path, 'r', encoding='utf-8') as f:
        vcf_text = f.read()
    vcards = parser.parse(vcf_text)

    # Load expected JSON data
    with open(json_path, 'r', encoding='utf-8') as f:
        expected_data = json.load(f)

    expected_vcards = expected_data['vcards']
    assert len(vcards) == len(expected_vcards), \
        f"{test_name}: Expected {len(expected_vcards)} vCards, got {len(vcards)}"

    for i, (vcard, expected) in enumerate(zip(vcards, expected_vcards)):
        # Compare basic properties
        assert_property(test_name, f"vCard[{i}].VERSION", expected.get('version'), vcard.version)
        assert_property(test_name, f"vCard[{i}].FN", expected.get('fn'), vcard.formatted_name)

        # Compare structured name
        if 'n' in expected:
            actual_n = vcard.name
            if actual_n:
                parts = actual_n.split(';')
                n_data = expected['n']
                assert_property(test_name, f"vCard[{i}].N.Family", n_data.get('family'), parts[0] if len(parts) > 0 else "")
                assert_property(test_name, f"vCard[{i}].N.Given", n_data.get('given'), parts[1] if len(parts) > 1 else "")
                assert_property(test_name, f"vCard[{i}].N.Additional", n_data.get('additional'), parts[2] if len(parts) > 2 else "")
                assert_property(test_name, f"vCard[{i}].N.Prefix", n_data.get('prefix'), parts[3] if len(parts) > 3 else "")
                assert_property(test_name, f"vCard[{i}].N.Suffix", n_data.get('suffix'), parts[4] if len(parts) > 4 else "")

        # Compare optional simple properties
        if 'nickname' in expected:
            nickname_prop = vcard.get_property("NICKNAME")
            assert_property(test_name, f"vCard[{i}].NICKNAME", expected['nickname'], nickname_prop.value if nickname_prop else None)
        if 'gender' in expected:
            gender_prop = vcard.get_property("GENDER")
            assert_property(test_name, f"vCard[{i}].GENDER", expected['gender'], gender_prop.value if gender_prop else None)
        if 'bday' in expected:
            bday_prop = vcard.get_property("BDAY")
            assert_property(test_name, f"vCard[{i}].BDAY", expected['bday'], bday_prop.value if bday_prop else None)
        if 'org' in expected:
            org_prop = vcard.get_property("ORG")
            assert_property(test_name, f"vCard[{i}].ORG", expected['org'], org_prop.value if org_prop else None)
        if 'title' in expected:
            title_prop = vcard.get_property("TITLE")
            assert_property(test_name, f"vCard[{i}].TITLE", expected['title'], title_prop.value if title_prop else None)

        # Compare telephones
        if 'telephones' in expected:
            expected_tels = expected['telephones']
            actual_tels = vcard.get_properties("TEL")
            actual_tels = actual_tels if actual_tels else []
            assert len(actual_tels) == len(expected_tels), \
                f"{test_name}: vCard[{i}] expected {len(expected_tels)} telephones, got {len(actual_tels)}"

            for j, (tel_prop, expected_tel) in enumerate(zip(actual_tels, expected_tels)):
                assert_property(test_name, f"vCard[{i}].TEL[{j}].Value", expected_tel['value'], tel_prop.value)
                if 'types' in expected_tel and expected_tel['types']:
                    actual_types = tel_prop.get_parameters("TYPE")
                    for expected_type in expected_tel['types']:
                        assert expected_type.lower() in [t.lower() for t in actual_types], \
                            f"{test_name}: vCard[{i}].TEL[{j}] should have type {expected_type}"

        # Compare emails
        if 'emails' in expected:
            expected_emails = expected['emails']
            actual_emails = vcard.get_properties("EMAIL")
            actual_emails = actual_emails if actual_emails else []
            assert len(actual_emails) == len(expected_emails), \
                f"{test_name}: vCard[{i}] expected {len(expected_emails)} emails, got {len(actual_emails)}"

            for j, (email_prop, expected_email) in enumerate(zip(actual_emails, expected_emails)):
                assert_property(test_name, f"vCard[{i}].EMAIL[{j}].Value", expected_email['value'], email_prop.value)
                if 'types' in expected_email and expected_email['types']:
                    actual_types = email_prop.get_parameters("TYPE")
                    for expected_type in expected_email['types']:
                        assert expected_type.lower() in [t.lower() for t in actual_types], \
                            f"{test_name}: vCard[{i}].EMAIL[{j}] should have type {expected_type}"

        # Compare addresses
        if 'addresses' in expected:
            expected_addrs = expected['addresses']
            actual_addrs = vcard.get_properties("ADR")
            actual_addrs = actual_addrs if actual_addrs else []
            assert len(actual_addrs) == len(expected_addrs), \
                f"{test_name}: vCard[{i}] expected {len(expected_addrs)} addresses, got {len(actual_addrs)}"

            for j, (adr_prop, expected_adr) in enumerate(zip(actual_addrs, expected_addrs)):
                # Address format: PO Box;Extended;Street;Locality;Region;PostalCode;Country
                parts = adr_prop.value.split(';')
                assert_property(test_name, f"vCard[{i}].ADR[{j}].POBox", expected_adr.get('pobox', ''), parts[0] if len(parts) > 0 else "")
                assert_property(test_name, f"vCard[{i}].ADR[{j}].Extended", expected_adr.get('extended', ''), parts[1] if len(parts) > 1 else "")
                assert_property(test_name, f"vCard[{i}].ADR[{j}].Street", expected_adr.get('street', ''), parts[2] if len(parts) > 2 else "")
                assert_property(test_name, f"vCard[{i}].ADR[{j}].Locality", expected_adr.get('locality', ''), parts[3] if len(parts) > 3 else "")
                assert_property(test_name, f"vCard[{i}].ADR[{j}].Region", expected_adr.get('region', ''), parts[4] if len(parts) > 4 else "")
                assert_property(test_name, f"vCard[{i}].ADR[{j}].PostalCode", expected_adr.get('postalcode', ''), parts[5] if len(parts) > 5 else "")
                assert_property(test_name, f"vCard[{i}].ADR[{j}].Country", expected_adr.get('country', ''), parts[6] if len(parts) > 6 else "")
                if 'types' in expected_adr and expected_adr['types']:
                    actual_types = adr_prop.get_parameters("TYPE")
                    for expected_type in expected_adr['types']:
                        assert expected_type.lower() in [t.lower() for t in actual_types], \
                            f"{test_name}: vCard[{i}].ADR[{j}] should have type {expected_type}"


def create_vcard_from_json(data: Dict[str, Any]) -> VCardObject:
    """Create a VCardObject from JSON test data."""
    builder = VCardBuilder()

    if 'version' in data:
        builder = builder.with_version(data['version'])
    if 'fn' in data:
        builder = builder.with_formatted_name(data['fn'])

    # Structured name
    if 'n' in data:
        n_data = data['n']
        builder = builder.with_name_parts(
            family_name=n_data.get('family', ''),
            given_name=n_data.get('given', ''),
            additional_names=n_data.get('additional', ''),
            honorific_prefixes=n_data.get('prefix', ''),
            honorific_suffixes=n_data.get('suffix', '')
        )

    # Telephones
    if 'telephones' in data:
        for tel_data in data['telephones']:
            types = []
            if 'types' in tel_data:
                for type_str in tel_data['types']:
                    tel_type = parse_tel_type(type_str)
                    if tel_type:
                        types.append(tel_type)
            builder = builder.with_telephone(tel_data['value'], types if types else None)

    # Emails
    if 'emails' in data:
        for email_data in data['emails']:
            types = []
            if 'types' in email_data:
                for type_str in email_data['types']:
                    email_type = parse_email_type(type_str)
                    if email_type:
                        types.append(email_type)
            builder = builder.with_email(email_data['value'], types if types else None)

    # Addresses
    if 'addresses' in data:
        for adr_data in data['addresses']:
            types = []
            if 'types' in adr_data:
                for type_str in adr_data['types']:
                    adr_type = parse_adr_type(type_str)
                    if adr_type:
                        types.append(adr_type)
            builder = builder.with_address_parts(
                po_box=adr_data.get('pobox', ''),
                extended=adr_data.get('extended', ''),
                street=adr_data.get('street', ''),
                locality=adr_data.get('locality', ''),
                region=adr_data.get('region', ''),
                postal_code=adr_data.get('postalcode', ''),
                country=adr_data.get('country', ''),
                types=types if types else None
            )

    # Other simple properties
    if 'nickname' in data:
        builder = builder.with_nickname(data['nickname'])
    if 'org' in data:
        builder = builder.with_organization(data['org'])
    if 'title' in data:
        builder = builder.with_title(data['title'])
    if 'bday' in data:
        builder = builder.with_birthday(data['bday'])
    if 'gender' in data:
        builder = builder.with_gender(data['gender'])

    return builder.build()


@pytest.mark.parametrize("test_name,vcf_path,json_path", get_test_files())
def test_create_from_json_compare_to_vcf(test_name: str, vcf_path: str, json_path: str):
    """
    Create vCard from JSON data, serialize it, and compare to the original VCF.

    Args:
        test_name: Name of the test case
        vcf_path: Path to the .vcf file
        json_path: Path to the .json file
    """
    # Load JSON data
    with open(json_path, 'r', encoding='utf-8') as f:
        test_data = json.load(f)

    # Create vCard objects from JSON
    vcards = []
    for vcard_data in test_data['vcards']:
        vcard = create_vcard_from_json(vcard_data)
        vcards.append(vcard)

    # Serialize to VCF format
    serializer = VCardSerializer()
    if len(vcards) == 1:
        actual_vcf_text = serializer.serialize(vcards[0])
    else:
        actual_vcf_text = serializer.serialize_multiple(vcards)

    # Parse both expected and actual for semantic comparison
    parser = VCardParser()
    with open(vcf_path, 'r', encoding='utf-8') as f:
        expected_vcf_text = f.read()

    expected_vcards = parser.parse(expected_vcf_text)
    actual_vcards = parser.parse(actual_vcf_text)

    # Compare counts
    assert len(actual_vcards) == len(expected_vcards), \
        f"{test_name}: Expected {len(expected_vcards)} vCards, got {len(actual_vcards)}"

    # Compare key properties
    for i, (expected_vcard, actual_vcard) in enumerate(zip(expected_vcards, actual_vcards)):
        assert_property(test_name, f"vCard[{i}].VERSION", expected_vcard.version, actual_vcard.version)
        assert_property(test_name, f"vCard[{i}].FN", expected_vcard.formatted_name, actual_vcard.formatted_name)

        # For semantic comparison, check that key properties match
        if expected_vcard.name:
            assert_property(test_name, f"vCard[{i}].N", expected_vcard.name, actual_vcard.name)
