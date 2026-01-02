using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace VCard.Tests
{
    public class VCardTestCaseData
    {
        [JsonPropertyName("vcards")]
        public List<VCardData> VCards { get; set; } = new List<VCardData>();
    }

    public class VCardData
    {
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("fn")]
        public string? Fn { get; set; }

        [JsonPropertyName("n")]
        public NameData? N { get; set; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("bday")]
        public string? Bday { get; set; }

        [JsonPropertyName("anniversary")]
        public string? Anniversary { get; set; }

        [JsonPropertyName("org")]
        public string? Org { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("telephones")]
        public List<TelephoneData>? Telephones { get; set; }

        [JsonPropertyName("emails")]
        public List<EmailData>? Emails { get; set; }

        [JsonPropertyName("addresses")]
        public List<AddressData>? Addresses { get; set; }

        [JsonPropertyName("geo")]
        public string? Geo { get; set; }

        [JsonPropertyName("tz")]
        public string? Tz { get; set; }

        [JsonPropertyName("urls")]
        public List<UrlData>? Urls { get; set; }

        [JsonPropertyName("notes")]
        public List<NoteData>? Notes { get; set; }

        [JsonPropertyName("categories")]
        public List<CategoryData>? Categories { get; set; }

        [JsonPropertyName("uid")]
        public string? Uid { get; set; }

        [JsonPropertyName("rev")]
        public string? Rev { get; set; }

        [JsonPropertyName("prodid")]
        public string? Prodid { get; set; }

        [JsonPropertyName("kind")]
        public string? Kind { get; set; }

        [JsonPropertyName("members")]
        public List<string>? Members { get; set; }

        [JsonPropertyName("extensions")]
        public Dictionary<string, string>? Extensions { get; set; }
    }

    public class NameData
    {
        [JsonPropertyName("family")]
        public string? Family { get; set; }

        [JsonPropertyName("given")]
        public string? Given { get; set; }

        [JsonPropertyName("additional")]
        public string? Additional { get; set; }

        [JsonPropertyName("prefix")]
        public string? Prefix { get; set; }

        [JsonPropertyName("suffix")]
        public string? Suffix { get; set; }
    }

    public class TelephoneData
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("types")]
        public List<string>? Types { get; set; }
    }

    public class EmailData
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("types")]
        public List<string>? Types { get; set; }
    }

    public class AddressData
    {
        [JsonPropertyName("pobox")]
        public string? Pobox { get; set; }

        [JsonPropertyName("extended")]
        public string? Extended { get; set; }

        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("locality")]
        public string? Locality { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("postalcode")]
        public string? Postalcode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("types")]
        public List<string>? Types { get; set; }
    }

    public class UrlData
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("types")]
        public List<string>? Types { get; set; }
    }

    public class NoteData
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public class CategoryData
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public class VCardTestCaseTests
    {
        private static string GetTestCasesPath()
        {
            // Navigate from tests directory up to repo root, then to testcases
            var currentDir = Directory.GetCurrentDirectory();
            var repoRoot = currentDir;

            // Try to find the testcases directory
            while (!Directory.Exists(Path.Combine(repoRoot, "testcases")) && Directory.GetParent(repoRoot) != null)
            {
                repoRoot = Directory.GetParent(repoRoot)!.FullName;
            }

            var testCasesPath = Path.Combine(repoRoot, "testcases");
            if (!Directory.Exists(testCasesPath))
            {
                throw new DirectoryNotFoundException($"Testcases directory not found. Searched from: {currentDir}");
            }

            return testCasesPath;
        }

        public static IEnumerable<object[]> GetTestCaseFiles()
        {
            var testCasesPath = GetTestCasesPath();
            var vcfFiles = Directory.GetFiles(testCasesPath, "*.vcf")
                .OrderBy(f => f)
                .ToList();

            foreach (var vcfFile in vcfFiles)
            {
                var jsonFile = Path.ChangeExtension(vcfFile, ".json");
                if (File.Exists(jsonFile))
                {
                    yield return new object[] { Path.GetFileName(vcfFile), vcfFile, jsonFile };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetTestCaseFiles))]
        public void ParseVcfAndCompareToJson(string testName, string vcfPath, string jsonPath)
        {
            // Parse the VCF file
            var parser = new VCardParser();
            var vcardText = File.ReadAllText(vcfPath);
            var vcards = parser.Parse(vcardText);

            // Parse the JSON file
            var jsonText = File.ReadAllText(jsonPath);
            var expectedData = JsonSerializer.Deserialize<VCardTestCaseData>(jsonText);

            Assert.NotNull(expectedData);
            Assert.Equal(expectedData.VCards.Count, vcards.Count);

            for (int i = 0; i < vcards.Count; i++)
            {
                var vcard = vcards[i];
                var expected = expectedData.VCards[i];

                // Compare basic properties
                AssertProperty(testName, $"vCard[{i}].VERSION", expected.Version, vcard.Version);
                AssertProperty(testName, $"vCard[{i}].FN", expected.Fn, vcard.FormattedName);

                // Compare structured name
                if (expected.N != null)
                {
                    var actualN = vcard.Name;
                    if (!string.IsNullOrEmpty(actualN))
                    {
                        var parts = actualN.Split(';');
                        AssertProperty(testName, $"vCard[{i}].N.Family", expected.N.Family, parts.Length > 0 ? parts[0] : "");
                        AssertProperty(testName, $"vCard[{i}].N.Given", expected.N.Given, parts.Length > 1 ? parts[1] : "");
                        AssertProperty(testName, $"vCard[{i}].N.Additional", expected.N.Additional, parts.Length > 2 ? parts[2] : "");
                        AssertProperty(testName, $"vCard[{i}].N.Prefix", expected.N.Prefix, parts.Length > 3 ? parts[3] : "");
                        AssertProperty(testName, $"vCard[{i}].N.Suffix", expected.N.Suffix, parts.Length > 4 ? parts[4] : "");
                    }
                }

                // Compare optional properties
                if (expected.Nickname != null)
                    AssertProperty(testName, $"vCard[{i}].NICKNAME", expected.Nickname, vcard.Nickname);
                if (expected.Gender != null)
                    AssertProperty(testName, $"vCard[{i}].GENDER", expected.Gender, vcard.Gender);
                if (expected.Bday != null)
                    AssertProperty(testName, $"vCard[{i}].BDAY", expected.Bday, vcard.Birthday);
                if (expected.Anniversary != null)
                    AssertProperty(testName, $"vCard[{i}].ANNIVERSARY", expected.Anniversary, vcard.Anniversary);
                if (expected.Org != null)
                    AssertProperty(testName, $"vCard[{i}].ORG", expected.Org, vcard.Organization);
                if (expected.Title != null)
                    AssertProperty(testName, $"vCard[{i}].TITLE", expected.Title, vcard.Title);
                if (expected.Role != null)
                    AssertProperty(testName, $"vCard[{i}].ROLE", expected.Role, vcard.Role);
                if (expected.Geo != null)
                    AssertProperty(testName, $"vCard[{i}].GEO", expected.Geo, vcard.Geo);
                if (expected.Tz != null)
                    AssertProperty(testName, $"vCard[{i}].TZ", expected.Tz, vcard.TimeZone);
                if (expected.Uid != null)
                    AssertProperty(testName, $"vCard[{i}].UID", expected.Uid, vcard.Uid);
                if (expected.Rev != null)
                    AssertProperty(testName, $"vCard[{i}].REV", expected.Rev, vcard.Revision);
                if (expected.Prodid != null)
                    AssertProperty(testName, $"vCard[{i}].PRODID", expected.Prodid, vcard.ProductId);
                if (expected.Kind != null)
                    AssertProperty(testName, $"vCard[{i}].KIND", expected.Kind, vcard.Kind);

                // Compare telephones
                if (expected.Telephones != null)
                {
                    Assert.Equal(expected.Telephones.Count, vcard.Telephones.Count);
                    for (int j = 0; j < expected.Telephones.Count; j++)
                    {
                        AssertProperty(testName, $"vCard[{i}].TEL[{j}].Value", expected.Telephones[j].Value, vcard.Telephones[j].Value);
                        if (expected.Telephones[j].Types != null && expected.Telephones[j].Types.Count > 0)
                        {
                            foreach (var type in expected.Telephones[j].Types)
                            {
                                var telType = ParseTelType(type);
                                if (telType != TelType.None)
                                {
                                    Assert.True(vcard.Telephones[j].Types.HasFlag(telType),
                                        $"{testName}: vCard[{i}].TEL[{j}] should have type {type}");
                                }
                            }
                        }
                    }
                }

                // Compare emails
                if (expected.Emails != null)
                {
                    Assert.Equal(expected.Emails.Count, vcard.Emails.Count);
                    for (int j = 0; j < expected.Emails.Count; j++)
                    {
                        AssertProperty(testName, $"vCard[{i}].EMAIL[{j}].Value", expected.Emails[j].Value, vcard.Emails[j].Value);
                        if (expected.Emails[j].Types != null && expected.Emails[j].Types.Count > 0)
                        {
                            foreach (var type in expected.Emails[j].Types)
                            {
                                var emailType = ParseEmailType(type);
                                if (emailType != EmailType.None)
                                {
                                    Assert.True(vcard.Emails[j].Types.HasFlag(emailType),
                                        $"{testName}: vCard[{i}].EMAIL[{j}] should have type {type}");
                                }
                            }
                        }
                    }
                }

                // Compare addresses
                if (expected.Addresses != null)
                {
                    Assert.Equal(expected.Addresses.Count, vcard.Addresses.Count);
                    for (int j = 0; j < expected.Addresses.Count; j++)
                    {
                        var expectedAddr = expected.Addresses[j];
                        var actualAddr = vcard.Addresses[j];
                        AssertProperty(testName, $"vCard[{i}].ADR[{j}].Street", expectedAddr.Street, actualAddr.Street);
                        AssertProperty(testName, $"vCard[{i}].ADR[{j}].City", expectedAddr.Locality, actualAddr.City);
                        AssertProperty(testName, $"vCard[{i}].ADR[{j}].State", expectedAddr.Region, actualAddr.State);
                        AssertProperty(testName, $"vCard[{i}].ADR[{j}].PostalCode", expectedAddr.Postalcode, actualAddr.PostalCode);
                        AssertProperty(testName, $"vCard[{i}].ADR[{j}].Country", expectedAddr.Country, actualAddr.Country);
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetTestCaseFiles))]
        public void CreateFromJsonAndCompareToVcf(string testName, string vcfPath, string jsonPath)
        {
            // Parse the JSON file
            var jsonText = File.ReadAllText(jsonPath);
            var testData = JsonSerializer.Deserialize<VCardTestCaseData>(jsonText);
            Assert.NotNull(testData);

            // Create vCards from JSON data
            var vcards = new List<VCardObject>();
            foreach (var vcardData in testData.VCards)
            {
                var vcard = CreateVCardFromJson(vcardData);
                vcards.Add(vcard);
            }

            // Serialize to vCard format
            var serializer = new VCardSerializer();
            var actualVcfText = vcards.Count == 1
                ? serializer.Serialize(vcards[0])
                : serializer.SerializeMultiple(vcards);

            // Parse both the expected and actual VCF to compare semantically
            var parser = new VCardParser();
            var expectedVcards = parser.Parse(File.ReadAllText(vcfPath));
            var actualVcards = parser.Parse(actualVcfText);

            Assert.Equal(expectedVcards.Count, actualVcards.Count);

            for (int i = 0; i < expectedVcards.Count; i++)
            {
                var expected = expectedVcards[i];
                var actual = actualVcards[i];

                // Compare key properties (semantic comparison, not string comparison)
                AssertProperty(testName, $"Serialized vCard[{i}].VERSION", expected.Version, actual.Version);
                AssertProperty(testName, $"Serialized vCard[{i}].FN", expected.FormattedName, actual.FormattedName);

                if (!string.IsNullOrEmpty(expected.Name))
                    AssertProperty(testName, $"Serialized vCard[{i}].N", expected.Name, actual.Name);

                Assert.Equal(expected.Telephones.Count, actual.Telephones.Count);
                Assert.Equal(expected.Emails.Count, actual.Emails.Count);
                Assert.Equal(expected.Addresses.Count, actual.Addresses.Count);
            }
        }

        private VCardObject CreateVCardFromJson(VCardData data)
        {
            var vcard = new VCardObject
            {
                Version = data.Version ?? "4.0",
                FormattedName = data.Fn ?? ""
            };

            // Set structured name
            if (data.N != null)
            {
                vcard.Name = $"{data.N.Family ?? ""};{data.N.Given ?? ""};{data.N.Additional ?? ""};{data.N.Prefix ?? ""};{data.N.Suffix ?? ""}";
            }

            // Set optional properties
            if (data.Nickname != null) vcard.Nickname = data.Nickname;
            if (data.Gender != null) vcard.Gender = data.Gender;
            if (data.Bday != null) vcard.Birthday = data.Bday;
            if (data.Anniversary != null) vcard.Anniversary = data.Anniversary;
            if (data.Org != null) vcard.Organization = data.Org;
            if (data.Title != null) vcard.Title = data.Title;
            if (data.Role != null) vcard.Role = data.Role;
            if (data.Geo != null) vcard.Geo = data.Geo;
            if (data.Tz != null) vcard.TimeZone = data.Tz;
            if (data.Uid != null) vcard.Uid = data.Uid;
            if (data.Rev != null) vcard.Revision = data.Rev;
            if (data.Prodid != null) vcard.ProductId = data.Prodid;
            if (data.Kind != null) vcard.Kind = data.Kind;

            // Add telephones
            if (data.Telephones != null)
            {
                vcard.Telephones = data.Telephones.Select(t => new Telephone
                {
                    Value = t.Value ?? "",
                    Types = t.Types?.Aggregate(TelType.None, (acc, type) => acc | ParseTelType(type)) ?? TelType.None
                }).ToList();
            }

            // Add emails
            if (data.Emails != null)
            {
                vcard.Emails = data.Emails.Select(e => new Email
                {
                    Value = e.Value ?? "",
                    Types = e.Types?.Aggregate(EmailType.None, (acc, type) => acc | ParseEmailType(type)) ?? EmailType.None
                }).ToList();
            }

            // Add addresses
            if (data.Addresses != null)
            {
                vcard.Addresses = data.Addresses.Select(a => new Address
                {
                    PostOfficeBox = a.Pobox ?? "",
                    ExtendedAddress = a.Extended ?? "",
                    Street = a.Street ?? "",
                    City = a.Locality ?? "",
                    State = a.Region ?? "",
                    PostalCode = a.Postalcode ?? "",
                    Country = a.Country ?? "",
                    Types = a.Types?.Aggregate(AdrType.None, (acc, type) => acc | ParseAdrType(type)) ?? AdrType.None
                }).ToList();
            }

            // Add URLs
            if (data.Urls != null)
            {
                foreach (var url in data.Urls)
                {
                    vcard.AddProperty(new VCardProperty("URL", url.Value ?? ""));
                }
            }

            // Add notes
            if (data.Notes != null)
            {
                foreach (var note in data.Notes)
                {
                    vcard.AddProperty(new VCardProperty("NOTE", note.Value ?? ""));
                }
            }

            // Add categories
            if (data.Categories != null)
            {
                foreach (var category in data.Categories)
                {
                    vcard.AddProperty(new VCardProperty("CATEGORIES", category.Value ?? ""));
                }
            }

            // Add extensions
            if (data.Extensions != null)
            {
                foreach (var ext in data.Extensions)
                {
                    vcard.AddProperty(new VCardProperty(ext.Key, ext.Value));
                }
            }

            return vcard;
        }

        private void AssertProperty(string testName, string propertyPath, string? expected, string? actual)
        {
            if (expected == null) return;
            var actualValue = actual ?? "";
            Assert.True(expected == actualValue,
                $"{testName}: Property {propertyPath} mismatch. Expected '{expected}', got '{actualValue}'");
        }

        private TelType ParseTelType(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "text" => TelType.Text,
                "voice" => TelType.Voice,
                "fax" => TelType.Fax,
                "cell" => TelType.Cell,
                "video" => TelType.Video,
                "pager" => TelType.Pager,
                "textphone" => TelType.TextPhone,
                "work" => TelType.Work,
                "home" => TelType.Home,
                _ => TelType.None
            };
        }

        private EmailType ParseEmailType(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "work" => EmailType.Work,
                "home" => EmailType.Home,
                "internet" => EmailType.Internet,
                _ => EmailType.None
            };
        }

        private AdrType ParseAdrType(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "work" => AdrType.Work,
                "home" => AdrType.Home,
                "postal" => AdrType.Postal,
                "parcel" => AdrType.Parcel,
                "dom" => AdrType.Dom,
                "intl" => AdrType.Intl,
                _ => AdrType.None
            };
        }
    }
}
