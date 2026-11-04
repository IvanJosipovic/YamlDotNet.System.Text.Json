using System.Globalization;
using System.Text.Json;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using Scalar = YamlDotNet.Core.Events.Scalar;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Converts a YAML event stream into JSON tokens written to a <see cref="Utf8JsonWriter"/>.
/// </summary>
/// <remarks>
/// The converter supports a JSON compatible subset of YAML: mappings with string keys, sequences,
/// scalars (plain, quoted and block styles), comments (ignored) and the standard YAML tags
/// (<c>!!str</c>, <c>!!int</c>, <c>!!float</c>, <c>!!bool</c>, <c>!!null</c>). Anchors and aliases,
/// merge keys, complex mapping keys, custom tags and multiple documents are not supported and
/// produce a <see cref="YamlException"/> instead of silently incorrect results.
/// </remarks>
internal static class YamlToJsonWriter
{
    private const string TagPrefix = "tag:yaml.org,2002:";

    private const string StrTag = TagPrefix + "str";
    private const string NullTag = TagPrefix + "null";
    private const string BoolTag = TagPrefix + "bool";
    private const string IntTag = TagPrefix + "int";
    private const string FloatTag = TagPrefix + "float";

    public static void Convert(IParser parser, Utf8JsonWriter writer)
    {
        parser.TryConsume<StreamStart>(out _);

        if (!parser.TryConsume<DocumentStart>(out var documentStart))
        {
            throw new YamlException("The YAML input does not contain a document.");
        }

        if (parser.TryConsume<DocumentEnd>(out _))
        {
            // An empty document (e.g. '---') deserializes to JSON null.
            writer.WriteNullValue();
        }
        else
        {
            ConvertNode(parser, writer);
            ConsumeDocumentEnd(parser);
        }

        if (parser.TryConsume<DocumentStart>(out _))
        {
            throw new YamlException("Multiple YAML documents are not supported. Provide a single document per call.");
        }

        parser.TryConsume<StreamEnd>(out _);
    }

    private static void ConsumeDocumentEnd(IParser parser)
    {
        if (!parser.TryConsume<DocumentEnd>(out _))
        {
            throw new YamlException("The YAML input is not valid. A document end was expected.");
        }
    }

    private static void ConvertNode(IParser parser, Utf8JsonWriter writer)
    {
        if (parser.Accept<AnchorAlias>(out var alias))
        {
            throw new YamlException(alias.Start, alias.End,
                "YAML anchors and aliases are not supported by YamlSerializer because they have no JSON equivalent and may introduce cyclic references. Use SerializerBuilder based APIs instead.");
        }

        if (parser.Accept<Scalar>(out var scalar))
        {
            parser.Consume<Scalar>();
            WriteScalar(scalar, writer);
            return;
        }

        if (parser.Accept<SequenceStart>(out var sequenceStart))
        {
            parser.Consume<SequenceStart>();
            writer.WriteStartArray();

            while (!parser.TryConsume<SequenceEnd>(out _))
            {
                ConvertNode(parser, writer);
            }

            writer.WriteEndArray();
            return;
        }

        if (parser.Accept<MappingStart>(out var mappingStart))
        {
            parser.Consume<MappingStart>();
            writer.WriteStartObject();

            while (!parser.TryConsume<MappingEnd>(out _))
            {
                WriteMappingEntry(parser, writer);
            }

            writer.WriteEndObject();
            return;
        }

        throw new YamlException("The YAML input contains an event that is not supported.");
    }

    private static void WriteMappingEntry(IParser parser, Utf8JsonWriter writer)
    {
        if (parser.Accept<AnchorAlias>(out var alias))
        {
            throw new YamlException(alias.Start, alias.End,
                "YAML anchors and aliases are not supported by YamlSerializer because they have no JSON equivalent and may introduce cyclic references. Use SerializerBuilder based APIs instead.");
        }

        if (parser.Accept<Scalar>(out var key))
        {
            parser.Consume<Scalar>();

            if (key.Value == "<<")
            {
                throw new YamlException(key.Start, key.End,
                    "YAML merge keys ('<<') are not supported by YamlSerializer. Use SerializerBuilder based APIs instead.");
            }

            if (IsTagged(key.Tag) && key.Tag.Value != StrTag)
            {
                throw new YamlException(key.Start, key.End,
                    $"The mapping key tag '{key.Tag.Value}' is not supported. Mapping keys must be strings.");
            }

            writer.WritePropertyName(key.Value);
            ConvertNode(parser, writer);
            return;
        }

        if (parser.Accept<SequenceStart>(out var sequenceKey))
        {
            throw new YamlException(sequenceKey.Start, sequenceKey.End,
                "Complex mapping keys (sequences or mappings used as keys) are not supported because JSON requires string keys.");
        }

        if (parser.Accept<MappingStart>(out var mappingKey))
        {
            throw new YamlException(mappingKey.Start, mappingKey.End,
                "Complex mapping keys (sequences or mappings used as keys) are not supported because JSON requires string keys.");
        }

        throw new YamlException("The YAML input contains an event that is not supported.");
    }

    private static void WriteScalar(Scalar scalar, Utf8JsonWriter writer)
    {
        var tag = GetTag(scalar.Tag);

        switch (tag)
        {
            case "":
                break;
            case StrTag:
                writer.WriteStringValue(scalar.Value);
                return;
            case NullTag:
                writer.WriteNullValue();
                return;
            case BoolTag:
            {
                if (!JsonScalarResolver.TryResolveBoolean(scalar.Value, out var b) || scalar.Style != ScalarStyle.Plain)
                {
                    throw new YamlException(scalar.Start, scalar.End,
                        $"The value '{scalar.Value}' cannot be resolved as a bool ('!!bool').");
                }

                writer.WriteBooleanValue(b);
                return;
            }
            case IntTag:
            case FloatTag:
            {
                if (scalar.Style != ScalarStyle.Plain || !JsonScalarResolver.TryParseNumber(scalar.Value, out var d, out _) ||
                    (tag == IntTag && JsonScalarResolver.IsFloatScalar(scalar.Value)))
                {
                    throw new YamlException(scalar.Start, scalar.End,
                        $"The value '{scalar.Value}' cannot be resolved as a number ('{tag}').");
                }

                writer.WriteNumberValue(d);
                return;
            }
            default:
                throw new YamlException(scalar.Start, scalar.End,
                    $"The YAML tag '{tag}' is not supported. Only the standard YAML tags (str, int, float, bool, null) and untagged scalars are supported.");
        }

        if (scalar.Style != ScalarStyle.Plain)
        {
            // Quoted and block scalars are always strings.
            writer.WriteStringValue(scalar.Value);
            return;
        }

        JsonScalarResolver.RejectSpecialFloats(scalar);

        if (JsonScalarResolver.IsNullScalar(scalar.Value))
        {
            writer.WriteNullValue();
            return;
        }

        if (JsonScalarResolver.TryResolveBoolean(scalar.Value, out var boolValue))
        {
            writer.WriteBooleanValue(boolValue);
            return;
        }

        if (JsonScalarResolver.IsIntegerScalar(scalar.Value))
        {
            if (long.TryParse(scalar.Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var longValue))
            {
                writer.WriteNumberValue(longValue);
            }
            else
            {
                // Integers larger than long.MaxValue: emit the exact text (normalized to valid JSON)
                // instead of losing precision to a floating point number.
                writer.WriteRawValue(NormalizeInteger(scalar.Value), skipInputValidation: false);
            }

            return;
        }

        if (JsonScalarResolver.IsFloatScalar(scalar.Value))
        {
            if (JsonScalarResolver.TryParseNumber(scalar.Value, out var decimalValue, out var doubleValue))
            {
                if (decimalValue != 0)
                {
                    writer.WriteNumberValue(decimalValue);
                }
                else
                {
                    writer.WriteNumberValue(doubleValue);
                }
            }

            return;
        }

        writer.WriteStringValue(scalar.Value);
    }

    /// <summary>
    /// Determines whether the scalar carries a resolvable tag. <see cref="YamlDotNet.Core.TagName.Value"/>
    /// throws for empty and non-specific tags, which must be checked first.
    /// </summary>
    private static bool IsTagged(TagName tag) => !tag.IsEmpty && !tag.IsNonSpecific;

    /// <summary>
    /// Returns the tag text of a scalar, or an empty string when the tag is empty or non-specific.
    /// </summary>
    private static string GetTag(TagName tag) => IsTagged(tag) ? tag.Value : "";

    /// <summary>
    /// Normalizes a YAML integer scalar into a JSON number literal, dropping leading zeros and
    /// explicit plus signs that are invalid in JSON.
    /// </summary>
    private static string NormalizeInteger(string value)
    {
        var negative = value[0] == '-';
        var start = value[0] is '-' or '+' ? 1 : 0;

        while (start < value.Length - 1 && value[start] == '0')
        {
            start++;
        }

        var digits = value.AsSpan(start);

        if (!negative || digits.SequenceEqual("0"))
        {
            return digits.ToString();
        }

        return "-" + digits.ToString();
    }
}
