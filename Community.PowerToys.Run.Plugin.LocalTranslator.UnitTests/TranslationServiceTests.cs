using Community.PowerToys.Run.Plugin.LocalTranslator.Translation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.LocalTranslator.UnitTests;

[TestClass]
public sealed class TranslationServiceTests
{
    private TranslationService translator = null!;

    [TestInitialize]
    public void Initialize()
    {
        translator = new TranslationService(new BuiltInDictionaryRepository());
    }

    [TestCleanup]
    public void Cleanup()
    {
        translator.Dispose();
    }

    [TestMethod]
    public void Translate_english_word_to_chinese()
    {
        var response = translator.Translate("hello");

        Assert.IsNull(response.Error);
        Assert.AreEqual(SourceLanguage.English, response.SourceLanguage);
        StringAssert.Contains(response.Entries[0].Translation, "你好");
    }

    [TestMethod]
    public void Translate_chinese_phrase_to_english()
    {
        var response = translator.Translate("早上好");

        Assert.IsNull(response.Error);
        Assert.AreEqual(SourceLanguage.Chinese, response.SourceLanguage);
        StringAssert.Contains(response.Entries[0].Translation, "good morning");
    }

    [TestMethod]
    [DataRow("你是谁", "who are you")]
    [DataRow("你是谁？", "who are you")]
    [DataRow("你是谁;", "who are you")]
    [DataRow("who are you", "你是谁")]
    public void Translate_supports_who_are_you_in_both_directions(string input, string expected)
    {
        var response = translator.Translate(input);

        Assert.IsNull(response.Error);
        Assert.IsNotEmpty(response.Entries);
        StringAssert.Contains(response.Entries[0].Translation, expected);
    }

    [TestMethod]
    public void Translate_returns_prefix_suggestions_when_exact_entry_is_missing()
    {
        var response = translator.Translate("transl");

        Assert.IsGreaterThanOrEqualTo(response.Entries.Count, 2);
        Assert.IsTrue(response.Entries.All(entry => entry.IsSuggestion));
    }

    [TestMethod]
    public void Translate_rejects_non_language_input()
    {
        var response = translator.Translate("12345");

        Assert.IsNotNull(response.Error);
        Assert.IsEmpty(response.Entries);
    }
}
