using System.Reflection;

namespace Community.PowerToys.Run.Plugin.LocalTranslator.Translation;

internal sealed class TranslationService : IDisposable
{
    private const int MaximumInputLength = 200;
    private readonly ITranslationRepository repository;

    internal TranslationService()
        : this(CreateRepository())
    {
    }

    internal TranslationService(ITranslationRepository repository)
    {
        this.repository = repository;
    }

    internal TranslationResponse Translate(string input, int maxResults = 8)
    {
        var language = LanguageDetector.Detect(input);
        if (language == SourceLanguage.Unknown)
        {
            return TranslationResponse.Failure("请输入英文或中文单词/短语");
        }

        var normalizedText = LanguageDetector.Normalize(input, language);
        if (normalizedText.Length == 0)
        {
            return TranslationResponse.Failure("请输入英文或中文单词/短语");
        }

        if (normalizedText.Length > MaximumInputLength)
        {
            return TranslationResponse.Failure($"当前版本支持 {MaximumInputLength} 个字符以内的单词和短语");
        }

        var entries = repository.Lookup(normalizedText, language, maxResults);
        return new TranslationResponse(language, normalizedText, entries, null);
    }

    public void Dispose() => repository.Dispose();

    private static ITranslationRepository CreateRepository()
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var dictionaryPath = Path.Combine(assemblyDirectory, "Data", "localtranslator.dict");

        if (!File.Exists(dictionaryPath))
        {
            return new BuiltInDictionaryRepository();
        }

        try
        {
            return new CompositeTranslationRepository(
                new BuiltInDictionaryRepository(),
                new BinaryDictionaryRepository(dictionaryPath));
        }
        catch (IOException)
        {
            return new BuiltInDictionaryRepository();
        }
        catch (InvalidDataException)
        {
            return new BuiltInDictionaryRepository();
        }
    }
}

internal sealed record TranslationResponse(
    SourceLanguage SourceLanguage,
    string NormalizedText,
    IReadOnlyList<TranslationEntry> Entries,
    string? Error)
{
    internal static TranslationResponse Failure(string message) =>
        new(SourceLanguage.Unknown, string.Empty, [], message);
}
