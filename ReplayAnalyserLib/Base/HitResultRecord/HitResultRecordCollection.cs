using osu.Game.Rulesets.Scoring;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReplayAnalyserLib.Base.HitResultRecord
{
    public class HitResultRecordCollection:Dictionary<HitResult,List<HitResultRecord>>
    {
        public void AddResult(HitResultRecord record)
        {
            List<HitResultRecord> list;
            if (!TryGetValue(record.Result,out list))
            {
                list = new List<HitResultRecord>();
                this[record.Result] = list;
            }

            list.Add(record);
        }
    }
}
