namespace Community.PowerToys.Run.Plugin.LocalTranslator.Translation;

internal interface ITranslationRepository : IDisposable
{
    IReadOnlyList<TranslationEntry> Lookup(string normalizedText, SourceLanguage language, int maxResults);
}
