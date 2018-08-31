using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReplayAnalyserLib.Judgement
{
    public class LengacyHitWindow:OsuHitWindows
    {
        public override void SetDifficulty(double difficulty)
        {
            base.SetDifficulty(difficulty);

            //https://osu.ppy.sh/help/wiki/osu!_File_Formats/Osu_(file_format)#overall-difficulty
            /*
            Meh = 150 + 50 * (5 - difficulty) / 5;
            Good = 100 + 40 * (5 - difficulty) / 5;
            Perfect = 50 + 30 * (5 - difficulty) / 5;
            */
            Meh -= 1;
            Great -= 1;
            Good -= 1;
        }
    }
}
