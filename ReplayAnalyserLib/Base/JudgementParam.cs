using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using ReplayAnalyserLib.Base.HitResultRecord;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReplayAnalyserLib.Base
{
    public class JudgementParam
    {
        //public OsuHitObject HitObject { get; set; }
        public Dictionary<OsuAction, List<WrapperMouseAction>> MouseActions { get; set; }
        public IEnumerable<WrapReplayFrame> RawFrames { get; set; }
        public HitResultRecordCollection ResultCollection { get; set; }
        public Score Score { get; set; }

        public Dictionary<string, object> Extra { get; set; } = new Dictionary<string, object>(); 
    }
}
