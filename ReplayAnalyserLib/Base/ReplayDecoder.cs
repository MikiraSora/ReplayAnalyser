using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring.Legacy;
using ReplayAnalyserLib.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReplayAnalyserLib
{
    public class ReplayDecoder : LegacyScoreParser
    {
        private readonly string osr_Path;
        private readonly WorkingBeatmap beatmap;

        public ReplayDecoder(string osr_path, IBeatmap beatmap)
        {
            osr_Path = osr_path;
            this.beatmap = new MyWorkingBeatmap(beatmap);
        }

        protected override WorkingBeatmap GetBeatmap(string md5Hash)
        {
            return beatmap;
        }

        protected override Ruleset GetRuleset(int rulesetId)
        {
            return new OsuRuleset();
        }
    }
}
