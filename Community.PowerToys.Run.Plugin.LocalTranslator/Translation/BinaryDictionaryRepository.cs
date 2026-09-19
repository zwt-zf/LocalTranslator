using System.Text;
using System.Runtime.InteropServices;

namespace Community.PowerToys.Run.Plugin.LocalTranslator.Translation;

internal sealed class BinaryDictionaryRepository : ITranslationRepository
{
    internal const string FileMagic = "LTDICT01";
    internal const int FileVersion = 1;

    private readonly object syncRoot = new();
    private readonly FileStream stream;
    private readonly BinaryReader reader;
    private readonly long[] offsets;
    private bool disposed;

    internal BinaryDictionaryRepository(string path)
    {
        stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 32 * 1024,
            FileOptions.RandomAccess);
        reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        var magic = Encoding.ASCII.GetString(reader.ReadBytes(FileMagic.Length));
        var version = reader.ReadInt32();
        var count = reader.ReadInt32();

        if (magic != FileMagic || version != FileVersion || count < 0)
        {
            throw new InvalidDataException("Unsupported LocalTranslator dictionary format.");
        }

        offsets = new long[count];
        stream.ReadExactly(MemoryMarshal.AsBytes(offsets.AsSpan()));
    }

    public IReadOnlyList<TranslationEntry> Lookup(string normalizedText, SourceLanguage language, int maxResults)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        lock (syncRoot)
        {
            var start = LowerBound(normalizedText);
            var matches = new List<TranslationEntry>(maxResults);

            for (var index = start; index < offsets.Length && matches.Count < maxResults; index++)
            {
                var entry = ReadEntry(index);
                var comparison = string.Compare(entry.Source, normalizedText, StringComparison.Ordinal);
                if (comparison != 0)
                {
                    break;
                }

                matches.Add(entry);
            }

            if (matches.Count > 0 || normalizedText.Length < 2)
            {
                return matches;
            }

            for (var index = start; index < offsets.Length && matches.Count < maxResults; index++)
            {
                var entry = ReadEntry(index);
                if (!entry.Source.StartsWith(normalizedText, StringComparison.Ordinal))
                {
                    break;
                }

                matches.Add(entry with { IsSuggestion = true });
            }

            return matches;
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        reader.Dispose();
        stream.Dispose();
        disposed = true;
    }

    private int LowerBound(string key)
    {
        var low = 0;
        var high = offsets.Length;

        while (low < high)
        {
            var middle = low + ((high - low) / 2);
            var middleKey = ReadKey(middle);
            if (string.Compare(middleKey, key, StringComparison.Ordinal) < 0)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }

    private string ReadKey(int index)
    {
        stream.Position = offsets[index];
        return reader.ReadString();
    }

    private TranslationEntry ReadEntry(int index)
    {
        stream.Position = offsets[index];
        return new TranslationEntry(
            reader.ReadString(),
            reader.ReadString(),
            reader.ReadString(),
            reader.ReadString(),
            reader.ReadInt32());
    }
}
