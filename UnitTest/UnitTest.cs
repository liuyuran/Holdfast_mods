using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TranslateCN;

namespace UnitTest
{
    [TestClass]
    public class UnitTestTranslate
    {
        [TestMethod]
        public void TestReplaceBox()
        {
            Main.loadReplaceBox();
            Assert.IsTrue(Main.translateReplace.ContainsKey("（"));
            Assert.IsTrue(Main.translateReplace.ContainsKey("）"));
            Assert.IsTrue(Main.translateReplace.ContainsKey("，"));
            Assert.IsTrue(Main.translateReplace.ContainsKey("。"));
            Assert.IsTrue(Main.translateReplace.ContainsKey("「"));
            Assert.IsTrue(Main.translateReplace.ContainsKey("」"));
            Assert.IsTrue(Main.translateReplace.ContainsKey("："));
            Assert.IsTrue(Main.translateReplace.ContainsKey("！"));
            Assert.IsTrue(Main.translateReplace.ContainsKey("《"));
            Assert.IsTrue(Main.translateReplace.ContainsKey("》"));
            Assert.IsTrue(Main.translateReplace.ContainsKey("；"));
        }
    }
}
