#nullable disable
using SoMRandomizer.config;
using SoMRandomizer.util;
using System.Collections.Generic;
using Avalonia.Media;

namespace SoMRandomizer.forms
{
    /// <summary>
    /// Utilities for graphing difficulty values for earlier modes.
    /// May not be particularly accurate.
    /// </summary>
    ///
    /// <remarks>Author: Moppleton</remarks>
    public class DifficultyVisualizer
    {
        public static void visualizePhysicalDamageToEnemies(GraphControl graph, DifficultySettings difficulty, int numFloors, bool numHits, int numKillsPerFloor, bool boss)
        {
            double scaleFactor = 24.0 / numFloors;
            List<int> valuesBoy = DamageCalculations.generatePhysicalDamageToEnemiesCurve(
                numFloors, 0, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_DEF),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EVADE),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_HP),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), false, numHits,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_HP_MULTIPLIER) : 1.0, scaleFactor
                );
            List<int> valuesGirl = DamageCalculations.generatePhysicalDamageToEnemiesCurve(
                numFloors, 1, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_DEF),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EVADE),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_HP),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), false, numHits,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_HP_MULTIPLIER) : 1.0, scaleFactor
                );
            List<int> valuesSprite = DamageCalculations.generatePhysicalDamageToEnemiesCurve(
                numFloors, 2, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_DEF),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EVADE),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_HP),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), false, numHits,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_HP_MULTIPLIER) : 1.0, scaleFactor
                );

            Dictionary<Color, List<int>> allValues = new Dictionary<Color, List<int>>();
            allValues.Add(Colors.Blue, valuesBoy);
            allValues.Add(Colors.Red, valuesGirl);
            allValues.Add(Colors.Green, valuesSprite);
            graph.SetData(allValues);
        }

        public static void visualizePhysicalDamageToPlayers(GraphControl graph, DifficultySettings difficulty, int numFloors, bool numHits, int numKillsPerFloor, int evadeProgression, int armorProgression, bool boss)
        {
            double scaleFactor = 24.0 / numFloors;
            List<int> valuesBoy = DamageCalculations.generatePhysicalDamageToPlayersCurve(
                numFloors, 0, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_STR),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_AGI),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_WEAPON_DAMAGE),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_WEAPON_LEV),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), false, numHits, evadeProgression, armorProgression,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0, scaleFactor
                );
            List<int> valuesGirl = DamageCalculations.generatePhysicalDamageToPlayersCurve(
                numFloors, 1, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_STR),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_AGI),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_WEAPON_DAMAGE),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_WEAPON_LEV),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), false, numHits, evadeProgression, armorProgression,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0, scaleFactor
                );
            List<int> valuesSprite = DamageCalculations.generatePhysicalDamageToPlayersCurve(
                numFloors, 2, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_STR),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_AGI),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_WEAPON_DAMAGE),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_WEAPON_LEV),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), false, numHits, evadeProgression, armorProgression,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0, scaleFactor
                );

            Dictionary<Color, List<int>> allValues = new Dictionary<Color, List<int>>();
            allValues.Add(Colors.Blue, valuesBoy);
            allValues.Add(Colors.Red, valuesGirl);
            allValues.Add(Colors.Green, valuesSprite);
            graph.SetData(allValues);
        }

        public static void visualizeMagicDamageToEnemies(GraphControl graph, DifficultySettings difficulty, int numFloors, bool numHits, int numKillsPerFloor, bool boss)
        {
            double scaleFactor = 24.0 / numFloors;
            // 2b, 3d
            List<int> valuesLow = DamageCalculations.generateMagicDamageToEnemiesCurve(
                numFloors, 0x2B, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_MDEF),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_MEVADE),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_HP),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), false, numHits,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_HP_MULTIPLIER) : 1.0, scaleFactor
                );
            List<int> valuesHigh = DamageCalculations.generateMagicDamageToEnemiesCurve(
                numFloors, 0x3D, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_MDEF),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_MEVADE),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_HP),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), false, numHits,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_HP_MULTIPLIER) : 1.0, scaleFactor
                );

            Dictionary<Color, List<int>> allValues = new Dictionary<Color, List<int>>();
            allValues.Add(Color.FromRgb(254, 0, 0), valuesLow);
            allValues.Add(Color.FromRgb(255, 0, 0), valuesHigh);
            graph.SetData(allValues);
        }

        public static void visualizeMagicDamageToPlayers(GraphControl graph, DifficultySettings difficulty, int numFloors, bool numHits, int numKillsPerFloor, int evadeProgression, int armorProgression, bool boss)
        {
            double scaleFactor = 24.0 / numFloors;
            // 2b, 3d
            List<int> valuesLowBoy = DamageCalculations.generateMagicDamageToPlayersCurve(
                numFloors, 0, 0x2B, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_INT),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_MAGIC_LEV),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), numHits, evadeProgression, armorProgression,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0, scaleFactor
                );
            List<int> valuesHighBoy = DamageCalculations.generateMagicDamageToPlayersCurve(
                numFloors, 0, 0x3D, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_INT),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_MAGIC_LEV),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), numHits, evadeProgression, armorProgression,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0, scaleFactor
                );
            List<int> valuesLowGirl = DamageCalculations.generateMagicDamageToPlayersCurve(
                numFloors, 1, 0x2B, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_INT),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_MAGIC_LEV),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), numHits, evadeProgression, armorProgression,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0, scaleFactor
                );
            List<int> valuesHighGirl = DamageCalculations.generateMagicDamageToPlayersCurve(
                numFloors, 1, 0x3D, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_INT),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_MAGIC_LEV),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), numHits, evadeProgression, armorProgression,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0, scaleFactor
                );
            List<int> valuesLowSprite = DamageCalculations.generateMagicDamageToPlayersCurve(
                numFloors, 2, 0x2B, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_INT),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_MAGIC_LEV),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), numHits, evadeProgression, armorProgression,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0, scaleFactor
                );
            List<int> valuesHighSprite = DamageCalculations.generateMagicDamageToPlayersCurve(
                numFloors, 2, 0x3D, numKillsPerFloor,
                difficulty.getGrowthValue(DifficultySettings.ENEMY_INT),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_MAGIC_LEV),
                difficulty.getGrowthValue(DifficultySettings.ENEMY_EXP),
                difficulty.getGrowthValue(DifficultySettings.MANA_POWER), numHits, evadeProgression, armorProgression,
                boss ? difficulty.getDoubleValue(DifficultySettings.BOSS_STAT_MULTIPLIER) : 1.0, scaleFactor
                );

            Dictionary<Color, List<int>> allValues = new Dictionary<Color, List<int>>();
            allValues.Add(Color.FromRgb(0, 0, 254), valuesLowBoy);
            allValues.Add(Color.FromRgb(0, 0, 255), valuesHighBoy);
            allValues.Add(Color.FromRgb(254, 0, 0), valuesLowGirl);
            allValues.Add(Color.FromRgb(255, 0, 0), valuesHighGirl);
            allValues.Add(Color.FromRgb(0, 254, 0), valuesLowSprite);
            allValues.Add(Color.FromRgb(0, 255, 0), valuesHighSprite);
            graph.SetData(allValues);
        }
    }
}
