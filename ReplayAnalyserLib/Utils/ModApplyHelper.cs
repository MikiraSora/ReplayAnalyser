using osu.Game.Rulesets.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReplayAnalyserLib.Utils
{
    public static class ModApplyHelper
    {
        public static double AdjustDifficulty(double difficulty,Mod[] mods)
        {
            return (ApplyModsToDifficulty(difficulty, 1.3, mods) - 5) / 5;
        }

        private static double ApplyModsToDifficulty(double difficulty, double hr_factor, Mod[] mods)
        {
            if (mods.Any(m=>m.ShortenedName=="EZ"))
                difficulty = Math.Max(0, difficulty / 2);
            if (mods.Any(m => m.ShortenedName == "HR"))
                difficulty = Math.Min(10, difficulty * hr_factor);

            return difficulty;
        }
    }
}
