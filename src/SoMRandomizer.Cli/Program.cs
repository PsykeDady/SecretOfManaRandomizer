using SoMRandomizer.config.settings;
using SoMRandomizer.processing.ancientcave;
using SoMRandomizer.processing.bossrush;
using SoMRandomizer.processing.chaos;
using SoMRandomizer.processing.common;
using SoMRandomizer.processing.openworld;
using SoMRandomizer.processing.vanillarando;
using SoMRandomizer.util;
using System;
using System.Collections.Generic;

namespace SoMRandomizer.Cli
{
    /// <summary>
    /// Command-line ROM generator. Works for all modes (open world, vanilla rando,
    /// ancient cave, boss rush, chaos) selected via the "mode" option.
    ///
    /// Required arguments (key=value):
    ///   srcRom="path"   dstRom="path"   seed="value"   options="..."
    ///
    /// The options string has the same format produced by the UI options manager:
    ///   version=1.49 mode=open key=value key=value ...
    /// </summary>
    static class Program
    {
        static int Main()
        {
            // arg [0] is the path to the binary
            string[] cmdLine = Environment.GetCommandLineArgs();
            try
            {
                Dictionary<string, string> cmdArgsProcessed = CmdArgParser.processCmdArgs(cmdLine);

                if (!cmdArgsProcessed.ContainsKey("srcRom"))
                {
                    Console.WriteLine("missing srcRom=(path)");
                    return 1;
                }
                if (!cmdArgsProcessed.ContainsKey("dstRom"))
                {
                    Console.WriteLine("missing dstRom=(path)");
                    return 1;
                }
                if (!cmdArgsProcessed.ContainsKey("seed"))
                {
                    Console.WriteLine("missing seed=(value)");
                    return 1;
                }
                if (!cmdArgsProcessed.ContainsKey("options"))
                {
                    Console.WriteLine("missing options=(value)");
                    return 1;
                }

                // process individual options, similar to how OptionsManager does it for the UI
                string[] allEntries = cmdArgsProcessed["options"].Trim().Split(new char[] { ' ' });
                Dictionary<string, string> allEntriesMap = new Dictionary<string, string>();
                foreach (string entry in allEntries)
                {
                    string str = entry.Trim();
                    if (str.Length == 0)
                    {
                        continue;
                    }
                    int equalsIndex = str.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        allEntriesMap[str.Substring(0, equalsIndex)] = str.Substring(equalsIndex + 1);
                    }
                    else
                    {
                        Console.WriteLine("Unexpected string: " + entry);
                        return 1;
                    }
                }

                // determine mode from the options; defaults to open world if not specified
                string modeKey = allEntriesMap.ContainsKey(CommonSettings.PROPERTYNAME_MODE)
                    ? allEntriesMap[CommonSettings.PROPERTYNAME_MODE]
                    : OpenWorldSettings.MODE_KEY;

                // create default settings and apply our overrides
                CommonSettings commonSettings = new CommonSettings();
                RandoSettings modeSettings = createSettings(modeKey, commonSettings);

                // set a few common options for the log that the UI normally sets
                commonSettings.set(CommonSettings.PROPERTYNAME_MODE, modeKey);
                commonSettings.set(CommonSettings.PROPERTYNAME_ALL_ENTERED_OPTIONS, cmdArgsProcessed["options"]);
                commonSettings.set(CommonSettings.PROPERTYNAME_VERSION, RomGenerator.VERSION_NUMBER);

                modeSettings.processNewSettings(allEntriesMap);

                RomGenerator generator = createGenerator(modeKey);
                Dictionary<string, RomGenerator> generatorsByRomType = new Dictionary<string, RomGenerator> { { modeKey, generator } };
                Dictionary<string, RandoSettings> settingsByRomType = new Dictionary<string, RandoSettings> { { modeKey, modeSettings } };

                // run rom generation
                // note there are no checks here for whether the dstRom exists - it will overwrite
                try
                {
                    RomGenerator.initGeneration(cmdArgsProcessed["srcRom"], cmdArgsProcessed["dstRom"], cmdArgsProcessed["seed"], generatorsByRomType, commonSettings, settingsByRomType);
                    Console.WriteLine("done!");
                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                    return 1;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("exception encountered: " + ee.Message);
                return 1;
            }
        }

        private static RandoSettings createSettings(string modeKey, CommonSettings commonSettings)
        {
            switch (modeKey)
            {
                case VanillaRandoSettings.MODE_KEY:
                    return new VanillaRandoSettings(commonSettings);
                case OpenWorldSettings.MODE_KEY:
                    return new OpenWorldSettings(commonSettings);
                case AncientCaveSettings.MODE_KEY:
                    return new AncientCaveSettings(commonSettings);
                case BossRushSettings.MODE_KEY:
                    return new BossRushSettings(commonSettings);
                case ChaosSettings.MODE_KEY:
                    return new ChaosSettings(commonSettings);
                default:
                    throw new Exception("Unknown mode: " + modeKey);
            }
        }

        private static RomGenerator createGenerator(string modeKey)
        {
            switch (modeKey)
            {
                case VanillaRandoSettings.MODE_KEY:
                    return new VanillaRandoGenerator();
                case OpenWorldSettings.MODE_KEY:
                    return new OpenWorldGenerator();
                case AncientCaveSettings.MODE_KEY:
                    return new AncientCaveGenerator();
                case BossRushSettings.MODE_KEY:
                    return new BossRushGenerator();
                case ChaosSettings.MODE_KEY:
                    return new ChaosGenerator();
                default:
                    throw new Exception("Unknown mode: " + modeKey);
            }
        }
    }
}
