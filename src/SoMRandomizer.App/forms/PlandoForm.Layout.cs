#nullable disable
// Generated from the original WinForms PlandoForm.Designer.cs layout.
// Kept as code (like MainForm) instead of a hand-written XAML file so the
// absolute WinForms coordinates can be reproduced 1:1.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using SoMRandomizer.config.settings;
using SoMRandomizer.processing.common;

namespace SoMRandomizer.forms
{
    public partial class PlandoForm
    {
        private static Bitmap infoIcon;

        private static Bitmap loadInfoIcon()
        {
            if (infoIcon == null)
            {
                using (Stream stream = typeof(CommonSettings).Assembly.GetManifestResourceStream("SoMRandomizer.Resources.plando_info.png"))
                {
                    if (stream != null)
                    {
                        infoIcon = new Bitmap(stream);
                    }
                }
            }
            return infoIcon;
        }

        private static void addTo(Canvas canvas, Control control, double x, double y)
        {
            Canvas.SetLeft(control, x);
            Canvas.SetTop(control, y);
            canvas.Children.Add(control);
        }

        private class AutoSizeCanvas : Canvas
        {
            protected override Size MeasureOverride(Size availableSize)
            {
                Size baseSize = base.MeasureOverride(availableSize);
                double maxWidth = baseSize.Width;
                double maxHeight = baseSize.Height;
                foreach (Control child in Children)
                {
                    child.Measure(Size.Infinity);
                    double right = Canvas.GetLeft(child) + child.DesiredSize.Width;
                    double bottom = Canvas.GetTop(child) + child.DesiredSize.Height;
                    if (right > maxWidth) { maxWidth = right; }
                    if (bottom > maxHeight) { maxHeight = bottom; }
                }
                return new Size(maxWidth, maxHeight);
            }
        }

        private static Button makeSmallButton(string text)
        {
            return new Button { Content = text, Padding = new Thickness(6, 0, 6, 0), Height = 23 };
        }

        /// <summary>
        /// The tab pages are still absolute canvases (see the note in InitializeComponent);
        /// wrapping them in a scroll viewer means the content is never clipped if the
        /// window is too small for the full 373x452 page.
        /// </summary>
        private static ScrollViewer wrapTabContent(Control content)
        {
            return new ScrollViewer
            {
                Content = new Border
                {
                    Child = content,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding = new Thickness(0, 6, 0, 6),
                },
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            };
        }

        private void InitializeComponent()
        {
            // Flow layout: the original WinForms window was a fixed 400x518 canvas
            // with coordinates tuned for 8.25pt MS Sans Serif.  Keeping the inner
            // canvases absolute (they are static grids of controls) but hosting
            // them in a docked TabControl avoids the clipped content and wrapped
            // tab headers we got from copying the WinForms geometry verbatim.
            this.rootPanel = new DockPanel { Margin = new Thickness(12, 8, 12, 10) };
            this.Content = this.rootPanel;

            this.label1 = new TextBlock { Text = "Location", FontSize = 13 };
            this.panel1 = new ScrollViewer { Width = 420, Height = 403, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
            this.panel1Content = new AutoSizeCanvas();
            this.panel1.Content = this.panel1Content;
            this.panel2 = new ScrollViewer { Width = 420, Height = 403, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
            this.panel2Content = new AutoSizeCanvas();
            this.panel2.Content = this.panel2Content;
            this.panel3 = new ScrollViewer { Width = 420, Height = 403, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
            this.panel3Content = new AutoSizeCanvas();
            this.panel3.Content = this.panel3Content;
            this.button2 = makeSmallButton("Set");
            this.button2.IsVisible = false;
            this.button1 = makeSmallButton("Add");
            this.label2 = new TextBlock { Text = "Reward", FontSize = 13 };
            this.tabControl1 = new TabControl();
            this.tabPage1 = new TabItem { Header = "Prizes" };
            this.tabPage1Content = new Canvas { Width = 430, Height = 452 };
            this.tabPage1.Content = wrapTabContent(this.tabPage1Content);
            this.tabPage2 = new TabItem { Header = "Spell Orbs" };
            this.tabPage2Content = new Canvas { Width = 430, Height = 452 };
            this.tabPage2.Content = wrapTabContent(this.tabPage2Content);
            this.tabPage3 = new TabItem { Header = "Characters" };
            this.tabPage3Content = new Canvas { Width = 430, Height = 452 };
            this.tabPage3.Content = wrapTabContent(this.tabPage3Content);
            this.tabPage5 = new TabItem { Header = "Bosses" };
            this.tabPage5Content = new Canvas { Width = 430, Height = 452 };
            this.tabPage5.Content = wrapTabContent(this.tabPage5Content);
            this.tabPage6 = new TabItem { Header = "Boss Elements" };
            this.tabPage6Content = new Canvas { Width = 430, Height = 452 };
            this.tabPage6.Content = wrapTabContent(this.tabPage6Content);
            this.tabPage4 = new TabItem { Header = "Misc" };
            this.tabPage4Content = new Canvas { Width = 430, Height = 452 };
            this.tabPage4.Content = wrapTabContent(this.tabPage4Content);
            this.tabControl1.Items.Add(this.tabPage1);
            this.tabControl1.Items.Add(this.tabPage2);
            this.tabControl1.Items.Add(this.tabPage3);
            this.tabControl1.Items.Add(this.tabPage5);
            this.tabControl1.Items.Add(this.tabPage6);
            this.tabControl1.Items.Add(this.tabPage4);
            this.tabControl1.SelectedIndex = 0;

            this.pictureBox3 = new Image { Source = loadInfoIcon(), Width = 16, Height = 16 };
            this.button19 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button20 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button21 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label19 = new TextBlock { Text = "Fire Palace 3:" };
            this.label20 = new TextBlock { Text = "Fire Palace 2:" };
            this.label21 = new TextBlock { Text = "Fire Palace 1:" };
            this.pictureBox1 = new Image { Source = loadInfoIcon(), Width = 16, Height = 16 };
            this.button18 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label18 = new TextBlock { Text = "Upper Land:" };
            this.button17 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button16 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button15 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button14 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button13 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button12 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button11 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button10 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button9 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button3 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label12 = new TextBlock { Text = "Grand Palace 7:" };
            this.label11 = new TextBlock { Text = "Grand Palace 6:" };
            this.label10 = new TextBlock { Text = "Grand Palace 5:" };
            this.label9 = new TextBlock { Text = "Grand Palace 4:" };
            this.label8 = new TextBlock { Text = "Grand Palace 3:" };
            this.label7 = new TextBlock { Text = "Grand Palace 2:" };
            this.label6 = new TextBlock { Text = "Grand Palace 1:" };
            this.label5 = new TextBlock { Text = "Luna Palace:" };
            this.label4 = new TextBlock { Text = "Matango Cave:" };
            this.label3 = new TextBlock { Text = "Earth Palace:" };
            this.button8 = new Button { Content = "(No change)", IsVisible = false, Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button7 = new Button { Content = "(No change)", IsVisible = false, Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button6 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button5 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.button4 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.pictureBox11 = new Image { Source = loadInfoIcon(), Width = 16, Height = 16 };
            this.label17 = new TextBlock { Text = "Other char 2 weapon:", IsVisible = false };
            this.label16 = new TextBlock { Text = "Other char 1 weapon:", IsVisible = false };
            this.label15 = new TextBlock { Text = "Sprite weapon:" };
            this.label14 = new TextBlock { Text = "Girl weapon:" };
            this.label13 = new TextBlock { Text = "Boy weapon:" };
            this.button22 = new Button { Content = "Oops, all...", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.pictureBox5 = new Image { Source = loadInfoIcon(), Width = 16, Height = 16 };
            this.label24 = new TextBlock { Text = "Boss", FontSize = 13 };
            this.label23 = new TextBlock { Text = "Location", FontSize = 13 };
            this.label59 = new TextBlock { Text = "Dark Lich" };
            this.button89 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label58 = new TextBlock { Text = "Buffy" };
            this.button87 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label57 = new TextBlock { Text = "Blue Dragon" };
            this.button85 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label56 = new TextBlock { Text = "Thunder Gigas" };
            this.button83 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label55 = new TextBlock { Text = "Fire Dragon" };
            this.button81 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label54 = new TextBlock { Text = "Axe Beak" };
            this.button79 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label53 = new TextBlock { Text = "Snow Dragon" };
            this.button77 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label52 = new TextBlock { Text = "Dragon Worm" };
            this.button75 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label51 = new TextBlock { Text = "Mech Rider 3" };
            this.button73 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label50 = new TextBlock { Text = "Hexas" };
            this.button71 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label49 = new TextBlock { Text = "Snap Dragon" };
            this.button69 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label48 = new TextBlock { Text = "Watermelon" };
            this.button67 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label47 = new TextBlock { Text = "Kettle Kin" };
            this.button65 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label46 = new TextBlock { Text = "Triple Tonpole", IsVisible = false };
            this.button63 = new Button { Content = "(No change)", IsVisible = false, Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label45 = new TextBlock { Text = "Kilroy" };
            this.button61 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label44 = new TextBlock { Text = "Hydra" };
            this.button59 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label43 = new TextBlock { Text = "Gorgon Bull" };
            this.button57 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label42 = new TextBlock { Text = "Blue Spike" };
            this.button55 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label41 = new TextBlock { Text = "." };
            this.label40 = new TextBlock { Text = "Mech Rider 2" };
            this.button53 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label39 = new TextBlock { Text = "Metal Mantis" };
            this.button51 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label38 = new TextBlock { Text = "Vampire" };
            this.button49 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label37 = new TextBlock { Text = "Doom\'s Wall" };
            this.button47 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label36 = new TextBlock { Text = "Wall Face" };
            this.button45 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label35 = new TextBlock { Text = "Minotaur" };
            this.button43 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label34 = new TextBlock { Text = "Frost Gigas" };
            this.button41 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label33 = new TextBlock { Text = "Boreal Face" };
            this.button39 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label32 = new TextBlock { Text = "Great Viper" };
            this.button37 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label31 = new TextBlock { Text = "Spring Beak" };
            this.button35 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label30 = new TextBlock { Text = "Fire Gigas" };
            this.button33 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label29 = new TextBlock { Text = "Spikey Tiger" };
            this.button31 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label28 = new TextBlock { Text = "Jabberwocky" };
            this.button29 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label27 = new TextBlock { Text = "Tonpole" };
            this.button27 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label26 = new TextBlock { Text = "Tropicallo" };
            this.button25 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label25 = new TextBlock { Text = "Mantis Ant" };
            this.button23 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.pictureBox7 = new Image { Source = loadInfoIcon(), Width = 16, Height = 16 };
            this.label61 = new TextBlock { Text = "Element", FontSize = 13 };
            this.label62 = new TextBlock { Text = "Boss", FontSize = 13 };
            this.label63 = new TextBlock { Text = "Dark Lich" };
            this.button90 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label64 = new TextBlock { Text = "Buffy" };
            this.button92 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label65 = new TextBlock { Text = "Blue Dragon" };
            this.button94 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label66 = new TextBlock { Text = "Thunder Gigas" };
            this.button96 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label67 = new TextBlock { Text = "Fire Dragon" };
            this.button98 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label68 = new TextBlock { Text = "Axe Beak" };
            this.button100 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label69 = new TextBlock { Text = "Snow Dragon" };
            this.button102 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label70 = new TextBlock { Text = "Dragon Worm" };
            this.button104 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label71 = new TextBlock { Text = "Mech Rider 3" };
            this.button106 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label72 = new TextBlock { Text = "Hexas" };
            this.button108 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label73 = new TextBlock { Text = "Snap Dragon" };
            this.button110 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label74 = new TextBlock { Text = "Watermelon" };
            this.button112 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label75 = new TextBlock { Text = "Kettle Kin" };
            this.button114 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label76 = new TextBlock { Text = "Biting Lizard" };
            this.button116 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label77 = new TextBlock { Text = "Kilroy" };
            this.button118 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label78 = new TextBlock { Text = "Hydra" };
            this.button120 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label79 = new TextBlock { Text = "Gorgon Bull" };
            this.button122 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label80 = new TextBlock { Text = "Blue Spike" };
            this.button124 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label81 = new TextBlock { Text = "." };
            this.label82 = new TextBlock { Text = "Mech Rider 2" };
            this.button126 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label83 = new TextBlock { Text = "Metal Mantis" };
            this.button128 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label84 = new TextBlock { Text = "Vampire" };
            this.button130 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label85 = new TextBlock { Text = "Doom\'s Wall" };
            this.button132 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label86 = new TextBlock { Text = "Wall Face" };
            this.button134 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label87 = new TextBlock { Text = "Minotaur" };
            this.button136 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label88 = new TextBlock { Text = "Frost Gigas" };
            this.button138 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label89 = new TextBlock { Text = "Boreal Face" };
            this.button140 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label90 = new TextBlock { Text = "Great Viper" };
            this.button142 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label91 = new TextBlock { Text = "Spring Beak" };
            this.button144 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label92 = new TextBlock { Text = "Fire Gigas" };
            this.button146 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label93 = new TextBlock { Text = "Spikey Tiger" };
            this.button148 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label94 = new TextBlock { Text = "Jabberwocky" };
            this.button150 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label95 = new TextBlock { Text = "Tonpole" };
            this.button152 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label96 = new TextBlock { Text = "Tropicallo" };
            this.button154 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.label97 = new TextBlock { Text = "Mantis Ant" };
            this.button156 = new Button { Content = "(No change)", Padding = new Thickness(6, 0, 6, 0), Height = 23 };
            this.checkBox3 = new CheckBox { Content = "Bypass validation" };
            this.checkBox1 = new CheckBox { Content = "Obfuscate settings string", IsChecked = true };
            this.checkBox2 = new CheckBox { Content = "Enable Plandomizer" };
            this.pictureBox2 = new Image { Source = loadInfoIcon(), Width = 16, Height = 16 };

            addTo(this.panel1Content, this.button2, 264, 21);
            addTo(this.panel1Content, this.button1, 23, 21);
            addTo(this.tabPage1Content, this.panel1, 6, 39);
            addTo(this.tabPage1Content, this.label2, 279, 13);
            addTo(this.tabPage1Content, this.label1, 36, 11);
            addTo(this.tabPage2Content, this.pictureBox3, 132, 167);
            addTo(this.tabPage2Content, this.button19, 272, 192);
            addTo(this.tabPage2Content, this.button20, 272, 163);
            addTo(this.tabPage2Content, this.button21, 272, 134);
            addTo(this.tabPage2Content, this.label19, 18, 197);
            addTo(this.tabPage2Content, this.label20, 18, 168);
            addTo(this.tabPage2Content, this.label21, 18, 139);
            addTo(this.tabPage2Content, this.pictureBox1, 112, 51);
            addTo(this.tabPage2Content, this.button18, 272, 47);
            addTo(this.tabPage2Content, this.label18, 18, 52);
            addTo(this.tabPage2Content, this.button17, 272, 398);
            addTo(this.tabPage2Content, this.button16, 272, 367);
            addTo(this.tabPage2Content, this.button15, 272, 337);
            addTo(this.tabPage2Content, this.button14, 272, 308);
            addTo(this.tabPage2Content, this.button13, 272, 279);
            addTo(this.tabPage2Content, this.button12, 272, 250);
            addTo(this.tabPage2Content, this.button11, 272, 221);
            addTo(this.tabPage2Content, this.button10, 272, 105);
            addTo(this.tabPage2Content, this.button9, 272, 76);
            addTo(this.tabPage2Content, this.button3, 272, 19);
            addTo(this.tabPage2Content, this.label12, 18, 403);
            addTo(this.tabPage2Content, this.label11, 18, 372);
            addTo(this.tabPage2Content, this.label10, 18, 342);
            addTo(this.tabPage2Content, this.label9, 18, 313);
            addTo(this.tabPage2Content, this.label8, 18, 284);
            addTo(this.tabPage2Content, this.label7, 18, 255);
            addTo(this.tabPage2Content, this.label6, 18, 226);
            addTo(this.tabPage2Content, this.label5, 18, 110);
            addTo(this.tabPage2Content, this.label4, 18, 81);
            addTo(this.tabPage2Content, this.label3, 18, 24);
            addTo(this.tabPage3Content, this.button8, 277, 190);
            addTo(this.tabPage3Content, this.button7, 277, 149);
            addTo(this.tabPage3Content, this.button6, 277, 109);
            addTo(this.tabPage3Content, this.button5, 277, 71);
            addTo(this.tabPage3Content, this.button4, 277, 33);
            addTo(this.tabPage3Content, this.pictureBox11, 16, 10);
            addTo(this.tabPage3Content, this.label17, 16, 195);
            addTo(this.tabPage3Content, this.label16, 16, 154);
            addTo(this.tabPage3Content, this.label15, 16, 114);
            addTo(this.tabPage3Content, this.label14, 16, 76);
            addTo(this.tabPage3Content, this.label13, 16, 38);
            addTo(this.tabPage5Content, this.button22, 137, 7);
            addTo(this.tabPage5Content, this.pictureBox5, 294, 11);
            addTo(this.tabPage5Content, this.label24, 252, 10);
            addTo(this.tabPage5Content, this.label23, 36, 11);
            addTo(this.tabPage5Content, this.panel2, 6, 39);
            addTo(this.panel2Content, this.label59, 31, 950);
            addTo(this.panel2Content, this.button89, 205, 945);
            addTo(this.panel2Content, this.label58, 38, 921);
            addTo(this.panel2Content, this.button87, 205, 916);
            addTo(this.panel2Content, this.label57, 29, 892);
            addTo(this.panel2Content, this.button85, 205, 887);
            addTo(this.panel2Content, this.label56, 22, 863);
            addTo(this.panel2Content, this.button83, 205, 858);
            addTo(this.panel2Content, this.label55, 29, 834);
            addTo(this.panel2Content, this.button81, 205, 829);
            addTo(this.panel2Content, this.label54, 33, 805);
            addTo(this.panel2Content, this.button79, 205, 800);
            addTo(this.panel2Content, this.label53, 25, 776);
            addTo(this.panel2Content, this.button77, 205, 771);
            addTo(this.panel2Content, this.label52, 25, 747);
            addTo(this.panel2Content, this.button75, 205, 742);
            addTo(this.panel2Content, this.label51, 25, 718);
            addTo(this.panel2Content, this.button73, 205, 713);
            addTo(this.panel2Content, this.label50, 40, 689);
            addTo(this.panel2Content, this.button71, 205, 684);
            addTo(this.panel2Content, this.label49, 27, 631);
            addTo(this.panel2Content, this.button69, 205, 626);
            addTo(this.panel2Content, this.label48, 29, 660);
            addTo(this.panel2Content, this.button67, 205, 655);
            addTo(this.panel2Content, this.label47, 34, 602);
            addTo(this.panel2Content, this.button65, 205, 597);
            addTo(this.panel2Content, this.label46, 113, 296);
            addTo(this.panel2Content, this.button63, 97, 312);
            addTo(this.panel2Content, this.label45, 43, 167);
            addTo(this.panel2Content, this.button61, 205, 162);
            addTo(this.panel2Content, this.label44, 41, 573);
            addTo(this.panel2Content, this.button59, 205, 568);
            addTo(this.panel2Content, this.label43, 33, 544);
            addTo(this.panel2Content, this.button57, 205, 539);
            addTo(this.panel2Content, this.label42, 33, 515);
            addTo(this.panel2Content, this.button55, 205, 510);
            addTo(this.panel2Content, this.label41, 14, 973);
            addTo(this.panel2Content, this.label40, 27, 486);
            addTo(this.panel2Content, this.button53, 205, 481);
            addTo(this.panel2Content, this.label39, 27, 457);
            addTo(this.panel2Content, this.button51, 205, 452);
            addTo(this.panel2Content, this.label38, 36, 428);
            addTo(this.panel2Content, this.button49, 205, 423);
            addTo(this.panel2Content, this.label37, 26, 399);
            addTo(this.panel2Content, this.button47, 205, 394);
            addTo(this.panel2Content, this.label36, 32, 196);
            addTo(this.panel2Content, this.button45, 205, 191);
            addTo(this.panel2Content, this.label35, 33, 370);
            addTo(this.panel2Content, this.button43, 205, 365);
            addTo(this.panel2Content, this.label34, 30, 341);
            addTo(this.panel2Content, this.button41, 205, 336);
            addTo(this.panel2Content, this.label33, 27, 312);
            addTo(this.panel2Content, this.button39, 205, 307);
            addTo(this.panel2Content, this.label32, 30, 283);
            addTo(this.panel2Content, this.button37, 205, 278);
            addTo(this.panel2Content, this.label31, 29, 254);
            addTo(this.panel2Content, this.button35, 205, 249);
            addTo(this.panel2Content, this.label30, 33, 138);
            addTo(this.panel2Content, this.button33, 205, 133);
            addTo(this.panel2Content, this.label29, 28, 80);
            addTo(this.panel2Content, this.button31, 205, 75);
            addTo(this.panel2Content, this.label28, 26, 225);
            addTo(this.panel2Content, this.button29, 205, 220);
            addTo(this.panel2Content, this.label27, 35, 109);
            addTo(this.panel2Content, this.button27, 205, 104);
            addTo(this.panel2Content, this.label26, 32, 51);
            addTo(this.panel2Content, this.button25, 205, 46);
            addTo(this.panel2Content, this.label25, 32, 22);
            addTo(this.panel2Content, this.button23, 205, 17);
            addTo(this.tabPage6Content, this.pictureBox7, 297, 12);
            addTo(this.tabPage6Content, this.label61, 234, 11);
            addTo(this.tabPage6Content, this.label62, 42, 11);
            addTo(this.tabPage6Content, this.panel3, 6, 39);
            addTo(this.panel3Content, this.label63, 31, 979);
            addTo(this.panel3Content, this.button90, 227, 974);
            addTo(this.panel3Content, this.label64, 38, 950);
            addTo(this.panel3Content, this.button92, 227, 945);
            addTo(this.panel3Content, this.label65, 29, 921);
            addTo(this.panel3Content, this.button94, 227, 916);
            addTo(this.panel3Content, this.label66, 22, 892);
            addTo(this.panel3Content, this.button96, 227, 887);
            addTo(this.panel3Content, this.label67, 29, 863);
            addTo(this.panel3Content, this.button98, 227, 858);
            addTo(this.panel3Content, this.label68, 33, 834);
            addTo(this.panel3Content, this.button100, 227, 829);
            addTo(this.panel3Content, this.label69, 25, 805);
            addTo(this.panel3Content, this.button102, 227, 800);
            addTo(this.panel3Content, this.label70, 25, 776);
            addTo(this.panel3Content, this.button104, 227, 771);
            addTo(this.panel3Content, this.label71, 25, 747);
            addTo(this.panel3Content, this.button106, 227, 742);
            addTo(this.panel3Content, this.label72, 40, 718);
            addTo(this.panel3Content, this.button108, 227, 713);
            addTo(this.panel3Content, this.label73, 27, 660);
            addTo(this.panel3Content, this.button110, 227, 655);
            addTo(this.panel3Content, this.label74, 29, 689);
            addTo(this.panel3Content, this.button112, 227, 684);
            addTo(this.panel3Content, this.label75, 34, 631);
            addTo(this.panel3Content, this.button114, 227, 626);
            addTo(this.panel3Content, this.label76, 26, 138);
            addTo(this.panel3Content, this.button116, 227, 133);
            addTo(this.panel3Content, this.label77, 43, 196);
            addTo(this.panel3Content, this.button118, 227, 191);
            addTo(this.panel3Content, this.label78, 41, 602);
            addTo(this.panel3Content, this.button120, 227, 597);
            addTo(this.panel3Content, this.label79, 33, 573);
            addTo(this.panel3Content, this.button122, 227, 568);
            addTo(this.panel3Content, this.label80, 33, 544);
            addTo(this.panel3Content, this.button124, 227, 539);
            addTo(this.panel3Content, this.label81, 14, 1002);
            addTo(this.panel3Content, this.label82, 27, 515);
            addTo(this.panel3Content, this.button126, 227, 510);
            addTo(this.panel3Content, this.label83, 27, 486);
            addTo(this.panel3Content, this.button128, 227, 481);
            addTo(this.panel3Content, this.label84, 36, 457);
            addTo(this.panel3Content, this.button130, 227, 452);
            addTo(this.panel3Content, this.label85, 26, 428);
            addTo(this.panel3Content, this.button132, 227, 423);
            addTo(this.panel3Content, this.label86, 32, 225);
            addTo(this.panel3Content, this.button134, 227, 220);
            addTo(this.panel3Content, this.label87, 33, 399);
            addTo(this.panel3Content, this.button136, 227, 394);
            addTo(this.panel3Content, this.label88, 30, 370);
            addTo(this.panel3Content, this.button138, 227, 365);
            addTo(this.panel3Content, this.label89, 27, 341);
            addTo(this.panel3Content, this.button140, 227, 336);
            addTo(this.panel3Content, this.label90, 30, 312);
            addTo(this.panel3Content, this.button142, 227, 307);
            addTo(this.panel3Content, this.label91, 29, 283);
            addTo(this.panel3Content, this.button144, 227, 278);
            addTo(this.panel3Content, this.label92, 33, 167);
            addTo(this.panel3Content, this.button146, 227, 162);
            addTo(this.panel3Content, this.label93, 28, 80);
            addTo(this.panel3Content, this.button148, 227, 75);
            addTo(this.panel3Content, this.label94, 26, 254);
            addTo(this.panel3Content, this.button150, 227, 249);
            addTo(this.panel3Content, this.label95, 35, 109);
            addTo(this.panel3Content, this.button152, 227, 104);
            addTo(this.panel3Content, this.label96, 32, 51);
            addTo(this.panel3Content, this.button154, 227, 46);
            addTo(this.panel3Content, this.label97, 32, 22);
            addTo(this.panel3Content, this.button156, 227, 17);
            addTo(this.tabPage4Content, this.checkBox3, 18, 44);
            addTo(this.tabPage4Content, this.checkBox1, 18, 18);

            StackPanel headerRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center,
            };
            headerRow.Children.Add(this.checkBox2);
            headerRow.Children.Add(this.pictureBox2);
            DockPanel.SetDock(headerRow, Dock.Top);
            this.rootPanel.Children.Add(headerRow);

            this.tabControl1.Margin = new Thickness(0, 10, 0, 0);
            this.tabControl1.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.tabControl1.VerticalAlignment = VerticalAlignment.Stretch;
            this.rootPanel.Children.Add(this.tabControl1);

            this.checkBox3.IsCheckedChanged += this.checkBox3_CheckedChanged;
            this.checkBox1.IsCheckedChanged += this.checkBox1_CheckedChanged;
            this.checkBox2.IsCheckedChanged += this.checkBox2_CheckedChanged;
            this.tabControl1.IsEnabled = false;
        }

        private DockPanel rootPanel;
        private TextBlock label1;
        private ScrollViewer panel1;
        private Canvas panel1Content;
        private Button button2;
        private Button button1;
        private TextBlock label2;
        private TabControl tabControl1;
        private TabItem tabPage1;
        private Canvas tabPage1Content;
        private TabItem tabPage2;
        private Canvas tabPage2Content;
        private TabItem tabPage3;
        private Canvas tabPage3Content;
        private TabItem tabPage5;
        private Canvas tabPage5Content;
        private TabItem tabPage6;
        private Canvas tabPage6Content;
        private TabItem tabPage4;
        private Canvas tabPage4Content;
        private Image pictureBox3;
        private Button button19;
        private Button button20;
        private Button button21;
        private TextBlock label19;
        private TextBlock label20;
        private TextBlock label21;
        private Image pictureBox1;
        private Button button18;
        private TextBlock label18;
        private Button button17;
        private Button button16;
        private Button button15;
        private Button button14;
        private Button button13;
        private Button button12;
        private Button button11;
        private Button button10;
        private Button button9;
        private Button button3;
        private TextBlock label12;
        private TextBlock label11;
        private TextBlock label10;
        private TextBlock label9;
        private TextBlock label8;
        private TextBlock label7;
        private TextBlock label6;
        private TextBlock label5;
        private TextBlock label4;
        private TextBlock label3;
        private Button button8;
        private Button button7;
        private Button button6;
        private Button button5;
        private Button button4;
        private Image pictureBox11;
        private TextBlock label17;
        private TextBlock label16;
        private TextBlock label15;
        private TextBlock label14;
        private TextBlock label13;
        private Button button22;
        private Image pictureBox5;
        private TextBlock label24;
        private TextBlock label23;
        private ScrollViewer panel2;
        private Canvas panel2Content;
        private TextBlock label59;
        private Button button89;
        private TextBlock label58;
        private Button button87;
        private TextBlock label57;
        private Button button85;
        private TextBlock label56;
        private Button button83;
        private TextBlock label55;
        private Button button81;
        private TextBlock label54;
        private Button button79;
        private TextBlock label53;
        private Button button77;
        private TextBlock label52;
        private Button button75;
        private TextBlock label51;
        private Button button73;
        private TextBlock label50;
        private Button button71;
        private TextBlock label49;
        private Button button69;
        private TextBlock label48;
        private Button button67;
        private TextBlock label47;
        private Button button65;
        private TextBlock label46;
        private Button button63;
        private TextBlock label45;
        private Button button61;
        private TextBlock label44;
        private Button button59;
        private TextBlock label43;
        private Button button57;
        private TextBlock label42;
        private Button button55;
        private TextBlock label41;
        private TextBlock label40;
        private Button button53;
        private TextBlock label39;
        private Button button51;
        private TextBlock label38;
        private Button button49;
        private TextBlock label37;
        private Button button47;
        private TextBlock label36;
        private Button button45;
        private TextBlock label35;
        private Button button43;
        private TextBlock label34;
        private Button button41;
        private TextBlock label33;
        private Button button39;
        private TextBlock label32;
        private Button button37;
        private TextBlock label31;
        private Button button35;
        private TextBlock label30;
        private Button button33;
        private TextBlock label29;
        private Button button31;
        private TextBlock label28;
        private Button button29;
        private TextBlock label27;
        private Button button27;
        private TextBlock label26;
        private Button button25;
        private TextBlock label25;
        private Button button23;
        private Image pictureBox7;
        private TextBlock label61;
        private TextBlock label62;
        private ScrollViewer panel3;
        private Canvas panel3Content;
        private TextBlock label63;
        private Button button90;
        private TextBlock label64;
        private Button button92;
        private TextBlock label65;
        private Button button94;
        private TextBlock label66;
        private Button button96;
        private TextBlock label67;
        private Button button98;
        private TextBlock label68;
        private Button button100;
        private TextBlock label69;
        private Button button102;
        private TextBlock label70;
        private Button button104;
        private TextBlock label71;
        private Button button106;
        private TextBlock label72;
        private Button button108;
        private TextBlock label73;
        private Button button110;
        private TextBlock label74;
        private Button button112;
        private TextBlock label75;
        private Button button114;
        private TextBlock label76;
        private Button button116;
        private TextBlock label77;
        private Button button118;
        private TextBlock label78;
        private Button button120;
        private TextBlock label79;
        private Button button122;
        private TextBlock label80;
        private Button button124;
        private TextBlock label81;
        private TextBlock label82;
        private Button button126;
        private TextBlock label83;
        private Button button128;
        private TextBlock label84;
        private Button button130;
        private TextBlock label85;
        private Button button132;
        private TextBlock label86;
        private Button button134;
        private TextBlock label87;
        private Button button136;
        private TextBlock label88;
        private Button button138;
        private TextBlock label89;
        private Button button140;
        private TextBlock label90;
        private Button button142;
        private TextBlock label91;
        private Button button144;
        private TextBlock label92;
        private Button button146;
        private TextBlock label93;
        private Button button148;
        private TextBlock label94;
        private Button button150;
        private TextBlock label95;
        private Button button152;
        private TextBlock label96;
        private Button button154;
        private TextBlock label97;
        private Button button156;
        private CheckBox checkBox3;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private Image pictureBox2;
    }
}
