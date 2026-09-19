using System.Globalization;
using System.Text;

namespace Community.PowerToys.Run.Plugin.LocalTranslator.Translation;

internal enum SourceLanguage
{
    Unknown,
    English,
    Chinese,
}

internal static class LanguageDetector
{
    internal static SourceLanguage Detect(string text)
    {
        var latinCount = 0;
        var hanCount = 0;

        foreach (var rune in text.EnumerateRunes())
        {
            if (IsHan(rune.Value))
            {
                hanCount++;
                continue;
            }

            if ((rune.Value >= 'A' && rune.Value <= 'Z') ||
                (rune.Value >= 'a' && rune.Value <= 'z'))
            {
                latinCount++;
            }
        }

        if (hanCount > 0)
        {
            return SourceLanguage.Chinese;
        }

        return latinCount > 0 ? SourceLanguage.English : SourceLanguage.Unknown;
    }

    internal static string Normalize(string text, SourceLanguage language)
    {
        var normalized = text.Normalize(NormalizationForm.FormKC).Trim();
        normalized = TrimOuterPunctuation(normalized);

        if (language == SourceLanguage.English)
        {
            normalized = string.Join(' ', normalized.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            return normalized.ToLower(CultureInfo.InvariantCulture);
        }

        return normalized.Replace(" ", string.Empty, StringComparison.Ordinal);
    }

    private static string TrimOuterPunctuation(string value)
    {
        var start = 0;
        var end = value.Length;

        while (start < end && IsOuterPunctuation(value[start]))
        {
            start++;
        }

        while (end > start && IsOuterPunctuation(value[end - 1]))
        {
            end--;
        }

        return value[start..end];
    }

    private static bool IsOuterPunctuation(char value) =>
        char.IsPunctuation(value) && value is not '-' and not '\'';

    private static bool IsHan(int value) =>
        (value >= 0x3400 && value <= 0x4DBF) ||
        (value >= 0x4E00 && value <= 0x9FFF) ||
        (value >= 0xF900 && value <= 0xFAFF) ||
        (value >= 0x20000 && value <= 0x323AF);
}
