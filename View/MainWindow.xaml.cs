using Charrmander.Model;
using Charrmander.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        /// <summary>
        /// When shifting focus outside of the selected character name, trim the name
        /// because character names cannot have leading or trailing spaces.
        /// Do not trim the name when the property is set (which otherwise seems appropriate)
        /// because a character name can have multiple components separated by a space,
        /// so it must be possible to type spaces.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CharacterNameTextBox_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            (DataContext as ViewModelMain).TrimSelectedCharacterName();
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
            switch (value)
            {
                case Area.CompletionState v:
                    state = v;
                    break;
                case string v:
                    if (Enum.TryParse(v, out Area.CompletionState s))
                    {
                        state = s;
                    }

                    break;
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

    /// <summary>
    /// Converts a blank or null string to <c>0</c>.
    /// </summary>
    public class IntFromBlankTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value.ToString();
            return string.IsNullOrWhiteSpace(str) ? 0 : int.Parse(str);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
