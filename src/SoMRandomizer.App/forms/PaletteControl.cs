#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using SoMRandomizer.processing.common.structure;
using SoMRandomizer.util;
using System;
using System.Collections.Generic;

namespace SoMRandomizer.forms
{
    /// <summary>
    /// This is super old code from the old mana editor, but it's used here to provide a palette editor for character colors.
    /// Avalonia port: the original System.Drawing PictureBoxes are replaced by a custom rendered
    /// palette grid plus a Border used as the "current color" preview.
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    public class PaletteControl : UserControl
    {
        public delegate void PalIndexChangedHandler(int index);
        public delegate void WorkingColorModifiedHandler(int r, int g, int b);
        public delegate void PaletteModifiedHandler();

        public event PalIndexChangedHandler PalIndexChanged;
        public event WorkingColorModifiedHandler WorkingColorModified;
        public event PaletteModifiedHandler PaletteModified;

        bool editable = false;
        public SnesPalette pal;
        public int selectedIndex = 0;
        List<int> indexes = new List<int>();
        int transparentIndex = -1;

        private PaletteGridView pictureBox2;
        private Border pictureBox5;
        private TextBlock label9;
        private TextBlock paletteLabel;
        private TextBlock redLabel;
        private TextBlock greenLabel;
        private TextBlock blueLabel;
        private NumericUpDown numericUpDown1;
        private NumericUpDown numericUpDown2;
        private NumericUpDown numericUpDown3;
        private Button okButton;

        public PaletteControl()
        {
            // Flow layout: the original WinForms palette editor placed everything on a
            // 215x207 canvas, which clipped the "Current:" label and the Ok button once
            // Avalonia's font metrics and NumericUpDown template were in play.
            StackPanel root = new StackPanel { Spacing = 4 };
            Content = root;

            Grid top = new Grid { ColumnDefinitions = new ColumnDefinitions("128,12,Auto") };
            pictureBox2 = new PaletteGridView(this) { Width = 128, Height = 64 };
            top.Children.Add(pictureBox2);

            pictureBox5 = new Border
            {
                Width = 32,
                Height = 64,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Background = Brushes.White,
            };
            Grid.SetColumn(pictureBox5, 2);
            top.Children.Add(pictureBox5);
            root.Children.Add(top);

            Grid captions = new Grid { ColumnDefinitions = new ColumnDefinitions("128,12,*") };
            paletteLabel = new TextBlock { Text = "Palette", Width = 128, TextAlignment = TextAlignment.Center };
            captions.Children.Add(paletteLabel);
            label9 = new TextBlock { Text = "Current: 0", VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(label9, 2);
            captions.Children.Add(label9);
            root.Children.Add(captions);

            Grid colors = new Grid { ColumnDefinitions = new ColumnDefinitions("60,10,96") };
            for (int i = 0; i < 3; i++)
            {
                colors.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            redLabel = AddColorLabel(colors, "Red:", 0);
            greenLabel = AddColorLabel(colors, "Green:", 1);
            blueLabel = AddColorLabel(colors, "Blue:", 2);

            numericUpDown1 = AddNud(colors, 0);
            numericUpDown1.ValueChanged += (s, e) => fixColors();
            numericUpDown2 = AddNud(colors, 1);
            numericUpDown2.ValueChanged += (s, e) => fixColors();
            numericUpDown3 = AddNud(colors, 2);
            numericUpDown3.ValueChanged += (s, e) => fixColors();
            root.Children.Add(colors);

            okButton = new Button
            {
                Content = "Ok",
                Width = 96,
                Padding = new Thickness(8, 2, 8, 2),
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 4, 0, 0),
            };
            okButton.Click += button1_Click;
            root.Children.Add(okButton);

            numericUpDown1.IsVisible = false;
            numericUpDown2.IsVisible = false;
            numericUpDown3.IsVisible = false;
            redLabel.IsVisible = false;
            greenLabel.IsVisible = false;
            blueLabel.IsVisible = false;
            okButton.IsVisible = false;

            setColorBits(4);
            setPalette(new SnesPalette());
        }

        private static TextBlock AddColorLabel(Grid grid, string text, int row)
        {
            TextBlock tb = new TextBlock
            {
                Text = text,
                Width = 60,
                TextAlignment = TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 3),
            };
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, row);
            grid.Children.Add(tb);
            return tb;
        }

        private static NumericUpDown AddNud(Grid grid, int row)
        {
            NumericUpDown nud = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 255,
                Increment = 8,
                FormatString = "F0",
                Value = 0,
                Width = 96,
                Margin = new Thickness(0, 0, 0, 3),
            };
            Grid.SetColumn(nud, 2);
            Grid.SetRow(nud, row);
            grid.Children.Add(nud);
            return nud;
        }

        private void setColorBits(int bits)
        {
            int minColor = 0;
            int maxColor = (int)(Math.Pow(2, bits) - 1);
            indexes.Clear();
            for (int i = minColor; i <= maxColor; i++)
            {
                indexes.Add(i);
            }
        }

        public void setTransparentIndex(int index)
        {
            // optional and default to -1 so it effectively doesn't exist
            transparentIndex = index;
        }

        public void setPalette(SnesPalette pal)
        {
            this.pal = pal;
            if (pal != null)
            {
                SnesColor col = pal.getColor(selectedIndex);
                numericUpDown1.Value = col.getRed();
                numericUpDown2.Value = col.getGreen();
                numericUpDown3.Value = col.getBlue();
            }
            Draw();
        }

        private void selectIndex(int selIndex)
        {
            selectedIndex = selIndex;

            if (editable)
            {
                SnesColor col = pal.getColor(selectedIndex);
                numericUpDown1.Value = col.getRed();
                numericUpDown2.Value = col.getGreen();
                numericUpDown3.Value = col.getBlue();
            }

            PalIndexChanged?.Invoke(selectedIndex);
            Draw();
        }

        public void Draw()
        {
            if (pal != null)
            {
                if (!indexes.Contains(selectedIndex))
                {
                    selectedIndex = indexes[0];
                }

                SnesColor selectedCol = pal.getColor(selectedIndex);
                pictureBox5.Background = new SolidColorBrush(Color.FromRgb(selectedCol.getRed(), selectedCol.getGreen(), selectedCol.getBlue()));
                label9.Text = "Current: " + (selectedIndex == transparentIndex ? "BG" : selectedIndex.ToString());

                if (selectedIndex == transparentIndex)
                {
                    numericUpDown1.IsVisible = false;
                    numericUpDown2.IsVisible = false;
                    numericUpDown3.IsVisible = false;
                    redLabel.IsVisible = false;
                    greenLabel.IsVisible = false;
                    blueLabel.IsVisible = false;
                    okButton.IsVisible = false;
                }
                else if (editable)
                {
                    numericUpDown1.IsVisible = true;
                    numericUpDown2.IsVisible = true;
                    numericUpDown3.IsVisible = true;
                    redLabel.IsVisible = true;
                    greenLabel.IsVisible = true;
                    blueLabel.IsVisible = true;
                    okButton.IsVisible = true;
                }

                pictureBox2.InvalidateVisual();
            }
        }

        public void setEditable()
        {
            editable = true;
            numericUpDown1.IsVisible = true;
            numericUpDown2.IsVisible = true;
            numericUpDown3.IsVisible = true;
            redLabel.IsVisible = true;
            greenLabel.IsVisible = true;
            blueLabel.IsVisible = true;
            okButton.IsVisible = true;
        }

        public void setColor(int red, int green, int blue)
        {
            numericUpDown1.Value = red;
            numericUpDown2.Value = green;
            numericUpDown3.Value = blue;
        }

        private void button1_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SnesColor newCol = new SnesColor((byte)numericUpDown1.Value, (byte)numericUpDown2.Value, (byte)numericUpDown3.Value);
            pal.setColor(selectedIndex, newCol);
            PaletteModified?.Invoke();
            Draw();
        }

        private void fixColors()
        {
            // ensure entries are a multiple of 8
            byte r = (byte)((int)numericUpDown1.Value & 0xF8);
            byte g = (byte)((int)numericUpDown2.Value & 0xF8);
            byte b = (byte)((int)numericUpDown3.Value & 0xF8);

            pictureBox5.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
            WorkingColorModified?.Invoke(r, g, b);
        }

        /// <summary>
        /// Custom-drawn replacement for the original PictureBox swatch grid.
        /// </summary>
        private class PaletteGridView : Control
        {
            private readonly PaletteControl owner;

            public PaletteGridView(PaletteControl owner)
            {
                this.owner = owner;
                ClipToBounds = true;
                Cursor = new Cursor(StandardCursorType.Hand);
            }

            public override void Render(DrawingContext context)
            {
                base.Render(context);

                int width = (int)Bounds.Width;
                int height = (int)Bounds.Height;
                if (width <= 0 || height <= 0)
                {
                    return;
                }

                context.FillRectangle(new SolidColorBrush(Color.FromRgb(240, 240, 240)), new Rect(0, 0, width, height));

                if (owner.pal == null)
                {
                    return;
                }

                Rect selected = new Rect();
                for (int i = 0; i < 16; i++)
                {
                    if (owner.indexes.Contains(i))
                    {
                        int xleft = ((i % 8) * width) / 8;
                        int xright = (((i % 8) + 1) * width) / 8;
                        int ytop = ((i / 8) * height) / 2;
                        int ybottom = (((i / 8) + 1) * height) / 2;
                        Rect rect = new Rect(xleft, ytop, xright - xleft, ybottom - ytop);
                        if (i == owner.selectedIndex)
                        {
                            selected = new Rect(xleft + 1, ytop + 1, xright - xleft - 3, ybottom - ytop - 3);
                        }
                        SnesColor col = owner.pal.getColor(i);
                        context.FillRectangle(new SolidColorBrush(Color.FromRgb(col.getRed(), col.getGreen(), col.getBlue())), rect);
                    }
                }

                for (int i = 0; i < 16; i++)
                {
                    if (owner.indexes.Contains(i))
                    {
                        int xleft = ((i % 8) * width) / 8;
                        int xright = (((i % 8) + 1) * width) / 8;
                        int ytop = ((i / 8) * height) / 2;
                        int ybottom = (((i / 8) + 1) * height) / 2;
                        context.DrawRectangle(null, new Pen(Brushes.Black, 1), new Rect(xleft, ytop, xright - xleft - 1, ybottom - ytop - 1));
                    }
                }

                SnesColor selectedCol = owner.pal.getColor(owner.selectedIndex);
                context.DrawRectangle(null, new Pen(new SolidColorBrush(Color.FromRgb((byte)(selectedCol.getRed() / 4), (byte)(selectedCol.getGreen() / 4), (byte)(selectedCol.getBlue() / 4))), 1), selected);
                context.DrawRectangle(null, new Pen(new SolidColorBrush(Color.FromRgb((byte)(selectedCol.getRed() / 2), (byte)(selectedCol.getGreen() / 2), (byte)(selectedCol.getBlue() / 2))), 1),
                    new Rect(selected.Left + 1, selected.Top + 1, selected.Width - 2, selected.Height - 2));

                if (owner.indexes.Contains(owner.transparentIndex))
                {
                    int ti = owner.transparentIndex;
                    int xleft = ((ti % 8) * width) / 8;
                    int xright = (((ti % 8) + 1) * width) / 8;
                    int ytop = ((ti / 8) * height) / 2;
                    int ybottom = (((ti / 8) + 1) * height) / 2;
                    context.DrawLine(new Pen(Brushes.Black, 1), new Point(xleft, ytop), new Point(xright - 1, ybottom - 1));
                    context.DrawLine(new Pen(Brushes.Black, 1), new Point(xright - 1, ytop), new Point(xleft, ybottom - 1));
                }
            }

            protected override void OnPointerPressed(PointerPressedEventArgs e)
            {
                base.OnPointerPressed(e);

                if (owner.pal == null)
                {
                    return;
                }

                int width = (int)Bounds.Width;
                int height = (int)Bounds.Height;
                if (width <= 0 || height <= 0)
                {
                    return;
                }

                Point p = e.GetPosition(this);
                int index_x = (int)((p.X / width) * 8);
                index_x = DataUtil.clampToEndpoints(index_x, 0, 7);
                int index_y = (int)((p.Y / height) * 2);
                index_y = DataUtil.clampToEndpoints(index_y, 0, 1);
                int selIndex = index_y * 8 + index_x;
                if (owner.indexes.Contains(selIndex) && selIndex != owner.selectedIndex)
                {
                    owner.selectIndex(selIndex);
                }
            }
        }
    }
}
