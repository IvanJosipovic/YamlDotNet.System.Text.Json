using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Provides extension methods for configuring System.Text.Json serialization and deserialization support in YAML
/// serializer and deserializer builders.
/// </summary>
/// <remarks>Use the methods in this class to enable System.Text.Json type handling when working with YAML
/// serialization or deserialization. These extensions allow integration of System.Text.Json features, such as property
/// ordering and extension data, into YAML processing workflows.</remarks>
public static class BuilderExtensions
{
    /// <summary>
    /// Configures the serialization support for System.Text.Json types
    /// serialization.
    /// </summary>
    /// <remarks>Use this method to enable serialization support for System.Text.Json types</remarks>
    /// <param name="builder">The serializer builder to configure with System.Text.Json support.</param>
    /// <param name="sortAlphabetically">Specifies whether object properties should be sorted alphabetically during serialization. Set to <see langword="true"/> to sort properties; otherwise, properties retain their original order.</param>
    /// <param name="ignoreOrder">Specifies whether <see cref="JsonPropertyOrderAttribute"/> should be ignored during type inspection. Set to <see langword="true"/> to ignore the attribute-defined order; otherwise, any order defined via <see cref="JsonPropertyOrderAttribute"/> is preserved.</param>
    /// <returns>The same <see cref="SerializerBuilder"/> instance, configured to use System.Text.Json for YAML serialization.</returns>
    public static SerializerBuilder AddSystemTextJson(this SerializerBuilder builder, bool sortAlphabetically = false, bool ignoreOrder = false)
    {
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
#pragma warning restore CA1510 // Use ArgumentNullException throw helper
        builder.WithTypeConverter(new SystemTextJsonYamlTypeConverter(sortAlphabetically));
        builder.WithTypeInspector(x => new SystemTextJsonTypeInspector(x, ignoreOrder));

        return builder;
    }

    /// <summary>
    /// Configures the deserialization support for System.Text.Json types
    /// </summary>
    /// <remarks>This method enables deserialization support for System.Text.Json types</remarks>
    /// <param name="builder">The deserializer builder to configure with System.Text.Json support. Cannot be null.</param>
    /// <returns>The same <see cref="DeserializerBuilder"/> instance, configured to use System.Text.Json for type conversion and
    /// extension data handling.</returns>
    public static DeserializerBuilder AddSystemTextJson(this DeserializerBuilder builder)
    {
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
#pragma warning restore CA1510 // Use ArgumentNullException throw helper
        builder.WithTypeConverter(new SystemTextJsonYamlTypeConverter());
        builder.WithTypeInspector(x => new SystemTextJsonTypeInspector(x, true));

        return builder;
    }
}
