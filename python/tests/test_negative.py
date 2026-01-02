"""
Negative tests for vCard parser to ensure proper error handling
"""

import pytest
from pathlib import Path
from typing import List

from vcard.parser import VCardParser, ParseException


def get_negative_testcases_dir() -> Path:
    """Find the testcases/negative directory."""
    current = Path(__file__).parent
    while current.parent != current:
        testcases = current / "testcases" / "negative"
        if testcases.exists() and testcases.is_dir():
            return testcases
        current = current.parent
    raise FileNotFoundError("Could not find testcases/negative directory")


def read_test_file(filename: str) -> str:
    """Read a negative test file."""
    testcases_dir = get_negative_testcases_dir()
    file_path = testcases_dir / filename
    with open(file_path, 'r', encoding='utf-8') as f:
        return f.read()


# Structural Errors

def test_missing_begin_should_raise():
    """Test that missing BEGIN:VCARD raises ParseException"""
    content = read_test_file("missing_begin.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    assert "BEGIN:VCARD" in str(exc_info.value), \
        "Error message should indicate missing BEGIN"


def test_missing_end_should_raise():
    """Test that missing END:VCARD raises ParseException"""
    content = read_test_file("missing_end.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    assert "Unexpected end of input" in str(exc_info.value), \
        "Error message should indicate missing END"


def test_incomplete_vcard_should_raise():
    """Test that incomplete vCard raises ParseException"""
    content = read_test_file("incomplete_vcard.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    assert "Unexpected end of input" in str(exc_info.value)


def test_mismatched_begin_end_should_raise():
    """Test that mismatched BEGIN/END tags raise ParseException"""
    content = read_test_file("mismatched_begin_end.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    assert "Mismatched END tag" in str(exc_info.value), \
        "Error should indicate mismatched tags"


def test_wrong_component_type_should_raise():
    """Test that wrong component type raises ParseException"""
    content = read_test_file("wrong_component_type.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    assert "BEGIN:VCARD" in str(exc_info.value), \
        "Should expect BEGIN:VCARD"


def test_empty_file_should_raise():
    """Test that empty file raises ParseException"""
    content = read_test_file("empty_file.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    assert "No vCard data found" in str(exc_info.value)


def test_only_whitespace_should_raise():
    """Test that file with only whitespace raises ParseException"""
    content = read_test_file("only_whitespace.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    assert "No vCard data found" in str(exc_info.value)


# Required Property Violations

def test_missing_version_should_raise():
    """Test that missing VERSION property raises ParseException"""
    content = read_test_file("missing_version.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    error_msg = str(exc_info.value)
    assert "VERSION" in error_msg, "Error should mention missing VERSION"
    assert "RFC 6350" in error_msg, "Error should reference the RFC"


def test_missing_fn_should_raise():
    """Test that missing FN property raises ParseException"""
    content = read_test_file("missing_fn.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    error_msg = str(exc_info.value)
    assert "FN" in error_msg, "Error should mention missing FN"
    assert "Formatted Name" in error_msg, "Error should explain what FN is"
    assert "RFC 6350" in error_msg, "Error should reference the RFC"


# Version Support

def test_unsupported_version_21_should_raise():
    """Test that vCard version 2.1 raises ParseException"""
    content = read_test_file("unsupported_version_2_1.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    error_msg = str(exc_info.value)
    # vCard 2.1 uses different parameter syntax (TEL;HOME instead of TEL;TYPE=home)
    # So the parser may fail on parameter parsing before checking version
    # Both are valid failures for unsupported version
    has_version_error = ("Unsupported" in error_msg or "unsupported" in error_msg) and "2.1" in error_msg
    has_parameter_error = "parameter" in error_msg and "equals" in error_msg

    assert has_version_error or has_parameter_error, \
        f"Expected either version error or parameter syntax error, got: {error_msg}"


def test_unsupported_version_30_should_raise():
    """Test that vCard version 3.0 raises ParseException"""
    content = read_test_file("unsupported_version_3_0.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    error_msg = str(exc_info.value)
    assert "Unsupported" in error_msg or "unsupported" in error_msg, \
        "Error should indicate unsupported version"
    assert "3.0" in error_msg, "Error should mention version 3.0"
    assert "4.0" in error_msg, "Error should mention supported version 4.0"


def test_unsupported_version_10_should_raise():
    """Test that vCard version 1.0 raises ParseException"""
    content = read_test_file("unsupported_version_1_0.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    error_msg = str(exc_info.value)
    assert "Unsupported" in error_msg or "unsupported" in error_msg, \
        "Error should indicate unsupported version"
    assert "1.0" in error_msg, "Error should mention version 1.0"


def test_invalid_version_format_should_raise():
    """Test that invalid version format raises ParseException"""
    content = read_test_file("invalid_version_format.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    error_msg = str(exc_info.value)
    assert "Unsupported" in error_msg or "unsupported" in error_msg, \
        "Error should indicate unsupported version"


# Syntax Errors

def test_malformed_property_no_colon_should_raise():
    """Test that property without colon raises ParseException"""
    content = read_test_file("malformed_property_no_colon.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    error_msg = str(exc_info.value)
    assert "colon" in error_msg, "Error should mention missing colon"
    assert "TEL" in error_msg, "Error should show the problematic line"


def test_malformed_parameter_syntax_should_raise():
    """Test that malformed parameter syntax raises ParseException"""
    content = read_test_file("malformed_parameter_syntax.vcf")
    parser = VCardParser()

    with pytest.raises(ParseException) as exc_info:
        parser.parse(content)

    error_msg = str(exc_info.value)
    assert "parameter" in error_msg, "Error should mention parameter issue"
    assert "equals" in error_msg, "Error should mention missing equals sign"


# Multiple Test Cases Runner

def test_critical_negative_test_files_should_raise():
    """
    Test that critical negative test files raise exceptions.
    Note: Some tests are lenient (e.g., duplicate properties, unknown parameter values)
    which is acceptable parser behavior. This test focuses on critical errors only.
    """
    # These are critical errors that MUST be rejected
    critical_tests = [
        "missing_begin.vcf",
        "missing_end.vcf",
        "missing_version.vcf",
        "missing_fn.vcf",
        "empty_file.vcf",
        "only_whitespace.vcf",
        "incomplete_vcard.vcf",
        "malformed_property_no_colon.vcf",
        "malformed_parameter_syntax.vcf",
        "unsupported_version_2_1.vcf",
        "unsupported_version_3_0.vcf",
        "unsupported_version_1_0.vcf",
        "wrong_component_type.vcf",
        "mismatched_begin_end.vcf"
    ]

    parser = VCardParser()
    passed_count = 0
    failed_tests = []

    for filename in critical_tests:
        try:
            content = read_test_file(filename)
            parser.parse(content)

            # If we get here, the parser didn't raise - this is a failure
            failed_tests.append(
                f"{filename}: Parser accepted invalid vCard (should have raised exception)"
            )
        except ParseException as ex:
            # Expected - negative test should raise
            passed_count += 1
            print(f"✓ {filename}: {str(ex)[:80]}...")
        except Exception as ex:
            # Unexpected exception type
            failed_tests.append(
                f"{filename}: Unexpected exception type: {type(ex).__name__}"
            )

    # Report results
    print(f"\nCritical Negative Tests Summary: {passed_count}/{len(critical_tests)} passed")

    if failed_tests:
        failure_message = "Some critical negative tests failed:\n" + "\n".join(failed_tests)
        pytest.fail(failure_message)


if __name__ == "__main__":
    # Run tests with pytest
    pytest.main([__file__, "-v"])
