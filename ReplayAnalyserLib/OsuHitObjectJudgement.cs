using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using ReplayAnalyserLib.Base;
using ReplayAnalyserLib.Base.HitResultRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReplayAnalyserLib
{
    public static class OsuHitObjectJudgement
    {
        /// <summary>
        /// 将在符合此物件时间范围内的鼠标动作全都提供给物件使用判断
        /// </summary>
        /// <param name="hitObject">要判断的物件</param>
        /// <param name="candidate_actions">没被其他物件处理过的符合物件击打时间内的鼠标动作</param>
        /// <returns>鼠标动作，其内元素可能已经标记物件</returns>
        /// 
        public static void Judge(OsuHitObject circle, IEnumerable<WrapperMouseAction> candidate_actions,IEnumerable<WrapReplayFrame> raw_frames, HitResultRecordCollection result_collection, Score score)
        {
            switch (circle)
            {
                case HitCircle c:
                    Judge(c, candidate_actions,raw_frames, result_collection,score);
                    break;
                case Slider c:
                    Judge(c, candidate_actions,raw_frames, result_collection, score);
                    break;
                case Spinner c:
                    Judge(c, candidate_actions,raw_frames, result_collection, score);
                    break;
                default:
                    return;
            }
        }

        public static void Judge(HitCircle obj, IEnumerable<WrapperMouseAction> candidate_actions, IEnumerable<WrapReplayFrame> raw_frames, HitResultRecordCollection result_collection,Score score)
        {
            var miss_offset = obj.HitWindows.HalfWindowFor(HitResult.Miss);
            var select_action = candidate_actions.Min();

            /*   |----0----| o  
             *        +    + + mouse_click
             *  hitobject  |
             *       miss_offset
             */
            if (select_action.StartTime > obj.StartTime + miss_offset)
            {
                //物件没被击打，将被当做miss处理
                return;
            }

            HitResult hit_result = obj.HitWindows.ResultFor(Math.Abs(select_action.StartTime - obj.StartTime));

            if (hit_result != HitResult.None) //HitResult.None是过于提前以至于没被当做击打
            {
                ApplyTrigHitObject(result_collection, obj, select_action, hit_result);
            }
        }

        public static void Judge(Slider slider, IEnumerable<WrapperMouseAction> candidate_actions, IEnumerable<WrapReplayFrame> raw_frames, HitResultRecordCollection result_collection, Score score)
        {

        }

        public static void Judge(Spinner spinner, IEnumerable<WrapperMouseAction> candidate_actions, IEnumerable<WrapReplayFrame> raw_frames, HitResultRecordCollection result_collection, Score score)
        {
            SpinnerCounter counter = new SpinnerCounter(spinner, score);

            //这次不用candidate_actions的东西，因为甩盘牵扯到所有动作
            IEnumerable<WrapReplayFrame> select_frames = raw_frames.Where(f => f.Time >= spinner.StartTime && f.Time < spinner.EndTime);

            //counter.AddFrame(select_frames.First().PreviousFrame);

            foreach (var frame in select_frames)
            {
                counter.AddFrame(frame);
            }

            //counter.AddFrame(select_frames.Last().NextFrame);

            HitResult hit_result = counter.Hit;

            ApplyTrigHitObject(result_collection, spinner, null, hit_result);
        }

        private static void ApplyTrigHitObject(HitResultRecordCollection result_collection, OsuHitObject hit_object,WrapperMouseAction action,HitResult reuslt)
        {
            if (action!=null)
                action.TriggedHitObject = hit_object;
            HitResultRecord record = new HitResultRecord(reuslt, hit_object, action);
            result_collection.AddResult(record);
        }
    }
}
