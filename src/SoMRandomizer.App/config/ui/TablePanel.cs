#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace SoMRandomizer.config.ui
{
    /// <summary>
    /// 3-column table used for selection of rando property values (name, description, control).
    /// Avalonia port of the WinForms TablePanel.
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    public class TablePanel : ScrollViewer
    {
        private StackPanel content = new StackPanel();
        private int col1Width = 150;
        public int col3Width = 200;
        private int rowCount = 0;

        /// <summary>Number of header/separator rows plus data rows added so far.</summary>
        public int RowCount
        {
            get { return rowCount; }
        }

        public TablePanel()
        {
            Content = content;
            content.Orientation = Orientation.Vertical;
        }

        public void addHeader(string headerLine, int fontSizeChange)
        {
            TextBlock header = new TextBlock();
            header.Text = headerLine;
            header.FontSize = header.FontSize + fontSizeChange;
            header.Foreground = Brushes.Blue;
            header.TextWrapping = TextWrapping.Wrap;
            header.Margin = new Thickness(0, 8, 0, 4);
            content.Children.Add(header);
            rowCount++;
        }

        public void addRow(string name, string description, Control control)
        {
            Grid row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(col1Width, GridUnitType.Pixel)));
            row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
            row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(col3Width, GridUnitType.Pixel)));
            row.Margin = new Thickness(0, 4, 0, 4);

            TextBlock nameLabel = new TextBlock();
            nameLabel.Text = name;
            nameLabel.FontWeight = FontWeight.Bold;
            nameLabel.TextWrapping = TextWrapping.Wrap;
            nameLabel.Margin = new Thickness(0, 0, 8, 0);
            nameLabel.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(nameLabel, 0);

            TextBlock descLabel = new TextBlock();
            descLabel.Text = description;
            descLabel.TextWrapping = TextWrapping.Wrap;
            descLabel.Margin = new Thickness(0, 0, 8, 0);
            descLabel.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(descLabel, 1);

            control.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(control, 2);

            row.Children.Add(nameLabel);
            row.Children.Add(descLabel);
            row.Children.Add(control);
            content.Children.Add(row);
            rowCount++;
        }
    }
}
