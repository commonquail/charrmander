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

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (this.DataContext is ViewModelMain viewModel)
                {
                    viewModel.Open(files[0]);
                }
            }
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

        private void StoryChapterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            (DataContext as ViewModelMain).UpdateStoryChapterCompletion();
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

    /// <summary>
    /// Converts a check box checked state to a key if <c>True</c>, empty string otherwise.
    /// </summary>
    public class KeyRewardBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? "\U0001F5DD" : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
