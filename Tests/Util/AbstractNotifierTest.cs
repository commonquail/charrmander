using FluentAssertions;
using System;
using Xunit;

namespace Charrmander.Util
{
    public class AbstractNotifierTest
    {
        [Fact]
        public void raise_property_changed_for_nonexistent_property_blows_up()
        {
            var someNonexistentProperty = new Guid().ToString();
            var notifier = new TestableNotifier();

            Action act = () => notifier.RaisePropertyChanged(someNonexistentProperty);

            act.Should().Throw<Exception>()
                .WithMessage($"*Invalid*{someNonexistentProperty}*");
        }

        [Fact]
        public void can_raise_property_changed_event()
        {
            var notifier = new TestableNotifier();
            using var monitor = notifier.Monitor();

            notifier.Prop = new Guid();

            monitor.Should().RaisePropertyChangeFor(x => x.Prop);
        }

        private class TestableNotifier : AbstractNotifier
        {
            private Guid prop;

            public Guid Prop
            {
                get => prop;
                set
                {
                    prop = value;
                    RaisePropertyChanged(nameof(Prop));
                }
            }

            new public void RaisePropertyChanged(string property) =>
                base.RaisePropertyChanged(property);
        }
    }
}
