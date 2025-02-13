using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Charrmander.View
{
    /// <summary>
    /// Interaction logic for IntegerUpDown.xaml
    /// </summary>
    public partial class IntegerUpDown : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(int),
                typeof(IntegerUpDown),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnValueChangedCallback)));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(int),
                typeof(IntegerUpDown),
                new UIPropertyMetadata(int.MinValue));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(int),
                typeof(IntegerUpDown),
                new UIPropertyMetadata(int.MaxValue));

        public IntegerUpDown()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Dispatcher.BeginInvoke(
                new Action(
                    () => OnValueChangedCallback(this, new DependencyPropertyChangedEventArgs())));
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        private static void OnValueChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (IntegerUpDown)o;
            var v = ctrl.Value;
            ctrl.rpDecr.IsEnabled = v > ctrl.Minimum;
            ctrl.rpIncr.IsEnabled = v < ctrl.Maximum;
        }

        private void Incr_Click(object sender, RoutedEventArgs e)
        {
            Increment();
        }

        private void Decr_Click(object sender, RoutedEventArgs e)
        {
            Decrement();
        }

        private void Increment()
        {
            Value = Math.Min(Value + 1, Maximum);
        }

        private void Decrement()
        {
            Value = Math.Max(Value - 1, Minimum);
        }

        private void Val_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbVal.Dispatcher.BeginInvoke(new Action(() => tbVal.SelectAll()));
        }

        private void Val_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    Decrement();
                    e.Handled = true;
                    break;
                case Key.Up:
                    Increment();
                    e.Handled = true;
                    break;
            }
        }

        private void Val_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!int.TryParse(tbVal.Text, out _))
            {
                tbVal.Text = Value.ToString();
            }
        }
    }
}
