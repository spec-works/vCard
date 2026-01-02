using System;
using System.Collections.Generic;
using System.Linq;

namespace VCard
{
    // ============================================================================
    // Strongly Typed Enums with Flags Support
    // ============================================================================

    /// <summary>
    /// Telephone type parameter values (can be combined with bitwise OR)
    /// </summary>
    [Flags]
    public enum TelType
    {
        None = 0,
        Text = 1 << 0,
        Voice = 1 << 1,
        Fax = 1 << 2,
        Cell = 1 << 3,
        Video = 1 << 4,
        Pager = 1 << 5,
        TextPhone = 1 << 6,
        Work = 1 << 7,
        Home = 1 << 8
    }

    /// <summary>
    /// Email type parameter values (can be combined with bitwise OR)
    /// </summary>
    [Flags]
    public enum EmailType
    {
        None = 0,
        Work = 1 << 0,
        Home = 1 << 1,
        Internet = 1 << 2
    }

    /// <summary>
    /// Address type parameter values (can be combined with bitwise OR)
    /// </summary>
    [Flags]
    public enum AdrType
    {
        None = 0,
        Work = 1 << 0,
        Home = 1 << 1,
        Postal = 1 << 2,
        Parcel = 1 << 3,
        Dom = 1 << 4,
        Intl = 1 << 5
    }

    // ============================================================================
    // Strongly Typed Property Classes
    // ============================================================================

    /// <summary>
    /// Represents a telephone number with type information
    /// </summary>
    public class Telephone
    {
        /// <summary>
        /// The telephone number value
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The type(s) of this telephone number (can be combined with bitwise OR)
        /// </summary>
        public TelType Types { get; set; } = TelType.None;

        /// <summary>
        /// Convert to VCardProperty for serialization
        /// </summary>
        internal VCardProperty ToProperty()
        {
            var prop = new VCardProperty("TEL", Value);

            // Add each flag as a separate TYPE parameter
            foreach (TelType type in Enum.GetValues(typeof(TelType)))
            {
                if (type != TelType.None && Types.HasFlag(type))
                {
                    prop.AddParameter("TYPE", type.ToString().ToLowerInvariant());
                }
            }

            return prop;
        }

        /// <summary>
        /// Create from VCardProperty during parsing
        /// </summary>
        internal static Telephone FromProperty(VCardProperty property)
        {
            var tel = new Telephone { Value = property.Value };

            var types = property.GetParameters("TYPE");
            foreach (var type in types)
            {
                if (Enum.TryParse<TelType>(type, true, out var telType))
                {
                    tel.Types |= telType;
                }
            }

            return tel;
        }
    }

    /// <summary>
    /// Represents an email address with type information
    /// </summary>
    public class Email
    {
        /// <summary>
        /// The email address value
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The type(s) of this email address (can be combined with bitwise OR)
        /// </summary>
        public EmailType Types { get; set; } = EmailType.None;

        /// <summary>
        /// Convert to VCardProperty for serialization
        /// </summary>
        internal VCardProperty ToProperty()
        {
            var prop = new VCardProperty("EMAIL", Value);

            foreach (EmailType type in Enum.GetValues(typeof(EmailType)))
            {
                if (type != EmailType.None && Types.HasFlag(type))
                {
                    prop.AddParameter("TYPE", type.ToString().ToLowerInvariant());
                }
            }

            return prop;
        }

        /// <summary>
        /// Create from VCardProperty during parsing
        /// </summary>
        internal static Email FromProperty(VCardProperty property)
        {
            var email = new Email { Value = property.Value };

            var types = property.GetParameters("TYPE");
            foreach (var type in types)
            {
                if (Enum.TryParse<EmailType>(type, true, out var emailType))
                {
                    email.Types |= emailType;
                }
            }

            return email;
        }
    }

    /// <summary>
    /// Represents a postal address with type information
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Post office box
        /// </summary>
        public string PostOfficeBox { get; set; } = string.Empty;

        /// <summary>
        /// Extended address (e.g., apartment or suite number)
        /// </summary>
        public string ExtendedAddress { get; set; } = string.Empty;

        /// <summary>
        /// Street address
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// City or locality
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// State or region
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Postal code or ZIP code
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// The type(s) of this address (can be combined with bitwise OR)
        /// </summary>
        public AdrType Types { get; set; } = AdrType.None;

        /// <summary>
        /// Convert to VCardProperty for serialization
        /// </summary>
        internal VCardProperty ToProperty()
        {
            var value = $"{PostOfficeBox};{ExtendedAddress};{Street};{City};{State};{PostalCode};{Country}";
            var prop = new VCardProperty("ADR", value);

            foreach (AdrType type in Enum.GetValues(typeof(AdrType)))
            {
                if (type != AdrType.None && Types.HasFlag(type))
                {
                    prop.AddParameter("TYPE", type.ToString().ToLowerInvariant());
                }
            }

            return prop;
        }

        /// <summary>
        /// Create from VCardProperty during parsing
        /// </summary>
        internal static Address FromProperty(VCardProperty property)
        {
            var parts = property.Value.Split(';');
            var address = new Address
            {
                PostOfficeBox = parts.Length > 0 ? parts[0] : "",
                ExtendedAddress = parts.Length > 1 ? parts[1] : "",
                Street = parts.Length > 2 ? parts[2] : "",
                City = parts.Length > 3 ? parts[3] : "",
                State = parts.Length > 4 ? parts[4] : "",
                PostalCode = parts.Length > 5 ? parts[5] : "",
                Country = parts.Length > 6 ? parts[6] : ""
            };

            var types = property.GetParameters("TYPE");
            foreach (var type in types)
            {
                if (Enum.TryParse<AdrType>(type, true, out var adrType))
                {
                    address.Types |= adrType;
                }
            }

            return address;
        }
    }
}
