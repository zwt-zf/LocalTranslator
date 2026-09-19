namespace Community.PowerToys.Run.Plugin.LocalTranslator.Translation;

internal sealed class BuiltInDictionaryRepository : ITranslationRepository
{
    private static readonly TranslationEntry[] Entries =
    [
        new("apple", "苹果", "/ˈæpəl/", "n.", 100),
        new("book", "书；书籍", "/bʊk/", "n.", 100),
        new("china", "中国", "/ˈtʃaɪnə/", "n.", 100),
        new("chinese", "中文；中国人；中国的", "/ˌtʃaɪˈniːz/", "n./adj.", 100),
        new("computer", "计算机；电脑", "/kəmˈpjuːtə(r)/", "n.", 100),
        new("good", "好的；优秀的", "/ɡʊd/", "adj.", 100),
        new("good morning", "早上好", "/ɡʊd ˈmɔːnɪŋ/", "phrase", 120),
        new("good night", "晚安", "/ɡʊd naɪt/", "phrase", 120),
        new("hello", "你好；您好", "/həˈləʊ/", "interj.", 120),
        new("how are you", "你好吗；最近怎么样", string.Empty, "phrase", 120),
        new("language", "语言", "/ˈlæŋɡwɪdʒ/", "n.", 100),
        new("local", "本地的；当地的", "/ˈləʊkəl/", "adj.", 100),
        new("please", "请；请问", "/pliːz/", "adv./v.", 100),
        new("power toys", "PowerToys；微软实用工具集", string.Empty, "proper noun", 100),
        new("thank you", "谢谢你；谢谢", string.Empty, "phrase", 120),
        new("translate", "翻译；转化", "/trænzˈleɪt/", "v.", 100),
        new("translation", "翻译；译文", "/trænzˈleɪʃn/", "n.", 100),
        new("who am i", "我是谁", string.Empty, "phrase", 120),
        new("who are you", "你是谁", string.Empty, "phrase", 120),
        new("world", "世界", "/wɜːld/", "n.", 100),
        new("中国", "China", "Zhōng guó", "proper noun", 120),
        new("中文", "Chinese; Chinese language", "Zhōng wén", "n.", 120),
        new("你好", "hello; hi", "nǐ hǎo", "phrase", 120),
        new("你好吗", "how are you?", "nǐ hǎo ma", "phrase", 120),
        new("你是谁", "who are you?", "nǐ shì shéi", "phrase", 120),
        new("你是誰", "who are you?", "nǐ shì shéi", "phrase", 120),
        new("我是谁", "who am I?", "wǒ shì shéi", "phrase", 120),
        new("我是誰", "who am I?", "wǒ shì shéi", "phrase", 120),
        new("世界", "world", "shì jiè", "n.", 100),
        new("书", "book", "shū", "n.", 100),
        new("早上好", "good morning", "zǎo shang hǎo", "phrase", 120),
        new("晚安", "good night", "wǎn ān", "phrase", 120),
        new("本地", "local; locally", "běn dì", "n./adj.", 100),
        new("电脑", "computer", "diàn nǎo", "n.", 100),
        new("翻译", "to translate; translation", "fān yì", "v./n.", 120),
        new("苹果", "apple", "píng guǒ", "n.", 100),
        new("谢谢", "thanks; thank you", "xiè xie", "phrase", 120),
        new("语言", "language", "yǔ yán", "n.", 100),
        new("请", "please; to ask; to invite", "qǐng", "v.", 100),
    ];

    public IReadOnlyList<TranslationEntry> Lookup(string normalizedText, SourceLanguage language, int maxResults)
    {
        var exact = Entries
            .Where(entry => string.Equals(entry.Source, normalizedText, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(entry => entry.Priority)
            .Take(maxResults)
            .ToArray();

        if (exact.Length > 0)
        {
            return exact;
        }

        if (normalizedText.Length < 2)
        {
            return [];
        }

        return Entries
            .Where(entry => entry.Source.StartsWith(normalizedText, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(entry => entry.Priority)
            .ThenBy(entry => entry.Source.Length)
            .Take(maxResults)
            .Select(entry => entry with { IsSuggestion = true })
            .ToArray();
    }

    public void Dispose()
    {
    }
}
