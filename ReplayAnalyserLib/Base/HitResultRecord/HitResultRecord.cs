using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReplayAnalyserLib.Base.HitResultRecord
{
    public struct HitResultRecord
    {
        public HitResult Result { get; private set; }
        
        public OsuHitObject TrigHitObject { get; private set; }

        public WrapperMouseAction TrigMouseAction { get; private set; }

        public HitResultRecord(HitResult result,OsuHitObject @object,WrapperMouseAction action)
        {
            Result = result;
            TrigHitObject = @object;
            TrigMouseAction = action;
        }

        public override string ToString() => $"{TrigHitObject.StartTime}({Math.Abs(TrigMouseAction.StartTime-TrigHitObject.StartTime)} {Result})";
    }
}
