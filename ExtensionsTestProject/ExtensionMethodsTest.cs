using Charrmander.Util;
using FluentAssertions;
using System;
using Xunit;

namespace ExtensionsTestProject
{
    public class ExtensionMethodsTest
    {
        /// <summary>
        /// Any version is newer than <c>null</c>.
        /// </summary>
        [Fact]
        public void AnyIsNewerThanNull()
        {
            Version v = new("0.0.0.0");
            Version other = null;
            bool expected = true;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);

            actual.Should().Be(expected);
        }

        /// <summary>
        /// 1.0.0.0 is not newer than 1.0.0.0.
        /// </summary>
        [Fact]
        public void NoneIsNewerThanEqual()
        {
            var v = new Version("1.0.0.0");
            var other = new Version("1.0.0.0");
            bool expected = false;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);

            actual.Should().Be(expected);
        }

        /// <summary>
        /// The same instance is not newer than itself.
        /// </summary>
        [Fact]
        public void InstanceNotNewerThanSelf()
        {
            var v = new Version("1.0.0.0");
            bool expected = false;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, v);

            actual.Should().Be(expected);
        }

        /// <summary>
        /// 0.0.0.1 is newer than 0.0.0.0.
        ///</summary>
        [Fact]
        public void RevisionOneIsNewerThanRevisionZero()
        {
            var v = new Version("0.0.0.1");
            var other = new Version("0.0.0.0");
            bool expected = true;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);

            actual.Should().Be(expected);
        }

        /// <summary>
        /// 0.0.0.0 is note newer than 0.0.0.1.
        ///</summary>
        [Fact]
        public void RevisionZeroIsNotNewerThanRevisionOne()
        {
            var v = new Version("0.0.0.0");
            var other = new Version("0.0.0.1");
            bool expected = false;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);

            actual.Should().Be(expected);
        }

        /// <summary>
        /// 0.0.1.0 is newer than 0.0.0.9.
        ///</summary>
        [Fact]
        public void BuildOneIsNewerThanRevisionNine()
        {
            var v = new Version("0.0.1.0");
            var other = new Version("0.0.0.9");
            bool expected = true;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);

            actual.Should().Be(expected);
        }

        /// <summary>
        /// 0.0.1.0 is newer than 0.0.0.
        ///</summary>
        [Fact]
        public void BuildOneIsNewerThanNoRevision()
        {
            var v = new Version("0.0.1.0");
            var other = new Version("0.0.0");
            bool expected = true;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);

            actual.Should().Be(expected);
        }

        /// <summary>
        /// 1.0.0.0 is newer than 0.9.
        ///</summary>
        [Fact]
        public void MajorOneIsNewerThanMinorNine()
        {
            var v = new Version("1.0.0.0");
            var other = new Version("0.9");
            bool expected = true;
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);

            actual.Should().Be(expected);
        }
    }
}
