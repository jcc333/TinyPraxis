using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyPraxis;

namespace Tests
{
    /// <summary>
    /// Summary description for QueryTests
    /// </summary>
    [TestClass]
    public class QueryTests
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
        public void QueryNameTest()
        {
            var x = new Variable("x");
            Assert.AreEqual("John+loves+x", (KB.Query + "John" + "loves" + x).ToString());
        }

        [TestMethod]
        public void SimpleQueryTest()
        {
            var v = new Variable("v");
            Assert.AreEqual("John", (KB.Query+v+"loves"+"Kelly").Find(v));
            CollectionAssert.AreEqual(new object[] {"John"}, (KB.Query + v + "loves" + "Kelly").FindAll(v));
        }

        [TestMethod]
        public void MultiSolutionQueryTest()
        {
            var v = new Variable("v");
            CollectionAssert.AreEqual(new object[] { "Mary", "Kelly" }, (KB.Query + "John" + "loves" + v).FindAll(v));
            v = new Variable("v");
            CollectionAssert.AreEqual(new object[] { "John", "Kelly" }, (KB.Query + v + "loves" + "Mary").FindAll(v));
        }

        [TestMethod]
        public void VariableUsedTwiceQueryTest()
        {
            var v = new Variable("v");
            CollectionAssert.AreEqual(new object[] { "Newt" }, (KB.Query + v + "loves" + v).FindAll(v));
        }

        [TestMethod]
        public void ConjunctiveQueryTest()
        {
            var x = new Variable("x");
            var y = new Variable("y");
            var z = new Variable("z");
            CollectionAssert.AreEqual(
                new object[] {"Mary"},
                (KB.Query + x + "loves" + y & KB.Query+z+"loves"+y & x != z).FindAllUnique(y));
        }

    }
}
