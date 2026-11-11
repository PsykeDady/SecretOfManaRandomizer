#nullable disable
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using System;
using System.Collections.Generic;

namespace SoMRandomizer.config.ui
{
    /// <summary>
    /// Avalonia equivalent of the WinForms CheckedListBox: a scrollable vertical list of
    /// checkboxes used for multi-select options (e.g. ancient cave biome selection).
    /// </summary>
    public class CheckedListBox : ScrollViewer
    {
        private StackPanel itemsPanel = new StackPanel();
        private List<CheckBox> itemBoxes = new List<CheckBox>();
        private List<bool> checkedStates = new List<bool>();

        /// <summary>Raised whenever any item's checked state changes.</summary>
        public event EventHandler CheckedChanged;

        public CheckedListBox()
        {
            Content = itemsPanel;
            itemsPanel.Orientation = Orientation.Vertical;
            itemsPanel.Spacing = 2;
        }

        public void ClearItems()
        {
            itemsPanel.Children.Clear();
            itemBoxes.Clear();
            checkedStates.Clear();
        }

        public void AddItem(string displayValue, bool isChecked)
        {
            int index = itemBoxes.Count;
            CheckBox checkBox = new CheckBox();
            checkBox.Content = displayValue;
            checkBox.Tag = index;
            checkBox.IsChecked = isChecked;
            checkBox.IsCheckedChanged += CheckBox_IsCheckedChanged;
            itemBoxes.Add(checkBox);
            checkedStates.Add(isChecked);
            itemsPanel.Children.Add(checkBox);
        }

        private void CheckBox_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            int index = (int)checkBox.Tag;
            checkedStates[index] = checkBox.IsChecked == true;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Indices of all currently checked items.</summary>
        public IReadOnlyList<int> CheckedIndices
        {
            get
            {
                List<int> indices = new List<int>();
                for (int i = 0; i < checkedStates.Count; i++)
                {
                    if (checkedStates[i])
                    {
                        indices.Add(i);
                    }
                }
                return indices;
            }
        }

        public void SetItemChecked(int index, bool isChecked)
        {
            if (index >= 0 && index < itemBoxes.Count)
            {
                itemBoxes[index].IsChecked = isChecked;
            }
        }
    }
}
