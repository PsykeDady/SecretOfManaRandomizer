#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace SoMRandomizer.config.ui
{
    /// <summary>
    /// Form allowing selection of an options category, which in turn opens another form with the individual settings.
    /// Avalonia port of the WinForms OptionsCategoryForm.
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    public class OptionsCategoryForm : Window
    {
        private OptionsFormManager propManager;
        private bool shownYet = false;
        private TablePanel tablePanel1;
        private TextBlock label1;

        public OptionsCategoryForm(OptionsFormManager propManager, string title, string labelTitle)
        {
            this.propManager = propManager;

            BuildLayout();

            Title = title;
            label1.Text = labelTitle;

            Closing += OptionsCategoryForm_Closing;
            Opened += OptionsCategoryForm_Opened;
        }

        private void BuildLayout()
        {
            label1 = new TextBlock();
            label1.FontSize = 16;
            label1.Margin = new Thickness(20, 12, 20, 0);

            tablePanel1 = new TablePanel();

            Border panel1 = new Border();
            panel1.BorderThickness = new Thickness(1);
            panel1.BorderBrush = Brushes.Gray;
            panel1.Margin = new Thickness(10, 10, 10, 10);
            panel1.Child = tablePanel1;

            DockPanel root = new DockPanel();
            DockPanel.SetDock(label1, Dock.Top);
            root.Children.Add(label1);
            root.Children.Add(panel1);

            Content = root;
            Width = 690;
            Height = 640;
        }

        private void OptionsCategoryForm_Opened(object sender, EventArgs e)
        {
            if (!shownYet)
            {
                int numProperties = tablePanel1.RowCount;
                if (numProperties < 5)
                {
                    numProperties = 5;
                }
                if (numProperties > 10)
                {
                    numProperties = 10;
                }
                Height = 50 + numProperties * 50;
                shownYet = true;
            }
        }

        private void OptionsCategoryForm_Closing(object sender, WindowClosingEventArgs e)
        {
            Hide();
            e.Cancel = true; // this cancels the close event.
        }

        private Dictionary<Button, string> callbacks = new Dictionary<Button, string>();

        public void addCategory(string name, string description, string formName)
        {
            Button button = new Button();
            button.Content = "Options";
            button.Click += Button_Click;
            callbacks[button] = formName;
            tablePanel1.addRow(name, description, button);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            propManager.showForm(callbacks[(Button)sender]);
        }
    }
}
