using Community.PowerToys.Run.Plugin.LocalTranslator.Translation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.LocalTranslator.UnitTests;

[TestClass]
public sealed class LanguageDetectorTests
{
    [TestMethod]
    [DataRow("hello", "English")]
    [DataRow("good morning!", "English")]
    [DataRow("你好", "Chinese")]
    [DataRow("PowerToys 翻译", "Chinese")]
    [DataRow("123?!", "Unknown")]
    public void Detect_returns_expected_language(string input, string expected)
    {
        Assert.AreEqual(expected, LanguageDetector.Detect(input).ToString());
    }

    [TestMethod]
    public void Normalize_collapses_english_whitespace_and_case()
    {
        Assert.AreEqual(
            "good morning",
            LanguageDetector.Normalize("  GOOD    Morning!  ", SourceLanguage.English));
    }

    [TestMethod]
    public void Normalize_removes_spaces_between_chinese_characters()
    {
        Assert.AreEqual("本地翻译", LanguageDetector.Normalize(" 本地 翻译。 ", SourceLanguage.Chinese));
    }
}
