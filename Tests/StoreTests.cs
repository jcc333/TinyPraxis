using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyPraxis;

namespace Tests
{
    [TestClass]
    public class StoreTests
    {
        [TestInitialize]
        public void ClearKB()
        {
            KB.Clear();
        }

        [TestMethod]
        public void KBENameTest()
        {
            Assert.AreEqual("John+mother-Betty", (KB.Store + "John" + "mother" - "Betty").ToString());
        }

        [TestMethod]
        public void StoreNonExclusiveTest()
        {
            Assert.IsFalse(KB.Query + "John" + "loves" + "Mary");
            KB.Assert(KB.Store + "John" + "loves" + "Mary");
            KB.Assert(KB.Store + "John" + "loves" + "Kelly");
            KB.Assert(KB.Store + "John" + "loves" + "Mary");
            KB.Assert(KB.Store + "Kelly" + "loves" + "Mary");
            Assert.IsTrue(KB.Query + "John" + "loves" + "Mary");
            Assert.IsTrue(KB.Query + "John" + "loves" + "Kelly");
            Assert.IsTrue(KB.Query + "Kelly" + "loves" + "Mary");
            Assert.IsFalse(KB.Query + "Kelly" + "loves" + "John");
            Assert.IsTrue(KB.Query + "John" + "loves");
            Assert.IsTrue(KB.Query + "John");
        }

        [TestMethod]
        public void StoreExclusiveTest()
        {
            Assert.IsFalse(KB.Query + "John" + "loves" - "Mary");
            KB.Assert(KB.Store + "John" + "loves" - "Mary");
            Assert.IsTrue(KB.Query + "John" + "loves" - "Mary");
            Assert.IsFalse(KB.Query + "John" + "loves" - "Kelly");
            KB.Assert(KB.Store + "John" + "loves" - "Kelly");
            Assert.IsFalse(KB.Query + "John" + "loves" - "Mary");
            Assert.IsTrue(KB.Query + "John" + "loves" - "Kelly");
            Assert.IsTrue(KB.Query + "John" + "loves");
            Assert.IsTrue(KB.Query + "John");
        }

        [TestMethod, ExpectedException(typeof(KBExclusionException))]
        public void StoreExclusiveToNonExclusiveTest()
        {
            KB.Assert(KB.Store + "John" + "loves" - "Mary");
            KB.Assert(KB.Store + "John" + "loves" + "Kelly");
        }
        [TestMethod, ExpectedException(typeof(KBExclusionException))]
        public void StoreNonExclusiveToExclusiveTest()
        {
            KB.Assert(KB.Store + "John" + "loves" + "Mary");
            KB.Assert(KB.Store + "John" + "loves" - "Kelly");
        }

        [TestMethod]
        public void StoreVariableTest()
        {
            KB.Assert(KB.Store + "John" + "loves" + "Mary");
            KB.Assert(KB.Store + "John" + "loves" + "Kelly");
            KB.Assert(KB.Store + "John" + "loves" + "Mary");
            KB.Assert(KB.Store + "Kelly" + "loves" + "Mary");
            
            // Update: Mary loves everyone who loves her.
            var v = new Variable("v");
            (KB.Query + v + "loves" + "Mary").DoAll(
                () => KB.Assert(KB.Store+"Mary"+"loves"+v)
                );

            var v2 = new Variable("v2");
            CollectionAssert.AreEqual(new object[] { "John", "Kelly"},
                (KB.Query + "Mary" + "loves" + v2).FindAll(v2));
        }
    }
}
