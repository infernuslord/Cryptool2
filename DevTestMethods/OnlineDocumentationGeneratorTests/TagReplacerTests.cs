using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlineDocumentationGenerator;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;

namespace Tests.OnlineDocumentationGeneratorTests
{
    [TestClass]
    public class TagReplacerTests
    {
        private static readonly LocalizedPluginDocumentationPage _localizedPluginDocumentationPage = new LocalizedPluginDocumentationPage(typeof(KeySearcher.KeySearcher), null, "de");

        [TestMethod]
        public void ReplacePluginDocTags_InputWithOnePluginDocTag()
        {
            var html = "test <pluginDoc property=\"Name\"/> test";
            var res = TagReplacer.ReplacePluginDocTags(html, _localizedPluginDocumentationPage);
            Assert.AreEqual("test KeySearcher test", res);
        }

        [TestMethod]
        public void ReplacePluginDocTags_InputWithTwoPluginDocTag()
        {
            var html = "test <pluginDoc property=\"Name\"/> test <pluginDoc property=\"Lang\"/> test";
            var res = TagReplacer.ReplacePluginDocTags(html, _localizedPluginDocumentationPage);
            Assert.AreEqual("test KeySearcher test de test", res);
        }

        [TestMethod]
        public void FindPluginDocTag_ValidInput_ReturnsPropertyName()
        {
            int pos;
            int len;
            string html = "blablub <pluginDoc property=\"Test\"/> asdasds";
            string res = TagReplacer.FindPluginDocTag(html, out pos, out len);
            Assert.AreEqual("Test", res);
        }
        
        [TestMethod]
        public void FindPluginDocTag_ValidInputWithMultipleTags_ReturnsPropertyName()
        {
            int pos;
            int len;
            string html = "blablub <pluginDoc property=\"Test1\"/> asdasds <pluginDoc property=\"Test2\"/>";
            string res = TagReplacer.FindPluginDocTag(html, out pos, out len);
            Assert.AreEqual("Test1", res);
        }

        [TestMethod]
        public void FindPluginDocTag_InvalidInput_ReturnsNull()
        {
            int pos;
            int len;
            string html = "blablub <pluginDoc property qweasdasd";
            string res = TagReplacer.FindPluginDocTag(html, out pos, out len);
            Assert.AreEqual(null, res);
        }

        [TestMethod]
        public void ReplaceLanguageSelectionTag_InputWithOneLanguageSelectionTag()
        {
            var html = "test <languageSelection /> test";
            var res = TagReplacer.ReplaceLanguageSelectionTag(html, "de, en");
            Assert.AreEqual("test de, en test", res);
        }

        [TestMethod]
        public void ReplaceLanguageSelectionTag_InputWithoutLanguageSelectionTag_ReturnsInputString()
        {
            var html = "test <anguageSelection /> test";
            var res = TagReplacer.ReplaceLanguageSelectionTag(html, "de, en");
            Assert.AreEqual(html, res);
        }

        [TestMethod]
        public void FindLanguageSelectionTag_ValidInput_ReturnsTrue()
        {
            int pos;
            int len;
            string html = "blablub <languageSelection /> qweasdasd";
            var res = TagReplacer.FindLanguageSelectionTag(html, out pos, out len);
            Assert.AreEqual(true, res);
        }

        [TestMethod]
        public void FindLanguageSelectionTag_InvalidInput_ReturnsFalse()
        {
            int pos;
            int len;
            string html = "blablub <anguageSelection /> qweasdasd";
            var res = TagReplacer.FindLanguageSelectionTag(html, out pos, out len);
            Assert.AreEqual(false, res);
        }

        [TestMethod]
        public void ReplaceLanguageSwitchs_InputWithOneLanguageSwitchTag()
        {
            var html = "test <languageSwitch lang=\"en\"> Available languages: </languageSwitch> test";
            var res = TagReplacer.ReplaceLanguageSwitchs(html, "en");
            Assert.AreEqual("test  Available languages:  test", res);
        }

        [TestMethod]
        public void ReplaceLanguageSwitchs_InputWithOneLanguageSwitchTag2()
        {
            var html = "test <languageSwitch lang=\"en\"> Available languages: </languageSwitch> test";
            var res = TagReplacer.ReplaceLanguageSwitchs(html, "de");
            Assert.AreEqual("test  test", res);
        }

        [TestMethod]
        public void ReplaceLanguageSwitchs_InputWithTwoLanguageSwitchTag()
        {
            var html = "test <languageSwitch lang=\"en\"> Available languages: </languageSwitch><languageSwitch lang=\"de\">Verfügbare Sprachen:</languageSwitch> test";
            var res = TagReplacer.ReplaceLanguageSwitchs(html, "de");
            Assert.AreEqual("test Verfügbare Sprachen: test", res);
        }
    }
}
