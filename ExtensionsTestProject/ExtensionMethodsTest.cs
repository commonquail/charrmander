using Charrmander.Util;
using FluentAssertions;
using System;
using Xunit;

namespace ExtensionsTestProject
{
    public class ExtensionMethodsTest
    {
        [Theory]
        [InlineData("0.0.0.0", null, true, @"""any is newer than null""")]
        [InlineData("1.0.0.0", "1.0.0.0", false, @"""self is not newer than self""")]
        [InlineData("0.0.0.1", "0.0.0.0", true, @"""revision one is newer than revision zero""")]
        [InlineData("0.0.0.0", "0.0.0.1", false, @"""revision zero is not newer than revision one""")]
        [InlineData("0.0.1.0", "0.0.0.9", true, @"""build one is newer than revision 9""")]
        [InlineData("0.0.0.10", "0.0.0.1", true, @"""revision 10 is newer than revision 1""")]
        [InlineData("0.0.1.0", "0.0.0", true, @"""build one is newer than no revision""")]
        [InlineData("1.0.0.0", "0.9", true, @"""major one is newer than minor nine""")]
        public void lhs_is_newer_than_rhs(string lhs, string rhs, bool expected, string reason)
        {
            var v = new Version(lhs);
            var other = rhs == null ? null : new Version(rhs);
            bool actual;
            actual = ExtensionMethods.IsNewerThan(v, other);

            actual.Should().Be(expected, reason);
        }
    }
}
