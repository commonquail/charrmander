﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

namespace Charrmander.View
{
    /// <summary>
    /// Interaction logic for CompletionOverviewView.xaml
    /// </summary>
    public partial class CompletionOverviewView : Window
    {
        Style style;

        public CompletionOverviewView(DataTable table)
        {
            InitializeComponent();

            style = new Style();
            style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
            this.DataContext = table;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var datagrid = sender as DataGrid;
            if (datagrid != null)
            {
                // check if the current column is the target
                if (datagrid.Columns.Count > 0)
                {
                    // assuming it's a Text-type column
                    var column = (e.Column as DataGridTextColumn);
                    if (column != null)
                    {
                        var binding = column.Binding as Binding;
                        if (binding != null)
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
