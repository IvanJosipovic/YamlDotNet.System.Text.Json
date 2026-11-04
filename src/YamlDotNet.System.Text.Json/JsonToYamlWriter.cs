using System.Buffers;
using System.Text;
using System.Text.Json;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Converts a JSON token stream into YAML events written to a YamlDotNet emitter.
/// </summary>
/// <remarks>
/// Property order, numeric precision and string values are preserved. Strings that a YAML parser
/// could misinterpret as numbers, booleans or null are quoted so that a subsequent YAML parse
/// produces the original JSON value.
/// </remarks>
internal static class JsonToYamlWriter
{
    internal static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public static string ConvertToString(ReadOnlySpan<byte> utf8Json)
    {
        var writer = new StringWriter();
        Convert(utf8Json, writer);
        return writer.ToString();
    }

    public static void Convert(ReadOnlySpan<byte> utf8Json, TextWriter output)
    {
        var emitter = new Emitter(output);

        emitter.Emit(new StreamStart());
        emitter.Emit(new DocumentStart(null, null, true, Mark.Empty, Mark.Empty));

        var reader = new Utf8JsonReader(utf8Json);

        if (!reader.Read())
        {
            throw new InvalidOperationException("The JSON payload produced by System.Text.Json is empty.");
        }

        ConvertNode(ref reader, emitter);

        emitter.Emit(new DocumentEnd(true, Mark.Empty, Mark.Empty));
        emitter.Emit(new StreamEnd());
    }

    private static void ConvertNode(ref Utf8JsonReader reader, IEmitter emitter)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
            {
                emitter.Emit(new MappingStart(default, default, true, MappingStyle.Block));

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new InvalidOperationException($"Unexpected JSON token '{reader.TokenType}' inside an object.");
                    }

                    var name = reader.GetString()!;
                    WriteScalar(emitter, name);

                    reader.Read();
                    ConvertNode(ref reader, emitter);
                }

                emitter.Emit(new MappingEnd());
                break;
            }
            case JsonTokenType.StartArray:
            {
                emitter.Emit(new SequenceStart(default, default, true, SequenceStyle.Block));

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }

                    ConvertNode(ref reader, emitter);
                }

                emitter.Emit(new SequenceEnd());
                break;
            }
            case JsonTokenType.String:
                WriteScalar(emitter, reader.GetString()!);
                break;
            case JsonTokenType.Number:
                // Numbers keep their exact textual representation and are emitted as plain scalars.
                emitter.Emit(new Scalar(GetRawText(ref reader)));
                break;
            case JsonTokenType.True:
                emitter.Emit(new Scalar("true"));
                break;
            case JsonTokenType.False:
                emitter.Emit(new Scalar("false"));
                break;
            case JsonTokenType.Null:
                emitter.Emit(new Scalar("null"));
                break;
            default:
                throw new InvalidOperationException($"Unexpected JSON token '{reader.TokenType}'.");
        }
    }

    private static void WriteScalar(IEmitter emitter, string value)
    {
        if (value.Contains('\n'))
        {
            // Force a multi-line literal block so embedded newlines round-trip exactly.
            emitter.Emit(new Scalar(default, default, value, ScalarStyle.Literal, true, true));
        }
        else if (JsonScalarResolver.NeedsQuoting(value))
        {
            // The value could be misread as null, a boolean or a number; quote it to preserve the string.
            emitter.Emit(new Scalar(default, default, value, ScalarStyle.SingleQuoted, false, true));
        }
        else
        {
            emitter.Emit(new Scalar(value));
        }
    }

    private static string GetRawText(ref Utf8JsonReader reader)
    {
        if (reader.HasValueSequence)
        {
            var sequence = reader.ValueSequence;
            var buffer = ArrayPool<byte>.Shared.Rent((int)sequence.Length);

            try
            {
                sequence.CopyTo(buffer);
                return Encoding.UTF8.GetString(buffer, 0, (int)sequence.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        return Encoding.UTF8.GetString(reader.ValueSpan.ToArray());
    }
}
