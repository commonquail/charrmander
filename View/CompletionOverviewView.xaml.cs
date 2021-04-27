using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Charrmander.View
{
    /// <summary>
    /// Interaction logic for CompletionOverviewView.xaml
    /// </summary>
    public partial class CompletionOverviewView : Window
    {
        readonly Style style;

        public CompletionOverviewView(DataTable table)
        {
            InitializeComponent();

            style = new Style();
            style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
            this.DataContext = table;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (sender is DataGrid datagrid)
            {
                // check if the current column is the target
                if (datagrid.Columns.Count > 0)
                {
                    // assuming it's a Text-type column
                    if (e.Column is DataGridBoundColumn column)
                    {
                        if (column.Binding is Binding binding)
                        {
                            // add a converter to the binding
                            binding.Converter = new CompletionStateConverter();
                        }

                        column.CellStyle = style;
                    }
                }
            }
        }
    }
}
