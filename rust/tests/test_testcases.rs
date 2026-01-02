use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::fs;
use std::path::{Path, PathBuf};
use vcard::{VCardObject, VCardParser, VCardProperty};

#[derive(Debug, Deserialize, Serialize)]
struct VCardTestCaseData {
    vcards: Vec<VCardData>,
}

#[derive(Debug, Deserialize, Serialize)]
struct VCardData {
    #[serde(skip_serializing_if = "Option::is_none")]
    version: Option<String>,
    #[serde(rename = "fn")]
    #[serde(skip_serializing_if = "Option::is_none")]
    fn_field: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    n: Option<NameData>,
    #[serde(skip_serializing_if = "Option::is_none")]
    nickname: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    gender: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    bday: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    org: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    title: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    telephones: Option<Vec<TelephoneData>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    emails: Option<Vec<EmailData>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    addresses: Option<Vec<AddressData>>,
}

#[derive(Debug, Deserialize, Serialize)]
struct NameData {
    family: String,
    given: String,
    additional: String,
    prefix: String,
    suffix: String,
}

#[derive(Debug, Deserialize, Serialize)]
struct TelephoneData {
    value: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    types: Option<Vec<String>>,
}

#[derive(Debug, Deserialize, Serialize)]
struct EmailData {
    value: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    types: Option<Vec<String>>,
}

#[derive(Debug, Deserialize, Serialize)]
struct AddressData {
    pobox: String,
    extended: String,
    street: String,
    locality: String,
    region: String,
    postalcode: String,
    country: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    types: Option<Vec<String>>,
}

fn get_testcases_dir() -> PathBuf {
    // Start from the current directory (where cargo test is run from)
    let mut current = std::env::current_dir().expect("Failed to get current directory");

    // Try multiple paths to find the testcases directory
    let attempts = vec![
        current.join("testcases"),
        current.join("..").join("testcases"),
        current.join("..").join("..").join("testcases"),
        current.join("..").join("..").join("..").join("testcases"),
        current.join("..").join("..").join("..").join("..").join("testcases"),
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
        let testcases = current.join("testcases");
        if testcases.exists() && testcases.is_dir() {
            return testcases;
        }
        if !current.pop() {
            break;
        }
    }
    panic!("Could not find testcases directory");
}

fn get_test_files() -> Vec<(String, PathBuf, PathBuf)> {
    let testcases_dir = get_testcases_dir();
    let mut test_files = Vec::new();

    if let Ok(entries) = fs::read_dir(&testcases_dir) {
        let mut vcf_files: Vec<PathBuf> = entries
            .filter_map(|e| e.ok())
            .map(|e| e.path())
            .filter(|p| p.extension().and_then(|s| s.to_str()) == Some("vcf"))
            .collect();

        vcf_files.sort();

        for vcf_file in vcf_files {
            let json_file = vcf_file.with_extension("json");
            if json_file.exists() {
                let test_name = vcf_file.file_stem().unwrap().to_str().unwrap().to_string();
                test_files.push((test_name, vcf_file, json_file));
            }
        }
    }

    test_files
}

fn assert_property(
    test_name: &str,
    property_path: &str,
    expected: &Option<String>,
    actual: Option<&str>,
) -> Result<(), String> {
    if expected.is_none() {
        return Ok(());
    }

    let expected_val = expected.as_ref().unwrap();
    let actual_val = actual.unwrap_or("");

    if expected_val != actual_val {
        return Err(format!(
            "{}: Property {} mismatch. Expected '{}', got '{}'",
            test_name, property_path, expected_val, actual_val
        ));
    }

    Ok(())
}

fn test_parse_vcf_compare_to_json(
    test_name: &str,
    vcf_path: &Path,
    json_path: &Path,
) -> Result<(), String> {
    // Parse VCF file
    let vcf_text = fs::read_to_string(vcf_path)
        .map_err(|e| format!("Failed to read VCF file: {}", e))?;
    let mut parser = VCardParser::new();
    let vcards = parser
        .parse(&vcf_text)
        .map_err(|e| format!("Failed to parse VCF: {}", e))?;

    // Load expected JSON data
    let json_text = fs::read_to_string(json_path)
        .map_err(|e| format!("Failed to read JSON file: {}", e))?;
    let expected_data: VCardTestCaseData = serde_json::from_str(&json_text)
        .map_err(|e| format!("Failed to parse JSON: {}", e))?;

    // Compare counts
    if vcards.len() != expected_data.vcards.len() {
        return Err(format!(
            "{}: Expected {} vCards, got {}",
            test_name,
            expected_data.vcards.len(),
            vcards.len()
        ));
    }

    // Compare each vCard
    for (i, (vcard, expected)) in vcards.iter().zip(expected_data.vcards.iter()).enumerate() {
        // Compare basic properties
        assert_property(test_name, &format!("vCard[{}].VERSION", i), &expected.version, vcard.version())?;
        assert_property(test_name, &format!("vCard[{}].FN", i), &expected.fn_field, vcard.formatted_name())?;

        // Compare structured name
        if let Some(ref n_data) = expected.n {
            if let Some(actual_n) = vcard.name() {
                let parts: Vec<&str> = actual_n.split(';').collect();
                assert_property(test_name, &format!("vCard[{}].N.Family", i), &Some(n_data.family.clone()), parts.get(0).copied())?;
                assert_property(test_name, &format!("vCard[{}].N.Given", i), &Some(n_data.given.clone()), parts.get(1).copied())?;
                assert_property(test_name, &format!("vCard[{}].N.Additional", i), &Some(n_data.additional.clone()), parts.get(2).copied())?;
                assert_property(test_name, &format!("vCard[{}].N.Prefix", i), &Some(n_data.prefix.clone()), parts.get(3).copied())?;
                assert_property(test_name, &format!("vCard[{}].N.Suffix", i), &Some(n_data.suffix.clone()), parts.get(4).copied())?;
            }
        }

        // Compare optional simple properties
        if let Some(ref nickname) = expected.nickname {
            let actual_nickname = vcard.get_property("NICKNAME").map(|p| p.value.as_str());
            assert_property(test_name, &format!("vCard[{}].NICKNAME", i), &Some(nickname.clone()), actual_nickname)?;
        }

        if let Some(ref gender) = expected.gender {
            let actual_gender = vcard.get_property("GENDER").map(|p| p.value.as_str());
            assert_property(test_name, &format!("vCard[{}].GENDER", i), &Some(gender.clone()), actual_gender)?;
        }

        if let Some(ref bday) = expected.bday {
            let actual_bday = vcard.get_property("BDAY").map(|p| p.value.as_str());
            assert_property(test_name, &format!("vCard[{}].BDAY", i), &Some(bday.clone()), actual_bday)?;
        }

        if let Some(ref org) = expected.org {
            let actual_org = vcard.organization();
            assert_property(test_name, &format!("vCard[{}].ORG", i), &Some(org.clone()), actual_org)?;
        }

        if let Some(ref title) = expected.title {
            let actual_title = vcard.title();
            assert_property(test_name, &format!("vCard[{}].TITLE", i), &Some(title.clone()), actual_title)?;
        }

        // Compare telephones
        if let Some(ref expected_tels) = expected.telephones {
            let empty_vec = vec![];
            let actual_tels = vcard.telephones().unwrap_or(&empty_vec);
            if actual_tels.len() != expected_tels.len() {
                return Err(format!(
                    "{}: vCard[{}] expected {} telephones, got {}",
                    test_name,
                    i,
                    expected_tels.len(),
                    actual_tels.len()
                ));
            }

            for (j, (tel_prop, expected_tel)) in
                actual_tels.iter().zip(expected_tels.iter()).enumerate()
            {
                assert_property(test_name, &format!("vCard[{}].TEL[{}].Value", i, j), &Some(expected_tel.value.clone()), Some(&tel_prop.value))?;

                if let Some(ref expected_types) = expected_tel.types {
                    let empty_types = vec![];
                    let actual_types = tel_prop.get_parameters("TYPE").unwrap_or(&empty_types);
                    for expected_type in expected_types {
                        let has_type = actual_types
                            .iter()
                            .any(|t| t.to_lowercase() == expected_type.to_lowercase());
                        if !has_type {
                            return Err(format!(
                                "{}: vCard[{}].TEL[{}] should have type {}",
                                test_name, i, j, expected_type
                            ));
                        }
                    }
                }
            }
        }

        // Compare emails
        if let Some(ref expected_emails) = expected.emails {
            let empty_vec = vec![];
            let actual_emails = vcard.emails().unwrap_or(&empty_vec);
            if actual_emails.len() != expected_emails.len() {
                return Err(format!(
                    "{}: vCard[{}] expected {} emails, got {}",
                    test_name,
                    i,
                    expected_emails.len(),
                    actual_emails.len()
                ));
            }

            for (j, (email_prop, expected_email)) in
                actual_emails.iter().zip(expected_emails.iter()).enumerate()
            {
                assert_property(test_name, &format!("vCard[{}].EMAIL[{}].Value", i, j), &Some(expected_email.value.clone()), Some(&email_prop.value))?;

                if let Some(ref expected_types) = expected_email.types {
                    let empty_types = vec![];
                    let actual_types = email_prop.get_parameters("TYPE").unwrap_or(&empty_types);
                    for expected_type in expected_types {
                        let has_type = actual_types
                            .iter()
                            .any(|t| t.to_lowercase() == expected_type.to_lowercase());
                        if !has_type {
                            return Err(format!(
                                "{}: vCard[{}].EMAIL[{}] should have type {}",
                                test_name, i, j, expected_type
                            ));
                        }
                    }
                }
            }
        }

        // Compare addresses
        if let Some(ref expected_addrs) = expected.addresses {
            let empty_vec = vec![];
            let actual_addrs = vcard.addresses().unwrap_or(&empty_vec);
            if actual_addrs.len() != expected_addrs.len() {
                return Err(format!(
                    "{}: vCard[{}] expected {} addresses, got {}",
                    test_name,
                    i,
                    expected_addrs.len(),
                    actual_addrs.len()
                ));
            }

            for (j, (adr_prop, expected_adr)) in
                actual_addrs.iter().zip(expected_addrs.iter()).enumerate()
            {
                // Address format: PO Box;Extended;Street;Locality;Region;PostalCode;Country
                let parts: Vec<&str> = adr_prop.value.split(';').collect();
                assert_property(test_name, &format!("vCard[{}].ADR[{}].POBox", i, j), &Some(expected_adr.pobox.clone()), parts.get(0).copied())?;
                assert_property(test_name, &format!("vCard[{}].ADR[{}].Extended", i, j), &Some(expected_adr.extended.clone()), parts.get(1).copied())?;
                assert_property(test_name, &format!("vCard[{}].ADR[{}].Street", i, j), &Some(expected_adr.street.clone()), parts.get(2).copied())?;
                assert_property(test_name, &format!("vCard[{}].ADR[{}].Locality", i, j), &Some(expected_adr.locality.clone()), parts.get(3).copied())?;
                assert_property(test_name, &format!("vCard[{}].ADR[{}].Region", i, j), &Some(expected_adr.region.clone()), parts.get(4).copied())?;
                assert_property(test_name, &format!("vCard[{}].ADR[{}].PostalCode", i, j), &Some(expected_adr.postalcode.clone()), parts.get(5).copied())?;
                assert_property(test_name, &format!("vCard[{}].ADR[{}].Country", i, j), &Some(expected_adr.country.clone()), parts.get(6).copied())?;

                if let Some(ref expected_types) = expected_adr.types {
                    let empty_types = vec![];
                    let actual_types = adr_prop.get_parameters("TYPE").unwrap_or(&empty_types);
                    for expected_type in expected_types {
                        let has_type = actual_types
                            .iter()
                            .any(|t| t.to_lowercase() == expected_type.to_lowercase());
                        if !has_type {
                            return Err(format!(
                                "{}: vCard[{}].ADR[{}] should have type {}",
                                test_name, i, j, expected_type
                            ));
                        }
                    }
                }
            }
        }
    }

    Ok(())
}

#[test]
fn run_all_testcase_tests() {
    let test_files = get_test_files();
    let mut passed = 0;
    let mut failed = 0;
    let mut failures = Vec::new();

    for (test_name, vcf_path, json_path) in test_files {
        match test_parse_vcf_compare_to_json(&test_name, &vcf_path, &json_path) {
            Ok(_) => {
                println!("✓ {}: parse_vcf_compare_to_json", test_name);
                passed += 1;
            }
            Err(e) => {
                println!("✗ {}: parse_vcf_compare_to_json - {}", test_name, e);
                failures.push(format!("{}: {}", test_name, e));
                failed += 1;
            }
        }
    }

    println!("\n=== Test Summary ===");
    println!("Passed: {}", passed);
    println!("Failed: {}", failed);

    if !failures.is_empty() {
        println!("\nFailures:");
        for failure in &failures {
            println!("  - {}", failure);
        }
        panic!("{} test(s) failed", failed);
    }
}
