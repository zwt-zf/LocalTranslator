using Community.PowerToys.Run.Plugin.LocalTranslator.Translation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Reflection;

namespace Community.PowerToys.Run.Plugin.LocalTranslator.UnitTests;

[TestClass]
public sealed class BinaryDictionaryRepositoryTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Dictionary_supports_fast_repeated_exact_lookups()
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Main))!.Location)!;
        var path = Path.Combine(assemblyDirectory, "Data", "localtranslator.dict");
        var openStopwatch = Stopwatch.StartNew();
        using var repository = new BinaryDictionaryRepository(path);
        openStopwatch.Stop();
        TestContext.WriteLine($"Opened dictionary in {openStopwatch.Elapsed.TotalMilliseconds:N2} ms.");

        var inputs = new[] { "hello", "good morning", "你好", "翻译" };
        foreach (var input in inputs)
        {
            _ = repository.Lookup(input, LanguageDetector.Detect(input), 8);
        }

        const int iterations = 500;
        var stopwatch = Stopwatch.StartNew();
        for (var index = 0; index < iterations; index++)
        {
            foreach (var input in inputs)
            {
                _ = repository.Lookup(input, LanguageDetector.Detect(input), 8);
            }
        }

        stopwatch.Stop();
        var lookupCount = iterations * inputs.Length;
        var averageMilliseconds = stopwatch.Elapsed.TotalMilliseconds / lookupCount;
        TestContext.WriteLine($"{lookupCount:N0} lookups in {stopwatch.Elapsed.TotalMilliseconds:N2} ms; average {averageMilliseconds:N4} ms.");

        Assert.IsLessThan(1.0, averageMilliseconds, "Warm local dictionary lookup should average under 1 ms.");
        Assert.IsLessThan(1_000.0, openStopwatch.Elapsed.TotalMilliseconds, "Opening the local dictionary should stay under one second.");
    }
}
