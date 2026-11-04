using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using Scalar = YamlDotNet.Core.Events.Scalar;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Resolves YAML plain scalars into JSON token semantics.
/// </summary>
/// <remarks>
/// YAML 1.1 and YAML 1.2 disagree on several scalar classes (for example <c>yes</c>, <c>on</c> and
/// <c>~</c>). This resolver follows the YAML 1.2 core schema, which aligns with JSON semantics:
/// <list type="bullet">
///   <item><description>Booleans are <c>true</c>/<c>false</c> in any capitalization; <c>yes</c>, <c>no</c>, <c>on</c> and <c>off</c> are strings.</description></item>
///   <item><description>Null is an empty scalar, <c>null</c> (any capitalization) or <c>~</c>.</description></item>
///   <item><description>Integers are optionally signed decimal digits. Leading zeros are interpreted as decimal (YAML 1.1 octal such as <c>00123</c> is parsed as the decimal number <c>123</c>). Hexadecimal (<c>0x1A</c>), octal (<c>0o17</c>) and sexagesimal notation are strings.</description></item>
///   <item><description>Floats are decimal numbers with optional exponent; <c>.nan</c> and <c>.inf</c> are not representable in JSON and are rejected.</description></item>
///   <item><description>Timestamps such as <c>2026-01-01</c> have no JSON equivalent and remain strings.</description></item>
///   <item><description>Quoted scalars are always strings.</description></item>
/// </list>
/// All numeric parsing is culture invariant.
/// </remarks>
internal static class JsonScalarResolver
{
    /// <summary>
    /// Determines whether the plain scalar resolves to JSON <c>null</c>.
    /// </summary>
    public static bool IsNullScalar(string? value)
    {
        return string.IsNullOrEmpty(value)
            || value is "null" or "Null" or "NULL" or "~";
    }

    /// <summary>
    /// Determines whether the plain scalar resolves to a JSON boolean.
    /// </summary>
    public static bool TryResolveBoolean(string? value, out bool result)
    {
        switch (value)
        {
            case "true" or "True" or "TRUE":
                result = true;
                return true;
            case "false" or "False" or "FALSE":
                result = false;
                return true;
            default:
                result = false;
                return false;
        }
    }

    /// <summary>
    /// Determines whether the plain scalar resolves to a JSON integer.
    /// </summary>
    public static bool IsIntegerScalar(string? value)
    {
        if (value is not { Length: > 0 })
        {
            return false;
        }

        var start = value[0] is '-' or '+' ? 1 : 0;

        if (start == value.Length)
        {
            return false;
        }

        for (var i = start; i < value.Length; i++)
        {
            if (value[i] is < '0' or > '9')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the plain scalar resolves to a JSON floating point number.
    /// </summary>
    public static bool IsFloatScalar(string? value)
    {
        if (value is not { Length: > 0 })
        {
            return false;
        }

        var start = value[0] is '-' or '+' ? 1 : 0;

        if (start == value.Length)
        {
            return false;
        }

        var mantissaDigits = 0;
        var exponentDigits = 0;
        var sawDot = false;
        var sawExponent = false;

        for (var i = start; i < value.Length; i++)
        {
            var c = value[i];

            if (c is >= '0' and <= '9')
            {
                if (sawExponent)
                {
                    exponentDigits++;
                }
                else
                {
                    mantissaDigits++;
                }
            }
            else if (c == '.' && !sawDot && !sawExponent)
            {
                sawDot = true;
            }
            else if ((c == 'e' || c == 'E') && !sawExponent && mantissaDigits > 0)
            {
                sawExponent = true;
            }
            else if ((c == '-' || c == '+') && sawExponent && exponentDigits == 0 && (value[i - 1] == 'e' || value[i - 1] == 'E'))
            {
                // exponent sign
            }
            else
            {
                return false;
            }
        }

        // Pure integers are handled by IsIntegerScalar; floats require a dot or an exponent.
        return (sawDot || sawExponent) && mantissaDigits > 0 && (exponentDigits > 0 || !sawExponent);
    }

    /// <summary>
    /// Determines whether a JSON string written as a plain YAML scalar could be misinterpreted
    /// as a non-string value when parsed again.
    /// </summary>
    public static bool NeedsQuoting(string? value)
    {
        return string.IsNullOrEmpty(value)
            || IsNullScalar(value)
            || TryResolveBoolean(value, out _)
            || IsIntegerScalar(value)
            || IsFloatScalar(value);
    }

    /// <summary>
    /// Throws when the scalar uses a YAML 1.1 special float that has no lossless JSON representation.
    /// </summary>
    public static void RejectSpecialFloats(Scalar scalar)
    {
        if (scalar.Style == ScalarStyle.Plain)
        {
            switch (scalar.Value)
            {
                case ".nan" or ".NaN" or ".NAN" or
                     ".inf" or ".Inf" or ".INF" or
                     "+.inf" or "+.Inf" or "+.INF" or
                     "-.inf" or "-.Inf" or "-.INF":
                    throw new YamlException(scalar.Start, scalar.End,
                        $"The scalar '{scalar.Value}' is not supported because it has no JSON number representation. Quote the value ('\"{scalar.Value}\"') to treat it as a string.");
            }
        }
    }

    /// <summary>
    /// Parses a float scalar using invariant culture, preferring <see cref="decimal"/> to preserve precision.
    /// </summary>
    public static bool TryParseNumber(string value, out decimal decimalValue, out double doubleValue)
    {
        if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimalValue))
        {
            doubleValue = 0;
            return true;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out doubleValue))
        {
            return true;
        }

        return false;
    }
}
