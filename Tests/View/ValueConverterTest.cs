using FluentAssertions;
using System.Globalization;
using System.Windows.Data;
using Xunit;
using static Charrmander.Model.Area;

namespace Charrmander.View
{
    public class ValueConverterTest
    {
        [Theory]
        [ClassData(typeof(CompletionStateConverterTestData))]
        [ClassData(typeof(GameIconConverterTestData))]
        [ClassData(typeof(IntFromBlankTextConverterTestData))]
        [ClassData(typeof(KeyRewardBoolConverterTestData))]
        [ClassData(typeof(NamelessCharacterConverterTestData))]
        public void converts_value(
            IValueConverter converter,
            object value,
            object parameter,
            object expected)
        {
            var actual = converter.Convert(
                value,
                expected.GetType(),
                parameter,
                CultureInfo.InvariantCulture);

            actual.Should().Be(expected);
        }
    }

    internal abstract class ConverterSignature<ConcreteValueConverter>
        : TheoryData<ConcreteValueConverter, object, object, object>
        where ConcreteValueConverter : IValueConverter, new()
    {
        protected readonly ConcreteValueConverter Converter = new();
    }

    internal class CompletionStateConverterTestData
        : ConverterSignature<CompletionStateConverter>
    {
        public CompletionStateConverterTestData()
        {
            Add(Converter, null, null, "");
            Add(Converter, "", null, "");
            Add(Converter, "Unvisited", null, "");
            Add(Converter, "Visited", null, "\u2606");
            Add(Converter, "Completed", null, "\u2605");
            Add(Converter, "foo", null, "");
            Add(Converter, CompletionState.Completed, null, "\u2605");
            Add(Converter, CompletionState.Unvisited, null, "");
            Add(Converter, CompletionState.Visited, null, "\u2606");
        }
    }

    internal class GameIconConverterTestData
        : ConverterSignature<GameIconConverter>
    {
        public GameIconConverterTestData()
        {
            Add(Converter, null, null, "/Icons/Game//.png");
            Add(Converter, "foo", null, "/Icons/Game//foo.png");
            Add(Converter, null, "bar", "/Icons/Game/bar/.png");
            Add(Converter, "foo", "", "/Icons/Game//foo.png");
            Add(Converter, "", "bar", "/Icons/Game/bar/.png");
            Add(Converter, "Jeweler", "Crafts", "/Icons/Game/Crafts/Jeweler.png");
        }
    }

    internal class IntFromBlankTextConverterTestData
        : ConverterSignature<IntFromBlankTextConverter>
    {
        public IntFromBlankTextConverterTestData()
        {
            Add(Converter, null, null, 0);
            Add(Converter, "", null, 0);
            Add(Converter, "foo", null, 0);
            Add(Converter, "0", null, 0);
            Add(Converter, "1", null, 1);
            Add(Converter, "-1", null, -1);
        }
    }

    internal class KeyRewardBoolConverterTestData
        : ConverterSignature<KeyRewardBoolConverter>
    {
        public KeyRewardBoolConverterTestData()
        {
            Add(Converter, null, null, "");
            Add(Converter, false, null, "");
            Add(Converter, true, null, "\U0001F5DD");
        }
    }

    internal class NamelessCharacterConverterTestData
        : ConverterSignature<NamelessCharacterConverter>
    {
        public NamelessCharacterConverterTestData()
        {
            Add(Converter, null, null, "[Unnamed]");
            Add(Converter, "", null, "[Unnamed]");
            Add(Converter, "foo", null, "foo");
        }
    }
}
