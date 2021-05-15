using FluentAssertions;
using Xunit;

namespace Charrmander.Model
{
    public class AreaTest
    {
        [Fact]
        public void levelrange_uses_minmax_level()
        {
            var someArea = new Area("foo")
            {
                MinLevel = "37",
                MaxLevel = "42",
            };
            var levelRangeSubstring = $"{someArea.MinLevel}-{someArea.MaxLevel}";
            someArea.LevelRange.Should().Contain(levelRangeSubstring);
        }
    }
}
