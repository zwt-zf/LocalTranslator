using Community.PowerToys.Run.Plugin.LocalTranslator.Translation;
using ManagedCommon;
using System.Windows;
using System.Windows.Input;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.LocalTranslator;

/// <summary>
/// Offline Chinese-English translation plugin for PowerToys Run.
/// </summary>
public sealed class Main : IPlugin, IContextMenu, IDisposable
{
    private readonly TranslationService translator;
    private PluginInitContext? context;
    private string iconPath = "Images/localtranslator.dark.png";
    private bool disposed;

    public Main()
        : this(new TranslationService())
    {
    }

    internal Main(TranslationService translator)
    {
        this.translator = translator;
    }

    public static string PluginID => "B008DD20E2654C0996AD590FA0313A2C";

    public string Name => "本地翻译";

    public string Description => "纯本地中英单词和短语互译，自动识别语言";

    public List<Result> Query(Query query)
    {
        var search = query?.Search?.Trim() ?? string.Empty;
        if (search.Length == 0)
        {
            return
            [
                CreateInformationResult("输入要翻译的单词或短语", "示例：tr hello  或  tr 你好"),
            ];
        }

        var response = translator.Translate(search);
        if (response.Error is not null)
        {
            return [CreateInformationResult(response.Error, "支持中文和英文；语言会自动识别")];
        }

        if (response.Entries.Count == 0)
        {
            return
            [
                CreateInformationResult(
                    $"未找到“{response.NormalizedText}”",
                    "可运行 scripts\\build-dictionary.ps1 -Full 安装完整离线词典"),
            ];
        }

        var direction = response.SourceLanguage == SourceLanguage.English ? "英 → 中" : "中 → 英";
        return response.Entries.Select(entry => CreateTranslationResult(entry, direction)).ToList();
    }

    public void Init(PluginInitContext pluginContext)
    {
        context = pluginContext ?? throw new ArgumentNullException(nameof(pluginContext));
        context.API.ThemeChanged += OnThemeChanged;
        UpdateIconPath(context.API.GetCurrentTheme());
    }

    public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    {
        if (selectedResult.ContextData is not TranslationResultContext resultContext)
        {
            return [];
        }

        return
        [
            new ContextMenuResult
            {
                PluginName = Name,
                Title = "复制译文 (Ctrl+C)",
                Glyph = "\xE8C8",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                AcceleratorKey = Key.C,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = _ => CopyToClipboard(resultContext.Translation),
            },
            new ContextMenuResult
            {
                PluginName = Name,
                Title = "复制原文 (Ctrl+Shift+C)",
                Glyph = "\xF0E3",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                AcceleratorKey = Key.C,
                AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                Action = _ => CopyToClipboard(resultContext.Source),
            },
        ];
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (context?.API is not null)
        {
            context.API.ThemeChanged -= OnThemeChanged;
        }

        translator.Dispose();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    private Result CreateTranslationResult(TranslationEntry entry, string direction)
    {
        var title = ToSingleLine(entry.Translation);
        var metadata = new[] { direction, entry.Source, entry.Phonetic, entry.PartOfSpeech }
            .Where(value => !string.IsNullOrWhiteSpace(value));
        var subtitle = string.Join(" · ", metadata);
        if (entry.IsSuggestion)
        {
            subtitle = "候选 · " + subtitle;
        }

        return new Result
        {
            QueryTextDisplay = entry.Source,
            IcoPath = iconPath,
            Title = title,
            SubTitle = subtitle,
            ToolTipData = new ToolTipData(entry.Source, entry.Translation),
            ContextData = new TranslationResultContext(entry.Source, entry.Translation),
            Score = entry.Priority,
            Action = _ => CopyToClipboard(entry.Translation),
        };
    }

    private Result CreateInformationResult(string title, string subtitle) => new()
    {
        IcoPath = iconPath,
        Title = title,
        SubTitle = subtitle,
        Score = 1,
        Action = _ => false,
    };

    private static string ToSingleLine(string value) =>
        value.Replace("\\n", "；", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();

    private static bool CopyToClipboard(string text)
    {
        try
        {
            Clipboard.SetText(text);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void UpdateIconPath(Theme theme) =>
        iconPath = theme is Theme.Light or Theme.HighContrastWhite
            ? "Images/localtranslator.light.png"
            : "Images/localtranslator.dark.png";

    private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);

    private sealed record TranslationResultContext(string Source, string Translation);
}
