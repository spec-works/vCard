using System;
using System.Collections.Generic;
using System.Linq;

namespace VCard
{
    /// <summary>
    /// Base class for all vCard components
    /// </summary>
    public abstract class VCardComponent
    {
        public Dictionary<string, List<VCardProperty>> Properties { get; set; } = new Dictionary<string, List<VCardProperty>>();

        public void AddProperty(VCardProperty property)
        {
            if (!Properties.ContainsKey(property.Name))
            {
                Properties[property.Name] = new List<VCardProperty>();
            }
            Properties[property.Name].Add(property);
        }

        public VCardProperty GetProperty(string name)
        {
            return Properties.ContainsKey(name) ? Properties[name].FirstOrDefault() : null;
        }

        public List<VCardProperty> GetProperties(string name)
        {
            return Properties.ContainsKey(name) ? Properties[name] : new List<VCardProperty>();
        }

        public abstract string ComponentType { get; }
    }

    /// <summary>
    /// Represents a vCard property with parameters and value
    /// </summary>
    public class VCardProperty
    {
        public string Name { get; set; }
        public Dictionary<string, List<string>> Parameters { get; set; } = new Dictionary<string, List<string>>();
        public string Value { get; set; }

        public VCardProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public void AddParameter(string paramName, string paramValue)
        {
            if (!Parameters.ContainsKey(paramName))
            {
                Parameters[paramName] = new List<string>();
            }
            Parameters[paramName].Add(paramValue);
        }

        public string GetParameter(string paramName)
        {
            return Parameters.ContainsKey(paramName) ? Parameters[paramName].FirstOrDefault() : null;
        }

        public List<string> GetParameters(string paramName)
        {
            return Parameters.ContainsKey(paramName) ? Parameters[paramName] : new List<string>();
        }
    }

    /// <summary>
    /// Root vCard object (VCARD)
    /// </summary>
    public class VCardObject : VCardComponent
    {
        public override string ComponentType => "VCARD";

        // Required properties
        public string Version
        {
            get => GetProperty("VERSION")?.Value;
            set => AddProperty(new VCardProperty("VERSION", value));
        }

        public string FormattedName
        {
            get => GetProperty("FN")?.Value;
            set => AddProperty(new VCardProperty("FN", value));
        }

        // Identification properties
        public string Name
        {
            get => GetProperty("N")?.Value;
            set => AddProperty(new VCardProperty("N", value));
        }

        public string Nickname
        {
            get => GetProperty("NICKNAME")?.Value;
            set => AddProperty(new VCardProperty("NICKNAME", value));
        }

        public string Photo
        {
            get => GetProperty("PHOTO")?.Value;
            set => AddProperty(new VCardProperty("PHOTO", value));
        }

        public string Birthday
        {
            get => GetProperty("BDAY")?.Value;
            set => AddProperty(new VCardProperty("BDAY", value));
        }

        public string Anniversary
        {
            get => GetProperty("ANNIVERSARY")?.Value;
            set => AddProperty(new VCardProperty("ANNIVERSARY", value));
        }

        public string Gender
        {
            get => GetProperty("GENDER")?.Value;
            set => AddProperty(new VCardProperty("GENDER", value));
        }

        // Strongly-typed communication and addressing properties
        private List<Telephone> _telephones = new List<Telephone>();
        private List<Email> _emails = new List<Email>();
        private List<Address> _addresses = new List<Address>();

        /// <summary>
        /// Telephone numbers with type information
        /// </summary>
        public List<Telephone> Telephones
        {
            get => _telephones;
            set => _telephones = value ?? new List<Telephone>();
        }

        /// <summary>
        /// Email addresses with type information
        /// </summary>
        public List<Email> Emails
        {
            get => _emails;
            set => _emails = value ?? new List<Email>();
        }

        /// <summary>
        /// Postal addresses with type information
        /// </summary>
        public List<Address> Addresses
        {
            get => _addresses;
            set => _addresses = value ?? new List<Address>();
        }

        // Other communications properties (kept as generic for now)
        public List<VCardProperty> Impps => GetProperties("IMPP");
        public List<VCardProperty> Languages => GetProperties("LANG");

        // Geographical properties
        public string TimeZone
        {
            get => GetProperty("TZ")?.Value;
            set => AddProperty(new VCardProperty("TZ", value));
        }

        public string Geo
        {
            get => GetProperty("GEO")?.Value;
            set => AddProperty(new VCardProperty("GEO", value));
        }

        // Organizational properties
        public string Title
        {
            get => GetProperty("TITLE")?.Value;
            set => AddProperty(new VCardProperty("TITLE", value));
        }

        public string Role
        {
            get => GetProperty("ROLE")?.Value;
            set => AddProperty(new VCardProperty("ROLE", value));
        }

        public string Logo
        {
            get => GetProperty("LOGO")?.Value;
            set => AddProperty(new VCardProperty("LOGO", value));
        }

        public string Organization
        {
            get => GetProperty("ORG")?.Value;
            set => AddProperty(new VCardProperty("ORG", value));
        }

        public List<VCardProperty> Members => GetProperties("MEMBER");
        public List<VCardProperty> Related => GetProperties("RELATED");

        // Explanatory properties
        public List<VCardProperty> Categories => GetProperties("CATEGORIES");
        public List<VCardProperty> Notes => GetProperties("NOTE");

        public string ProductId
        {
            get => GetProperty("PRODID")?.Value;
            set => AddProperty(new VCardProperty("PRODID", value));
        }

        public string Revision
        {
            get => GetProperty("REV")?.Value;
            set => AddProperty(new VCardProperty("REV", value));
        }

        public string Sound
        {
            get => GetProperty("SOUND")?.Value;
            set => AddProperty(new VCardProperty("SOUND", value));
        }

        public string Uid
        {
            get => GetProperty("UID")?.Value;
            set => AddProperty(new VCardProperty("UID", value));
        }

        public List<VCardProperty> ClientPidMaps => GetProperties("CLIENTPIDMAP");

        public List<VCardProperty> Urls => GetProperties("URL");
        public List<VCardProperty> Keys => GetProperties("KEY");

        // Security properties
        public string Kind
        {
            get => GetProperty("KIND")?.Value;
            set => AddProperty(new VCardProperty("KIND", value));
        }

        // Calendar properties
        public List<VCardProperty> FbUrls => GetProperties("FBURL");
        public List<VCardProperty> CalAdrs => GetProperties("CALADRURI");
        public List<VCardProperty> CalUris => GetProperties("CALURI");
    }

    /// <summary>
    /// Represents a structured name value
    /// </summary>
    public class StructuredName
    {
        public string FamilyName { get; set; }
        public string GivenName { get; set; }
        public string AdditionalNames { get; set; }
        public string HonorificPrefixes { get; set; }
        public string HonorificSuffixes { get; set; }

        public static StructuredName Parse(string value)
        {
            var parts = value.Split(';');
            return new StructuredName
            {
                FamilyName = parts.Length > 0 ? parts[0] : "",
                GivenName = parts.Length > 1 ? parts[1] : "",
                AdditionalNames = parts.Length > 2 ? parts[2] : "",
                HonorificPrefixes = parts.Length > 3 ? parts[3] : "",
                HonorificSuffixes = parts.Length > 4 ? parts[4] : ""
            };
        }

        public override string ToString()
        {
            return $"{FamilyName};{GivenName};{AdditionalNames};{HonorificPrefixes};{HonorificSuffixes}";
        }
    }

    /// <summary>
    /// Represents a structured address value
    /// </summary>
    public class StructuredAddress
    {
        public string PostOfficeBox { get; set; }
        public string ExtendedAddress { get; set; }
        public string StreetAddress { get; set; }
        public string Locality { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string CountryName { get; set; }

        public static StructuredAddress Parse(string value)
        {
            var parts = value.Split(';');
            return new StructuredAddress
            {
                PostOfficeBox = parts.Length > 0 ? parts[0] : "",
                ExtendedAddress = parts.Length > 1 ? parts[1] : "",
                StreetAddress = parts.Length > 2 ? parts[2] : "",
                Locality = parts.Length > 3 ? parts[3] : "",
                Region = parts.Length > 4 ? parts[4] : "",
                PostalCode = parts.Length > 5 ? parts[5] : "",
                CountryName = parts.Length > 6 ? parts[6] : ""
            };
        }

        public override string ToString()
        {
            return $"{PostOfficeBox};{ExtendedAddress};{StreetAddress};{Locality};{Region};{PostalCode};{CountryName}";
        }
    }
}
