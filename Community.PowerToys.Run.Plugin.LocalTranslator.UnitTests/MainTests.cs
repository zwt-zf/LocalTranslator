using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.LocalTranslator.UnitTests;

[TestClass]
public sealed class MainTests
{
    private Main plugin = null!;

    [TestInitialize]
    public void Initialize()
    {
        plugin = new Main();
    }

    [TestCleanup]
    public void Cleanup()
    {
        plugin.Dispose();
    }

    [TestMethod]
    public void Query_returns_translation_result()
    {
        var result = plugin.Query(new Query("hello")).First();

        StringAssert.Contains(result.Title, "你好");
        StringAssert.Contains(result.SubTitle, "英 → 中");
    }

    [TestMethod]
    public void Query_empty_input_returns_usage_hint()
    {
        var result = plugin.Query(new Query(string.Empty)).Single();

        StringAssert.Contains(result.Title, "输入");
    }

    [TestMethod]
    public void Query_translates_who_are_you()
    {
        var result = plugin.Query(new Query("你是谁;")).First();

        StringAssert.Contains(result.Title, "who are you");
        StringAssert.Contains(result.SubTitle, "中 → 英");
    }

    [TestMethod]
    public void Translation_result_has_copy_context_menus()
    {
        var result = plugin.Query(new Query("你好")).First();
        var menus = plugin.LoadContextMenus(result);

        Assert.HasCount(2, menus);
    }
}
