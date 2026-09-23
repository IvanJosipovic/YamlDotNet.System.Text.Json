internal static class Program
{
    private static int Main()
    {
        JsonAttributesAndExtensionDataExample.Run();
        SerializationOptionsExample.Run();
        JsonInputExample.Run();
        BuilderConfigurationExample.Run();
        StaticContextExample.Run();
        Console.WriteLine("All library samples passed.");
        return 0;
    }
}

internal static class SampleAssert
{
    /// <summary>Compares complete YAML documents after normalizing line endings.</summary>
    /// <param name="actual">The YAML produced by the sample.</param>
    /// <param name="expected">The complete expected YAML document.</param>
    /// <param name="message">The failure message to display.</param>
    public static void YamlEquals(string actual, string expected, string message)
    {
        var normalizedActual = actual.ReplaceLineEndings("\n");
        var normalizedExpected = expected.ReplaceLineEndings("\n");
        if (!string.Equals(normalizedActual, normalizedExpected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{message}\nExpected:\n{normalizedExpected}\nActual:\n{normalizedActual}");
        }
    }

    /// <summary>Throws when a sample condition is false.</summary>
    /// <param name="condition">The condition that must be true.</param>
    /// <param name="message">The failure message to display.</param>
    public static void That(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
