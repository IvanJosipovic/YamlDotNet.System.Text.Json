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
    public static void YamlEquals(string actual, string expected, string message)
    {
        var normalizedActual = actual.ReplaceLineEndings("\n");
        var normalizedExpected = expected.ReplaceLineEndings("\n");
        if (!string.Equals(normalizedActual, normalizedExpected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{message}\nExpected:\n{normalizedExpected}\nActual:\n{normalizedActual}");
        }
    }

    public static void That(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
