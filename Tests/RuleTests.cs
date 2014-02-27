using TinyPraxis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// Summary description for RuleTests
    /// </summary>
    [TestClass]
    public class RuleTests
    {
        [TestInitialize()]
        public void MyTestInitialize()
        {
            KB.Clear();
            KB.Assert(
                KB.Store + "Newt" + "loves" + "Newt",
                KB.Store + "John" + "loves" + "Mary",
                KB.Store + "John" + "loves" + "Kelly",
                KB.Store + "John" + "loves" + "Mary",
                KB.Store + "Kelly" + "loves" + "Mary"
                );
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            KB.Clear();
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void SimpleRuleTest()
        {
            // Update: Mary loves everyone who loves her.
            var v = new Variable("v");
            new RuleSet("test ruleset",
                new Rule(
                    "Mary loves those who love her",
                    KB.Query + v + "loves" + "Mary",
                    () => KB.Assert(KB.Store + "Mary" + "loves" + v))).Run();

            var v2 = new Variable("v2");
            CollectionAssert.AreEqual(new object[] { "John", "Kelly" }, (KB.Query + "Mary" + "loves" + v2).FindAll(v2));
        }
    }
}
