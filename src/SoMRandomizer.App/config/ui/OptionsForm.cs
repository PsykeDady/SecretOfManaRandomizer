#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SoMRandomizer.config.settings;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SoMRandomizer.config.ui
{
    /// <summary>
    /// Table of editable properties with descriptions. Avalonia port of the WinForms OptionsForm.
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    public class OptionsForm : Window
    {
        private Dictionary<string, string> propertyValues;
        private Dictionary<string, Dictionary<string, Window>> propertyForms = new Dictionary<string, Dictionary<string, Window>>();
        private Dictionary<Control, string> propertyMapping = new Dictionary<Control, string>();
        private Dictionary<string, string[]> propertyValueMapping = new Dictionary<string, string[]>();
        private UiOptionsManager optionsManager;
        private List<string> supportedProperties = new List<string>();
        private bool shownYet = false;
        private TablePanel tablePanel1;
        private TextBlock label1;

        public OptionsForm(string title, Dictionary<string, string> propertyValues, UiOptionsManager optionsManager)
        {
            this.propertyValues = propertyValues;
            this.optionsManager = optionsManager;

            BuildLayout();

            Title = title;
            label1.Text = title;

            Closing += OptionsForm_Closing;
            Opened += OptionsForm_Opened;
            tablePanel1.col3Width = 200;
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
            Width = 720;
            Height = 640;
        }

        private void OptionsForm_Opened(object sender, EventArgs e)
        {
            if (!shownYet)
            {
                // set form size based on number of properties; min 5, max 10
                int numProperties = supportedProperties.Count;
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

        private void OptionsForm_Closing(object sender, WindowClosingEventArgs e)
        {
            Hide();
            e.Cancel = true; // this cancels the close event.
        }

        public bool hasProperty(string name)
        {
            return supportedProperties.Contains(name);
        }

        // create a dropdown with selectable options for a property
        public void makeMultiStringValueProperty(string propertyKey, string propertyName, string propertyDescription, StringValueSettings settings, Dictionary<string, Window> forms)
        {
            supportedProperties.Add(propertyKey);
            ComboBox combo = new ComboBox();
            string[] displayStrings = settings.getDisplayValues(propertyKey);
            foreach (string option in displayStrings)
            {
                combo.Items.Add(option);
            }
            combo.SelectedItem = displayStrings[settings.getDefaultIndex(propertyKey)];
            string[] optionValues = settings.getOptionsValues(propertyKey);
            propertyValues[propertyKey] = optionValues[combo.SelectedIndex];
            propertyForms[propertyKey] = forms;
            propertyMapping[combo] = propertyKey;
            propertyValueMapping[propertyKey] = optionValues;
            combo.SelectionChanged += Combo_SelectionChanged;
            optionsManager.addComboBox(combo, propertyKey, settings);
            tablePanel1.addRow(propertyName, propertyDescription, combo);
        }

        // callback for combobox options
        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (propertyMapping.ContainsKey(combo))
            {
                string propKey = propertyMapping[combo];
                if (propertyForms.ContainsKey(propKey))
                {
                    Dictionary<string, Window> forms = propertyForms[propKey];
                    if (forms != null)
                    {
                        string selItem = (string)combo.SelectedItem;
                        if (selItem != null && forms.ContainsKey(selItem))
                        {
                            forms[selItem].Show();
                        }
                    }
                }

                int selectedIndex = combo.SelectedIndex;
                if (selectedIndex >= 0)
                {
                    propertyValues[propKey] = propertyValueMapping[propKey][selectedIndex];
                }
            }
        }

        public void makeSeparator(string label)
        {
            tablePanel1.addHeader(label, 3);
        }

        // make a combo with yes/no for boolean options
        public void makeBooleanValueProperty(string propertyKey, string propertyName, string propertyDescription, StringValueSettings settings)
        {
            makeMultiStringValueProperty(propertyKey, propertyName, propertyDescription, settings, null);
        }

        // make a numeric up/down for selection of value for numeric options
        public void makeNumericValueProperty(string propertyKey, string propertyName, string propertyDescription, StringValueSettings settings)
        {
            supportedProperties.Add(propertyKey);
            NumericUpDown numericUpDown = new NumericUpDown();
            numericUpDown.Minimum = (decimal)settings.getDoubleMinimum(propertyKey);
            numericUpDown.Maximum = (decimal)settings.getDoubleMaximum(propertyKey);
            int decimalPlaces = settings.getDecimalPlaces(propertyKey);
            numericUpDown.FormatString = "F" + decimalPlaces;
            numericUpDown.NumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;
            double defaultValue = settings.getDoubleDefault(propertyKey);
            numericUpDown.Increment = (decimal)(1.0 / Math.Pow(10, decimalPlaces));
            numericUpDown.Value = (decimal)defaultValue;
            propertyValues[propertyKey] = defaultValue.ToString(CultureInfo.InvariantCulture);
            propertyMapping[numericUpDown] = propertyKey;
            numericUpDown.ValueChanged += NumericUpDown_ValueChanged;
            optionsManager.addNumericUpDown(numericUpDown, propertyKey, settings);
            tablePanel1.addRow(propertyName, propertyDescription, numericUpDown);
        }

        // callback for numeric up/down options
        private void NumericUpDown_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;
            if (propertyMapping.ContainsKey(numericUpDown))
            {
                string propKey = propertyMapping[numericUpDown];
                double newValue = (double)(e.NewValue ?? 0);
                propertyValues[propKey] = newValue.ToString(CultureInfo.InvariantCulture);
            }
        }

        public void setPropertyValue(string propertyKey, string propertyValue)
        {
            string[] vals = propertyValueMapping[propertyKey];
            for (int i = 0; i < vals.Length; i++)
            {
                if (vals[i] == propertyValue)
                {
                    foreach (ComboBox box in propertyMapping.Keys)
                    {
                        if (propertyMapping[box] == propertyKey)
                        {
                            box.SelectedIndex = i;
                        }
                    }
                }
            }
        }

        public ComboBox getPropertyCombo(string propertyKey)
        {
            foreach (Control cb in propertyMapping.Keys)
            {
                if (propertyMapping[cb] == propertyKey)
                {
                    return (ComboBox)cb;
                }
            }
            return null;
        }

        public NumericUpDown getPropertyUpDown(string propertyKey)
        {
            foreach (Control cb in propertyMapping.Keys)
            {
                if (propertyMapping[cb] == propertyKey)
                {
                    return (NumericUpDown)cb;
                }
            }
            return null;
        }

        public Dictionary<ComboBox, string> getAllPropertyCombos()
        {
            Dictionary<ComboBox, string> propertyCombos = new Dictionary<ComboBox, string>();
            foreach (Control c in propertyMapping.Keys)
            {
                if (c is ComboBox)
                {
                    propertyCombos[(ComboBox)c] = propertyMapping[c];
                }
            }
            return propertyCombos;
        }

        public Dictionary<NumericUpDown, string> getAllNumericUpDowns()
        {
            Dictionary<NumericUpDown, string> propertyUpDowns = new Dictionary<NumericUpDown, string>();
            foreach (Control c in propertyMapping.Keys)
            {
                if (c is NumericUpDown)
                {
                    propertyUpDowns[(NumericUpDown)c] = propertyMapping[c];
                }
            }
            return propertyUpDowns;
        }
    }
}
