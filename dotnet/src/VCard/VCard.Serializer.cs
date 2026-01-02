using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VCard
{
    /// <summary>
    /// Serializer for text/vcard format (RFC 6350)
    /// </summary>
    public class VCardSerializer
    {
        private const int MaxLineLength = 75;

        /// <summary>
        /// Serializes a VCardObject to vCard format string
        /// </summary>
        public string Serialize(VCardObject vcard)
        {
            var builder = new StringBuilder();
            SerializeComponent(vcard, builder);
            return builder.ToString();
        }

        /// <summary>
        /// Serializes multiple VCardObjects to vCard format string
        /// </summary>
        public string SerializeMultiple(List<VCardObject> vcards)
        {
            var builder = new StringBuilder();
            foreach (var vcard in vcards)
            {
                SerializeComponent(vcard, builder);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Serializes a VCardObject to a file
        /// </summary>
        public void SerializeToFile(VCardObject vcard, string filePath)
        {
            var content = Serialize(vcard);
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Serializes multiple VCardObjects to a file
        /// </summary>
        public void SerializeMultipleToFile(List<VCardObject> vcards, string filePath)
        {
            var content = SerializeMultiple(vcards);
            File.WriteAllText(filePath, content);
        }

        private void SerializeComponent(VCardComponent component, StringBuilder builder)
        {
            // Write BEGIN
            WriteLine(builder, $"BEGIN:{component.ComponentType}");

            // Write properties in a specific order for vCard
            // VERSION must come first (after BEGIN)
            var versionProp = component.GetProperty("VERSION");
            if (versionProp != null)
            {
                SerializeProperty(versionProp, builder);
            }

            // Write all other properties
            foreach (var propertyList in component.Properties.Values)
            {
                foreach (var property in propertyList)
                {
                    // Skip VERSION as it's already written
                    if (property.Name != "VERSION")
                    {
                        SerializeProperty(property, builder);
                    }
                }
            }

            // Write strongly-typed properties for VCardObject
            if (component is VCardObject vcard)
            {
                // Serialize telephones
                foreach (var telephone in vcard.Telephones)
                {
                    SerializeProperty(telephone.ToProperty(), builder);
                }

                // Serialize emails
                foreach (var email in vcard.Emails)
                {
                    SerializeProperty(email.ToProperty(), builder);
                }

                // Serialize addresses
                foreach (var address in vcard.Addresses)
                {
                    SerializeProperty(address.ToProperty(), builder);
                }
            }

            // Write END
            WriteLine(builder, $"END:{component.ComponentType}");
        }

        private void SerializeProperty(VCardProperty property, StringBuilder builder)
        {
            var line = new StringBuilder();
            line.Append(property.Name);

            // Add parameters
            if (property.Parameters.Count > 0)
            {
                foreach (var parameterList in property.Parameters)
                {
                    var paramName = parameterList.Key;
                    var paramValues = parameterList.Value;

                    foreach (var paramValue in paramValues)
                    {
                        line.Append(';');
                        line.Append(paramName);
                        line.Append('=');

                        // Quote parameter value if it contains special characters
                        if (NeedsQuoting(paramValue))
                        {
                            line.Append('"');
                            line.Append(paramValue);
                            line.Append('"');
                        }
                        else
                        {
                            line.Append(paramValue);
                        }
                    }
                }
            }

            line.Append(':');
            line.Append(EscapeValue(property.Value));

            WriteLine(builder, line.ToString());
        }

        private void WriteLine(StringBuilder builder, string line)
        {
            if (line.Length <= MaxLineLength)
            {
                builder.AppendLine(line);
                return;
            }

            // Fold long lines (RFC 6350 Section 3.2)
            var firstLine = line.Substring(0, MaxLineLength);
            builder.AppendLine(firstLine);

            var remaining = line.Substring(MaxLineLength);
            while (remaining.Length > 0)
            {
                var chunkLength = Math.Min(MaxLineLength - 1, remaining.Length);
                var chunk = remaining.Substring(0, chunkLength);
                builder.Append(' ');
                builder.AppendLine(chunk);
                remaining = remaining.Substring(chunkLength);
            }
        }

        private bool NeedsQuoting(string value)
        {
            return value.Contains(':') ||
                   value.Contains(';') ||
                   value.Contains(',') ||
                   value.Contains(' ') ||
                   value.Contains('\t');
        }

        private string EscapeValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace("\n", "\\n")
                .Replace("\r", "");
        }
    }

    /// <summary>
    /// Extension methods for convenient serialization
    /// </summary>
    public static class VCardSerializerExtensions
    {
        /// <summary>
        /// Converts a VCardObject to vCard format string
        /// </summary>
        public static string ToVCard(this VCardObject vcard)
        {
            var serializer = new VCardSerializer();
            return serializer.Serialize(vcard);
        }

        /// <summary>
        /// Saves a VCardObject to a file in vCard format
        /// </summary>
        public static void SaveToFile(this VCardObject vcard, string filePath)
        {
            var serializer = new VCardSerializer();
            serializer.SerializeToFile(vcard, filePath);
        }
    }
}
