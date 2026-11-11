#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using SoMRandomizer.config;
using SoMRandomizer.config.settings;
using SoMRandomizer.processing.common;
using SoMRandomizer.processing.common.structure;
using SoMRandomizer.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SoMRandomizer.forms
{
    /// <summary>
    /// Palette picker for playable characters.
    /// Avalonia port of the WinForms CharacterDesigner.
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    public class CharacterDesigner : Window
    {
        private SnesPalette boyDefault;
        private SnesPalette girlDefault;
        private SnesPalette spriteDefault;

        public SnesPalette boyCurrent;
        public SnesPalette girlCurrent;
        public SnesPalette spriteCurrent;

        private ComboBox comboBox1;
        private ComboBox comboBox2;
        private Image pictureBox1;
        private PaletteControl paletteControl1;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private NumericUpDown numericUpDown1;
        private NumericUpDown numericUpDown2;
        private NumericUpDown numericUpDown3;

        // decoded source images, cached because they are re-read every time the preview is redrawn
        private static readonly Dictionary<string, Bitmap> sourceImages = new Dictionary<string, Bitmap>();

        public CharacterDesigner()
        {
            BuildUi();

            Title = "Character Designer";
            SizeToContent = SizeToContent.WidthAndHeight;
            CanResize = false;
            CanMaximize = false;

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 2;

            // vanilla palettes
            boyDefault = new SnesPalette(new byte[] { 0xff, 0x7f, 0xa6, 0x10, 0x10, 0x0d, 0x79, 0x11, 0x7f, 0x12, 0x51, 0x1d, 0xfa, 0x29, 0xfe, 0x3e, 0xbd, 0x54, 0xdf, 0x52, 0x24, 0x39, 0x47, 0x62, 0x91, 0x7b, 0x1b, 0x25, 0xbf, 0x16, 0xff, 0x7f });
            girlDefault = new SnesPalette(new byte[] { 0xff, 0x7f, 0xe9, 0x18, 0x74, 0x1d, 0xfb, 0x01, 0xbe, 0x12, 0x55, 0x1d, 0x1b, 0x2e, 0x7f, 0x57, 0xaa, 0x36, 0xb3, 0x57, 0xb4, 0x38, 0x5d, 0x5d, 0x5f, 0x76, 0x7f, 0x53, 0x60, 0x39, 0xde, 0x7b });
            spriteDefault = new SnesPalette(new byte[] { 0xff, 0x7f, 0xc7, 0x14, 0xd5, 0x20, 0x7a, 0x35, 0x5d, 0x3e, 0x91, 0x15, 0x3a, 0x3a, 0x1f, 0x57, 0x1f, 0x43, 0x72, 0x0c, 0xe4, 0x2d, 0xca, 0x36, 0xee, 0x5b, 0x38, 0x55, 0xdf, 0x7a, 0xff, 0x7f });

            boyCurrent = new SnesPalette(boyDefault);
            girlCurrent = new SnesPalette(girlDefault);
            spriteCurrent = new SnesPalette(spriteDefault);

            paletteControl1.PaletteModified += paletteModified;
            paletteControl1.setPalette(boyCurrent);
            Closing += MyForm_FormClosing;
            reDraw();
            paletteControl1.setEditable();
        }

        private void BuildUi()
        {
            // Flow layout instead of the absolute WinForms canvas: the old coordinates
            // assumed 8.25pt MS Sans Serif, so at the Avalonia font metrics the bottom
            // row of buttons and the color transformations box were clipped.
            Grid root = new Grid
            {
                Margin = new Thickness(12),
                ColumnDefinitions = new ColumnDefinitions("Auto,20,Auto,20,Auto"),
                VerticalAlignment = VerticalAlignment.Top,
            };
            Content = root;

            // left column: sprite preview + file buttons
            StackPanel left = new StackPanel { Spacing = 10 };
            pictureBox1 = new Image { Stretch = Avalonia.Media.Stretch.None };
            left.Children.Add(pictureBox1);

            StackPanel fileButtons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            Button button1 = AddButton(fileButtons, "Save all", 80);
            button1.Click += button1_Click;
            Button button2 = AddButton(fileButtons, "Load all", 80);
            button2.Click += button2_Click;
            Button button9 = AddButton(fileButtons, "Reset all", 80);
            button9.Click += button9_Click;
            left.Children.Add(fileButtons);
            Grid.SetColumn(left, 0);
            root.Children.Add(left);

            // middle column: character/backdrop selectors, palette editor, import button
            StackPanel middle = new StackPanel { Spacing = 10 };
            Grid selectors = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,24,Auto") };

            StackPanel charBox = new StackPanel { Spacing = 4 };
            charBox.Children.Add(new TextBlock { Text = "Character:" });
            comboBox1 = AddCombo(new string[] { "Boy", "Girl", "Sprite" }, 120);
            comboBox1.SelectionChanged += comboBox1_SelectedIndexChanged;
            charBox.Children.Add(comboBox1);
            selectors.Children.Add(charBox);

            StackPanel backBox = new StackPanel { Spacing = 4 };
            backBox.Children.Add(new TextBlock { Text = "Backdrop:" });
            comboBox2 = AddCombo(new string[] { "Black", "White", "Forest", "Cave", "Ruins", "Castle" }, 120);
            comboBox2.SelectionChanged += comboBox2_SelectedIndexChanged;
            backBox.Children.Add(comboBox2);
            Grid.SetColumn(backBox, 2);
            selectors.Children.Add(backBox);

            middle.Children.Add(selectors);

            paletteControl1 = new PaletteControl();
            middle.Children.Add(paletteControl1);

            Button button10 = new Button
            {
                Content = "Import this character from ROM...",
                Padding = new Thickness(10, 2, 10, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            button10.Click += button10_Click;
            middle.Children.Add(button10);

            Grid.SetColumn(middle, 2);
            root.Children.Add(middle);

            Control groupBox = AddGroupBox();
            Grid.SetColumn(groupBox, 4);
            root.Children.Add(groupBox);
        }

        private static Button AddButton(StackPanel container, string text, double width)
        {
            Button button = new Button { Content = text, MinWidth = width, Padding = new Thickness(6, 2, 6, 2), HorizontalAlignment = HorizontalAlignment.Left };
            container.Children.Add(button);
            return button;
        }

        private static ComboBox AddCombo(string[] items, double width)
        {
            ComboBox combo = new ComboBox { Width = width };
            foreach (string item in items)
            {
                combo.Items.Add(item);
            }
            return combo;
        }

        private static Button AddGroupButton(Grid grid, string text, int row)
        {
            Button button = new Button
            {
                Content = text,
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(0, 0, 0, 4),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            Grid.SetColumn(button, 0);
            Grid.SetRow(button, row);
            grid.Children.Add(button);
            return button;
        }

        private static NumericUpDown AddGroupNud(Grid grid, int row, double min)
        {
            NumericUpDown nud = new NumericUpDown
            {
                Minimum = (decimal)min,
                Maximum = 100,
                Increment = 1,
                FormatString = "F0",
                NumberFormat = System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat,
                Value = 0,
                Margin = new Thickness(0, 0, 0, 4),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(nud, 2);
            Grid.SetRow(nud, row);
            grid.Children.Add(nud);
            return nud;
        }

        private Control AddGroupBox()
        {
            Border groupBox1 = new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush = Avalonia.Media.Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Top,
            };

            DockPanel dock = new DockPanel();
            TextBlock header = new TextBlock { Text = "Color transformations", Margin = new Thickness(8, 4, 8, 4) };
            DockPanel.SetDock(header, Dock.Top);
            dock.Children.Add(header);
            groupBox1.Child = dock;

            StackPanel body = new StackPanel { Spacing = 2, Margin = new Thickness(10, 2, 10, 10) };
            dock.Children.Add(body);

            Grid grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("126,10,86,6,Auto"),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto"),
            };
            body.Children.Add(grid);

            Button button3 = AddGroupButton(grid, "Swap R, G", 0);
            button3.Click += button3_Click;
            Button button4 = AddGroupButton(grid, "Swap G, B", 1);
            button4.Click += button4_Click;
            Button button5 = AddGroupButton(grid, "Swap R, B", 2);
            button5.Click += button5_Click;
            Button button6 = AddGroupButton(grid, "Brightness +", 3);
            button6.Click += button6_Click;
            Button button7 = AddGroupButton(grid, "Saturation +", 4);
            button7.Click += button7_Click;
            Button button8 = AddGroupButton(grid, "Hue +", 5);
            button8.Click += button8_Click;

            numericUpDown1 = AddGroupNud(grid, 3, -100);
            numericUpDown2 = AddGroupNud(grid, 4, -100);
            numericUpDown3 = AddGroupNud(grid, 5, -100);

            grid.Children.Add(AddPercentLabel(grid, 3));
            grid.Children.Add(AddPercentLabel(grid, 4));
            grid.Children.Add(AddPercentLabel(grid, 5));

            StackPanel applyTo = new StackPanel { Spacing = 2, Margin = new Thickness(4, 10, 0, 0) };
            applyTo.Children.Add(new TextBlock { Text = "Apply to:" });
            radioButton1 = new RadioButton { Content = "This color", GroupName = "applyTo", IsChecked = true };
            radioButton2 = new RadioButton { Content = "All colors", GroupName = "applyTo" };
            applyTo.Children.Add(radioButton1);
            applyTo.Children.Add(radioButton2);
            body.Children.Add(applyTo);

            return groupBox1;
        }

        private static TextBlock AddPercentLabel(Grid grid, int row)
        {
            TextBlock label = new TextBlock { Text = "%", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 0, 4) };
            Grid.SetColumn(label, 4);
            Grid.SetRow(label, row);
            return label;
        }

        private static Bitmap loadSourceImage(string resourceName)
        {
            if (sourceImages.TryGetValue(resourceName, out Bitmap cached))
            {
                return cached;
            }
            using (Stream stream = typeof(CommonSettings).Assembly.GetManifestResourceStream("SoMRandomizer.Resources." + resourceName))
            {
                if (stream == null)
                {
                    return null;
                }
                Bitmap bitmap = new Bitmap(stream);
                sourceImages[resourceName] = bitmap;
                return bitmap;
            }
        }

        private static byte[] readBgra(Bitmap bitmap, out int width, out int height, out int stride)
        {
            PixelSize size = bitmap.PixelSize;
            width = size.Width;
            height = size.Height;
            stride = width * 4;
            byte[] pixels = new byte[stride * height];
            IntPtr buffer = Marshal.AllocHGlobal(pixels.Length);
            try
            {
                bitmap.CopyPixels(new PixelRect(0, 0, width, height), buffer, pixels.Length, stride);
                Marshal.Copy(buffer, pixels, 0, pixels.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return pixels;
        }

        private static WriteableBitmap makeBitmap(byte[] pixels, int width, int height)
        {
            WriteableBitmap bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Unpremul);
            using (ILockedFramebuffer frameBuffer = bitmap.Lock())
            {
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(pixels, y * width * 4, IntPtr.Add(frameBuffer.Address, y * frameBuffer.RowBytes), width * 4);
                }
            }
            return bitmap;
        }

        private void reDraw()
        {
            int charIndex = comboBox1.SelectedIndex;
            int bgIndex = comboBox2.SelectedIndex;
            if (charIndex < 0 || bgIndex < 0)
            {
                return;
            }
            Bitmap newBitmap = null;
            Bitmap bgBitmap = null;
            SnesPalette defaultPalette = null;
            SnesPalette currentPalette = null;

            switch (charIndex)
            {
                case 0:
                    newBitmap = loadSourceImage("sprite_boy.png");
                    defaultPalette = boyDefault;
                    currentPalette = boyCurrent;
                    break;
                case 1:
                    newBitmap = loadSourceImage("sprite_girl.png");
                    defaultPalette = girlDefault;
                    currentPalette = girlCurrent;
                    break;
                case 2:
                    newBitmap = loadSourceImage("sprite_sprite.png");
                    defaultPalette = spriteDefault;
                    currentPalette = spriteCurrent;
                    break;
            }

            switch (bgIndex)
            {
                case 2:
                    bgBitmap = loadSourceImage("backdrop_forest.png");
                    break;
                case 3:
                    bgBitmap = loadSourceImage("backdrop_cave.png");
                    break;
                case 4:
                    bgBitmap = loadSourceImage("backdrop_ruins.png");
                    break;
                case 5:
                    bgBitmap = loadSourceImage("backdrop_castle.png");
                    break;
            }

            if (currentPalette == null || newBitmap == null)
            {
                return;
            }

            byte[] spritePixels = readBgra(newBitmap, out int width, out int height, out int stride);
            byte[] bgPixels = null;
            int bgStride = 0;
            if (bgBitmap != null)
            {
                bgPixels = readBgra(bgBitmap, out int bgWidth, out int bgHeight, out bgStride);
                if (bgWidth != width || bgHeight != height)
                {
                    bgPixels = null;
                }
            }

            byte[] output = new byte[stride * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x * 4;
                    byte baseBlue = spritePixels[offset];
                    byte baseGreen = spritePixels[offset + 1];
                    byte baseRed = spritePixels[offset + 2];

                    int matchIndex = 0;
                    for (int i = 1; i < 16 && matchIndex == 0; i++)
                    {
                        SnesColor col = defaultPalette.getColor(i);
                        if (col.getRed() == baseRed &&
                            col.getGreen() == baseGreen &&
                            col.getBlue() == baseBlue)
                        {
                            matchIndex = i;
                        }
                    }

                    if (matchIndex == 0)
                    {
                        if (bgPixels == null)
                        {
                            if (bgIndex == 0)
                            {
                                // black
                                output[offset] = 0;
                                output[offset + 1] = 0;
                                output[offset + 2] = 0;
                            }
                            else
                            {
                                // white
                                output[offset] = 255;
                                output[offset + 1] = 255;
                                output[offset + 2] = 255;
                            }
                        }
                        else
                        {
                            output[offset] = bgPixels[offset];
                            output[offset + 1] = bgPixels[offset + 1];
                            output[offset + 2] = bgPixels[offset + 2];
                        }
                    }
                    else
                    {
                        SnesColor newCol = currentPalette.getColor(matchIndex);
                        output[offset] = newCol.getBlue();
                        output[offset + 1] = newCol.getGreen();
                        output[offset + 2] = newCol.getRed();
                    }
                    output[offset + 3] = 255;
                }
            }

            pictureBox1.Source = makeBitmap(output, width, height);
        }

        public void paletteModified()
        {
            int charIndex = comboBox1.SelectedIndex;
            SnesPalette currentPalette = null;
            if (charIndex < 0)
            {
                return;
            }

            switch (charIndex)
            {
                case 0:
                    currentPalette = boyCurrent;
                    break;
                case 1:
                    currentPalette = girlCurrent;
                    break;
                case 2:
                    currentPalette = spriteCurrent;
                    break;
            }

            if (currentPalette == null)
            {
                return;
            }

            currentPalette.copyFrom(paletteControl1.pal);
            reDraw();
        }

        private void comboBox1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            int charIndex = comboBox1.SelectedIndex;
            SnesPalette currentPalette = null;
            if (charIndex < 0)
            {
                return;
            }

            switch (charIndex)
            {
                case 0:
                    currentPalette = boyCurrent;
                    break;
                case 1:
                    currentPalette = girlCurrent;
                    break;
                case 2:
                    currentPalette = spriteCurrent;
                    break;
            }

            if (currentPalette == null)
            {
                return;
            }
            paletteControl1.setPalette(currentPalette);

            reDraw();
        }

        private void comboBox2_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            reDraw();
        }

        private void MyForm_FormClosing(object sender, WindowClosingEventArgs e)
        {
            Hide();
            e.Cancel = true; // this cancels the close event.
        }

        private void SaveSettings(string filename)
        {
            StreamWriter fileOut = new StreamWriter(filename);
            try
            {
                for (int i = 1; i < 16; i++)
                {
                    SnesColor boyColor = boyCurrent.getColor(i);
                    SnesColor girlColor = girlCurrent.getColor(i);
                    SnesColor spriteColor = spriteCurrent.getColor(i);
                    fileOut.WriteLine("boyR" + i + "=" + boyColor.getRed());
                    fileOut.WriteLine("boyG" + i + "=" + boyColor.getGreen());
                    fileOut.WriteLine("boyB" + i + "=" + boyColor.getBlue());
                    fileOut.WriteLine("girlR" + i + "=" + girlColor.getRed());
                    fileOut.WriteLine("girlG" + i + "=" + girlColor.getGreen());
                    fileOut.WriteLine("girlB" + i + "=" + girlColor.getBlue());
                    fileOut.WriteLine("spriteR" + i + "=" + spriteColor.getRed());
                    fileOut.WriteLine("spriteG" + i + "=" + spriteColor.getGreen());
                    fileOut.WriteLine("spriteB" + i + "=" + spriteColor.getBlue());
                }
            }
            finally
            {
                fileOut.Close();
            }
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            // save
            TopLevel top = TopLevel.GetTopLevel(this);
            if (top == null)
            {
                return;
            }
            IStorageFile file = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save all",
                SuggestedFileName = "character_colors.properties",
                FileTypeChoices = new FilePickerFileType[]
                {
                    new FilePickerFileType("Properties File") { Patterns = new string[] { "*.properties" } },
                },
            });
            string filename = file?.TryGetLocalPath();
            if (filename == null)
            {
                return;
            }
            try
            {
                SaveSettings(filename);
            }
            catch (Exception)
            {
                await DialogUtil.ShowMessage(this, "Error", "Unable to write to file.");
            }
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            // load
            TopLevel top = TopLevel.GetTopLevel(this);
            if (top == null)
            {
                return;
            }
            IReadOnlyList<IStorageFile> files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Load all",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[]
                {
                    new FilePickerFileType("Properties File") { Patterns = new string[] { "*.properties" } },
                },
            });
            string filename = files.Count > 0 ? files[0].TryGetLocalPath() : null;
            if (filename != null)
            {
                try
                {
                    PropertyFileReader pfr = new PropertyFileReader();
                    Dictionary<string, string> properties = pfr.readFile(filename);

                    for (int i = 1; i < 16; i++)
                    {
                        boyCurrent.setColor(i, new SnesColor(Int32.Parse(properties["boyR" + i]), Int32.Parse(properties["boyG" + i]), Int32.Parse(properties["boyB" + i])));
                        girlCurrent.setColor(i, new SnesColor(Int32.Parse(properties["girlR" + i]), Int32.Parse(properties["girlG" + i]), Int32.Parse(properties["girlB" + i])));
                        spriteCurrent.setColor(i, new SnesColor(Int32.Parse(properties["spriteR" + i]), Int32.Parse(properties["spriteG" + i]), Int32.Parse(properties["spriteB" + i])));
                    }
                }
                catch (Exception)
                {
                    await DialogUtil.ShowMessage(this, "Error", "Unable to read from file.  Verify all properties exist!");
                }
            }

            comboBox1_SelectedIndexChanged(null, null);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            // swap r/g
            bool allColors = radioButton2.IsChecked == true;

            SnesPalette currentPal = paletteControl1.pal;

            if (allColors)
            {
                for (int i = 1; i < 16; i++)
                {
                    SnesColor col = currentPal.getColor(i);
                    byte r = currentPal.getColor(i).getRed();
                    col.setRed(col.getGreen());
                    col.setGreen(r);
                    currentPal.setColor(i, col);
                }
            }
            else
            {
                int i = paletteControl1.selectedIndex;
                SnesColor col = currentPal.getColor(i);
                byte r = currentPal.getColor(i).getRed();
                col.setRed(col.getGreen());
                col.setGreen(r);
                currentPal.setColor(i, col);
            }

            paletteControl1.setPalette(currentPal);
            reDraw();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            // swap g/b
            bool allColors = radioButton2.IsChecked == true;

            SnesPalette currentPal = paletteControl1.pal;

            if (allColors)
            {
                for (int i = 1; i < 16; i++)
                {
                    SnesColor col = currentPal.getColor(i);
                    byte g = currentPal.getColor(i).getGreen();
                    col.setGreen(col.getBlue());
                    col.setBlue(g);
                    currentPal.setColor(i, col);
                }
            }
            else
            {
                int i = paletteControl1.selectedIndex;
                SnesColor col = currentPal.getColor(i);
                byte g = currentPal.getColor(i).getGreen();
                col.setGreen(col.getBlue());
                col.setBlue(g);
                currentPal.setColor(i, col);
            }

            paletteControl1.setPalette(currentPal);
            reDraw();
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            // swap r/b
            bool allColors = radioButton2.IsChecked == true;

            SnesPalette currentPal = paletteControl1.pal;

            if (allColors)
            {
                for (int i = 1; i < 16; i++)
                {
                    SnesColor col = currentPal.getColor(i);
                    byte r = currentPal.getColor(i).getRed();
                    col.setRed(col.getBlue());
                    col.setBlue(r);
                    currentPal.setColor(i, col);
                }
            }
            else
            {
                int i = paletteControl1.selectedIndex;
                SnesColor col = currentPal.getColor(i);
                byte r = currentPal.getColor(i).getRed();
                col.setRed(col.getBlue());
                col.setBlue(r);
                currentPal.setColor(i, col);
            }

            paletteControl1.setPalette(currentPal);
            reDraw();
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            List<int> indexes = new List<int>();
            int amount = (int)numericUpDown1.Value;
            if (radioButton2.IsChecked == true)
            {
                for (int i = 1; i < 16; i++)
                {
                    indexes.Add(i);
                }
            }
            else
            {
                indexes.Add(paletteControl1.selectedIndex);
            }

            SnesPalette currentPal = paletteControl1.pal;

            foreach (int i in indexes)
            {
                SnesColor col = currentPal.getColor(i);
                double h, s, v;
                ColorUtil.rgbToHsv(col.getRed(), col.getGreen(), col.getBlue(), out h, out s, out v);
                v += (amount / 100.0) * v;
                int r, g, b;
                ColorUtil.hsvToRgb(h, s, v, out r, out g, out b);
                SnesColor newCol = new SnesColor(r, g, b);
                currentPal.setColor(i, newCol);
            }

            paletteControl1.setPalette(currentPal);
            reDraw();
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            List<int> indexes = new List<int>();
            int amount = (int)numericUpDown2.Value;
            if (radioButton2.IsChecked == true)
            {
                for (int i = 1; i < 16; i++)
                {
                    indexes.Add(i);
                }
            }
            else
            {
                indexes.Add(paletteControl1.selectedIndex);
            }

            SnesPalette currentPal = paletteControl1.pal;

            foreach (int i in indexes)
            {
                SnesColor col = currentPal.getColor(i);
                double h, s, v;
                ColorUtil.rgbToHsv(col.getRed(), col.getGreen(), col.getBlue(), out h, out s, out v);
                s += (amount / 100.0) * s;
                int r, g, b;
                ColorUtil.hsvToRgb(h, s, v, out r, out g, out b);
                SnesColor newCol = new SnesColor(r, g, b);
                currentPal.setColor(i, newCol);
            }

            paletteControl1.setPalette(currentPal);
            reDraw();
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            List<int> indexes = new List<int>();
            int amount = (int)numericUpDown3.Value;
            if (radioButton2.IsChecked == true)
            {
                for (int i = 1; i < 16; i++)
                {
                    indexes.Add(i);
                }
            }
            else
            {
                indexes.Add(paletteControl1.selectedIndex);
            }

            SnesPalette currentPal = paletteControl1.pal;

            foreach (int i in indexes)
            {
                SnesColor col = currentPal.getColor(i);
                double h, s, v;
                ColorUtil.rgbToHsv(col.getRed(), col.getGreen(), col.getBlue(), out h, out s, out v);
                h += (amount / 100.0) * h;
                int r, g, b;
                ColorUtil.hsvToRgb(h, s, v, out r, out g, out b);
                SnesColor newCol = new SnesColor(r, g, b);
                currentPal.setColor(i, newCol);
            }

            paletteControl1.setPalette(currentPal);
            reDraw();
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            boyCurrent.copyFrom(boyDefault);
            girlCurrent.copyFrom(girlDefault);
            spriteCurrent.copyFrom(spriteDefault);
            comboBox1_SelectedIndexChanged(null, null);
        }

        private async void button10_Click(object sender, RoutedEventArgs e)
        {
            TopLevel top = TopLevel.GetTopLevel(this);
            if (top == null)
            {
                return;
            }
            IReadOnlyList<IStorageFile> files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Import this character from ROM...",
                AllowMultiple = false,
            });
            string filename = files.Count > 0 ? files[0].TryGetLocalPath() : null;
            int charIndex = comboBox1.SelectedIndex;
            if (filename != null)
            {
                byte[] thisRom = File.ReadAllBytes(filename);
                byte[] palData = new byte[32];
                for (int i = 0; i < 32; i++)
                {
                    palData[i] = thisRom[0x80FFE + (0x80 + charIndex) * 0x1E + i];
                }
                if (charIndex == 0)
                {
                    boyCurrent = new SnesPalette(palData);
                }
                else if (charIndex == 1)
                {
                    girlCurrent = new SnesPalette(palData);
                }
                else if (charIndex == 2)
                {
                    spriteCurrent = new SnesPalette(palData);
                }
                reDraw();
            }
        }
    }
}
