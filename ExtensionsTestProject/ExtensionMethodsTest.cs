using Charrmander.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ExtensionsTestProject
{


    /// <summary>
    ///This is a test class for ExtensionMethodsTest and is intended
    ///to contain all ExtensionMethodsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ExtensionMethodsTest
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        /// Any version is newer than <c>null</c>.
        /// </summary>
        [TestMethod()]
        public void AnyIsNewerThanNull()
        {
            Version v = new("0.0.0.0");
            Version other = null;
            bool expected = true;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// 1.0.0.0 is not newer than 1.0.0.0.
        /// </summary>
        [TestMethod()]
        public void NoneIsNewerThanEqual()
        {
            var v = new Version("1.0.0.0");
            var other = new Version("1.0.0.0");
            bool expected = false;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// The same instance is not newer than itself.
        /// </summary>
        [TestMethod()]
        public void InstanceNotNewerThanSelf()
        {
            var v = new Version("1.0.0.0");
            bool expected = false;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, v);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// 0.0.0.1 is newer than 0.0.0.0.
        ///</summary>
        [TestMethod()]
        public void RevisionOneIsNewerThanRevisionZero()
        {
            var v = new Version("0.0.0.1");
            var other = new Version("0.0.0.0");
            bool expected = true;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// 0.0.0.0 is note newer than 0.0.0.1.
        ///</summary>
        [TestMethod()]
        public void RevisionZeroIsNotNewerThanRevisionOne()
        {
            var v = new Version("0.0.0.0");
            var other = new Version("0.0.0.1");
            bool expected = false;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// 0.0.1.0 is newer than 0.0.0.9.
        ///</summary>
        [TestMethod()]
        public void BuildOneIsNewerThanRevisionNine()
        {
            var v = new Version("0.0.1.0");
            var other = new Version("0.0.0.9");
            bool expected = true;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// 0.0.1.0 is newer than 0.0.0.
        ///</summary>
        [TestMethod()]
        public void BuildOneIsNewerThanNoRevision()
        {
            var v = new Version("0.0.1.0");
            var other = new Version("0.0.0");
            bool expected = true;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// 1.0.0.0 is newer than 0.9.
        ///</summary>
        [TestMethod()]
        public void MajorOneIsNewerThanMinorNine()
        {
            var v = new Version("1.0.0.0");
            var other = new Version("0.9");
            bool expected = true;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);
            Assert.AreEqual(expected, actual);
        }
    }
}
