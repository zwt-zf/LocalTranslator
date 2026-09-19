namespace Community.PowerToys.Run.Plugin.LocalTranslator.Translation;

internal sealed class CompositeTranslationRepository : ITranslationRepository
{
    private readonly IReadOnlyList<ITranslationRepository> repositories;

    internal CompositeTranslationRepository(params ITranslationRepository[] repositories)
    {
        this.repositories = repositories;
    }

    public IReadOnlyList<TranslationEntry> Lookup(string normalizedText, SourceLanguage language, int maxResults)
    {
        var suggestions = new List<TranslationEntry>();
        foreach (var repository in repositories)
        {
            var entries = repository.Lookup(normalizedText, language, maxResults);
            var exactEntries = entries.Where(entry => !entry.IsSuggestion).Take(maxResults).ToArray();
            if (exactEntries.Length > 0)
            {
                return exactEntries;
            }

            suggestions.AddRange(entries.Where(entry => entry.IsSuggestion));
        }

        return suggestions
            .DistinctBy(entry => (entry.Source, entry.Translation))
            .Take(maxResults)
            .ToArray();
    }

    public void Dispose()
    {
        foreach (var repository in repositories)
        {
            repository.Dispose();
        }
    }
}
