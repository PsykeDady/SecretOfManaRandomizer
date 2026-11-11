#nullable disable
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using SoMRandomizer.config.settings;
using SoMRandomizer.forms;
using SoMRandomizer.processing.common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SoMRandomizer.config.ui
{
    /// <summary>
    /// A collection of all property-controlling UI elements and their associated properties, used to generate the full options string.
    /// Avalonia port of the WinForms UiOptionsManager.
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    public class UiOptionsManager
    {
        Dictionary<Control, string> controlProperties = new Dictionary<Control, string>();
        Dictionary<Control, StringValueSettings> controlSettings = new Dictionary<Control, StringValueSettings>();

        private void notifyChange()
        {
            foreach (OptionsListener listener in listeners)
            {
                listener.optionsChanged();
            }
        }

        // /////////////////////////////////////
        // checkbox
        // /////////////////////////////////////
        public void addCheckbox(CheckBox c, string propertyName, StringValueSettings settings)
        {
            controlProperties[c] = propertyName;
            controlSettings[c] = settings;
            c.IsChecked = settings.getDefaultBool(propertyName);
            c.IsCheckedChanged += CheckboxChange;
        }

        private void CheckboxChange(object sender, RoutedEventArgs e)
        {
            // set the property and notify the UI to update the flags display
            CheckBox box = (CheckBox)sender;
            string propertyName = controlProperties[box];
            StringValueSettings propertySettings = controlSettings[box];
            propertySettings.setBool(propertyName, box.IsChecked == true);
            notifyChange();
        }

        // /////////////////////////////////////
        // radio button
        // /////////////////////////////////////
        public void addRadioButtonGroup(RadioButton[] buttons, string propertyName, StringValueSettings settings)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].IsChecked = false;
                controlProperties[buttons[i]] = propertyName;
                controlSettings[buttons[i]] = settings;
            }
            buttons[settings.getDefaultIndex(propertyName)].IsChecked = true;
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Tag = settings.getOptionsValues(propertyName)[i];
                buttons[i].IsCheckedChanged += RadioButtonChange;
            }
        }

        private void RadioButtonChange(object sender, RoutedEventArgs e)
        {
            // set the property and notify the UI to update the flags display
            RadioButton button = (RadioButton)sender;
            string propertyName = controlProperties[button];
            StringValueSettings propertySettings = controlSettings[button];
            if (button.IsChecked == true)
            {
                propertySettings.set(propertyName, propertySettings.getIndex(propertyName, (string)button.Tag));
            }
            notifyChange();
        }

        // /////////////////////////////////////
        // trackbar (WinForms TrackBar -> Avalonia Slider)
        // /////////////////////////////////////
        public void addTrackbar(Slider bar, string propertyName, StringValueSettings settings)
        {
            controlProperties[bar] = propertyName;
            controlSettings[bar] = settings;
            bar.Minimum = 0;
            bar.Value = 0;
            bar.Maximum = settings.getOptionsValues(propertyName).Length - 1;
            bar.Value = settings.getDefaultIndex(propertyName);
            bar.ValueChanged += TrackBarChange;
        }

        private void TrackBarChange(object sender, RangeBaseValueChangedEventArgs e)
        {
            // set the property and notify the UI to update the flags display
            Slider bar = (Slider)sender;
            string propertyName = controlProperties[bar];
            StringValueSettings propertySettings = controlSettings[bar];
            propertySettings.set(propertyName, (int)Math.Round(bar.Value));
            notifyChange();
        }

        // /////////////////////////////////////
        // numeric up/down
        // /////////////////////////////////////
        public void addNumericUpDown(NumericUpDown upDown, string propertyName, StringValueSettings settings)
        {
            controlProperties[upDown] = propertyName;
            controlSettings[upDown] = settings;
            upDown.Minimum = (decimal)settings.getDoubleMinimum(propertyName);
            upDown.Maximum = (decimal)settings.getDoubleMaximum(propertyName);
            int decimalPlaces = settings.getDecimalPlaces(propertyName);
            upDown.FormatString = "F" + decimalPlaces;
            upDown.NumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;
            upDown.Increment = (decimal)(1.0 / Math.Pow(10, decimalPlaces));
            upDown.Value = (decimal)settings.getDoubleDefault(propertyName);
            upDown.ValueChanged += NumericUpDownChange;
        }

        private void NumericUpDownChange(object sender, NumericUpDownValueChangedEventArgs e)
        {
            NumericUpDown upDown = (NumericUpDown)sender;
            string propertyName = controlProperties[upDown];
            StringValueSettings propertySettings = controlSettings[upDown];
            propertySettings.setDouble(propertyName, (double)(upDown.Value ?? 0));
            notifyChange();
        }

        // /////////////////////////////////////
        // combo box
        // /////////////////////////////////////
        public void addComboBox(ComboBox comboBox, string propertyName, StringValueSettings settings)
        {
            controlProperties[comboBox] = propertyName;
            controlSettings[comboBox] = settings;
            comboBox.SelectedIndex = settings.getDefaultIndex(propertyName);
            comboBox.SelectionChanged += ComboBoxChange;
        }

        private void ComboBoxChange(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            string propertyName = controlProperties[box];
            StringValueSettings propertySettings = controlSettings[box];
            if (box.SelectedIndex >= 0)
            {
                propertySettings.set(propertyName, box.SelectedIndex);
            }
            notifyChange();
        }

        // /////////////////////////////////////
        // tab control
        // /////////////////////////////////////
        public void addTabControl(TabControl tabs, string propertyName, StringValueSettings settings)
        {
            controlProperties[tabs] = propertyName;
            controlSettings[tabs] = settings;
            tabs.SelectedIndex = settings.getDefaultIndex(propertyName);
            tabs.SelectionChanged += TabIndexChanged;
        }

        private void TabIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabs = (TabControl)sender;
            string propertyName = controlProperties[tabs];
            StringValueSettings propertySettings = controlSettings[tabs];
            propertySettings.set(propertyName, tabs.SelectedIndex);
            notifyChange();
        }

        // /////////////////////////////////////
        // checked listbox (multi-select)
        // /////////////////////////////////////
        public void addCheckedListBox(CheckedListBox checkedListBox, string propertyName, StringValueSettings settings)
        {
            controlProperties[checkedListBox] = propertyName;
            controlSettings[checkedListBox] = settings;
            checkedListBox.ClearItems();

            string[] allDisplayValues = settings.getDisplayValues(propertyName);
            string[] allValues = settings.getOptionsValues(propertyName);
            List<string> defaultValues = settings.getDefaultArray(propertyName).ToList();
            for (int i = 0; i < allValues.Length; i++)
            {
                checkedListBox.AddItem(allDisplayValues[i], defaultValues.Contains(allValues[i]));
            }

            checkedListBox.CheckedChanged += CheckedListBoxChanged;
        }

        private void CheckedListBoxChanged(object sender, EventArgs e)
        {
            CheckedListBox box = (CheckedListBox)sender;
            string propertyName = controlProperties[box];
            StringValueSettings propertySettings = controlSettings[box];
            List<int> selectedIndexes = new List<int>(box.CheckedIndices);
            propertySettings.setIndexes(propertyName, selectedIndexes.ToArray());
            notifyChange();
        }

        // /////////////////////////////////////
        // plandomizer form
        // /////////////////////////////////////
        public void addPlando(PlandoForm plandoForm, string propertyName, StringValueSettings settings)
        {
            controlProperties[plandoForm] = propertyName;
            controlSettings[plandoForm] = settings;
            plandoForm.setPlandoSettingsString(settings.getDefault(propertyName));
            plandoForm.PlandoSettingsChanged += PlandoChanged;
        }

        private void PlandoChanged(object sender, EventArgs e)
        {
            PlandoForm plandoForm = (PlandoForm)sender;
            string propertyName = controlProperties[plandoForm];
            StringValueSettings propertySettings = controlSettings[plandoForm];
            propertySettings.set(propertyName, plandoForm.getPlandoSettingsString());
            notifyChange();
        }

        private List<OptionsListener> listeners = new List<OptionsListener>();
        public interface OptionsListener
        {
            void optionsChanged();
        }

        public void addOptionsListener(OptionsListener listener)
        {
            listeners.Add(listener);
        }

        // get the options string for the UI based on all properties that have been modified.
        // TabControl values are always included since the only one we have currently indicates the game mode.
        public string getOptionsString()
        {
            string opStr = "version=" + RomGenerator.VERSION_NUMBER + " ";
            foreach (Control currentControl in controlProperties.Keys)
            {
                StringValueSettings settings = controlSettings[currentControl];
                string propertyName = controlProperties[currentControl];
                string value = settings.get(propertyName);
                string defaultValue = settings.getDefault(propertyName);
                if (currentControl is TabControl)
                {
                    // always show this one since it's the main mode
                    opStr += propertyName + "=" + value + " ";
                }
                else
                {
                    // only include if not the default value
                    if (value != defaultValue)
                    {
                        opStr += propertyName + "=" + value + " ";
                    }
                }
            }

            return opStr.Trim();
        }

        // set the selection on each associated UI element based on the given options string.
        // for each property not present in the string, set the selected option to the default.
        public void setOptions(string enteredString)
        {
            string[] allEntries = enteredString.Trim().Split(new char[] { ' ' });
            Dictionary<string, string> allEntriesMap = new Dictionary<string, string>();
            bool? versionMatch = null;
            List<string> unhandledProperties = new List<string>();
            foreach (string entry in allEntries)
            {
                string str = entry.Trim();
                int equalsIndex = str.IndexOf('=');
                List<string> values = new List<string>();
                if (equalsIndex >= 0)
                {
                    values.Add(str.Substring(0, equalsIndex));
                    values.Add(str.Substring(equalsIndex + 1));
                }
                if (values.Count == 2)
                {
                    allEntriesMap[values[0]] = values[1];
                    if (values[0] == "version")
                    {
                        versionMatch = values[1] == RomGenerator.VERSION_NUMBER;
                    }
                    else
                    {
                        unhandledProperties.Add(values[0]);
                    }
                }
                else
                {
                    throw new OptionsException(versionMatch, "Unexpected string: " + entry);
                }
            }

            // if entries has a replacement, use it, otherwise use the defaults.
            foreach (Control currentControl in controlProperties.Keys)
            {
                StringValueSettings settings = controlSettings[currentControl];
                string propertyName = controlProperties[currentControl];
                unhandledProperties.Remove(propertyName);
                string value = null;
                if (allEntriesMap.ContainsKey(propertyName))
                {
                    value = allEntriesMap[propertyName];
                }

                if (currentControl is CheckBox)
                {
                    CheckBox checkBox = (CheckBox)currentControl;
                    checkBox.IsChecked = value == null ? settings.getDefaultBool(propertyName) : StringValueSettings.stringToBool(value);
                }
                else if (currentControl is RadioButton)
                {
                    RadioButton button = (RadioButton)currentControl;
                    string tag = (string)button.Tag;
                    if (tag == value)
                    {
                        button.IsChecked = true;
                    }
                }
                else if (currentControl is Slider)
                {
                    Slider bar = (Slider)currentControl;
                    if (value != null)
                    {
                        int selectedValue = settings.getIndex(propertyName, value);
                        if (selectedValue < 0)
                        {
                            throw new OptionsException(versionMatch, "for " + propertyName + ", " + value + " was out of range.");
                        }
                        bar.Value = selectedValue;
                    }
                    else
                    {
                        bar.Value = settings.getDefaultIndex(propertyName);
                    }
                }
                else if (currentControl is NumericUpDown)
                {
                    NumericUpDown upDown = (NumericUpDown)currentControl;
                    if (value != null)
                    {
                        try
                        {
                            upDown.Value = (decimal)StringValueSettings.stringToDouble(value);
                        }
                        catch (Exception)
                        {
                            throw new OptionsException(versionMatch, "for " + propertyName + ", " + value + " could not be parsed as a double.");
                        }
                    }
                    else
                    {
                        upDown.Value = (decimal)settings.getDoubleDefault(propertyName);
                    }
                }
                else if (currentControl is ComboBox)
                {
                    ComboBox combo = (ComboBox)currentControl;
                    if (value != null)
                    {
                        int selectedValue = settings.getIndex(propertyName, value);
                        if (selectedValue < 0)
                        {
                            throw new OptionsException(versionMatch, "for " + propertyName + ", " + value + " was out of range.");
                        }
                        combo.SelectedIndex = selectedValue;
                    }
                    else
                    {
                        combo.SelectedIndex = settings.getDefaultIndex(propertyName);
                    }
                }
                else if (currentControl is TabControl)
                {
                    TabControl tabs = (TabControl)currentControl;
                    if (value != null)
                    {
                        int selectedValue = settings.getIndex(propertyName, value);
                        if (selectedValue < 0)
                        {
                            throw new OptionsException(versionMatch, "for " + propertyName + ", " + value + " was out of range.");
                        }
                        tabs.SelectedIndex = selectedValue;
                    }
                    else
                    {
                        tabs.SelectedIndex = settings.getDefaultIndex(propertyName);
                    }
                }
                else if (currentControl is CheckedListBox)
                {
                    CheckedListBox box = (CheckedListBox)currentControl;
                    List<int> indexes;
                    if (value != null)
                    {
                        // derive indexes from the parsed value (the WinForms original incorrectly
                        // read the current state here via settings.getIndexes(propertyName))
                        indexes = new List<int>();
                        string[] vals = value.Split(new char[] { ',' });
                        foreach (string v in vals)
                        {
                            int idx = settings.getIndex(propertyName, v);
                            if (idx >= 0)
                            {
                                indexes.Add(idx);
                            }
                        }
                    }
                    else
                    {
                        indexes = settings.getDefaultIndexes(propertyName).ToList();
                    }
                    int total = settings.getOptionsValues(propertyName).Length;
                    for (int i = 0; i < total; i++)
                    {
                        box.SetItemChecked(i, indexes.Contains(i));
                    }
                }
                else if (currentControl is PlandoForm)
                {
                    PlandoForm plandoForm = (PlandoForm)currentControl;
                    plandoForm.setPlandoSettingsString(value ?? "");
                }
            }

            if (unhandledProperties.Count > 0)
            {
                string propertyNames = "";
                for (int i = 0; i < unhandledProperties.Count; i++)
                {
                    propertyNames += unhandledProperties[i];
                    if (i < unhandledProperties.Count - 1)
                    {
                        propertyNames += ", ";
                    }
                }
                throw new OptionsException(versionMatch, "Unrecognized properties: " + propertyNames);
            }

            if (versionMatch == null || !(bool)versionMatch)
            {
                throw new OptionsException(versionMatch, null);
            }
        }
    }
}
