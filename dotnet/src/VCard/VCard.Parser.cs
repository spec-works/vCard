using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VCard
{
    /// <summary>
    /// Parser for text/vcard format (RFC 6350)
    /// </summary>
    public class VCardParser
    {
        private List<string> _lines;
        private int _currentLine;

        public List<VCardObject> Parse(string vcardText)
        {
            var vcards = new List<VCardObject>();
            _lines = UnfoldLines(vcardText);
            _currentLine = 0;

            while (_currentLine < _lines.Count)
            {
                // Skip empty lines
                while (_currentLine < _lines.Count && string.IsNullOrWhiteSpace(_lines[_currentLine]))
                {
                    _currentLine++;
                }

                if (_currentLine >= _lines.Count)
                    break;

                var line = _lines[_currentLine];
                if (line.Equals("BEGIN:VCARD", StringComparison.OrdinalIgnoreCase))
                {
                    _currentLine++;
                    var vcard = new VCardObject();
                    ParseComponent(vcard);
                    vcards.Add(vcard);
                }
                else
                {
                    throw new ParseException($"Expected BEGIN:VCARD but got: {line}");
                }
            }

            if (vcards.Count == 0)
            {
                throw new ParseException("No vCard data found");
            }

            return vcards;
        }

        public List<VCardObject> ParseFile(string filePath)
        {
            var content = File.ReadAllText(filePath);
            return Parse(content);
        }

        private List<string> UnfoldLines(string vcardText)
        {
            var unfoldedLines = new List<string>();
            var lines = vcardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            StringBuilder currentLine = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.Length > 0 && (line[0] == ' ' || line[0] == '\t'))
                {
                    // Continuation line - remove leading whitespace and append
                    currentLine.Append(line.Substring(1));
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        var unfoldedLine = currentLine.ToString();
                        if (!string.IsNullOrWhiteSpace(unfoldedLine))
                        {
                            unfoldedLines.Add(unfoldedLine);
                        }
                    }
                    currentLine = new StringBuilder(line);
                }
            }

            if (currentLine.Length > 0)
            {
                var unfoldedLine = currentLine.ToString();
                if (!string.IsNullOrWhiteSpace(unfoldedLine))
                {
                    unfoldedLines.Add(unfoldedLine);
                }
            }

            return unfoldedLines;
        }

        private void ParseComponent(VCardComponent component)
        {
            while (_currentLine < _lines.Count)
            {
                var line = _lines[_currentLine];

                if (line.StartsWith("END:", StringComparison.OrdinalIgnoreCase))
                {
                    var endComponentType = line.Substring(4).ToUpperInvariant();
                    if (endComponentType != component.ComponentType)
                    {
                        throw new ParseException($"Mismatched END tag: expected END:{component.ComponentType} but got END:{endComponentType}");
                    }
                    _currentLine++;

                    // Validate required properties for vCard
                    if (component is VCardObject vcard)
                    {
                        ValidateVCard(vcard);
                    }

                    return;
                }
                else
                {
                    var property = ParseProperty(line);

                    // Handle strongly-typed properties for VCardObject
                    if (component is VCardObject vcard)
                    {
                        switch (property.Name)
                        {
                            case "TEL":
                                vcard.Telephones.Add(Telephone.FromProperty(property));
                                break;
                            case "EMAIL":
                                vcard.Emails.Add(Email.FromProperty(property));
                                break;
                            case "ADR":
                                vcard.Addresses.Add(Address.FromProperty(property));
                                break;
                            default:
                                component.AddProperty(property);
                                break;
                        }
                    }
                    else
                    {
                        component.AddProperty(property);
                    }

                    _currentLine++;
                }
            }

            throw new ParseException($"Unexpected end of input while parsing {component.ComponentType}");
        }

        private VCardProperty ParseProperty(string line)
        {
            var colonIndex = FindUnquotedChar(line, ':');
            if (colonIndex == -1)
            {
                throw new ParseException($"Invalid property line (missing colon): {line}");
            }

            var nameAndParams = line.Substring(0, colonIndex);
            var value = line.Substring(colonIndex + 1);

            // Unescape value
            value = UnescapeValue(value);

            // Parse name and parameters
            var semicolonIndex = FindUnquotedChar(nameAndParams, ';');
            string propertyName;
            string paramsPart = null;

            if (semicolonIndex != -1)
            {
                propertyName = nameAndParams.Substring(0, semicolonIndex).ToUpperInvariant();
                paramsPart = nameAndParams.Substring(semicolonIndex + 1);
            }
            else
            {
                propertyName = nameAndParams.ToUpperInvariant();
            }

            var property = new VCardProperty(propertyName, value);

            if (paramsPart != null)
            {
                ParseParameters(paramsPart, property);
            }

            return property;
        }

        private void ParseParameters(string paramsPart, VCardProperty property)
        {
            var parameters = SplitParameters(paramsPart);

            foreach (var param in parameters)
            {
                var equalsIndex = param.IndexOf('=');
                if (equalsIndex == -1)
                {
                    throw new ParseException($"Invalid parameter (missing equals): {param}");
                }

                var paramName = param.Substring(0, equalsIndex).ToUpperInvariant();
                var paramValue = param.Substring(equalsIndex + 1);

                // Remove quotes if present
                if (paramValue.StartsWith("\"") && paramValue.EndsWith("\"") && paramValue.Length >= 2)
                {
                    paramValue = paramValue.Substring(1, paramValue.Length - 2);
                }

                // Handle comma-separated values
                var values = SplitParameterValues(paramValue);
                foreach (var value in values)
                {
                    property.AddParameter(paramName, value);
                }
            }
        }

        private List<string> SplitParameters(string paramsPart)
        {
            var parameters = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < paramsPart.Length; i++)
            {
                char c = paramsPart[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    current.Append(c);
                }
                else if (c == ';' && !inQuotes)
                {
                    parameters.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                parameters.Add(current.ToString());
            }

            return parameters;
        }

        private List<string> SplitParameterValues(string paramValue)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < paramValue.Length; i++)
            {
                char c = paramValue[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                values.Add(current.ToString());
            }

            return values;
        }

        private int FindUnquotedChar(string str, char target)
        {
            bool inQuotes = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (str[i] == target && !inQuotes)
                {
                    return i;
                }
            }
            return -1;
        }

        private string UnescapeValue(string value)
        {
            return value
                .Replace("\\n", "\n")
                .Replace("\\N", "\n")
                .Replace("\\;", ";")
                .Replace("\\,", ",")
                .Replace("\\\\", "\\");
        }

        private void ValidateVCard(VCardObject vcard)
        {
            // VERSION is required (RFC 6350 Section 6.7.9)
            if (string.IsNullOrEmpty(vcard.Version))
            {
                throw new ParseException("Missing required VERSION property (RFC 6350 Section 6.7.9). vCard must include VERSION:4.0");
            }

            // Only version 4.0 is supported (per ADR 0004)
            if (vcard.Version != "4.0")
            {
                throw new ParseException($"Unsupported vCard version: {vcard.Version}. Only version 4.0 is supported (see ADR 0004).");
            }

            // FN (formatted name) is required (RFC 6350 Section 6.2.1)
            if (string.IsNullOrEmpty(vcard.FormattedName))
            {
                throw new ParseException("Missing required FN (Formatted Name) property (RFC 6350 Section 6.2.1). vCard must include FN property.");
            }
        }
    }

    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }

        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
