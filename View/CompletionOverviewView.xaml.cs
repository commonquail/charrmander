using System.Collections.Generic;
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
        private readonly Style _completionStateStyle = new();

        private readonly CompletionStateConverter _completionStateConverter = new();

        public CompletionOverviewView(DataTable table, IReadOnlySet<string> names)
        {
            InitializeComponent();

            _completionStateStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
            DataContext = this;
            Table = table;

            StyleSelector = new WorldCompletionAreaStyleSelector(names);
        }

        public DataTable Table { get; }

        public WorldCompletionAreaStyleSelector StyleSelector { get; }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (sender is DataGrid datagrid)
            {
                // The first column is not a completion state column. All
                // others are.
                if (datagrid.Columns.Count == 0)
                {
                    return;
                }

                if (e.Column is DataGridBoundColumn completionStateColumn)
                {
                    if (completionStateColumn.Binding is Binding binding)
                    {
                        binding.Converter = _completionStateConverter;
                    }

                    completionStateColumn.CellStyle = _completionStateStyle;
                }
            }
        }
    }
}
