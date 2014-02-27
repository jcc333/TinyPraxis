using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyPraxis;

namespace Tests
{
    /// <summary>
    /// Summary description for RetractTests
    /// </summary>
    [TestClass]
    public class RetractTests
    {
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

        [TestMethod]
        public void SimpleRetractTest()
        {
            KB.Retract(KB.Query+"John"+"loves"+"Mary");
            Assert.IsFalse(KB.Query + "John" + "loves" + "Mary");
            Assert.IsTrue(KB.Query + "John" + "loves" + "Kelly");
        }

        [TestMethod]
        public void RetractWithChildrenTest()
        {
            KB.Retract(KB.Query + "John" + "loves");
            Assert.IsFalse(KB.Query + "John" + "loves" + "Mary");
            Assert.IsFalse(KB.Query + "John" + "loves" + "Kelly");
            Assert.IsFalse(KB.Query + "John" + "loves");
            Assert.IsTrue(KB.Query + "John");
        }

        [TestMethod]
        public void RetractWildcardTest()
        {
            var x = new Variable("x");
            KB.Retract(KB.Query + "John" + "loves" + x);
            Assert.IsFalse(KB.Query + "John" + "loves" + "Mary");
            Assert.IsTrue(KB.Query + "John" + "loves" + "Kelly");
            Assert.IsTrue(KB.Query + "John" + "loves");
            Assert.IsTrue(KB.Query + "John");
        }

        [TestMethod]
        public void RetractAllEndingWildcardTest()
        {
            var x = new Variable("x");
            KB.RetractAll(KB.Query + "John" + "loves" + x);
            Assert.IsFalse(KB.Query + "John" + "loves" + "Mary");
            Assert.IsFalse(KB.Query + "John" + "loves" + "Kelly");
            Assert.IsTrue(KB.Query + "John" + "loves");
            Assert.IsTrue(KB.Query + "John");
        }

        [TestMethod]
        public void RetractAllStartingWildcardTest()
        {
            var x = new Variable("x");
            KB.RetractAll(KB.Query + x + "loves" + "Mary");
            Assert.IsFalse(KB.Query + "John" + "loves" + "Mary");
            Assert.IsFalse(KB.Query + "Kelly" + "loves" + "Mary");
            Assert.IsTrue(KB.Query + "John" + "loves" + "Kelly");
            Assert.IsTrue(KB.Query + "John" + "loves");
            Assert.IsTrue(KB.Query + "John");
        }

        [TestMethod]
        public void RetractAllPatternTest()
        {
            var x = new Variable("x");
            KB.RetractAll(KB.Query + x + "loves" + x);
            Assert.IsTrue(KB.Query + "John" + "loves" + "Mary");
            Assert.IsTrue(KB.Query + "Kelly" + "loves" + "Mary");
            Assert.IsTrue(KB.Query + "John" + "loves" + "Kelly");
            Assert.IsTrue(KB.Query + "John" + "loves");
            Assert.IsTrue(KB.Query + "John");
            Assert.IsFalse(KB.Query+"Newt" + "loves" + "Newt");
        }
    }
}
