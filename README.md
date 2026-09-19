# LocalTranslator for PowerToys Run

一个纯本地、低延迟的 PowerToys Run 中英翻译插件。当前版本面向单词和短语，自动检测输入语言，不调用任何远程翻译 API。

## 使用

1. 按 `Alt + Space` 打开 PowerToys Run。
2. 输入 `tr`、空格和要翻译的内容。
3. 按回车复制第一条译文，或用 `Ctrl+C` 复制选中的译文。

示例：

```text
tr hello
tr good morning
tr 你好
tr 本地翻译
```

插件会自动选择“英 → 中”或“中 → 英”。输入没有精确命中时，会显示本地词典中的前缀候选。

## 构建

需要 .NET 9 SDK。默认构建使用仓库自带的小型开发词典：

```powershell
.\scripts\build.ps1
```

生成包含完整 ECDICT 和 CC-CEDICT 数据的本地词典并打包：

```powershell
.\scripts\pack.ps1 -FullDictionary
```

完整词典只在构建阶段下载一次。安装后的插件查询 `Data/localtranslator.dict`，不会访问网络。生成的 zip 位于 `artifacts`。

## 安装

推荐直接运行图形化安装程序：

```text
artifacts\LocalTranslator-Setup-0.1.1-x64.exe
```

安装器会把插件固定安装到当前用户的 PowerToys Run 插件目录，并在 Windows“应用和功能”中注册卸载项。安装或卸载前需要先从系统托盘退出 PowerToys。

当前生成的安装器尚未进行商业代码签名，因此 Windows SmartScreen 可能显示“未知发布者”；这不影响本地安装和卸载功能。

如需重新生成安装器，请先安装 Inno Setup，然后运行：

```powershell
.\scripts\build-installer.ps1
```

也可以使用脚本安装。退出 PowerToys，然后运行：

```powershell
.\scripts\install.ps1
```

也可以手动把 Release 输出目录复制到：

```text
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\LocalTranslator
```

重新打开 PowerToys 后，可在 PowerToys Run 的插件设置中看到“本地翻译 LocalTranslator”，默认触发词为 `tr`。

## 为什么首版使用词典而不是小模型

当前需求限定为单词和短语。磁盘排序索引的查询通常只需要约 20 次二分比较，没有模型冷启动、推理抖动或 GPU 依赖，内存只保存偏移表。完整词典包含英文词条、固定搭配、中文词语和成语，适合 PowerToys Run 的逐键即时查询场景。

代码把数据访问隔离在 `ITranslationRepository` 后面；如果以后需要完整句子翻译，可以增加本地 ONNX 实现而不改变 PowerToys 的交互层。

## 项目结构

- `Community.PowerToys.Run.Plugin.LocalTranslator`：PowerToys Run 插件和本地查询引擎。
- `tools/LocalTranslator.DictionaryBuilder`：把 ECDICT/CC-CEDICT 转换为只读二进制索引。
- `dictionary-sources/seed`：无需下载即可测试的小型词典。
- `scripts`：词典构建、编译、打包和安装脚本。

第三方词典的来源与许可见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。PowerToys 官方的第三方插件安装目录说明见 [Microsoft PowerToys 文档](https://github.com/microsoft/PowerToys/blob/main/doc/thirdPartyRunPlugins.md)。
