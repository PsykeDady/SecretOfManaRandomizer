#nullable disable
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using SoMRandomizer.config.ui;
using SoMRandomizer.util.rng;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoMRandomizer.forms
{
    /// <summary>
    /// Form used to select presets for open world mode.
    /// </summary>
    ///
    /// <remarks>Author: Moppleton</remarks>
    public class PresetsForm : Window
    {
        private TextBox seedTextBox;
        private Preset resetPreset;
        private TextBlock label1;
        private Button button1;
        private TablePanel tablePanel1;

        private Dictionary<Button, Preset> callbacks = new Dictionary<Button, Preset>();

        public PresetsForm(string title, List<Preset> presets, TextBox seedTextBox, Preset resetPreset)
        {
            this.seedTextBox = seedTextBox;
            this.resetPreset = resetPreset;
            BuildUi();
            label1.Text = title;

            Closing += MyForm_FormClosing;

            foreach (Preset preset in presets)
            {
                addPreset(preset);
            }
        }

        private void BuildUi()
        {
            Title = "Presets";
            Width = 720;
            Height = 520;
            MinWidth = 500;
            MinHeight = 200;
            CanResize = true;

            DockPanel dock = new DockPanel();

            StackPanel topBar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Margin = new Avalonia.Thickness(12, 8),
            };
            label1 = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontWeight = Avalonia.Media.FontWeight.Bold };
            button1 = new Button { Content = "Reset to defaults" };
            button1.Click += button1_Click;
            ToolTip.SetTip(button1, "Reset to the default settings shown when you load the randomizer.");
            topBar.Children.Add(label1);
            topBar.Children.Add(button1);

            DockPanel.SetDock(topBar, Avalonia.Controls.Dock.Top);
            dock.Children.Add(topBar);

            tablePanel1 = new TablePanel { Margin = new Avalonia.Thickness(12, 0, 12, 12) };
            dock.Children.Add(tablePanel1);

            Content = dock;
        }

        private void MyForm_FormClosing(object sender, WindowClosingEventArgs e)
        {
            Hide();
            e.Cancel = true; // this cancels the close event.
        }

        private void addPreset(Preset preset)
        {
            if (preset.separatorOnly)
            {
                tablePanel1.addHeader(preset.name, preset.fontSizeChange);
            }
            else
            {
                Button button = new Button();
                button.Content = "Select";
                button.Click += Button_Click;
                ToolTip.SetTip(button, preset.tooltip);
                callbacks[button] = preset;
                tablePanel1.addRow(preset.name, preset.description, button);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            callbacks[(Button)sender].select(this, seedTextBox.Text);
        }

        public class Preset
        {
            public Dictionary<TabControl, string[]> tabNameSelections = new Dictionary<TabControl, string[]>();
            public Dictionary<ComboBox, string[]> comboBoxSelections = new Dictionary<ComboBox, string[]>();
            public Dictionary<CheckBox, bool[]> checkBoxSelections = new Dictionary<CheckBox, bool[]>();
            public Dictionary<NumericUpDown, double[]> numericUpDownSelections = new Dictionary<NumericUpDown, double[]>();

            public bool hideUi = false;
            public MainForm mainForm;
            public bool separatorOnly = false;
            public int fontSizeChange = 0;

            public string name;
            public string description;
            public string tooltip;

            public async void select(PresetsForm presetsForm, string seedSelection)
            {
                Random r = new DotNet110Random(HashcodeUtil.GetDeterministicHashCode(seedSelection));
                bool confirmHide = false;
                if (hideUi)
                {
                    bool dialogResult = await DialogUtil.ShowYesNo(presetsForm, "Confirm",
                        "Would you like to obscure the chosen settings?  If you pick \"Yes,\" the main window will be hidden and the current selections will " +
                        "be generated into a ROM immediately.  When the \"Done\" popup comes up, you'll know it's ready.  " +
                        "If you pick no, the settings will be selected on the main window, and you'll be able to see what it picked for you.");
                    confirmHide = dialogResult;

                    if (confirmHide)
                    {
                        presetsForm.Hide();
                        if (mainForm != null)
                        {
                            mainForm.Hide();
                        }
                    }
                }

                foreach (TabControl t in tabNameSelections.Keys)
                {
                    string target = tabNameSelections[t][r.Next() % tabNameSelections[t].Length];
                    foreach (object item in t.Items)
                    {
                        TabItem tabItem = item as TabItem;
                        if (tabItem != null && tabItem.Header != null && tabItem.Header.ToString() == target)
                        {
                            t.SelectedItem = tabItem;
                        }
                    }
                }

                foreach (ComboBox t in comboBoxSelections.Keys)
                {
                    t.SelectedItem = comboBoxSelections[t][r.Next() % comboBoxSelections[t].Length];
                }

                foreach (CheckBox t in checkBoxSelections.Keys)
                {
                    t.IsChecked = checkBoxSelections[t][r.Next() % checkBoxSelections[t].Length];
                }

                foreach (NumericUpDown t in numericUpDownSelections.Keys)
                {
                    t.Value = (decimal)numericUpDownSelections[t][r.Next() % numericUpDownSelections[t].Length];
                }

                if (hideUi && confirmHide)
                {
                    mainForm.generateWithCurrentSettings(true);
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            resetPreset.select(this, seedTextBox.Text);
        }
    }
}
