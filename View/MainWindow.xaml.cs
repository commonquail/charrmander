using Charrmander.ViewModel;
using System.Windows;
using System.Windows.Controls;

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
            (DataContext as ViewModelMain)?.UpdateBiographyOptions();
        }

        private void StoryChapterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            (DataContext as ViewModelMain)?.UpdateStoryChapterCompletion();
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
            (DataContext as ViewModelMain)?.TrimSelectedCharacterName();
        }

        private void EliteSpecializationCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            (DataContext as ViewModelMain)?.ComputeAvailableSkillPoints();
        }
    }
}
