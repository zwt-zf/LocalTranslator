using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

const string fileMagic = "LTDICT01";
const int fileVersion = 1;

var arguments = ParseArguments(args);
if (!arguments.TryGetValue("output", out var outputPath))
{
    Console.Error.WriteLine("Usage: --output <file> [--ecdict <csv>] [--cedict <u8/txt>]");
    return 2;
}

var entries = new List<DictionaryRecord>();
if (arguments.TryGetValue("ecdict", out var ecdictPath))
{
    ReadEcdict(ecdictPath, entries);
}

if (arguments.TryGetValue("cedict", out var cedictPath))
{
    ReadCedict(cedictPath, entries);
}

if (entries.Count == 0)
{
    Console.Error.WriteLine("No dictionary entries were read.");
    return 3;
}

entries.Sort(DictionaryRecordComparer.Instance);
RemoveDuplicates(entries);
WriteDictionary(outputPath, entries);
Console.WriteLine($"Wrote {entries.Count:N0} local dictionary entries to {outputPath}");
return 0;

static Dictionary<string, string> ParseArguments(string[] values)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var index = 0; index + 1 < values.Length; index += 2)
    {
        var key = values[index].TrimStart('-');
        result[key] = values[index + 1];
    }

    return result;
}

static void ReadEcdict(string path, List<DictionaryRecord> destination)
{
    Console.WriteLine($"Reading ECDICT: {path}");
    using var parser = new TextFieldParser(path, Encoding.UTF8)
    {
        TextFieldType = FieldType.Delimited,
        HasFieldsEnclosedInQuotes = true,
        TrimWhiteSpace = false,
    };
    parser.SetDelimiters(",");

    var header = parser.ReadFields() ?? throw new InvalidDataException("ECDICT header is missing.");
    var columns = header
        .Select((name, index) => (name, index))
        .ToDictionary(item => item.name, item => item.index, StringComparer.OrdinalIgnoreCase);

    while (!parser.EndOfData)
    {
        var fields = parser.ReadFields();
        if (fields is null)
        {
            continue;
        }

        var word = GetField(fields, columns, "word");
        var translation = CleanText(GetField(fields, columns, "translation"));
        if (string.IsNullOrWhiteSpace(word) || string.IsNullOrWhiteSpace(translation))
        {
            continue;
        }

        var normalizedWord = NormalizeEnglish(word);
        if (normalizedWord.Length == 0)
        {
            continue;
        }

        var phonetic = GetField(fields, columns, "phonetic");
        if (phonetic.Length > 0 && !phonetic.StartsWith('/'))
        {
            phonetic = $"/{phonetic}/";
        }

        var collins = ParseInteger(GetField(fields, columns, "collins"));
        var bnc = ParseInteger(GetField(fields, columns, "bnc"));
        var frq = ParseInteger(GetField(fields, columns, "frq"));
        var priority = (collins * 100_000) + FrequencyBonus(bnc) + FrequencyBonus(frq);

        destination.Add(new DictionaryRecord(
            normalizedWord,
            translation,
            phonetic,
            GetField(fields, columns, "pos"),
            priority));
    }
}

static void ReadCedict(string path, List<DictionaryRecord> destination)
{
    Console.WriteLine($"Reading CC-CEDICT: {path}");
    var pattern = new Regex(
        @"^(?<traditional>\S+)\s+(?<simplified>\S+)\s+\[(?<pinyin>[^\]]+)\]\s+/(?<definitions>.+)/$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    foreach (var line in File.ReadLines(path, Encoding.UTF8))
    {
        if (line.Length == 0 || line[0] == '#')
        {
            continue;
        }

        var match = pattern.Match(line);
        if (!match.Success)
        {
            continue;
        }

        var traditional = NormalizeChinese(match.Groups["traditional"].Value);
        var simplified = NormalizeChinese(match.Groups["simplified"].Value);
        var pinyin = match.Groups["pinyin"].Value;
        var definitions = match.Groups["definitions"].Value.Replace('/', ';').Trim(' ', ';');
        var priority = Math.Max(1, 50_000 - definitions.Length);

        destination.Add(new DictionaryRecord(simplified, definitions, pinyin, string.Empty, priority));
        if (!string.Equals(traditional, simplified, StringComparison.Ordinal))
        {
            destination.Add(new DictionaryRecord(traditional, definitions, pinyin, string.Empty, priority - 1));
        }
    }
}

static string GetField(string[] fields, IReadOnlyDictionary<string, int> columns, string name)
{
    if (!columns.TryGetValue(name, out var index) || index < 0 || index >= fields.Length)
    {
        return string.Empty;
    }

    return fields[index].Trim();
}

static string CleanText(string value) =>
    value.Replace("\\n", "；", StringComparison.Ordinal)
        .Replace('\r', ' ')
        .Replace('\n', ' ')
        .Trim();

static string NormalizeEnglish(string value) =>
    string.Join(' ', value.Normalize(NormalizationForm.FormKC)
        .Trim()
        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .ToLower(CultureInfo.InvariantCulture);

static string NormalizeChinese(string value) =>
    value.Normalize(NormalizationForm.FormKC).Replace(" ", string.Empty, StringComparison.Ordinal);

static int ParseInteger(string value) =>
    int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0;

static int FrequencyBonus(int rank) => rank > 0 ? Math.Max(0, 50_000 - rank) : 0;

static void RemoveDuplicates(List<DictionaryRecord> entries)
{
    if (entries.Count < 2)
    {
        return;
    }

    var writeIndex = 1;
    for (var readIndex = 1; readIndex < entries.Count; readIndex++)
    {
        var previous = entries[writeIndex - 1];
        var current = entries[readIndex];
        if (string.Equals(previous.Source, current.Source, StringComparison.Ordinal) &&
            string.Equals(previous.Translation, current.Translation, StringComparison.Ordinal))
        {
            continue;
        }

        entries[writeIndex++] = current;
    }

    if (writeIndex < entries.Count)
    {
        entries.RemoveRange(writeIndex, entries.Count - writeIndex);
    }
}

static void WriteDictionary(string path, IReadOnlyList<DictionaryRecord> entries)
{
    var fullPath = Path.GetFullPath(path);
    var directory = Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException("Output directory is invalid.");
    Directory.CreateDirectory(directory);

    var temporaryPath = fullPath + ".tmp";
    using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
    using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
    {
        writer.Write(Encoding.ASCII.GetBytes(fileMagic));
        writer.Write(fileVersion);
        writer.Write(entries.Count);

        var indexStart = stream.Position;
        for (var index = 0; index < entries.Count; index++)
        {
            writer.Write(0L);
        }

        var offsets = new long[entries.Count];
        for (var index = 0; index < entries.Count; index++)
        {
            offsets[index] = stream.Position;
            var entry = entries[index];
            writer.Write(entry.Source);
            writer.Write(entry.Translation);
            writer.Write(entry.Phonetic);
            writer.Write(entry.PartOfSpeech);
            writer.Write(entry.Priority);
        }

        stream.Position = indexStart;
        foreach (var offset in offsets)
        {
            writer.Write(offset);
        }
    }

    File.Move(temporaryPath, fullPath, overwrite: true);
}

internal sealed record DictionaryRecord(
    string Source,
    string Translation,
    string Phonetic,
    string PartOfSpeech,
    int Priority);

internal sealed class DictionaryRecordComparer : IComparer<DictionaryRecord>
{
    internal static DictionaryRecordComparer Instance { get; } = new();

    public int Compare(DictionaryRecord? x, DictionaryRecord? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var sourceComparison = string.Compare(x.Source, y.Source, StringComparison.Ordinal);
        if (sourceComparison != 0)
        {
            return sourceComparison;
        }

        var priorityComparison = y.Priority.CompareTo(x.Priority);
        return priorityComparison != 0
            ? priorityComparison
            : string.Compare(x.Translation, y.Translation, StringComparison.Ordinal);
    }
}
