using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Charrmander.View
{
    public class WorldCompletionAreaStyleSelectorTest
    {
        [Fact]
        public void selects_null_for_unsupported_item_type()
        {
            var styleSelector = new WorldCompletionAreaStyleSelector(new HashSet<string>());

            var resultStyle = styleSelector.SelectStyle(new(), new());
            resultStyle.Should().BeNull();
        }

        [Fact]
        public void selects_null_for_no_names()
        {
            var dataRowView = NewDataRowViewWithColumnValue("foo");

            var noNames = new HashSet<string>();
            var styleSelector = new WorldCompletionAreaStyleSelector(noNames);

            var resultStyle = styleSelector.SelectStyle(dataRowView, new());
            resultStyle.Should().BeNull();
        }

        [Fact]
        public void selects_null_for_unrecognized_name()
        {
            var dataRowView = NewDataRowViewWithColumnValue("foo");

            var unrecognizedNames = new HashSet<string>() { new Guid().ToString() };
            var styleSelector = new WorldCompletionAreaStyleSelector(unrecognizedNames);

            var resultStyle = styleSelector.SelectStyle(dataRowView, new());
            resultStyle.Should().BeNull();
        }

        [Fact]
        public void selects_world_completion_style_for_recognized_name()
        {
            string someRecognizedName = new Guid().ToString();
            var dataRowView = NewDataRowViewWithColumnValue(someRecognizedName);

            var recognizedNames = new HashSet<string>() { someRecognizedName };
            var styleSelector = new WorldCompletionAreaStyleSelector(recognizedNames);

            var resultStyle = styleSelector.SelectStyle(dataRowView, new());
            resultStyle.Should().NotBeNull()
                .And.Be(WorldCompletionAreaStyleSelector.WorldCompletionAreaStyle);
        }

        private static DataRowView NewDataRowViewWithColumnValue(string columnValue)
        {
            using var dataTable = new DataTable();
            dataTable.Columns.Add();
            var dataRow = dataTable.NewRow();
            dataRow[0] = columnValue;
            dataTable.Rows.Add(dataRow);
            return dataTable.AsDataView()[0];
        }
    }
}
