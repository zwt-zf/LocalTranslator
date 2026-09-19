namespace Community.PowerToys.Run.Plugin.LocalTranslator.Translation;

internal sealed record TranslationEntry(
    string Source,
    string Translation,
    string Phonetic,
    string PartOfSpeech,
    int Priority = 0,
    bool IsSuggestion = false);
