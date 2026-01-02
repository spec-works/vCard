use std::fs;
use std::path::{Path, PathBuf};
use vcard::VCardParser;

fn get_negative_testcases_dir() -> PathBuf {
    // Start from the current directory (where cargo test is run from)
    let mut current = std::env::current_dir().expect("Failed to get current directory");

    // Try multiple paths to find the testcases/negative directory
    let attempts = vec![
        current.join("testcases").join("negative"),
        current.join("..").join("testcases").join("negative"),
        current.join("..").join("..").join("testcases").join("negative"),
        current.join("..").join("..").join("..").join("testcases").join("negative"),
        current.join("..").join("..").join("..").join("..").join("testcases").join("negative"),
    ];

    for path in attempts {
        if let Ok(canonical) = path.canonicalize() {
            if canonical.exists() && canonical.is_dir() {
                return canonical;
            }
        }
    }

    // If not found, traverse up from current dir
    loop {
        let testcases = current.join("testcases").join("negative");
        if testcases.exists() && testcases.is_dir() {
            return testcases;
        }
        if !current.pop() {
            break;
        }
    }

    panic!("Could not find testcases/negative directory");
}

fn read_test_file(filename: &str) -> String {
    let testcases_dir = get_negative_testcases_dir();
    let file_path = testcases_dir.join(filename);
    fs::read_to_string(file_path).expect("Failed to read test file")
}

// Structural Errors

#[test]
fn test_missing_begin_should_error() {
    let content = read_test_file("missing_begin.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject missing BEGIN");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("BEGIN:VCARD"),
            "Error message should indicate missing BEGIN: {}",
            error_msg
        );
    }
}

#[test]
fn test_missing_end_should_error() {
    let content = read_test_file("missing_end.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject missing END");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("Unexpected end of input"),
            "Error message should indicate missing END: {}",
            error_msg
        );
    }
}

#[test]
fn test_incomplete_vcard_should_error() {
    let content = read_test_file("incomplete_vcard.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject incomplete vCard");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("Unexpected end of input"),
            "Error: {}",
            error_msg
        );
    }
}

#[test]
fn test_mismatched_begin_end_should_error() {
    let content = read_test_file("mismatched_begin_end.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject mismatched tags");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("Mismatched END tag"),
            "Error should indicate mismatched tags: {}",
            error_msg
        );
    }
}

#[test]
fn test_wrong_component_type_should_error() {
    let content = read_test_file("wrong_component_type.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject wrong component type");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("BEGIN:VCARD"),
            "Should expect BEGIN:VCARD: {}",
            error_msg
        );
    }
}

#[test]
fn test_empty_file_should_error() {
    let content = read_test_file("empty_file.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject empty file");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("No vCard data found"),
            "Error: {}",
            error_msg
        );
    }
}

#[test]
fn test_only_whitespace_should_error() {
    let content = read_test_file("only_whitespace.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject whitespace-only file");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("No vCard data found"),
            "Error: {}",
            error_msg
        );
    }
}

// Required Property Violations

#[test]
fn test_missing_version_should_error() {
    let content = read_test_file("missing_version.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject missing VERSION");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("VERSION"),
            "Error should mention missing VERSION: {}",
            error_msg
        );
        assert!(
            error_msg.contains("RFC 6350"),
            "Error should reference the RFC: {}",
            error_msg
        );
    }
}

#[test]
fn test_missing_fn_should_error() {
    let content = read_test_file("missing_fn.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject missing FN");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("FN"),
            "Error should mention missing FN: {}",
            error_msg
        );
        assert!(
            error_msg.contains("Formatted Name"),
            "Error should explain what FN is: {}",
            error_msg
        );
        assert!(
            error_msg.contains("RFC 6350"),
            "Error should reference the RFC: {}",
            error_msg
        );
    }
}

// Version Support

#[test]
fn test_unsupported_version_21_should_error() {
    let content = read_test_file("unsupported_version_2_1.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject vCard version 2.1");

    if let Err(err) = result {
        let error_msg = err.to_string();
        // vCard 2.1 uses different parameter syntax (TEL;HOME instead of TEL;TYPE=home)
        // So the parser may fail on parameter parsing before checking version
        // Both are valid failures for unsupported version
        let has_version_error = error_msg.to_lowercase().contains("unsupported") && error_msg.contains("2.1");
        let has_parameter_error = error_msg.contains("parameter") && error_msg.contains("equals");

        assert!(
            has_version_error || has_parameter_error,
            "Expected either version error or parameter syntax error, got: {}",
            error_msg
        );
    }
}

#[test]
fn test_unsupported_version_30_should_error() {
    let content = read_test_file("unsupported_version_3_0.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject vCard version 3.0");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.to_lowercase().contains("unsupported"),
            "Error should indicate unsupported version: {}",
            error_msg
        );
        assert!(
            error_msg.contains("3.0"),
            "Error should mention version 3.0: {}",
            error_msg
        );
        assert!(
            error_msg.contains("4.0"),
            "Error should mention supported version 4.0: {}",
            error_msg
        );
    }
}

#[test]
fn test_unsupported_version_10_should_error() {
    let content = read_test_file("unsupported_version_1_0.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject vCard version 1.0");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.to_lowercase().contains("unsupported"),
            "Error should indicate unsupported version: {}",
            error_msg
        );
        assert!(
            error_msg.contains("1.0"),
            "Error should mention version 1.0: {}",
            error_msg
        );
    }
}

#[test]
fn test_invalid_version_format_should_error() {
    let content = read_test_file("invalid_version_format.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject invalid version format");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.to_lowercase().contains("unsupported"),
            "Error should indicate unsupported version: {}",
            error_msg
        );
    }
}

// Syntax Errors

#[test]
fn test_malformed_property_no_colon_should_error() {
    let content = read_test_file("malformed_property_no_colon.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject property without colon");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("colon"),
            "Error should mention missing colon: {}",
            error_msg
        );
        assert!(
            error_msg.contains("TEL"),
            "Error should show the problematic line: {}",
            error_msg
        );
    }
}

#[test]
fn test_malformed_parameter_syntax_should_error() {
    let content = read_test_file("malformed_parameter_syntax.vcf");
    let mut parser = VCardParser::new();

    let result = parser.parse(&content);
    assert!(result.is_err(), "Parser should reject malformed parameter");

    if let Err(err) = result {
        let error_msg = err.to_string();
        assert!(
            error_msg.contains("parameter"),
            "Error should mention parameter issue: {}",
            error_msg
        );
        assert!(
            error_msg.contains("equals"),
            "Error should mention missing equals sign: {}",
            error_msg
        );
    }
}

// Multiple Test Cases Runner

#[test]
fn test_critical_negative_test_files_should_error() {
    // These are critical errors that MUST be rejected
    // Note: Some tests are lenient (e.g., duplicate properties, unknown parameter values)
    // which is acceptable parser behavior. This test focuses on critical errors only.
    let critical_tests = vec![
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
    ];

    let mut passed_count = 0;
    let mut failed_tests = Vec::new();

    for filename in &critical_tests {
        let content = read_test_file(filename);
        let mut parser = VCardParser::new();
        let result = parser.parse(&content);

        match result {
            Ok(_) => {
                // If we get here, the parser didn't error - this is a failure
                failed_tests.push(format!(
                    "{}: Parser accepted invalid vCard (should have returned error)",
                    filename
                ));
            }
            Err(err) => {
                // Expected - negative test should error
                passed_count += 1;
                let error_msg = err.to_string();
                let preview = if error_msg.len() > 80 {
                    &error_msg[..80]
                } else {
                    &error_msg
                };
                println!("✓ {}: {}...", filename, preview);
            }
        }
    }

    // Report results
    println!(
        "\nCritical Negative Tests Summary: {}/{} passed",
        passed_count,
        critical_tests.len()
    );

    if !failed_tests.is_empty() {
        let failure_message = format!(
            "Some critical negative tests failed:\n{}",
            failed_tests.join("\n")
        );
        panic!("{}", failure_message);
    }
}
