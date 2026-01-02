using System;
using System.IO;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace VCard.Tests
{
    /// <summary>
    /// Negative tests for vCard parser to ensure proper error handling
    /// </summary>
    public class VCardNegativeTests
    {
        private readonly string _negativeTestCasesPath;

        public VCardNegativeTests()
        {
            // Find the testcases/negative directory
            var currentDir = Directory.GetCurrentDirectory();
            while (currentDir != null)
            {
                var testCasesDir = Path.Combine(currentDir, "testcases", "negative");
                if (Directory.Exists(testCasesDir))
                {
                    _negativeTestCasesPath = testCasesDir;
                    break;
                }
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            if (string.IsNullOrEmpty(_negativeTestCasesPath))
            {
                throw new DirectoryNotFoundException("Could not find testcases/negative directory");
            }
        }

        private string ReadTestFile(string filename)
        {
            var filePath = Path.Combine(_negativeTestCasesPath, filename);
            return File.ReadAllText(filePath);
        }

        #region Structural Errors

        [Fact]
        public void MissingBegin_ShouldThrowParseException()
        {
            var content = ReadTestFile("missing_begin.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("BEGIN:VCARD", "error message should indicate missing BEGIN");
        }

        [Fact]
        public void MissingEnd_ShouldThrowParseException()
        {
            var content = ReadTestFile("missing_end.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("Unexpected end of input", "error message should indicate missing END");
        }

        [Fact]
        public void IncompleteVCard_ShouldThrowParseException()
        {
            var content = ReadTestFile("incomplete_vcard.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("Unexpected end of input");
        }

        [Fact]
        public void MismatchedBeginEnd_ShouldThrowParseException()
        {
            var content = ReadTestFile("mismatched_begin_end.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("Mismatched END tag", "error should indicate mismatched tags");
        }

        [Fact]
        public void WrongComponentType_ShouldThrowParseException()
        {
            var content = ReadTestFile("wrong_component_type.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("BEGIN:VCARD", "should expect BEGIN:VCARD");
        }

        [Fact]
        public void EmptyFile_ShouldThrowParseException()
        {
            var content = ReadTestFile("empty_file.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("No vCard data found");
        }

        [Fact]
        public void OnlyWhitespace_ShouldThrowParseException()
        {
            var content = ReadTestFile("only_whitespace.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("No vCard data found");
        }

        #endregion

        #region Required Property Violations

        [Fact]
        public void MissingVersion_ShouldThrowParseException()
        {
            var content = ReadTestFile("missing_version.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("VERSION", "error should mention missing VERSION");
            exception.Message.Should().Contain("RFC 6350", "error should reference the RFC");
        }

        [Fact]
        public void MissingFN_ShouldThrowParseException()
        {
            var content = ReadTestFile("missing_fn.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("FN", "error should mention missing FN");
            exception.Message.Should().Contain("Formatted Name", "error should explain what FN is");
            exception.Message.Should().Contain("RFC 6350", "error should reference the RFC");
        }

        #endregion

        #region Version Support

        [Fact]
        public void UnsupportedVersion21_ShouldThrowParseException()
        {
            var content = ReadTestFile("unsupported_version_2_1.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            // vCard 2.1 uses different parameter syntax (TEL;HOME instead of TEL;TYPE=home)
            // So the parser may fail on parameter parsing before checking version
            // Both are valid failures for unsupported version
            var msg = exception.Message;
            var hasVersionError = msg.Contains("Unsupported") && msg.Contains("2.1");
            var hasParameterError = msg.Contains("parameter") && msg.Contains("equals");

            Assert.True(hasVersionError || hasParameterError,
                $"Expected either version error or parameter syntax error, got: {msg}");
        }

        [Fact]
        public void UnsupportedVersion30_ShouldThrowParseException()
        {
            var content = ReadTestFile("unsupported_version_3_0.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("Unsupported", "error should indicate unsupported version");
            exception.Message.Should().Contain("3.0", "error should mention version 3.0");
            exception.Message.Should().Contain("4.0", "error should mention supported version 4.0");
        }

        [Fact]
        public void UnsupportedVersion10_ShouldThrowParseException()
        {
            var content = ReadTestFile("unsupported_version_1_0.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("Unsupported", "error should indicate unsupported version");
            exception.Message.Should().Contain("1.0", "error should mention version 1.0");
        }

        [Fact]
        public void InvalidVersionFormat_ShouldThrowParseException()
        {
            var content = ReadTestFile("invalid_version_format.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("Unsupported", "error should indicate unsupported version");
        }

        #endregion

        #region Syntax Errors

        [Fact]
        public void MalformedPropertyNoColon_ShouldThrowParseException()
        {
            var content = ReadTestFile("malformed_property_no_colon.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("colon", "error should mention missing colon");
            exception.Message.Should().Contain("TEL", "error should show the problematic line");
        }

        [Fact]
        public void MalformedParameterSyntax_ShouldThrowParseException()
        {
            var content = ReadTestFile("malformed_parameter_syntax.vcf");
            var parser = new VCardParser();

            var exception = Assert.Throws<ParseException>(() => parser.Parse(content));
            exception.Message.Should().Contain("parameter", "error should mention parameter issue");
            exception.Message.Should().Contain("equals", "error should mention missing equals sign");
        }

        #endregion

        #region Multiple Test Cases Runner

        /// <summary>
        /// Test that critical negative test files throw exceptions.
        /// Note: Some tests are lenient (e.g., duplicate properties, unknown parameter values)
        /// which is acceptable parser behavior. This test focuses on critical errors only.
        /// </summary>
        [Fact]
        public void CriticalNegativeTestFiles_ShouldThrowExceptions()
        {
            // These are critical errors that MUST be rejected
            var criticalTests = new[]
            {
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
            };

            var parser = new VCardParser();
            var passedCount = 0;
            var failedTests = new System.Collections.Generic.List<string>();

            foreach (var filename in criticalTests)
            {
                try
                {
                    var content = ReadTestFile(filename);
                    parser.Parse(content);

                    // If we get here, the parser didn't throw - this is a failure
                    failedTests.Add($"{filename}: Parser accepted invalid vCard (should have thrown exception)");
                }
                catch (ParseException ex)
                {
                    // Expected - negative test should throw
                    passedCount++;
                    System.Diagnostics.Debug.WriteLine($"✓ {filename}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Unexpected exception type
                    failedTests.Add($"{filename}: Unexpected exception type: {ex.GetType().Name}");
                }
            }

            // Report results
            System.Diagnostics.Debug.WriteLine($"\nCritical Negative Tests Summary: {passedCount}/{criticalTests.Length} passed");

            if (failedTests.Any())
            {
                var failureMessage = $"Some critical negative tests failed:\n{string.Join("\n", failedTests)}";
                Assert.True(false, failureMessage);
            }
        }

        #endregion
    }
}
