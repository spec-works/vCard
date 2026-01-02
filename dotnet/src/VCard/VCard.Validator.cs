using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VCard
{
    /// <summary>
    /// Validator for vCard components according to RFC 6350
    /// </summary>
    public class VCardValidator
    {
        public ValidationResult Validate(VCardObject vcard)
        {
            var result = new ValidationResult();

            ValidateVCard(vcard, result);

            return result;
        }

        private void ValidateVCard(VCardObject vcard, ValidationResult result)
        {
            // VCARD MUST have VERSION and FN
            ValidateRequiredProperty(vcard, "VERSION", result);
            ValidateRequiredProperty(vcard, "FN", result);

            // VERSION must be 4.0 (or 3.0, 2.1 for backward compatibility)
            var version = vcard.GetProperty("VERSION");
            if (version != null)
            {
                var validVersions = new[] { "4.0", "3.0", "2.1" };
                if (!validVersions.Contains(version.Value))
                {
                    result.AddError($"VERSION must be one of: {string.Join(", ", validVersions)}, found: {version.Value}");
                }
                else if (version.Value != "4.0")
                {
                    result.AddWarning($"VERSION {version.Value} is supported but deprecated. Consider upgrading to 4.0");
                }
            }

            // FN (Formatted Name) must not be empty
            var fn = vcard.GetProperty("FN");
            if (fn != null && string.IsNullOrWhiteSpace(fn.Value))
            {
                result.AddError("FN (Formatted Name) cannot be empty");
            }

            // Validate N (Name) format if present
            var name = vcard.GetProperty("N");
            if (name != null)
            {
                ValidateStructuredName(name, result);
            }

            // Validate all telephone numbers
            foreach (var tel in vcard.Telephones)
            {
                ValidateTelephoneProperty(tel, result);
            }

            // Validate all email addresses
            foreach (var email in vcard.Emails)
            {
                ValidateEmailProperty(email, result);
            }

            // Validate all addresses
            foreach (var adr in vcard.Addresses)
            {
                ValidateAddressProperty(adr, result);
            }

            // Validate BDAY (Birthday) format if present
            var bday = vcard.GetProperty("BDAY");
            if (bday != null)
            {
                ValidateDateFormat(bday, result, "BDAY");
            }

            // Validate ANNIVERSARY format if present
            var anniversary = vcard.GetProperty("ANNIVERSARY");
            if (anniversary != null)
            {
                ValidateDateFormat(anniversary, result, "ANNIVERSARY");
            }

            // Validate GEO (Geographic Position) format if present
            var geo = vcard.GetProperty("GEO");
            if (geo != null)
            {
                ValidateGeoFormat(geo, result);
            }

            // Validate TZ (Time Zone) format if present
            var tz = vcard.GetProperty("TZ");
            if (tz != null)
            {
                ValidateTimeZoneFormat(tz, result);
            }

            // Validate UID format if present
            var uid = vcard.GetProperty("UID");
            if (uid != null && string.IsNullOrWhiteSpace(uid.Value))
            {
                result.AddError("UID cannot be empty if present");
            }

            // Validate URLs
            foreach (var url in vcard.Urls)
            {
                ValidateUrlFormat(url, result);
            }
        }

        private void ValidateRequiredProperty(VCardComponent component, string propertyName, ValidationResult result)
        {
            var property = component.GetProperty(propertyName);
            if (property == null)
            {
                result.AddError($"Required property {propertyName} is missing");
            }
        }

        private void ValidateStructuredName(VCardProperty property, ValidationResult result)
        {
            // N property format: Family;Given;Additional;Prefix;Suffix
            var parts = property.Value.Split(';');
            if (parts.Length > 5)
            {
                result.AddWarning($"N property has more than 5 components: {property.Value}");
            }

            // At least one component should be non-empty
            if (parts.All(p => string.IsNullOrWhiteSpace(p)))
            {
                result.AddError("N property must have at least one non-empty component");
            }
        }

        private void ValidateTelephoneProperty(Telephone telephone, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(telephone.Value))
            {
                result.AddError("TEL property cannot be empty");
            }

            // Validate TYPE flags if present
            if (telephone.Types != TelType.None)
            {
                var validTypes = TelType.Text | TelType.Voice | TelType.Fax | TelType.Cell |
                                TelType.Video | TelType.Pager | TelType.TextPhone | TelType.Work | TelType.Home;

                if ((telephone.Types & ~validTypes) != 0)
                {
                    result.AddWarning($"TEL TYPE parameter has non-standard value");
                }
            }
        }

        private void ValidateEmailProperty(Email email, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(email.Value))
            {
                result.AddError("EMAIL property cannot be empty");
                return;
            }

            // Basic email validation
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email.Value, emailPattern))
            {
                result.AddWarning($"EMAIL property may not be a valid email address: {email.Value}");
            }

            // Validate TYPE flags if present
            if (email.Types != EmailType.None)
            {
                var validTypes = EmailType.Work | EmailType.Home | EmailType.Internet;

                if ((email.Types & ~validTypes) != 0)
                {
                    result.AddWarning($"EMAIL TYPE parameter has non-standard value");
                }
            }
        }

        private void ValidateAddressProperty(Address address, ValidationResult result)
        {
            // Validate TYPE flags if present
            if (address.Types != AdrType.None)
            {
                var validTypes = AdrType.Work | AdrType.Home | AdrType.Postal | AdrType.Parcel | AdrType.Dom | AdrType.Intl;

                if ((address.Types & ~validTypes) != 0)
                {
                    result.AddWarning($"ADR TYPE parameter has non-standard value");
                }
            }

            // All address components are optional, so no required validation needed
        }

        private void ValidateDateFormat(VCardProperty property, ValidationResult result, string propertyName)
        {
            var value = property.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                result.AddError($"{propertyName} cannot be empty");
                return;
            }

            // vCard 4.0 supports multiple date formats:
            // - Complete date: YYYYMMDD or YYYY-MM-DD
            // - Partial date: --MMDD or --MM-DD
            // - Reduced precision: YYYY-MM or YYYY
            // - Date-time: YYYYMMDDTHHMMSS or with timezone

            var patterns = new[]
            {
                @"^\d{8}$",                           // YYYYMMDD
                @"^\d{4}-\d{2}-\d{2}$",               // YYYY-MM-DD
                @"^--\d{4}$",                         // --MMDD
                @"^--\d{2}-\d{2}$",                   // --MM-DD
                @"^\d{4}-\d{2}$",                     // YYYY-MM
                @"^\d{4}$",                           // YYYY
                @"^\d{8}T\d{6}$",                     // YYYYMMDDTHHMMSS
                @"^\d{8}T\d{6}Z$",                    // YYYYMMDDTHHMMSSZ
                @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$"  // YYYY-MM-DDTHH:MM:SS
            };

            bool isValid = patterns.Any(p => Regex.IsMatch(value, p));
            if (!isValid)
            {
                result.AddWarning($"{propertyName} has non-standard date format: {value}");
            }
        }

        private void ValidateGeoFormat(VCardProperty property, ValidationResult result)
        {
            var value = property.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                result.AddError("GEO property cannot be empty");
                return;
            }

            // GEO format: "geo:latitude,longitude" (RFC 5870)
            var geoPattern = @"^geo:[-+]?\d+\.?\d*,[-+]?\d+\.?\d*$";
            if (!Regex.IsMatch(value, geoPattern))
            {
                result.AddWarning($"GEO property may not be in correct format (expected geo:lat,long): {value}");
            }
        }

        private void ValidateTimeZoneFormat(VCardProperty property, ValidationResult result)
        {
            var value = property.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                result.AddError("TZ property cannot be empty");
                return;
            }

            // TZ can be either a UTC offset or a timezone ID
            // UTC offset format: +/-HHMM or +/-HH:MM
            // Timezone ID: text like "America/New_York"
            var utcOffsetPattern = @"^[+-]\d{2}:?\d{2}$";

            if (!Regex.IsMatch(value, utcOffsetPattern) && !value.Contains("/"))
            {
                result.AddWarning($"TZ property may not be in correct format: {value}");
            }
        }

        private void ValidateUrlFormat(VCardProperty property, ValidationResult result)
        {
            var value = property.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                result.AddError("URL property cannot be empty");
                return;
            }

            // Basic URL validation
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                result.AddWarning($"URL property may not be a valid URL: {value}");
            }
            else if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                result.AddWarning($"URL property uses non-HTTP(S) scheme: {value}");
            }
        }
    }

    /// <summary>
    /// Result of validation operation
    /// </summary>
    public class ValidationResult
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public bool IsValid => Errors.Count == 0;

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }

        public string GetSummary()
        {
            var summary = $"Validation Result: {(IsValid ? "VALID" : "INVALID")}\n";
            summary += $"Errors: {Errors.Count}\n";
            summary += $"Warnings: {Warnings.Count}\n";

            if (Errors.Count > 0)
            {
                summary += "\nErrors:\n";
                foreach (var error in Errors)
                {
                    summary += $"  - {error}\n";
                }
            }

            if (Warnings.Count > 0)
            {
                summary += "\nWarnings:\n";
                foreach (var warning in Warnings)
                {
                    summary += $"  - {warning}\n";
                }
            }

            return summary;
        }
    }
}
