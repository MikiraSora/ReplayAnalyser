using osu.Game.Rulesets.Mods;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReplayAnalyserLib.Utils
{
    public static class VirtualWindow
    {
        public const int DEFAULT_WIDTH = 512;
        public const int DEFAULT_HEIGHT = 384;

        //Builds of osu! up to 2013-05-04 had the gamefield being rounded down, which caused incorrect radius calculations
        //in widescreen cases. This ratio adjusts to allow for old replays to work post-fix, which in turn increases the lenience
        //for all plays, but by an amount so small it should only be effective in replays.
        public const float broken_gamefield_rounding_allowance = 1.00041f;

        public static float Width { get; set; } = 819.2f;
        public static float Height { get; set; } = 614.4f;

        public static float Ratio => Height / DEFAULT_HEIGHT;

        /*
        public static float GetSpriteDisplaySize(float cs, Mod[] mods) => (float)(Width / 8f * (1f - 0.7f * ModApplyHelper.AdjustDifficulty(cs, mods)));

        public static float HitObjectRadius = SpriteDisplaySize / 2f / GameBase.GameField.Ratio* broken_gamefield_rounding_allowance;
        */
    }
}
