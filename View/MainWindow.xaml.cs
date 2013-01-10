using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Charrmander.ViewModel;
using Charrmander.Model;

namespace Charrmander.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void txtGotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null)
            {
                tb.SelectAll();
            }
        }

        private void txtPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && !tb.IsKeyboardFocusWithin)
            {
                if (e.OriginalSource.GetType().Name == "TextBoxView")
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var viewModel = this.DataContext as ViewModelMain;
                if (viewModel != null)
                {
                    viewModel.Open(files[0]);
                }
            }
        }

        /// <summary>
        /// Restricts the calling slider to discrete integer values.
        /// <seealso cref="Slider_ValueChanged"/>
        /// </summary>
        /// <param name="sender">The Slider control.</param>
        /// <param name="e"></param>
        private void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as Slider).ValueChanged += Slider_ValueChanged;
        }

        /// <summary>
        /// Restricts the calling slider to discrete integer values.
        /// <seealso cref="Slider_ValueChanged"/>
        /// </summary>
        /// <param name="sender">The Slider control.</param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (sender as Slider).Value = Math.Round(e.NewValue);
        }

        /// <summary>
        /// When race or profession is changed, update the biography choice
        /// options.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as ViewModelMain).UpdateBiographyOptions();
        }
    }

    /// <summary>
    /// Converts the <see cref="Character.Name"/> property if it is unset.
    /// </summary>
    public class NamelessCharacterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace((value as string)) ? "[Unnamed]" : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts the <see cref="Character.Profession"/> property to the corresponding icon.
    /// </summary>
    public class GameIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "/Icons/Game/" + parameter + "/" + value + ".png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts the <see cref="Character.Profession"/> property to the corresponding icon.
    /// </summary>
    public class CompletionStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string res = string.Empty;
            Area.CompletionState state = Area.CompletionState.Unvisited;
            if (value is Area.CompletionState)
            {
                state = (Area.CompletionState)value;
            }
            else if (value is string)
            {
                Enum.TryParse<Area.CompletionState>(value as string, out state);
            }

            switch (state)
            {
                case Area.CompletionState.Unvisited:
                    break;
                case Area.CompletionState.Visited:
                    res = "\u2606"; // White star.
                    break;
                case Area.CompletionState.Completed:
                    res = "\u2605"; // Black star.
                    break;
                default:
                    break;
            }

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
