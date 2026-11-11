#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace SoMRandomizer.help
{
    /// <summary>
    /// A little documentation form for help on each game mode.
    /// Avalonia port of the WinForms InfoForm; the WinForms version absolutely
    /// positioned every Label inside an autoscrolling Form, this one lays the
    /// same controls out in a vertical StackPanel and scrolls to section
    /// headers via the generated table of contents.
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    public class InfoForm : Window
    {
        private readonly Control titleControl;
        private readonly List<Control> subtitleControls;
        private readonly List<Control> headerControls;
        private readonly List<Control> controls;
        private readonly StackPanel contentPanel;
        private readonly ScrollViewer scrollViewer;

        public InfoForm(string title, Control titleControl, List<Control> subtitleControls, List<Control> headerControls, List<Control> controls)
        {
            this.titleControl = titleControl;
            this.subtitleControls = subtitleControls;
            this.headerControls = headerControls;
            this.controls = controls;

            Title = title;
            ClientSize = new Size(800, 450);
            CanResize = true;

            contentPanel = new StackPanel { Margin = new Thickness(10), Spacing = 5 };
            scrollViewer = new ScrollViewer
            {
                Content = contentPanel,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            };
            Content = scrollViewer;

            reRender();
            Closing += MyForm_FormClosing;
        }

        private void reRender()
        {
            contentPanel.Children.Clear();
            addControl(titleControl);
            foreach (Control c in subtitleControls)
            {
                addControl(c);
            }

            foreach (Control c in headerControls)
            {
                string headerText = (c as TextBlock)?.Text ?? "";
                Button link = new Button
                {
                    Content = headerText.Replace("\n", ""),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    Foreground = Brushes.Blue,
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Cursor = new Cursor(StandardCursorType.Hand),
                };
                int dotPos = headerText.IndexOf(".");
                int xPos = 10;
                if (dotPos >= 0 && dotPos < headerText.Length - 1)
                {
                    int ignored;
                    if (Int32.TryParse("" + headerText[dotPos + 1], out ignored))
                    {
                        xPos += 20;
                    }
                }
                link.Margin = new Thickness(xPos - 10, 0, 0, 0);
                addControl(link);
                Control headerControl = c;
                link.Click += (sender, e) => Link_Click(headerControl);
            }

            foreach (Control c in controls)
            {
                addControl(c);
            }
        }

        private void Link_Click(Control headerControl)
        {
            Point? position = headerControl.TranslatePoint(new Point(0, 0), contentPanel);
            if (position.HasValue)
            {
                scrollViewer.Offset = new Vector(0, Math.Max(0, position.Value.Y));
            }
        }

        private void addControl(Control c)
        {
            if (c == null)
            {
                return;
            }
            c.HorizontalAlignment = HorizontalAlignment.Left;
            contentPanel.Children.Add(c);
        }

        private void MyForm_FormClosing(object sender, WindowClosingEventArgs e)
        {
            Hide();
            e.Cancel = true; // this cancels the close event.
        }
    }
}
