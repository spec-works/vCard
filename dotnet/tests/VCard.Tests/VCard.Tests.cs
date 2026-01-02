using System;
using System.Linq;
using Xunit;

namespace VCard.Tests
{
    public class VCardParserTests
    {
        [Fact]
        public void Parse_SimpleVCard_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.NotNull(vcards);
            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Equal("4.0", vcard.Version);
            Assert.Equal("John Doe", vcard.FormattedName);
        }

        [Fact]
        public void Parse_VCardWithStructuredName_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
N:Doe;John;Michael;Mr.;Jr.
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Equal("Doe;John;Michael;Mr.;Jr.", vcard.Name);

            var structuredName = StructuredName.Parse(vcard.Name);
            Assert.Equal("Doe", structuredName.FamilyName);
            Assert.Equal("John", structuredName.GivenName);
            Assert.Equal("Michael", structuredName.AdditionalNames);
            Assert.Equal("Mr.", structuredName.HonorificPrefixes);
            Assert.Equal("Jr.", structuredName.HonorificSuffixes);
        }

        [Fact]
        public void Parse_VCardWithTelephone_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
TEL;TYPE=work,voice:+1-555-555-1234
TEL;TYPE=home:555-555-5678
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Equal(2, vcard.Telephones.Count);

            var workTel = vcard.Telephones[0];
            Assert.Equal("+1-555-555-1234", workTel.Value);
            Assert.True(workTel.Types.HasFlag(TelType.Work));
            Assert.True(workTel.Types.HasFlag(TelType.Voice));

            var homeTel = vcard.Telephones[1];
            Assert.Equal("555-555-5678", homeTel.Value);
            Assert.True(homeTel.Types.HasFlag(TelType.Home));
        }

        [Fact]
        public void Parse_VCardWithEmail_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
EMAIL;TYPE=work:john.doe@example.com
EMAIL;TYPE=home:john@home.com
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Equal(2, vcard.Emails.Count);

            var workEmail = vcard.Emails[0];
            Assert.Equal("john.doe@example.com", workEmail.Value);
            Assert.True(workEmail.Types.HasFlag(EmailType.Work));

            var homeEmail = vcard.Emails[1];
            Assert.Equal("john@home.com", homeEmail.Value);
            Assert.True(homeEmail.Types.HasFlag(EmailType.Home));
        }

        [Fact]
        public void Parse_VCardWithAddress_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
ADR;TYPE=work:;;123 Main St;Springfield;IL;62701;USA
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Single(vcard.Addresses);

            var address = vcard.Addresses[0];
            Assert.True(address.Types.HasFlag(AdrType.Work));
            Assert.Equal("123 Main St", address.Street);
            Assert.Equal("Springfield", address.City);
            Assert.Equal("IL", address.State);
            Assert.Equal("62701", address.PostalCode);
            Assert.Equal("USA", address.Country);
        }

        [Fact]
        public void Parse_VCardWithOrganization_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
ORG:ABC Corporation;Engineering Department
TITLE:Software Engineer
ROLE:Developer
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Equal("ABC Corporation;Engineering Department", vcard.Organization);
            Assert.Equal("Software Engineer", vcard.Title);
            Assert.Equal("Developer", vcard.Role);
        }

        [Fact]
        public void Parse_VCardWithBirthday_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
BDAY:19850415
ANNIVERSARY:20100612
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Equal("19850415", vcard.Birthday);
            Assert.Equal("20100612", vcard.Anniversary);
        }

        [Fact]
        public void Parse_VCardWithGeo_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
GEO:geo:37.386013,-122.082932
TZ:-05:00
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Equal("geo:37.386013,-122.082932", vcard.Geo);
            Assert.Equal("-05:00", vcard.TimeZone);
        }

        [Fact]
        public void Parse_VCardWithUrl_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
URL:https://www.example.com
URL;TYPE=work:https://work.example.com
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Equal(2, vcard.Urls.Count);
            Assert.Equal("https://www.example.com", vcard.Urls[0].Value);
            Assert.Equal("https://work.example.com", vcard.Urls[1].Value);
        }

        [Fact]
        public void Parse_VCardWithNote_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
NOTE:This is a test note about the contact.
CATEGORIES:Friends,Colleagues
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Single(vcard.Notes);
            Assert.Equal("This is a test note about the contact.", vcard.Notes[0].Value);
            Assert.Single(vcard.Categories);
            Assert.Equal("Friends,Colleagues", vcard.Categories[0].Value);
        }

        [Fact]
        public void Parse_VCardWithUid_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
UID:urn:uuid:4fbe8971-0bc3-424c-9c26-36c3e1eff6b1
REV:20231201T120000Z
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Equal("urn:uuid:4fbe8971-0bc3-424c-9c26-36c3e1eff6b1", vcard.Uid);
            Assert.Equal("20231201T120000Z", vcard.Revision);
        }

        [Fact]
        public void Parse_UnfoldedLines_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
NOTE:This is a very long note that spans multiple lines and should be
  unfolded correctly when parsed by the vCard parser implementation.
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Single(vcard.Notes);
            var expectedNote = "This is a very long note that spans multiple lines and should be unfolded correctly when parsed by the vCard parser implementation.";
            Assert.Equal(expectedNote, vcard.Notes[0].Value);
        }

        [Fact]
        public void Parse_PropertyWithEscapedCharacters_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
NOTE:Line 1\nLine 2\;with semicolon\,and comma
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.Single(vcards);
            var vcard = vcards[0];
            Assert.Single(vcard.Notes);
            Assert.Contains("\n", vcard.Notes[0].Value);
            Assert.Contains(";", vcard.Notes[0].Value);
            Assert.Contains(",", vcard.Notes[0].Value);
        }

        [Fact]
        public void Parse_MultipleVCards_Success()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
END:VCARD
BEGIN:VCARD
VERSION:4.0
FN:Jane Smith
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(vcardData);

            Assert.NotNull(vcards);
            Assert.Equal(2, vcards.Count);
            Assert.Equal("John Doe", vcards[0].FormattedName);
            Assert.Equal("Jane Smith", vcards[1].FormattedName);
        }

        [Fact]
        public void Parse_InvalidVCard_ThrowsException()
        {
            var vcardData = @"VERSION:4.0
FN:John Doe
END:VCARD";

            var parser = new VCardParser();
            Assert.Throws<ParseException>(() => parser.Parse(vcardData));
        }

        [Fact]
        public void Parse_MismatchedEndTag_ThrowsException()
        {
            var vcardData = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
END:VCALENDAR";

            var parser = new VCardParser();
            Assert.Throws<ParseException>(() => parser.Parse(vcardData));
        }
    }

    public class VCardSerializerTests
    {
        [Fact]
        public void Serialize_SimpleVCard_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe"
            };

            var serializer = new VCardSerializer();
            var result = serializer.Serialize(vcard);

            Assert.Contains("BEGIN:VCARD", result);
            Assert.Contains("VERSION:4.0", result);
            Assert.Contains("FN:John Doe", result);
            Assert.Contains("END:VCARD", result);
        }

        [Fact]
        public void Serialize_VCardWithAllProperties_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe",
                Name = "Doe;John;;;",
                Organization = "ABC Corp",
                Title = "Engineer",
                Telephones = new System.Collections.Generic.List<Telephone>
                {
                    new Telephone { Value = "+1-555-555-1234", Types = TelType.Work }
                },
                Emails = new System.Collections.Generic.List<Email>
                {
                    new Email { Value = "john@example.com", Types = EmailType.Work }
                }
            };

            var serializer = new VCardSerializer();
            var result = serializer.Serialize(vcard);

            Assert.Contains("BEGIN:VCARD", result);
            Assert.Contains("VERSION:4.0", result);
            Assert.Contains("FN:John Doe", result);
            Assert.Contains(@"N:Doe\;John\;\;\;", result);
            Assert.Contains("TEL;TYPE=work:+1-555-555-1234", result);
            Assert.Contains("EMAIL;TYPE=work:john@example.com", result);
            Assert.Contains("END:VCARD", result);
        }

        [Fact]
        public void Serialize_EscapesSpecialCharacters()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe"
            };

            var noteProp = new VCardProperty("NOTE", "Line 1\nLine 2;with semicolon,and comma");
            vcard.AddProperty(noteProp);

            var serializer = new VCardSerializer();
            var result = serializer.Serialize(vcard);

            Assert.Contains("\\n", result);
            Assert.Contains("\\;", result);
            Assert.Contains("\\,", result);
        }

        [Fact]
        public void Serialize_FoldsLongLines()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe"
            };

            var longNote = new string('A', 100);
            var noteProp = new VCardProperty("NOTE", longNote);
            vcard.AddProperty(noteProp);

            var serializer = new VCardSerializer();
            var result = serializer.Serialize(vcard);

            // Check for line folding (lines starting with space)
            Assert.Contains("\r\n ", result);
        }

        [Fact]
        public void Serialize_ExtensionMethod_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe"
            };

            var result = vcard.ToVCard();

            Assert.Contains("BEGIN:VCARD", result);
            Assert.Contains("VERSION:4.0", result);
            Assert.Contains("FN:John Doe", result);
            Assert.Contains("END:VCARD", result);
        }
    }

    public class VCardValidatorTests
    {
        [Fact]
        public void Validate_ValidVCard_NoErrors()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe"
            };

            var validator = new VCardValidator();
            var result = validator.Validate(vcard);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_MissingVersion_HasError()
        {
            var vcard = new VCardObject
            {
                FormattedName = "John Doe"
            };

            var validator = new VCardValidator();
            var result = validator.Validate(vcard);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("VERSION"));
        }

        [Fact]
        public void Validate_MissingFN_HasError()
        {
            var vcard = new VCardObject
            {
                Version = "4.0"
            };

            var validator = new VCardValidator();
            var result = validator.Validate(vcard);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("FN"));
        }

        [Fact]
        public void Validate_InvalidVersion_HasError()
        {
            var vcard = new VCardObject
            {
                Version = "5.0",
                FormattedName = "John Doe"
            };

            var validator = new VCardValidator();
            var result = validator.Validate(vcard);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("VERSION"));
        }

        [Fact]
        public void Validate_EmptyFN_HasError()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = ""
            };

            var validator = new VCardValidator();
            var result = validator.Validate(vcard);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("FN"));
        }

        [Fact]
        public void Validate_EmptyTelephone_HasError()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe",
                Telephones = new System.Collections.Generic.List<Telephone>
                {
                    new Telephone { Value = "", Types = TelType.None }
                }
            };

            var validator = new VCardValidator();
            var result = validator.Validate(vcard);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("TEL"));
        }

        [Fact]
        public void Validate_InvalidEmail_HasWarning()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe",
                Emails = new System.Collections.Generic.List<Email>
                {
                    new Email { Value = "invalid-email", Types = EmailType.None }
                }
            };

            var validator = new VCardValidator();
            var result = validator.Validate(vcard);

            Assert.True(result.IsValid); // Still valid, just a warning
            Assert.Contains(result.Warnings, w => w.Contains("EMAIL"));
        }

        [Fact]
        public void Validate_ValidEmail_NoWarning()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe",
                Emails = new System.Collections.Generic.List<Email>
                {
                    new Email { Value = "john@example.com", Types = EmailType.None }
                }
            };

            var validator = new VCardValidator();
            var result = validator.Validate(vcard);

            Assert.True(result.IsValid);
            Assert.Empty(result.Warnings);
        }
    }

    public class VCardRoundTripTests
    {
        [Fact]
        public void RoundTrip_SimpleVCard_Success()
        {
            var original = @"BEGIN:VCARD
VERSION:4.0
FN:John Doe
N:Doe;John;;;
TEL;TYPE=work:+1-555-555-1234
EMAIL;TYPE=work:john@example.com
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(original);
            Assert.Single(vcards);
            var vcard = vcards[0];

            var serializer = new VCardSerializer();
            var serialized = serializer.Serialize(vcard);

            var vcardsAgain = parser.Parse(serialized);
            Assert.Single(vcardsAgain);
            var vcardAgain = vcardsAgain[0];

            Assert.Equal(vcard.Version, vcardAgain.Version);
            Assert.Equal(vcard.FormattedName, vcardAgain.FormattedName);
            Assert.Equal(vcard.Name, vcardAgain.Name);
            Assert.Equal(vcard.Telephones.Count, vcardAgain.Telephones.Count);
            Assert.Equal(vcard.Emails.Count, vcardAgain.Emails.Count);
        }

        [Fact]
        public void RoundTrip_ComplexVCard_Success()
        {
            var original = @"BEGIN:VCARD
VERSION:4.0
FN:John Michael Doe
N:Doe;John;Michael;Mr.;Jr.
NICKNAME:Johnny
BDAY:19850415
ANNIVERSARY:20100612
GENDER:M
TEL;TYPE=work,voice:+1-555-555-1234
TEL;TYPE=home:555-555-5678
EMAIL;TYPE=work:john@example.com
EMAIL;TYPE=home:johnny@home.com
ADR;TYPE=work:;;123 Main St;Springfield;IL;62701;USA
ORG:ABC Corporation;Engineering
TITLE:Software Engineer
ROLE:Developer
URL:https://www.example.com
NOTE:Important contact
CATEGORIES:Work,Friends
UID:urn:uuid:4fbe8971-0bc3-424c-9c26-36c3e1eff6b1
REV:20231201T120000Z
END:VCARD";

            var parser = new VCardParser();
            var vcards = parser.Parse(original);
            Assert.Single(vcards);
            var vcard = vcards[0];

            var serializer = new VCardSerializer();
            var serialized = serializer.Serialize(vcard);

            var vcardsAgain = parser.Parse(serialized);
            Assert.Single(vcardsAgain);
            var vcardAgain = vcardsAgain[0];

            Assert.Equal(vcard.Version, vcardAgain.Version);
            Assert.Equal(vcard.FormattedName, vcardAgain.FormattedName);
            Assert.Equal(vcard.Name, vcardAgain.Name);
            Assert.Equal(vcard.Nickname, vcardAgain.Nickname);
            Assert.Equal(vcard.Birthday, vcardAgain.Birthday);
            Assert.Equal(vcard.Organization, vcardAgain.Organization);
            Assert.Equal(vcard.Uid, vcardAgain.Uid);
        }

        // ============================================================================
        // Object Initializer Tests (Strongly Typed API)
        // ============================================================================

        [Fact]
        public void ObjectInitializer_BasicVCard_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe"
            };

            Assert.Equal("4.0", vcard.Version);
            Assert.Equal("John Doe", vcard.FormattedName);
        }

        [Fact]
        public void ObjectInitializer_WithTelephone_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe",
                Telephones = new System.Collections.Generic.List<Telephone>
                {
                    new Telephone
                    {
                        Value = "+1-555-555-1234",
                        Types = TelType.Work | TelType.Voice
                    }
                }
            };

            Assert.Single(vcard.Telephones);
            var tel = vcard.Telephones[0];
            Assert.Equal("+1-555-555-1234", tel.Value);
            Assert.True(tel.Types.HasFlag(TelType.Work));
            Assert.True(tel.Types.HasFlag(TelType.Voice));
        }

        [Fact]
        public void ObjectInitializer_WithEmail_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe",
                Emails = new System.Collections.Generic.List<Email>
                {
                    new Email
                    {
                        Value = "john@example.com",
                        Types = EmailType.Work
                    }
                }
            };

            Assert.Single(vcard.Emails);
            var email = vcard.Emails[0];
            Assert.Equal("john@example.com", email.Value);
            Assert.True(email.Types.HasFlag(EmailType.Work));
        }

        [Fact]
        public void ObjectInitializer_WithAddressParts_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe",
                Addresses = new System.Collections.Generic.List<Address>
                {
                    new Address
                    {
                        Street = "123 Main Street",
                        City = "Springfield",
                        State = "IL",
                        PostalCode = "62701",
                        Country = "USA",
                        Types = AdrType.Work
                    }
                }
            };

            Assert.Single(vcard.Addresses);
            var adr = vcard.Addresses[0];
            Assert.Equal("123 Main Street", adr.Street);
            Assert.Equal("Springfield", adr.City);
            Assert.Equal("IL", adr.State);
            Assert.Equal("62701", adr.PostalCode);
            Assert.Equal("USA", adr.Country);
            Assert.True(adr.Types.HasFlag(AdrType.Work));
        }

        [Fact]
        public void ObjectInitializer_WithNameParts_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Michael Doe",
                Name = "Doe;John;Michael;Mr.;Jr."
            };

            Assert.Equal("Doe;John;Michael;Mr.;Jr.", vcard.Name);

            var structuredName = StructuredName.Parse(vcard.Name);
            Assert.Equal("Doe", structuredName.FamilyName);
            Assert.Equal("John", structuredName.GivenName);
            Assert.Equal("Michael", structuredName.AdditionalNames);
            Assert.Equal("Mr.", structuredName.HonorificPrefixes);
            Assert.Equal("Jr.", structuredName.HonorificSuffixes);
        }

        [Fact]
        public void ObjectInitializer_CompleteVCard_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Michael Doe",
                Name = "Doe;John;Michael;Mr.;Jr.",
                Nickname = "Johnny",
                Organization = "ABC Corporation",
                Title = "Software Engineer",
                Birthday = "19850415",
                Telephones = new System.Collections.Generic.List<Telephone>
                {
                    new Telephone { Value = "+1-555-555-1234", Types = TelType.Work | TelType.Voice },
                    new Telephone { Value = "555-555-5678", Types = TelType.Home }
                },
                Emails = new System.Collections.Generic.List<Email>
                {
                    new Email { Value = "john@example.com", Types = EmailType.Work }
                }
            };

            vcard.AddProperty(new VCardProperty("URL", "https://www.example.com"));
            vcard.AddProperty(new VCardProperty("NOTE", "Important contact"));
            vcard.AddProperty(new VCardProperty("CATEGORIES", "Work,VIP"));

            Assert.Equal("John Michael Doe", vcard.FormattedName);
            Assert.Equal("Doe;John;Michael;Mr.;Jr.", vcard.Name);
            Assert.Equal("Johnny", vcard.Nickname);
            Assert.Equal("ABC Corporation", vcard.Organization);
            Assert.Equal("Software Engineer", vcard.Title);

            Assert.Equal(2, vcard.Telephones.Count);
            Assert.Equal("+1-555-555-1234", vcard.Telephones[0].Value);
            Assert.Equal("555-555-5678", vcard.Telephones[1].Value);

            Assert.Single(vcard.Emails);
            Assert.Equal("john@example.com", vcard.Emails[0].Value);
        }

        [Fact]
        public void ObjectInitializer_WithCustomProperty_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "John Doe"
            };

            vcard.AddProperty(new VCardProperty("X-CUSTOM", "Custom Value"));

            var custom = vcard.GetProperty("X-CUSTOM");
            Assert.NotNull(custom);
            Assert.Equal("Custom Value", custom.Value);
        }

        [Fact]
        public void ObjectInitializer_WithMultipleTypes_Success()
        {
            var vcard = new VCardObject
            {
                Version = "4.0",
                FormattedName = "Jane Smith",
                Telephones = new System.Collections.Generic.List<Telephone>
                {
                    new Telephone { Value = "555-1234", Types = TelType.Cell }
                },
                Emails = new System.Collections.Generic.List<Email>
                {
                    new Email { Value = "jane@example.com", Types = EmailType.Home | EmailType.Internet }
                },
                Organization = "XYZ Corp"
            };

            Assert.Equal("Jane Smith", vcard.FormattedName);
            Assert.Single(vcard.Telephones);
            Assert.Single(vcard.Emails);
            Assert.Equal("XYZ Corp", vcard.Organization);

            var email = vcard.Emails[0];
            Assert.True(email.Types.HasFlag(EmailType.Home));
            Assert.True(email.Types.HasFlag(EmailType.Internet));
        }

        [Fact]
        public void FlaggedEnums_BitwiseOperations_Success()
        {
            var telephone = new Telephone
            {
                Value = "+1-555-1234",
                Types = TelType.Work | TelType.Voice | TelType.Cell
            };

            Assert.True(telephone.Types.HasFlag(TelType.Work));
            Assert.True(telephone.Types.HasFlag(TelType.Voice));
            Assert.True(telephone.Types.HasFlag(TelType.Cell));
            Assert.False(telephone.Types.HasFlag(TelType.Home));
        }

        [Fact]
        public void FlaggedEnums_EmailTypes_Success()
        {
            var email = new Email
            {
                Value = "test@example.com",
                Types = EmailType.Work | EmailType.Internet
            };

            Assert.True(email.Types.HasFlag(EmailType.Work));
            Assert.True(email.Types.HasFlag(EmailType.Internet));
            Assert.False(email.Types.HasFlag(EmailType.Home));
        }

        [Fact]
        public void FlaggedEnums_AddressTypes_Success()
        {
            var address = new Address
            {
                Street = "123 Main St",
                City = "Springfield",
                Types = AdrType.Work | AdrType.Postal
            };

            Assert.True(address.Types.HasFlag(AdrType.Work));
            Assert.True(address.Types.HasFlag(AdrType.Postal));
            Assert.False(address.Types.HasFlag(AdrType.Home));
        }
    }
}
