#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using SoMRandomizer.config;
using SoMRandomizer.config.settings;
using SoMRandomizer.config.ui;
using SoMRandomizer.help;
using SoMRandomizer.processing.ancientcave;
using SoMRandomizer.processing.bossrush;
using SoMRandomizer.processing.chaos;
using SoMRandomizer.processing.common;
using SoMRandomizer.processing.common.structure;
using SoMRandomizer.processing.openworld;
using SoMRandomizer.processing.vanillarando;
using SoMRandomizer.util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace SoMRandomizer.forms
{
    /// <summary>
    /// Avalonia port of the WinForms MainForm.
    /// </summary>
    /// <remarks>Author: Moppleton</remarks>
    public partial class MainForm : Window, UiOptionsManager.OptionsListener
    {
        // generators to call out to to make rando roms
        private AncientCaveGenerator ancientCaveGenerator = new AncientCaveGenerator();
        private BossRushGenerator bossRushGenerator = new BossRushGenerator();
        private ChaosGenerator chaosGenerator = new ChaosGenerator();
        private VanillaRandoGenerator vanillaRandoGenerator = new VanillaRandoGenerator();
        private OpenWorldGenerator openWorldGenerator = new OpenWorldGenerator();
        private Dictionary<string, RomGenerator> generatorsByRomType = new Dictionary<string, RomGenerator>();

        // open world forms
        private PresetsForm openWorldPresetsForm;
        private PlandoForm plandoForm;

        // character palette form
        private CharacterDesigner characterDesigner = new CharacterDesigner();

        // components that get disabled while we're generating
        private List<Control> enableComponents = new List<Control>();

        // difficulty stuff for older modes
        private Dictionary<string, Dictionary<string, DifficultySettings>> difficulties = new Dictionary<string, Dictionary<string, DifficultySettings>>();
        private Dictionary<string, DifficultySettings> customDifficulty = new Dictionary<string, DifficultySettings>();
        private Dictionary<string, int> visOptions = new Dictionary<string, int>();
        private string selectedDifficulty = "Casual";
        private bool difficultyPopout = false;

        // width of the form for when we are and are not showing the difficulty adjustment thing for the older modes
        private const int noDifficultyWidth = 700;
        private const int difficultyWidth = 1250;
        private const int modePanelWidth = 660;

        // true if currently making a rom
        private bool generating = false;

        // state booleans for updating properties display
        private bool handleOptionChanges = true;
        private bool handleIndividualChanges = true;

        // options forms & handlers
        private UiOptionsManager optionsManager = new UiOptionsManager();
        private OptionsFormManager propertyManager;
        private OptionsCategoryForm generalOptionsCategory;
        private OptionsCategoryForm openWorldOptionsCategory;

        // settings for each mode, and common settings
        private CommonSettings commonSettings;
        private VanillaRandoSettings vanillaRandoSettings;
        private OpenWorldSettings openWorldSettings;
        private AncientCaveSettings ancientCaveSettings;
        private BossRushSettings bossRushSettings;
        private ChaosSettings chaosSettings;

        // settings mapped by mode
        private Dictionary<string, RandoSettings> settingsByRomType = new Dictionary<string, RandoSettings>();

        // --- controls ---
        private TextBox txtSeed;
        private TextBox txtOutputROM;
        private TextBox txtInputROM;
        private TextBox txtOptions;
        private Button btnRandomSeed;
        private Button btnWriteROM;
        private Button btnBrowseOutput;
        private Button btnBrowseInput;
        private Button btnOptionsCopy;
        private Button btnOptionsPaste;
        private Button btnGeneralOptions;
        private Button btnChoasMoreOptions;
        private Button btnBossRushMoreOptions;
        private Button btnOpenMoreOptions;
        private Button btnPresets;
        private Button btnPlando;
        private Button btnDifficultyLoad;
        private Button btnDifficultySave;
        private Button picCatBread;
        private Button picDiscord;
        private Button picTwitch;
        private Button picBlogspot;
        private Button picMail;
        private Button picVanillaDetails;
        private Button picACDetails;
        private Button picBossRushDetails;
        private Button picChaosDetails;
        private Button picOpenDetails;
        private Button llbNewVersion;
        private Button llbDifficultyDetails;

        private CheckBox chkAppendSeedToFilename;
        private CheckBox chkACProfanityFilter;
        private CheckBox chkACRandomMusic;
        private CheckBox chkACIncludeBoyChar;
        private CheckBox chkACIncludeGirlChar;
        private CheckBox chkACIncludeSpriteChar;
        private CheckBox chkBossRushIncludeBoyChar;
        private CheckBox chkBossRushIncludeGirlChar;
        private CheckBox chkBossRushIncludeSpriteChar;
        private CheckBox chkChaosIncludeBoyChar;
        private CheckBox chkChaosIncludeGirlChar;
        private CheckBox chkChaosIncludeSpriteChar;
        private CheckBox chkVanillaRandomizeBosses;
        private CheckBox chkVanillaRandomizeEnemies;
        private CheckBox chkVanillaRandomizeWeaponOrbs;
        private CheckBox chkVanillaRandomizeElements;
        private CheckBox chkVanillaRandomizeWeapons;
        private CheckBox chkVanillaRandomizeMusic;
        private CheckBox chkVanillaAutoSave;
        private CheckBox chkVanillaDialogCuts;
        private CheckBox chkVanillaPreserveEarlyBosses;
        private CheckBox chkOpenRandomizeWeapons;
        private CheckBox chkOpenRandomizeShops;
        private CheckBox chkOpenRandomizeMusic;
        private CheckBox chkOpenRandomMapColors;
        private CheckBox chkOpenFlammieDrumLogic;

        private ComboBox cmbACDifficulty;
        private ComboBox cmbACDialogSource;
        private ComboBox cmbACLength;
        private ComboBox cmbACBossFrequency;
        private ComboBox cmbBossRushDifficulty;
        private ComboBox cmbChaosDifficulty;
        private ComboBox cmbVanillaExpMultiplier;
        private ComboBox cmbVanillaGoldMultiplier;
        private ComboBox cmbVanillaStatusAilments;
        private ComboBox cmbVanillaSpecialMode;
        private ComboBox cmbOpenStatGrowth;
        private ComboBox cmbOpenStatGrowthDifficulty;
        private ComboBox cmbOpenOtherChars;
        private ComboBox cmbOpenStatusAilments;
        private ComboBox cmbOpenGoal;
        private ComboBox cmbOpenRandomizeEnemies;
        private ComboBox cmbOpenRandomizeBosses;
        private ComboBox cmbOpenLogicMode;
        private ComboBox cmbOpenStartingChar;
        private ComboBox cmbDifficultyValue;
        private ComboBox cmbDifficultyVisualize;
        private ComboBox cmbVisualizationOption;

        private Slider trbVanillaEnemyScaling;
        private Slider trbChaosBosses;
        private Slider trbChaosFloors;

        private NumericUpDown nudDifficultyExponent;
        private NumericUpDown nudVisualizationOptionValue;
        private NumericUpDown nudDifficultyLinearGrowth;
        private NumericUpDown nudDifficultyBaseValue;

        private RadioButton rdoChaosPalatteRidiculous;
        private RadioButton rdoChaosPalatteReasonable;
        private RadioButton rdoChaosPalatteNone;

        private ProgressBar prgGenerationProgress;
        private TabControl tabMode;
        private TabItem tpVanilla;
        private TabItem tpOpen;
        private TabItem tpAncientCave;
        private TabItem tpBossRush;
        private TabItem tpChaos;

        private Border groupBox1;
        private Border groupBox2;
        private Border groupBox3;
        private Border groupBox4;
        private Border groupBox5;
        private Border groupBox6;

        private CheckedListBox cklACBiomeTypes;

        private TextBlock label7;
        private TextBlock label12;
        private TextBlock label13;
        private TextBlock label16;
        private TextBlock label36;
        private TextBlock label47;
        private TextBlock lblVanillaNoRandoHint;
        private TextBlock noMusicPackLabel;

        private GraphControl picDifficultyGraph;

        // --- layout helpers ---
        //
        // The WinForms original was laid out with absolute coordinates tuned for
        // 8.25pt Microsoft Sans Serif.  Avalonia uses different font metrics and
        // control templates, so porting those coordinates verbatim produced
        // overlapping labels and clipped content.  The UI is therefore rebuilt
        // with flow-based panels (Grid/StackPanel) that size themselves from the
        // actual text metrics, plus scroll viewers so that a small window keeps
        // every option reachable.

        private static TextBlock MakeLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 4, 12, 4),
            };
        }

        private static TextBlock MakeSectionHeader(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontWeight = FontWeight.SemiBold,
                Margin = new Thickness(0, 8, 0, 2),
            };
        }

        private static TextBlock MakeBlueLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = Brushes.Blue,
                FontStyle = FontStyle.Italic,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            };
        }

        private static Grid MakeFormGrid()
        {
            return new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };
        }

        private static int AddFormRow(Grid grid, int row, string label, Control input)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            if (label != null)
            {
                TextBlock text = MakeLabel(label);
                Grid.SetRow(text, row);
                grid.Children.Add(text);
            }
            input.Margin = new Thickness(0, 2, 0, 2);
            Grid.SetRow(input, row);
            Grid.SetColumn(input, 1);
            grid.Children.Add(input);
            return row + 1;
        }

        private static CheckBox MakeCheckBox(string text, bool isChecked)
        {
            return new CheckBox
            {
                Content = text,
                IsChecked = isChecked,
                Margin = new Thickness(0, 1, 0, 1),
            };
        }

        private static ComboBox MakeComboBox(string[] items, double width)
        {
            ComboBox combo = new ComboBox
            {
                Width = width,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            foreach (string item in items)
            {
                combo.Items.Add(item);
            }
            return combo;
        }

        private static Slider MakeSlider(double minimum, double maximum, double value, double width)
        {
            return new Slider
            {
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                Width = width,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private static NumericUpDown MakeNud(int decimalPlaces, decimal max, double width)
        {
            NumericUpDown nud = new NumericUpDown();
            nud.Minimum = 0;
            nud.Maximum = max;
            nud.FormatString = "F" + decimalPlaces;
            nud.NumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;
            nud.Increment = (decimal)(1.0 / Math.Pow(10, decimalPlaces));
            nud.Value = 0;
            nud.Width = width;
            nud.HorizontalAlignment = HorizontalAlignment.Left;
            return nud;
        }

        private static Bitmap LoadResourceBitmap(string resourceName)
        {
            try
            {
                using (Stream stream = typeof(CommonSettings).Assembly.GetManifestResourceStream("SoMRandomizer.Resources." + resourceName))
                {
                    if (stream != null)
                    {
                        return new Bitmap(stream);
                    }
                }
            }
            catch (Exception)
            {
                // image is purely cosmetic; ignore any failure
            }
            return null;
        }

        private static Button MakeImageButton(string iconName, double iconSize)
        {
            return new Button
            {
                Content = new Image
                {
                    Source = LoadResourceBitmap(iconName),
                    Width = iconSize,
                    Height = iconSize,
                    Stretch = Stretch.Uniform,
                },
                Padding = new Thickness(2),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = new Cursor(StandardCursorType.Hand),
            };
        }

        private static Button MakeHelpButton()
        {
            Button button = MakeImageButton("icon_help.png", 16);
            button.VerticalAlignment = VerticalAlignment.Top;
            return button;
        }

        private static Button MakeTextButton(string text, double minWidth)
        {
            return new Button
            {
                Content = text,
                MinWidth = minWidth,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private static Border MakeGroupBox(string header, Control content)
        {
            DockPanel dock = new DockPanel();
            TextBlock hdr = new TextBlock { Text = header, Margin = new Thickness(6, 0, 0, 2) };
            DockPanel.SetDock(hdr, Dock.Top);
            dock.Children.Add(hdr);
            dock.Children.Add(content);
            return new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Gray,
                Child = dock,
            };
        }

        private static ScrollViewer MakeTabContent(Control content)
        {
            return new ScrollViewer
            {
                Content = content,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
        }

        private static WindowIcon loadWindowIcon()
        {
            try
            {
                using (Stream iconStream = typeof(CommonSettings).Assembly.GetManifestResourceStream("SoMRandomizer.Resources.tonpole.ico"))
                {
                    if (iconStream != null)
                    {
                        return new WindowIcon(iconStream);
                    }
                }
            }
            catch (Exception)
            {
                // icon is purely cosmetic; ignore any failure
            }
            return null;
        }

        // --- constructor ---
        public MainForm()
        {
            BuildUi();
            Icon = loadWindowIcon();
            characterDesigner.Icon = Icon;

            commonSettings = new CommonSettings();

            vanillaRandoSettings = new VanillaRandoSettings(commonSettings);
            openWorldSettings = new OpenWorldSettings(commonSettings);
            ancientCaveSettings = new AncientCaveSettings(commonSettings);
            bossRushSettings = new BossRushSettings(commonSettings);
            chaosSettings = new ChaosSettings(commonSettings);

            settingsByRomType[VanillaRandoSettings.MODE_KEY] = vanillaRandoSettings;
            settingsByRomType[OpenWorldSettings.MODE_KEY] = openWorldSettings;
            settingsByRomType[AncientCaveSettings.MODE_KEY] = ancientCaveSettings;
            settingsByRomType[BossRushSettings.MODE_KEY] = bossRushSettings;
            settingsByRomType[ChaosSettings.MODE_KEY] = chaosSettings;

            generatorsByRomType[VanillaRandoSettings.MODE_KEY] = vanillaRandoGenerator;
            generatorsByRomType[OpenWorldSettings.MODE_KEY] = openWorldGenerator;
            generatorsByRomType[AncientCaveSettings.MODE_KEY] = ancientCaveGenerator;
            generatorsByRomType[BossRushSettings.MODE_KEY] = bossRushGenerator;
            generatorsByRomType[ChaosSettings.MODE_KEY] = chaosGenerator;

            // populate your vanilla SoM ROM if you put one before
            string prevInputRom = AppConfig.APPCONFIG.getStringProperty("PreviousInputRom");
            if (prevInputRom != null)
            {
                txtInputROM.Text = prevInputRom;
            }

            // copy filename, and process with seed
            txtOutputROM.Text = txtInputROM.Text;
            processOutputFilename();

            lblVanillaNoRandoHint.Text = "Uncheck all randomization options to play the vanilla game" + Environment.NewLine + "with the selected hacks enabled.  Seed will be ignored.";
            optionsManager.addOptionsListener(this);
            propertyManager = new OptionsFormManager(optionsManager);

            // set up all the options controls
            initCommonComponents();
            initVanillaRandoComponents();
            initOpenWorldComponents();
            initAncientCaveComponents();
            initBossRushComponents();
            initChaosComponents();
            initDifficultyPaneComponents();

            txtSeed.TextChanged += txtSeed_TextChanged;
            randomizeSeed();

            cklACBiomeTypes.CheckedChanged += cklACBiomeTypes_CheckedChanged;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();

            tabMode.SelectionChanged += tabMode_SelectedIndexChanged;

            // disable some of the common components when we're running rom generation
            enableComponents.Add(btnBrowseInput);
            enableComponents.Add(btnBrowseOutput);
            enableComponents.Add(btnWriteROM);
            enableComponents.Add(btnRandomSeed);
            enableComponents.Add(txtInputROM);
            enableComponents.Add(txtOutputROM);
            enableComponents.Add(txtSeed);
            enableComponents.Add(txtOptions);
            enableComponents.Add(chkAppendSeedToFilename);
            enableComponents.Add(btnGeneralOptions);

            ToolTip.SetTip(btnOptionsCopy, "Copy options string to the clipboard.");
            ToolTip.SetTip(btnOptionsPaste, "Paste options string from the clipboard and replace current selections.");
            ToolTip.SetTip(noMusicPackLabel, "Randomized music will only have the vanilla music. Visit the blogspot for the latest music pack!");

            if (!Directory.Exists("./music"))
            {
                noMusicPackLabel.IsVisible = true;
            }

            makeOpenWorldPresets();

            // character selection for ancient cave mode
            RequireAtLeastOneCheckbox(new List<CheckBox> { chkACIncludeBoyChar, chkACIncludeGirlChar, chkACIncludeSpriteChar });
            // character selection for boss rush mode
            RequireAtLeastOneCheckbox(new List<CheckBox> { chkBossRushIncludeSpriteChar, chkBossRushIncludeGirlChar, chkBossRushIncludeBoyChar });
            // character selection for chaos mode
            RequireAtLeastOneCheckbox(new List<CheckBox> { chkChaosIncludeSpriteChar, chkChaosIncludeGirlChar, chkChaosIncludeBoyChar });

            Closing += MainForm_FormClosing;

            Task.Run(updateCheck);
            optionsChanged();
        }

        private async Task updateCheck()
        {
            if (await UpdateCheck.checkForNewVersion())
            {
                Dispatcher.UIThread.Post(newVersionAvailable);
            }
        }

        private void newVersionAvailable()
        {
            llbNewVersion.IsVisible = true;
            ToolTip.SetTip(llbNewVersion, "Click to open the Mana Rando GitHub to download the latest version of the randomizer.");
        }

        public string getInputRom()
        {
            return txtInputROM.Text;
        }

        private void MainForm_FormClosing(object sender, WindowClosingEventArgs e)
        {
            // save the input rom for next time when we close the window
            AppConfig.APPCONFIG.setConfigProperty("PreviousInputRom", txtInputROM.Text);
        }

        public void optionsChanged()
        {
            handleOptionChanges = false;
            if (handleIndividualChanges)
            {
                txtOptions.Text = optionsManager.getOptionsString();
                clearOptionsError();
            }
            handleOptionChanges = true;
        }

        private void initCheckbox(CheckBox cb, string propertyName, StringValueSettings settings)
        {
            enableComponents.Add(cb);
            optionsManager.addCheckbox(cb, propertyName, settings);
        }

        private void initTrackbar(Slider tb, string propertyName, StringValueSettings settings)
        {
            enableComponents.Add(tb);
            optionsManager.addTrackbar(tb, propertyName, settings);
        }

        private void initCheckedListbox(CheckedListBox clb, string propertyName, StringValueSettings settings)
        {
            enableComponents.Add(clb);
            optionsManager.addCheckedListBox(clb, propertyName, settings);
        }

        private void initCombobox(ComboBox cb, string propertyName, StringValueSettings settings)
        {
            enableComponents.Add(cb);
            optionsManager.addComboBox(cb, propertyName, settings);
        }

        private void initTabControl(TabControl tabs, string propertyName, StringValueSettings settings)
        {
            enableComponents.Add(tabs);
            optionsManager.addTabControl(tabs, propertyName, settings);
        }

        private void initRadioButtons(RadioButton[] rbs, string propertyName, StringValueSettings settings)
        {
            optionsManager.addRadioButtonGroup(rbs, propertyName, settings);
            foreach (RadioButton rb in rbs)
            {
                enableComponents.Add(rb);
            }
        }

        private void RequireAtLeastOneCheckbox(List<CheckBox> checkBoxes)
        {
            foreach (CheckBox cb in checkBoxes)
            {
                cb.IsCheckedChanged += (s, e) =>
                {
                    int checkedCount = 0;
                    foreach (CheckBox c in checkBoxes)
                    {
                        if (c.IsChecked == true)
                        {
                            checkedCount++;
                        }
                    }
                    if (checkedCount == 0)
                    {
                        cb.IsChecked = true;
                    }
                };
            }
        }

        private void cklACBiomeTypes_CheckedChanged(object sender, EventArgs e)
        {
            if (cklACBiomeTypes.CheckedIndices.Count == 0 && cklACBiomeTypes != null)
            {
                cklACBiomeTypes.SetItemChecked(0, true);
            }
        }

        private const string OPTIONS_TOOLTIP = "Paste options to quickly select randomizer settings, or copy to share them.\nTyping in here is allowed, but not recommended.";

        private void clearOptionsError()
        {
            txtOptions.Background = txtSeed.Background;
            ToolTip.SetTip(txtOptions, OPTIONS_TOOLTIP);
        }

        private void txtOptions_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (handleOptionChanges)
            {
                handleIndividualChanges = false;
                try
                {
                    optionsManager.setOptions(txtOptions.Text);
                    txtOptions.Background = txtSeed.Background;
                    ToolTip.SetTip(txtOptions, OPTIONS_TOOLTIP);
                }
                catch (OptionsException ee)
                {
                    string errorMsg = "";
                    if (ee.versionMatch == null)
                    {
                        errorMsg = "No version specified! Options may not be compatible";
                        txtOptions.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 128));
                    }
                    else if (!(bool)ee.versionMatch)
                    {
                        errorMsg = "Version mismatch! Options may not be compatible";
                        txtOptions.Background = new SolidColorBrush(Color.FromArgb(255, 255, 128, 128));
                    }
                    if (ee.valueError != null)
                    {
                        if (errorMsg.Length > 0)
                        {
                            errorMsg = errorMsg + "\n";
                        }
                        errorMsg = errorMsg + ee.valueError;
                        txtOptions.Background = new SolidColorBrush(Color.FromArgb(255, 255, 128, 128));
                    }
                    ToolTip.SetTip(txtOptions, errorMsg);
                }
                handleIndividualChanges = true;
            }
        }

        private void tabMode_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            string currentRomType = getRomType();
            if (currentRomType == VanillaRandoSettings.MODE_KEY || currentRomType == OpenWorldSettings.MODE_KEY)
            {
                propertyManager.setPropertyValue("generalMiscMechanical", CommonSettings.PROPERTYNAME_STARTER_GEAR, "no");
                llbDifficultyDetails.IsVisible = false;
                difficultyPopout = false;
                llbDifficultyDetails.Foreground = Brushes.Red;
                refreshWidth();
            }
            else
            {
                propertyManager.setPropertyValue("generalMiscMechanical", CommonSettings.PROPERTYNAME_STARTER_GEAR, "yes");
                llbDifficultyDetails.IsVisible = true;
            }

            switch (currentRomType)
            {
                case OpenWorldSettings.MODE_KEY:
                    label47.Text = "";
                    break;
                case BossRushSettings.MODE_KEY:
                    label47.Text = "Note: this mode still needs a lot of work! Don't expect much yet!";
                    break;
                case ChaosSettings.MODE_KEY:
                    label47.Text = "Note: this mode still needs a little bit of work! Don't expect much yet!";
                    break;
                case AncientCaveSettings.MODE_KEY:
                    label47.Text = "Note: this mode still needs a lot of work! Don't expect much yet!";
                    break;
                case VanillaRandoSettings.MODE_KEY:
                    label47.Text = "";
                    break;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            int currentProgress = commonSettings.getInt(CommonSettings.PROPERTYNAME_CURRENT_PROGRESS);
            prgGenerationProgress.Value = currentProgress;
        }

        private void picCatBread_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("http://secretofmanaancientcave.blogspot.com") { UseShellExecute = true });
        }

        private void txtSeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            processOutputFilename();
        }

        private async void btnBrowseInput_Click(object sender, RoutedEventArgs e)
        {
            var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Select input ROM", AllowMultiple = false });
            if (files.Count > 0)
            {
                string path = files[0].TryGetLocalPath();
                if (path != null)
                {
                    txtInputROM.Text = path;
                    if (string.IsNullOrEmpty(txtOutputROM.Text))
                    {
                        txtOutputROM.Text = txtInputROM.Text;
                        processOutputFilename();
                    }
                }
            }
        }

        private async void btnBrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions { Title = "Select output ROM", SuggestedFileName = "randomized.smc", DefaultExtension = "smc" });
            if (file != null)
            {
                string path = file.TryGetLocalPath();
                if (path != null)
                {
                    txtOutputROM.Text = path;
                }
            }
        }

        private void processOutputFilename()
        {
            string outFilename = txtOutputROM.Text ?? "";
            string seed = txtSeed.Text ?? "";
            int periodIndex = outFilename.LastIndexOf(".");
            string beforeExtension = outFilename;
            string extension = "";

            if (periodIndex >= 0)
            {
                beforeExtension = outFilename.Substring(0, periodIndex);
                extension = outFilename.Substring(periodIndex);
            }

            int oBracketIndex = beforeExtension.LastIndexOf("[");
            if (oBracketIndex >= 0)
            {
                beforeExtension = beforeExtension.Substring(0, oBracketIndex);
            }

            if (chkAppendSeedToFilename.IsChecked == true && beforeExtension.Length > 0)
            {
                beforeExtension += "[" + seed + "]";
            }
            txtOutputROM.Text = beforeExtension + extension;
        }

        private void btnRandomSeed_Click(object sender, RoutedEventArgs e)
        {
            randomizeSeed();
            processOutputFilename();
        }

        private void randomizeSeed()
        {
            txtSeed.Text = randomHexString();
        }

        private string randomHexString()
        {
            string chars = "0123456789ABCDEF";
            char[] stringChars = new char[16];
            Random random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        private void chkAppendSeedToFilename_CheckedChanged(object sender, RoutedEventArgs e)
        {
            processOutputFilename();
        }

        private void btnWriteROM_Click(object sender, RoutedEventArgs e)
        {
            if (generating)
            {
                return;
            }
            generateWithCurrentSettings(false);
        }

        public async void generateWithCurrentSettings(bool hideUI)
        {
            generating = true;
            label7.IsVisible = true;
            foreach (Control comp in enableComponents)
            {
                comp.IsEnabled = false;
            }
            picCatBread.IsEnabled = true;

            string romType = commonSettings.get(CommonSettings.PROPERTYNAME_MODE);

            // this is just for the log
            commonSettings.set(CommonSettings.PROPERTYNAME_ALL_ENTERED_OPTIONS, txtOptions.Text);
            commonSettings.set(CommonSettings.PROPERTYNAME_VERSION, RomGenerator.VERSION_NUMBER);
            propertyManager.closeAll();
            generalOptionsCategory.Hide();
            openWorldOptionsCategory.Hide();

            string src = txtInputROM.Text;
            string dst = txtOutputROM.Text;
            string seed = txtSeed.Text;

            if (!hideUI && dst != null && dst != "" && File.Exists(dst))
            {
                bool overwrite = await DialogUtil.ShowYesNo(this, "Overwrite", "Destination file already exists.  Sure you want to overwrite?");
                if (!overwrite)
                {
                    done();
                    return;
                }
            }

            string errorMessage = null;
            await Task.Run(() =>
            {
                try
                {
                    generate(src, dst, seed, romType);
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                }
            });

            if (errorMessage == null)
            {
                await DialogUtil.ShowMessage(this, "Done", "Done!");
            }
            else
            {
                await DialogUtil.ShowMessage(this, "Error", errorMessage);
            }

            done();

            if (hideUI)
            {
                Close();
            }
        }

        private void generate(string src, string dst, string seed, string romType)
        {
            // these difficulty settings are only applicable to the older modes; don't bother grabbing them for vanilla rando or open world
            if (romType == AncientCaveSettings.MODE_KEY || romType == BossRushSettings.MODE_KEY || romType == ChaosSettings.MODE_KEY)
            {
                string selDifficulty = this.selectedDifficulty;
                DifficultySettings diff = customDifficulty[romType];
                if (difficulties.ContainsKey(romType) && difficulties[romType].ContainsKey(selDifficulty))
                {
                    diff = difficulties[romType][selDifficulty];
                }
                foreach (string key in DifficultySettings.displayStrings.Keys)
                {
                    if (DifficultySettings.isGrowthValue(key))
                    {
                        IntGrowthValue val = diff.getGrowthValue(key);
                        commonSettings.set(DifficultySettings.DIFFICULTY_PROPERTY_PREFIX + DifficultySettings.displayStrings[key] + "_base", val.baseValue.ToString());
                        commonSettings.set(DifficultySettings.DIFFICULTY_PROPERTY_PREFIX + DifficultySettings.displayStrings[key] + "_growth", val.growthValue.ToString());
                        commonSettings.set(DifficultySettings.DIFFICULTY_PROPERTY_PREFIX + DifficultySettings.displayStrings[key] + "_exp", val.exponent.ToString());
                    }
                    else
                    {
                        double val = diff.getDoubleValue(key);
                        commonSettings.set(DifficultySettings.DIFFICULTY_PROPERTY_PREFIX + DifficultySettings.displayStrings[key], val.ToString());
                    }
                }
            }

            // pass the customized character colors down if specified
            if (commonSettings.get(CommonSettings.PROPERTYNAME_RANDOMIZE_CHAR_COLORS) == "custom")
            {
                for (int i = 1; i < 16; i++)
                {
                    SnesColor boyColor = characterDesigner.boyCurrent.getColor(i);
                    SnesColor girlColor = characterDesigner.girlCurrent.getColor(i);
                    SnesColor spriteColor = characterDesigner.spriteCurrent.getColor(i);
                    commonSettings.set(CommonSettings.PROPERTYNAME_PREFIX_CUSTOM_CHARACTER_COLORS + "BoyRed" + i, boyColor.getRed().ToString());
                    commonSettings.set(CommonSettings.PROPERTYNAME_PREFIX_CUSTOM_CHARACTER_COLORS + "BoyGreen" + i, boyColor.getGreen().ToString());
                    commonSettings.set(CommonSettings.PROPERTYNAME_PREFIX_CUSTOM_CHARACTER_COLORS + "BoyBlue" + i, boyColor.getBlue().ToString());
                    commonSettings.set(CommonSettings.PROPERTYNAME_PREFIX_CUSTOM_CHARACTER_COLORS + "GirlRed" + i, girlColor.getRed().ToString());
                    commonSettings.set(CommonSettings.PROPERTYNAME_PREFIX_CUSTOM_CHARACTER_COLORS + "GirlGreen" + i, girlColor.getGreen().ToString());
                    commonSettings.set(CommonSettings.PROPERTYNAME_PREFIX_CUSTOM_CHARACTER_COLORS + "GirlBlue" + i, girlColor.getBlue().ToString());
                    commonSettings.set(CommonSettings.PROPERTYNAME_PREFIX_CUSTOM_CHARACTER_COLORS + "SpriteRed" + i, spriteColor.getRed().ToString());
                    commonSettings.set(CommonSettings.PROPERTYNAME_PREFIX_CUSTOM_CHARACTER_COLORS + "SpriteGreen" + i, spriteColor.getGreen().ToString());
                    commonSettings.set(CommonSettings.PROPERTYNAME_PREFIX_CUSTOM_CHARACTER_COLORS + "SpriteBlue" + i, spriteColor.getBlue().ToString());
                }
            }

            RomGenerator.initGeneration(src, dst, seed, generatorsByRomType, commonSettings, settingsByRomType);
        }

        private void done()
        {
            label7.IsVisible = false;
            foreach (Control comp in enableComponents)
            {
                comp.IsEnabled = true;
            }
            generating = false;
        }

        private string getRomType()
        {
            return commonSettings.get(CommonSettings.PROPERTYNAME_MODE);
        }

        private string getSelectedTabName()
        {
            switch (tabMode.SelectedIndex)
            {
                case 0: return "Rando";
                case 1: return "Open";
                case 2: return "Ancient Cave";
                case 3: return "Boss Rush";
                case 4: return "Chaos";
            }
            return null;
        }

        private void btnGeneralOptions_Click(object sender, RoutedEventArgs e)
        {
            generalOptionsCategory.Show();
        }

        private void btnChoasMoreOptions_Click(object sender, RoutedEventArgs e)
        {
            propertyManager.showForm("chaos");
        }

        private void btnBossRushMoreOptions_Click(object sender, RoutedEventArgs e)
        {
            propertyManager.showForm("bossrush");
        }

        private void trbVanillaEnemyScaling_Scroll(object sender, RangeBaseValueChangedEventArgs e)
        {
            int sel = (int)trbVanillaEnemyScaling.Value;
            string printVal = "?";
            string[] percentStrings = vanillaRandoSettings.getOptionsValues(VanillaRandoSettings.PROPERTYNAME_ENEMY_SCALING);
            printVal = percentStrings[sel];

            if (trbVanillaEnemyScaling.Value == percentStrings.Length / 2)
            {
                label36.Text = "Normal [" + printVal + "%]";
            }
            else if (trbVanillaEnemyScaling.Value == 0)
            {
                label36.Text = "Easiest [" + printVal + "%]";
            }
            else if (trbVanillaEnemyScaling.Value < percentStrings.Length / 2)
            {
                label36.Text = "Easier [" + printVal + "%]";
            }
            else if (trbVanillaEnemyScaling.Value == percentStrings.Length - 1)
            {
                label36.Text = "Hardest [" + printVal + "%]";
            }
            else if (trbVanillaEnemyScaling.Value > percentStrings.Length / 2)
            {
                label36.Text = "Harder [" + printVal + "%]";
            }
        }

        private HelpPages helpPages = new HelpPages();

        private void openInfoForm()
        {
            // open help for selected tab
            string rt = getRomType();
            helpPages.openInfoForm(rt);
        }

        private void picVanillaDetails_Click(object sender, RoutedEventArgs e)
        {
            openInfoForm();
        }

        private void picDiscord_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://discord.gg/YfmUHqU") { UseShellExecute = true });
        }

        private void picTwitch_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://twitch.tv/moppleton") { UseShellExecute = true });
        }

        private void picBlogspot_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("http://secretofmanaancientcave.blogspot.com/") { UseShellExecute = true });
        }

        private void picMail_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("mailto:umokumok@gmail.com") { UseShellExecute = true });
        }

        private void picACDetails_Click(object sender, RoutedEventArgs e)
        {
            openInfoForm();
        }

        private void picBossRushDetails_Click(object sender, RoutedEventArgs e)
        {
            openInfoForm();
        }

        private void picChaosDetails_Click(object sender, RoutedEventArgs e)
        {
            openInfoForm();
        }

        private void llbNewVersion_LinkClicked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(UpdateCheck.updateDownloadUrl) { UseShellExecute = true });
        }

        private void cmbOpenStatGrowth_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((string)cmbOpenStatGrowth.SelectedItem).Contains("None"))
            {
                cmbOpenStatGrowthDifficulty.IsEnabled = false;
                if (cmbOpenRandomizeBosses.SelectedIndex == 2)
                {
                    cmbOpenRandomizeBosses.SelectedIndex = 1;
                }
            }
            else
            {
                cmbOpenStatGrowthDifficulty.IsEnabled = true;
                if (cmbOpenRandomizeBosses.SelectedIndex == 1)
                {
                    cmbOpenRandomizeBosses.SelectedIndex = 2;
                }
            }
        }

        private void picOpenDetails_Click(object sender, RoutedEventArgs e)
        {
            openInfoForm();
        }

        private void btnOpenMoreOptions_Click(object sender, RoutedEventArgs e)
        {
            openWorldOptionsCategory.Show();
        }

        private void btnPresets_Click(object sender, RoutedEventArgs e)
        {
            openWorldPresetsForm.Show();
        }

        private async void btnOptionsCopy_Click(object sender, RoutedEventArgs e)
        {
            var top = TopLevel.GetTopLevel(this);
            if (top != null)
            {
                await top.Clipboard.SetTextAsync(txtOptions.Text);
            }
        }

        private async void btnOptionsPaste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var top = TopLevel.GetTopLevel(this);
                string clipText = top != null ? await top.Clipboard.TryGetTextAsync() : null;
                if (!string.IsNullOrEmpty(clipText))
                {
                    bool yes = await DialogUtil.ShowYesNo(this, "Overwrite", "Pasting will replace current settings.  Are you sure?");
                    if (!yes)
                    {
                        return;
                    }

                    txtOptions.Text = clipText;
                }
                else
                {
                    await DialogUtil.ShowMessage(this, "Paste", "Nothing to paste!");
                }
            }
            catch (Exception)
            {
                await DialogUtil.ShowMessage(this, "Paste", "Can't paste clipboard contents!");
            }
        }

        private void btnPlando_Click(object sender, RoutedEventArgs e)
        {
            // plando button
            plandoForm.Show();
            if (plandoForm.WindowState == WindowState.Minimized)
            {
                plandoForm.WindowState = WindowState.Normal;
            }
        }

        // --- BuildUi ---
        private void BuildUi()
        {
            Title = "Secret of Mana Randomizer";
            Width = noDifficultyWidth;
            Height = 720;
            MinWidth = 700;
            MinHeight = 480;
            FontSize = 13;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // The Fluent checkbox template reserves a lot of vertical space;
            // the option lists are long, so make them a bit more compact.
            Style compactCheckBox = new Style(x => x.OfType<CheckBox>());
            compactCheckBox.Setters.Add(new Setter(TemplatedControl.MinHeightProperty, 28.0));
            Styles.Add(compactCheckBox);

            Grid layout = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*,Auto"),
                Margin = new Thickness(12),
            };

            layout.Children.Add(BuildTopSection());

            Grid middle = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions(modePanelWidth + ",Auto"),
                Margin = new Thickness(0, 8, 0, 8),
            };
            Grid.SetRow(middle, 1);
            middle.Children.Add(BuildGameModeGroup());
            layout.Children.Add(middle);

            groupBox2 = BuildDifficultyGroup();
            groupBox2.Width = 544;
            groupBox2.Margin = new Thickness(10, 0, 0, 0);
            groupBox2.HorizontalAlignment = HorizontalAlignment.Left;
            groupBox2.IsVisible = false;
            Grid.SetColumn(groupBox2, 1);
            middle.Children.Add(groupBox2);

            Control bottom = BuildBottomSection();
            Grid.SetRow(bottom, 2);
            layout.Children.Add(bottom);

            // Note: the window content is NOT wrapped in a ScrollViewer here.
            // A horizontal scroll viewer would measure the content with an
            // infinite width, making every Grid column stretch and pushing the
            // controls out of the window.  Each tab has its own vertical scroll
            // viewer and the window has a minimum size instead.
            Content = layout;
        }

        private Control BuildTopSection()
        {
            Grid section = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };

            StackPanel icons = new StackPanel
            {
                Spacing = 3,
                Margin = new Thickness(0, 0, 14, 0),
                VerticalAlignment = VerticalAlignment.Top,
            };
            picDiscord = MakeImageButton("icon_discord.png", 18);
            picTwitch = MakeImageButton("icon_twitch.png", 18);
            picBlogspot = MakeImageButton("icon_blogspot.png", 18);
            picMail = MakeImageButton("icon_mail.png", 18);
            picCatBread = MakeImageButton("icon_catbread.png", 36);
            icons.Children.Add(picDiscord);
            icons.Children.Add(picTwitch);
            icons.Children.Add(picBlogspot);
            icons.Children.Add(picMail);
            icons.Children.Add(picCatBread);
            section.Children.Add(icons);

            Grid fields = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto") };
            Grid.SetColumn(fields, 1);
            section.Children.Add(fields);

            int row = 0;

            // Avalonia TextBox.Text defaults to null whereas WinForms returns "";
            // the ported flow relies on empty strings, so initialize them explicitly.
            txtInputROM = new TextBox { Text = "" };
            btnBrowseInput = MakeTextButton("Browse", 90);
            btnBrowseInput.Click += btnBrowseInput_Click;
            row = AddFieldRow(fields, row, "Input ROM:", txtInputROM, btnBrowseInput);

            txtOutputROM = new TextBox { Text = "" };
            btnBrowseOutput = MakeTextButton("Browse", 90);
            btnBrowseOutput.Click += btnBrowseOutput_Click;
            row = AddFieldRow(fields, row, "Output ROM:", txtOutputROM, btnBrowseOutput);

            txtSeed = new TextBox { Text = "" };
            btnRandomSeed = MakeTextButton("Random", 90);
            btnRandomSeed.Click += btnRandomSeed_Click;
            row = AddFieldRow(fields, row, "Seed:", txtSeed, btnRandomSeed);

            txtOptions = new TextBox { Text = "" };
            btnOptionsCopy = MakeImageButton("icon_copy.png", 16);
            btnOptionsCopy.Click += btnOptionsCopy_Click;
            btnOptionsPaste = MakeImageButton("icon_paste.png", 16);
            btnOptionsPaste.Click += btnOptionsPaste_Click;

            Grid optionsRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto") };
            optionsRow.Children.Add(txtOptions);
            Grid.SetColumn(btnOptionsCopy, 1);
            optionsRow.Children.Add(btnOptionsCopy);
            Grid.SetColumn(btnOptionsPaste, 2);
            optionsRow.Children.Add(btnOptionsPaste);
            row = AddFieldRow(fields, row, "Options:", optionsRow, null);

            chkAppendSeedToFilename = MakeCheckBox("Append seed to filename", true);
            chkAppendSeedToFilename.IsCheckedChanged += chkAppendSeedToFilename_CheckedChanged;
            btnWriteROM = MakeTextButton("Write ROM", 120);
            btnWriteROM.Click += btnWriteROM_Click;

            Grid actions = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 6) };
            chkAppendSeedToFilename.VerticalAlignment = VerticalAlignment.Center;
            actions.Children.Add(chkAppendSeedToFilename);
            btnWriteROM.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(btnWriteROM, 1);
            actions.Children.Add(btnWriteROM);
            row = AddWideRow(fields, row, actions);

            llbNewVersion = new Button
            {
                Content = "New version available!",
                IsVisible = false,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.Blue,
                Padding = new Thickness(0, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Cursor = new Cursor(StandardCursorType.Hand),
            };
            llbNewVersion.Click += llbNewVersion_LinkClicked;
            row = AddWideRow(fields, row, llbNewVersion);

            label47 = new TextBlock { Foreground = Brushes.Red, TextWrapping = TextWrapping.Wrap };
            AddWideRow(fields, row, label47);

            return section;
        }

        private static int AddFieldRow(Grid grid, int row, string label, Control input, Control button, Control extraButton = null)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            TextBlock text = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 5),
            };
            Grid.SetRow(text, row);
            grid.Children.Add(text);

            input.VerticalAlignment = VerticalAlignment.Center;
            input.Margin = new Thickness(0, 0, 8, 5);
            Grid.SetRow(input, row);
            Grid.SetColumn(input, 1);
            grid.Children.Add(input);

            if (button != null)
            {
                button.Margin = new Thickness(0, 0, 6, 5);
                Grid.SetRow(button, row);
                Grid.SetColumn(button, 2);
                grid.Children.Add(button);
            }
            if (extraButton != null)
            {
                extraButton.Margin = new Thickness(0, 0, 0, 5);
                Grid.SetRow(extraButton, row);
                Grid.SetColumn(extraButton, 3);
                grid.Children.Add(extraButton);
            }
            return row + 1;
        }

        private static int AddWideRow(Grid grid, int row, Control content)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            content.Margin = new Thickness(0, 0, 0, 6);
            Grid.SetRow(content, row);
            Grid.SetColumn(content, 1);
            Grid.SetColumnSpan(content, 3);
            grid.Children.Add(content);
            return row + 1;
        }

        private Control BuildBottomSection()
        {
            Grid bottom = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto") };

            btnGeneralOptions = MakeTextButton("General options", 140);
            btnGeneralOptions.Click += btnGeneralOptions_Click;
            bottom.Children.Add(btnGeneralOptions);

            noMusicPackLabel = new TextBlock
            {
                Text = "No music pack detected!",
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 192, 64, 0)),
                IsVisible = false,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(12, 0, 12, 0),
            };
            Grid.SetColumn(noMusicPackLabel, 1);
            bottom.Children.Add(noMusicPackLabel);

            llbDifficultyDetails = new Button
            {
                Content = "Difficulty Details",
                IsVisible = false,
                Foreground = Brushes.Red,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(6, 4),
                Cursor = new Cursor(StandardCursorType.Hand),
            };
            llbDifficultyDetails.Click += linkLabel2_LinkClicked;
            Grid.SetColumn(llbDifficultyDetails, 2);
            bottom.Children.Add(llbDifficultyDetails);

            return bottom;
        }

        private Control BuildGameModeGroup()
        {
            tabMode = new TabControl { Padding = new Thickness(0) };

            // The Fluent TabItem template uses a much larger font than the
            // original WinForms tab strip, which made the five mode headers wrap
            // onto two rows and squeezed the content out of the group box.
            Style tabItemStyle = new Style(x => x.OfType<TabItem>());
            tabItemStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, 13.0));
            tabItemStyle.Setters.Add(new Setter(Layoutable.MinHeightProperty, 26.0));
            tabMode.Styles.Add(tabItemStyle);

            tpVanilla = new TabItem { Header = "Rando", Content = BuildVanillaTab() };
            tpOpen = new TabItem { Header = "Open", Content = BuildOpenTab() };
            tpAncientCave = new TabItem { Header = "Ancient Cave", Content = BuildAncientCaveTab() };
            tpBossRush = new TabItem { Header = "Boss Rush", Content = BuildBossRushTab() };
            tpChaos = new TabItem { Header = "Chaos", Content = BuildChaosTab() };

            tabMode.Items.Add(tpVanilla);
            tabMode.Items.Add(tpOpen);
            tabMode.Items.Add(tpAncientCave);
            tabMode.Items.Add(tpBossRush);
            tabMode.Items.Add(tpChaos);
            tabMode.SelectedIndex = 0;

            groupBox1 = MakeGroupBox("Game Mode", tabMode);
            groupBox1.HorizontalAlignment = HorizontalAlignment.Stretch;
            groupBox1.VerticalAlignment = VerticalAlignment.Stretch;
            return groupBox1;
        }

        private Control BuildVanillaTab()
        {
            StackPanel content = new StackPanel { Spacing = 2, Margin = new Thickness(14, 10, 14, 10) };

            Grid header = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };
            picVanillaDetails = MakeHelpButton();
            picVanillaDetails.Click += picVanillaDetails_Click;
            header.Children.Add(picVanillaDetails);
            TextBlock intro = MakeBlueLabel("Randomize bosses and other stuff in the vanilla game.");
            intro.TextWrapping = TextWrapping.Wrap;
            Grid.SetColumn(intro, 1);
            header.Children.Add(intro);
            content.Children.Add(header);

            Grid columns = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,28,*"),
                Margin = new Thickness(0, 10, 0, 0),
            };
            content.Children.Add(columns);

            StackPanel left = new StackPanel { Spacing = 2 };
            left.Children.Add(MakeSectionHeader("Randomize:"));
            chkVanillaRandomizeBosses = MakeCheckBox("Bosses", true);
            chkVanillaRandomizeEnemies = MakeCheckBox("Non-boss enemies", true);
            chkVanillaRandomizeWeaponOrbs = MakeCheckBox("Weapon orbs", true);
            chkVanillaRandomizeElements = MakeCheckBox("Elements", true);
            chkVanillaRandomizeWeapons = MakeCheckBox("Weapons", true);
            chkVanillaRandomizeMusic = MakeCheckBox("Music", true);
            left.Children.Add(chkVanillaRandomizeBosses);
            left.Children.Add(chkVanillaRandomizeEnemies);
            left.Children.Add(chkVanillaRandomizeWeaponOrbs);
            left.Children.Add(chkVanillaRandomizeElements);
            left.Children.Add(chkVanillaRandomizeWeapons);
            left.Children.Add(chkVanillaRandomizeMusic);
            Grid.SetColumn(left, 0);
            columns.Children.Add(left);

            StackPanel right = new StackPanel { Spacing = 2 };
            right.Children.Add(MakeSectionHeader("Options:"));
            chkVanillaDialogCuts = MakeCheckBox("Shorter dialogue", true);
            chkVanillaPreserveEarlyBosses = MakeCheckBox("Preserve early bosses", false);
            chkVanillaAutoSave = MakeCheckBox("Autosave", true);
            right.Children.Add(chkVanillaDialogCuts);
            right.Children.Add(chkVanillaPreserveEarlyBosses);
            right.Children.Add(chkVanillaAutoSave);

            Grid form = MakeFormGrid();
            form.Margin = new Thickness(0, 6, 0, 0);
            int row = 0;
            cmbVanillaExpMultiplier = MakeComboBox(new string[] { "Half", "Normal", "Double", "Triple" }, 100);
            cmbVanillaGoldMultiplier = MakeComboBox(new string[] { "Half", "Normal", "Double", "Triple" }, 100);
            StackPanel expGoldRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            expGoldRow.Children.Add(cmbVanillaExpMultiplier);
            expGoldRow.Children.Add(cmbVanillaGoldMultiplier);
            row = AddFormRow(form, row, "Experience / Gold:", expGoldRow);
            cmbVanillaStatusAilments = MakeComboBox(new string[] { "Location", "Enemy type", "Random (easy)", "Random (annoying)", "Random (awful)" }, 190);
            row = AddFormRow(form, row, "Status ailments:", cmbVanillaStatusAilments);
            cmbVanillaSpecialMode = MakeComboBox(new string[] { "None", "Halloween", "Christmas" }, 190);
            row = AddFormRow(form, row, "Special mode:", cmbVanillaSpecialMode);

            trbVanillaEnemyScaling = MakeSlider(1, 9, 5, 130);
            trbVanillaEnemyScaling.ValueChanged += trbVanillaEnemyScaling_Scroll;
            label36 = new TextBlock
            {
                Text = "Normal [100%]",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            };
            StackPanel enemyRow = new StackPanel { Orientation = Orientation.Horizontal };
            enemyRow.Children.Add(trbVanillaEnemyScaling);
            enemyRow.Children.Add(label36);
            AddFormRow(form, row, "Enemy difficulty:", enemyRow);

            right.Children.Add(form);
            Grid.SetColumn(right, 2);
            columns.Children.Add(right);

            lblVanillaNoRandoHint = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 10, 0, 0),
            };
            content.Children.Add(lblVanillaNoRandoHint);

            return MakeTabContent(content);
        }

        private Control BuildOpenTab()
        {
            StackPanel content = new StackPanel { Spacing = 2, Margin = new Thickness(12, 8, 12, 8) };

            Grid header = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };
            picOpenDetails = MakeHelpButton();
            picOpenDetails.Click += picOpenDetails_Click;
            header.Children.Add(picOpenDetails);
            TextBlock intro = MakeBlueLabel("Open world mode, with immediate access to Flammie.");
            intro.TextWrapping = TextWrapping.Wrap;
            Grid.SetColumn(intro, 1);
            header.Children.Add(intro);
            content.Children.Add(header);

            Grid columns = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,28,*"),
                Margin = new Thickness(0, 10, 0, 0),
            };
            content.Children.Add(columns);

            StackPanel left = new StackPanel { Spacing = 2 };
            left.Children.Add(MakeSectionHeader("Randomizations:"));
            chkOpenRandomizeWeapons = MakeCheckBox("Weapons", true);
            chkOpenRandomizeShops = MakeCheckBox("Shops", true);
            chkOpenRandomizeMusic = MakeCheckBox("Music", true);
            chkOpenRandomMapColors = MakeCheckBox("Map colors", false);

            Grid checks = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,24,Auto"),
                RowDefinitions = new RowDefinitions("Auto,Auto"),
            };
            checks.Children.Add(chkOpenRandomizeWeapons);
            Grid.SetColumn(chkOpenRandomizeShops, 2);
            checks.Children.Add(chkOpenRandomizeShops);
            Grid.SetRow(chkOpenRandomizeMusic, 1);
            checks.Children.Add(chkOpenRandomizeMusic);
            Grid.SetRow(chkOpenRandomMapColors, 1);
            Grid.SetColumn(chkOpenRandomMapColors, 2);
            checks.Children.Add(chkOpenRandomMapColors);
            left.Children.Add(checks);

            Grid form = MakeFormGrid();
            form.Margin = new Thickness(0, 6, 0, 0);
            int row = 0;
            cmbOpenStartingChar = MakeComboBox(new string[] { "Random", "Boy", "Girl", "Sprite" }, 155);
            row = AddFormRow(form, row, "Start char:", cmbOpenStartingChar);
            cmbOpenRandomizeBosses = MakeComboBox(new string[] { "Vanilla", "Swap", "Random" }, 155);
            row = AddFormRow(form, row, "Bosses:", cmbOpenRandomizeBosses);
            cmbOpenRandomizeEnemies = MakeComboBox(new string[] { "Vanilla", "Swap", "Random spawns", "Oops! All owls", "None" }, 155);
            row = AddFormRow(form, row, "Enemies:", cmbOpenRandomizeEnemies);
            cmbOpenGoal = MakeComboBox(new string[] { "Vanilla short", "Vanilla long", "Mana tree revival", "Gift exchange", "Reindeer search" }, 155);
            row = AddFormRow(form, row, "Goal:", cmbOpenGoal);
            cmbOpenLogicMode = MakeComboBox(new string[] { "Basic", "Restrictive" }, 155);
            row = AddFormRow(form, row, "Logic:", cmbOpenLogicMode);
            left.Children.Add(form);
            Grid.SetColumn(left, 0);
            columns.Children.Add(left);

            StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 14, 0, 0) };
            btnPlando = MakeTextButton("Plandomizer", 100);
            btnPlando.Click += btnPlando_Click;
            btnPresets = MakeTextButton("Presets", 80);
            btnPresets.Click += btnPresets_Click;
            btnOpenMoreOptions = MakeTextButton("More options", 110);
            btnOpenMoreOptions.Click += btnOpenMoreOptions_Click;
            buttons.Children.Add(btnPlando);
            buttons.Children.Add(btnPresets);
            buttons.Children.Add(btnOpenMoreOptions);

            StackPanel right = new StackPanel { Spacing = 2 };
            right.Children.Add(MakeSectionHeader("Enemy settings:"));
            Grid form2 = MakeFormGrid();
            form2.Margin = new Thickness(0, 6, 0, 0);
            int row2 = 0;
            cmbOpenStatGrowth = MakeComboBox(new string[] { "Match player", "Increase after bosses", "Timed", "No Future", "None (vanilla)" }, 190);
            cmbOpenStatGrowth.SelectionChanged += cmbOpenStatGrowth_SelectedIndexChanged;
            row2 = AddFormRow(form2, row2, "Enemy stat growth:", cmbOpenStatGrowth);
            cmbOpenStatGrowthDifficulty = MakeComboBox(new string[] { "Easy", "Sorta easy", "Normal", "Kinda hard", "Hard", "Impossible" }, 170);
            row2 = AddFormRow(form2, row2, "Difficulty:", cmbOpenStatGrowthDifficulty);
            cmbOpenOtherChars = MakeComboBox(new string[] { "Start with both", "Find both at level 1", "Find both at current level", "Start with one, find the other", "Start with one, other doesn't exist", "Find one at level 1", "Find one at current level", "They don't exist" }, 190);
            row2 = AddFormRow(form2, row2, "Other chars:", cmbOpenOtherChars);
            cmbOpenStatusAilments = MakeComboBox(new string[] { "Location", "Enemy type", "Random (easy)", "Random (annoying)", "Random (awful)" }, 190);
            AddFormRow(form2, row2, "Status ailments:", cmbOpenStatusAilments);
            right.Children.Add(form2);

            chkOpenFlammieDrumLogic = MakeCheckBox("Flammie Drum in Logic", false);
            chkOpenFlammieDrumLogic.Margin = new Thickness(0, 10, 0, 0);
            right.Children.Add(chkOpenFlammieDrumLogic);
            Grid.SetColumn(right, 2);
            columns.Children.Add(right);
            right.Children.Add(buttons);

            return MakeTabContent(content);
        }

        private Control BuildAncientCaveTab()
        {
            StackPanel content = new StackPanel { Spacing = 2, Margin = new Thickness(14, 10, 14, 10) };

            Grid header = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };
            picACDetails = MakeHelpButton();
            picACDetails.Click += picACDetails_Click;
            header.Children.Add(picACDetails);
            TextBlock intro = MakeBlueLabel("Run through a sequence of increasingly-difficult procedurally generated maps.");
            intro.TextWrapping = TextWrapping.Wrap;
            Grid.SetColumn(intro, 1);
            header.Children.Add(intro);
            content.Children.Add(header);

            Grid columns = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,32,*"),
                Margin = new Thickness(0, 10, 0, 0),
            };
            content.Children.Add(columns);

            StackPanel left = new StackPanel { Spacing = 2 };
            Grid form = MakeFormGrid();
            int row = 0;
            cmbACDifficulty = MakeComboBox(new string[] { "Casual", "Hard", "Really hard", "Custom" }, 160);
            cmbACDifficulty.SelectionChanged += cmbACDifficulty_SelectedIndexChanged;
            row = AddFormRow(form, row, "Difficulty:", cmbACDifficulty);
            cmbACDialogSource = MakeComboBox(new string[] { "Mitch Hedberg", "Demetri Martin" }, 160);
            AddFormRow(form, row, "NPC Dialogue:", cmbACDialogSource);
            left.Children.Add(form);

            chkACProfanityFilter = MakeCheckBox("Dialogue language filter", false);
            chkACRandomMusic = MakeCheckBox("Random music", true);
            left.Children.Add(chkACProfanityFilter);
            left.Children.Add(chkACRandomMusic);
            Grid.SetColumn(left, 0);
            columns.Children.Add(left);

            StackPanel right = new StackPanel { Spacing = 2 };
            Grid form2 = MakeFormGrid();
            int row2 = 0;
            cmbACLength = MakeComboBox(new string[] { "Short (8 Floors)", "Medium (16 Floors)", "Long (24 Floors)" }, 170);
            cmbACLength.SelectionChanged += cmbACLength_SelectedIndexChanged;
            row2 = AddFormRow(form2, row2, "Length:", cmbACLength);
            cmbACBossFrequency = MakeComboBox(new string[] { "Every floor", "Every few floors", "Final only" }, 170);
            row2 = AddFormRow(form2, row2, "Bosses:", cmbACBossFrequency);
            right.Children.Add(form2);

            right.Children.Add(MakeSectionHeader("Floor Types:"));
            cklACBiomeTypes = new CheckedListBox { Width = 240, Height = 120 };
            cklACBiomeTypes.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            right.Children.Add(cklACBiomeTypes);
            Grid.SetColumn(right, 2);
            columns.Children.Add(right);

            StackPanel chars = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 14,
                Margin = new Thickness(6, 2, 6, 4),
            };
            chkACIncludeBoyChar = MakeCheckBox("Boy", true);
            chkACIncludeGirlChar = MakeCheckBox("Girl", true);
            chkACIncludeSpriteChar = MakeCheckBox("Sprite", true);
            chars.Children.Add(chkACIncludeBoyChar);
            chars.Children.Add(chkACIncludeGirlChar);
            chars.Children.Add(chkACIncludeSpriteChar);
            groupBox3 = MakeGroupBox("Characters", chars);
            groupBox3.Margin = new Thickness(0, 8, 0, 0);
            groupBox3.HorizontalAlignment = HorizontalAlignment.Left;
            content.Children.Add(groupBox3);

            label7 = new TextBlock
            {
                Text = "Generating...",
                FontSize = 14.25,
                IsVisible = false,
                Margin = new Thickness(0, 10, 0, 4),
            };
            content.Children.Add(label7);

            prgGenerationProgress = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                Height = 6,
                Margin = new Thickness(0, 6, 0, 0),
            };
            content.Children.Add(prgGenerationProgress);

            return MakeTabContent(content);
        }

        private Control BuildBossRushTab()
        {
            StackPanel content = new StackPanel { Spacing = 2, Margin = new Thickness(14, 10, 14, 10) };

            Grid header = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };
            picBossRushDetails = MakeHelpButton();
            picBossRushDetails.Click += picBossRushDetails_Click;
            header.Children.Add(picBossRushDetails);
            TextBlock intro = MakeBlueLabel("Fight through every boss in the game with limited resources.");
            intro.TextWrapping = TextWrapping.Wrap;
            Grid.SetColumn(intro, 1);
            header.Children.Add(intro);
            content.Children.Add(header);

            Grid form = MakeFormGrid();
            form.Margin = new Thickness(0, 10, 0, 0);
            cmbBossRushDifficulty = MakeComboBox(new string[] { "Casual", "Hard", "Really hard", "Custom" }, 160);
            cmbBossRushDifficulty.SelectionChanged += cmbBossRushDifficulty_SelectedIndexChanged;
            AddFormRow(form, 0, "Difficulty:", cmbBossRushDifficulty);
            content.Children.Add(form);

            StackPanel chars = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 14,
                Margin = new Thickness(6, 2, 6, 4),
            };
            chkBossRushIncludeBoyChar = MakeCheckBox("Boy", true);
            chkBossRushIncludeGirlChar = MakeCheckBox("Girl", true);
            chkBossRushIncludeSpriteChar = MakeCheckBox("Sprite", true);
            chars.Children.Add(chkBossRushIncludeBoyChar);
            chars.Children.Add(chkBossRushIncludeGirlChar);
            chars.Children.Add(chkBossRushIncludeSpriteChar);
            groupBox4 = MakeGroupBox("Characters", chars);
            groupBox4.Margin = new Thickness(0, 10, 0, 0);
            groupBox4.HorizontalAlignment = HorizontalAlignment.Left;
            content.Children.Add(groupBox4);

            btnBossRushMoreOptions = MakeTextButton("More options", 130);
            btnBossRushMoreOptions.Click += btnBossRushMoreOptions_Click;
            btnBossRushMoreOptions.Margin = new Thickness(0, 12, 0, 0);
            btnBossRushMoreOptions.HorizontalAlignment = HorizontalAlignment.Left;
            content.Children.Add(btnBossRushMoreOptions);

            return MakeTabContent(content);
        }

        private Control BuildChaosTab()
        {
            StackPanel content = new StackPanel { Spacing = 2, Margin = new Thickness(14, 10, 14, 10) };

            Grid header = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };
            picChaosDetails = MakeHelpButton();
            picChaosDetails.Click += picChaosDetails_Click;
            header.Children.Add(picChaosDetails);
            TextBlock intro = MakeBlueLabel("Vanilla maps linked together randomly with increasing difficulty.");
            intro.TextWrapping = TextWrapping.Wrap;
            Grid.SetColumn(intro, 1);
            header.Children.Add(intro);
            content.Children.Add(header);

            Grid columns = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,32,*"),
                Margin = new Thickness(0, 10, 0, 0),
            };
            content.Children.Add(columns);

            StackPanel left = new StackPanel { Spacing = 2 };
            Grid form = MakeFormGrid();
            int row = 0;
            cmbChaosDifficulty = MakeComboBox(new string[] { "Casual", "Hard", "Really hard", "Custom" }, 160);
            cmbChaosDifficulty.SelectionChanged += cmbChaosDifficulty_SelectedIndexChanged;
            row = AddFormRow(form, row, "Difficulty:", cmbChaosDifficulty);
            trbChaosFloors = MakeSlider(1, 5, 3, 170);
            row = AddFormRow(form, row, "Length:", trbChaosFloors);
            trbChaosBosses = MakeSlider(1, 5, 3, 170);
            row = AddFormRow(form, row, "Bosses:", trbChaosBosses);
            left.Children.Add(form);

            StackPanel chars = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 14,
                Margin = new Thickness(6, 2, 6, 4),
            };
            chkChaosIncludeBoyChar = MakeCheckBox("Boy", true);
            chkChaosIncludeGirlChar = MakeCheckBox("Girl", true);
            chkChaosIncludeSpriteChar = MakeCheckBox("Sprite", true);
            chars.Children.Add(chkChaosIncludeBoyChar);
            chars.Children.Add(chkChaosIncludeGirlChar);
            chars.Children.Add(chkChaosIncludeSpriteChar);
            groupBox6 = MakeGroupBox("Characters", chars);
            groupBox6.Margin = new Thickness(0, 10, 0, 0);
            groupBox6.HorizontalAlignment = HorizontalAlignment.Left;
            left.Children.Add(groupBox6);
            Grid.SetColumn(left, 0);
            columns.Children.Add(left);

            StackPanel colors = new StackPanel { Spacing = 2, Margin = new Thickness(6, 2, 6, 4) };
            rdoChaosPalatteNone = new RadioButton { Content = "None" };
            rdoChaosPalatteReasonable = new RadioButton { Content = "Reasonable", IsChecked = true };
            rdoChaosPalatteRidiculous = new RadioButton { Content = "Ridiculous" };
            colors.Children.Add(rdoChaosPalatteNone);
            colors.Children.Add(rdoChaosPalatteReasonable);
            colors.Children.Add(rdoChaosPalatteRidiculous);
            groupBox5 = MakeGroupBox("Color randomization", colors);
            groupBox5.HorizontalAlignment = HorizontalAlignment.Left;
            groupBox5.VerticalAlignment = VerticalAlignment.Top;
            groupBox5.Margin = new Thickness(0, 0, 0, 0);
            Grid.SetColumn(groupBox5, 2);
            columns.Children.Add(groupBox5);

            btnChoasMoreOptions = MakeTextButton("More options", 130);
            btnChoasMoreOptions.Click += btnChoasMoreOptions_Click;
            btnChoasMoreOptions.Margin = new Thickness(0, 12, 0, 0);
            btnChoasMoreOptions.HorizontalAlignment = HorizontalAlignment.Left;
            content.Children.Add(btnChoasMoreOptions);

            return MakeTabContent(content);
        }

        private Border BuildDifficultyGroup()
        {
            StackPanel content = new StackPanel { Spacing = 4, Margin = new Thickness(12, 8, 12, 8) };

            Grid form = MakeFormGrid();
            int row = 0;
            cmbDifficultyValue = new ComboBox { Width = 220, HorizontalAlignment = HorizontalAlignment.Left };
            cmbDifficultyValue.SelectionChanged += cmbDifficultyValue_SelectedIndexChanged;
            row = AddFormRow(form, row, "Value:", cmbDifficultyValue);

            label12 = new TextBlock { Text = "Base Value" };
            nudDifficultyBaseValue = MakeNud(3, 65535m, 140);
            nudDifficultyBaseValue.ValueChanged += nudDifficultyBaseValue_ValueChanged;
            label13 = new TextBlock { Text = "Linear Growth" };
            nudDifficultyLinearGrowth = MakeNud(3, 65535m, 140);
            nudDifficultyLinearGrowth.ValueChanged += nudDifficultyLinearGrowth_ValueChanged;
            label16 = new TextBlock { Text = "Exponent" };
            nudDifficultyExponent = MakeNud(3, 65535m, 140);
            nudDifficultyExponent.ValueChanged += nudDifficultyExponent_ValueChanged;

            Grid growth = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,16,Auto,16,Auto"),
                Margin = new Thickness(0, 4, 0, 4),
            };
            StackPanel baseBox = new StackPanel { Spacing = 2 };
            baseBox.Children.Add(label12);
            baseBox.Children.Add(nudDifficultyBaseValue);
            StackPanel growthBox = new StackPanel { Spacing = 2 };
            growthBox.Children.Add(label13);
            growthBox.Children.Add(nudDifficultyLinearGrowth);
            StackPanel exponentBox = new StackPanel { Spacing = 2 };
            exponentBox.Children.Add(label16);
            exponentBox.Children.Add(nudDifficultyExponent);
            growth.Children.Add(baseBox);
            Grid.SetColumn(growthBox, 2);
            growth.Children.Add(growthBox);
            Grid.SetColumn(exponentBox, 4);
            growth.Children.Add(exponentBox);
            content.Children.Add(growth);

            Grid form2 = MakeFormGrid();
            int row2 = 0;
            cmbDifficultyVisualize = MakeComboBox(new string[]
            {
                "Physical damage to enemies",
                "Physical damage to you",
                "Number of hits to kill you",
                "Number of hits to kill enemies",
                "Magic damage to enemies",
                "Magic damage to you",
                "Number of spells to kill you",
                "Number of spells to kill enemies",
                "[Boss] Physical damage to enemies",
                "[Boss] Physical damage to you",
                "[Boss] Number of hits to kill you",
                "[Boss] Number of hits to kill enemies",
                "[Boss] Magic damage to enemies",
                "[Boss] Magic damage to you",
                "[Boss] Number of spells to kill you",
                "[Boss] Number of spells to kill enemies"
            }, 380);
            cmbDifficultyVisualize.SelectionChanged += cmbDifficultyVisualize_SelectedIndexChanged;
            row2 = AddFormRow(form2, row2, "Visualize:", cmbDifficultyVisualize);

            StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 2, 0, 2) };
            btnDifficultyLoad = MakeTextButton("Load Custom", 110);
            btnDifficultyLoad.Click += btnDifficultyLoad_Click;
            btnDifficultySave = MakeTextButton("Save Custom", 110);
            btnDifficultySave.Click += btnDifficultySave_Click;
            buttons.Children.Add(btnDifficultyLoad);
            buttons.Children.Add(btnDifficultySave);
            AddFormRow(form2, row2, null, buttons);
            content.Children.Add(form2);

            Grid form3 = MakeFormGrid();
            cmbVisualizationOption = MakeComboBox(new string[] { "Est. kills per floor", "Evade progression", "Armor progression" }, 190);
            cmbVisualizationOption.SelectionChanged += cmbVisualizationOption_SelectedIndexChanged;
            nudVisualizationOptionValue = MakeNud(0, 65535m, 110);
            nudVisualizationOptionValue.ValueChanged += nudVisualizationOptionValue_ValueChanged;
            StackPanel visRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            visRow.Children.Add(cmbVisualizationOption);
            visRow.Children.Add(nudVisualizationOptionValue);
            AddFormRow(form3, 0, "Visualization Option:", visRow);
            content.Children.Add(form3);

            picDifficultyGraph = new GraphControl { Height = 310, Margin = new Thickness(0, 8, 0, 0) };
            content.Children.Add(picDifficultyGraph);

            ScrollViewer scroller = new ScrollViewer
            {
                Content = content,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            Border border = MakeGroupBox("Custom Difficulty", scroller);
            border.Width = 544;
            border.VerticalAlignment = VerticalAlignment.Stretch;
            return border;
        }
    }
}
