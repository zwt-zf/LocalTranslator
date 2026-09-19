# LocalTranslator

LocalTranslator 是一款面向 [PowerToys Run](https://learn.microsoft.com/windows/powertoys/run) 的本地中英翻译插件。在 PowerToys Run 中输入 `tr` 和待翻译内容，即可快速查询英文单词、英文短语、中文词语及常用表达。

翻译数据和查询引擎均在本机运行，不需要 API Key，不会把输入内容发送到网络服务。

```text
tr hello          → 你好；您好
tr good morning   → 早上好
tr 你是谁          → who are you?
tr 亡羊补牢        → better late than never; ...
```

## 功能特性

- **中英双向翻译**：自动识别输入语言，无需手动选择翻译方向。
- **完全离线**：查询过程不访问远程 API，断网环境下也可使用。
- **即时响应**：采用只读二进制索引和二分查询，避免模型加载与推理等待。
- **丰富词库**：整合 ECDICT、CC-CEDICT，并为常用表达提供优先释义。
- **短语与成语支持**：除单词外，也可查询固定搭配、常用问句和中文成语。
- **输入容错**：自动处理英文大小写、多余空格以及中英文句末标点。
- **快速复制**：按回车复制首条译文，也可通过上下文菜单复制原文或译文。
- **轻量运行**：不依赖 GPU，不需要在后台运行额外的模型服务。

## 系统要求

- Windows 10 2004 或更高版本
- x64 处理器
- PowerToys 0.97 或更高版本
- 已启用 PowerToys Run

## 安装

### 使用安装程序

1. 从系统托盘退出 PowerToys。
2. 运行 [LocalTranslator-Setup-0.1.1-x64.exe](artifacts/LocalTranslator-Setup-0.1.1-x64.exe)。
3. 完成安装后重新启动 PowerToys。
4. 打开 PowerToys Run，输入 `tr hello` 验证插件。

安装程序会将插件安装到当前用户的标准插件目录：

```text
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\LocalTranslator
```

安装过程不需要管理员权限。插件可在 Windows 的“已安装的应用”中正常卸载或升级。

> [!NOTE]
> 当前发布的安装程序尚未进行商业代码签名，Windows SmartScreen 可能显示“未知发布者”。如需校验下载文件，请对照发布页面提供的 SHA-256 摘要。

### 手动安装

1. 下载并解压 `LocalTranslator-0.1.1-x64.zip`。
2. 退出 PowerToys。
3. 将压缩包中的全部文件复制到上述 `LocalTranslator` 插件目录。
4. 重新启动 PowerToys。

## 使用方法

按 `Alt + Space` 打开 PowerToys Run，然后输入触发词 `tr`、一个空格以及待翻译内容。

| 输入 | 翻译方向 | 示例结果 |
| --- | --- | --- |
| `tr apple` | 英译中 | 苹果 |
| `tr break a leg` | 英译中 | 祝好运；大获成功 |
| `tr 本地` | 中译英 | local; locally |
| `tr 你是谁？` | 中译英 | who are you? |
| `tr 人工智能` | 中译英 | artificial intelligence (AI) |

操作方式：

- `Enter`：复制当前结果的译文。
- `Ctrl+C`：通过结果上下文菜单复制译文。
- `Ctrl+Shift+C`：通过结果上下文菜单复制原文。

触发词可以在 PowerToys 设置的 PowerToys Run 插件页面中修改。

## 项目优势

### 隐私友好

LocalTranslator 的运行时代码不包含网络请求。所有输入只用于本机词典检索，不会被上传、记录或用于远程分析。

### 低延迟

词典以按键排序的二进制格式保存，运行时只加载偏移索引，不会把全部释义读入内存。查询通过二分查找定位词条，预热后的自动化测试平均查询时间低于 1 毫秒。

### 稳定且易于部署

插件没有 Python、GPU、Web 服务或本地模型进程依赖。安装目录包含运行所需的词典与程序集，安装后即可离线使用。

### 可扩展

翻译数据访问由 `ITranslationRepository` 抽象。开发者可以在不修改 PowerToys 交互层的情况下增加新词典、专业术语库或其他本地翻译后端。

## 从源码构建

### 开发环境

- .NET 9 SDK
- PowerShell 7 或 Windows PowerShell 5.1
- Inno Setup 6/7，仅在构建图形化安装程序时需要

### 构建插件

默认构建使用仓库内置的小型开发词典：

```powershell
.\scripts\build.ps1
```

生成完整离线词典并构建 Release：

```powershell
.\scripts\build.ps1 -Configuration Release -FullDictionary
```

构建 x64 压缩包：

```powershell
.\scripts\pack.ps1 -Platform x64 -FullDictionary
```

构建 Windows 安装程序：

```powershell
winget install --id JRSoftware.InnoSetup -e
.\scripts\build-installer.ps1
```

构建产物会写入 `artifacts` 目录。

### 运行测试

```powershell
dotnet test .\LocalTranslator.slnx -c Release -p:Platform=x64
```

测试覆盖语言识别、输入规范化、双向翻译、短语查询、PowerToys 结果生成以及本地词典性能。

## 技术架构

```text
PowerToys Run
    │
    ▼
语言检测与输入规范化
    │
    ├── 高频词与常用短语优先表
    │
    └── 本地二进制词典索引
            ├── ECDICT（英 → 中）
            └── CC-CEDICT（中 → 英）
```

项目目录：

```text
Community.PowerToys.Run.Plugin.LocalTranslator/           插件与查询引擎
Community.PowerToys.Run.Plugin.LocalTranslator.UnitTests/ 自动化测试
dictionary-sources/                                       开发词典源文件
installer/                                                Inno Setup 安装器
scripts/                                                  构建、打包与安装脚本
tools/LocalTranslator.DictionaryBuilder/                  词典转换工具
```

## 数据来源

- [ECDICT](https://github.com/skywind3000/ECDICT)：英文到中文词典数据。
- [CC-CEDICT](https://cc-cedict.org/)：中文到英文词典数据。

数据集及安装器翻译文件的详细版权与许可信息请参阅 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。

## 贡献

欢迎提交问题、改进词条或贡献代码。提交代码前请确保：

1. Release 构建无警告。
2. 所有自动化测试通过。
3. 新增翻译行为包含对应测试。
4. 新增数据源包含清晰的来源和许可证说明。

## 致谢

感谢 PowerToys、ECDICT、CC-CEDICT 以及 Inno Setup 社区提供的工具和开放数据。
