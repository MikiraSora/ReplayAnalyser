using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Scoring;
using ReplayAnalyserLib.Base;
using ReplayAnalyserLib.Base.HitResultRecord;
using ReplayAnalyserLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static void Judge(OsuHitObject circle, JudgementParam param)
        {
            switch (circle)
            {
                case HitCircle c:
                    Judge(c, param);
                    break;
                case Slider c:
                    Judge(c, param);
                    break;
                case Spinner c:
                    Judge(c, param);
                    break;
                default:
                    return;
            }
        }

        public static void Judge(HitCircle obj, JudgementParam param)
        {
            if (obj.StartTime==10752)
            {

            }

            //obj.HitWindows.SetDifficulty(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);
            var miss_offset = obj.HitWindows.HalfWindowFor(HitResult.Miss);

            var list = new List<WrapperMouseAction>();

            var hitobject_radius = CalculateHitObjectRadius(param);

            foreach (var actions in param.MouseActions.Values)
            {
                //从队列里面列举出时间范围内没被处理过的鼠标动作
                var cond_actions = actions.Where(a =>
                Math.Abs(a.StartTime - obj.StartTime) <= miss_offset && //可击打时间范围内的
                a.TriggedHitObject == null && //没被其他物件处理的
                a.Contains(obj, obj.StartTime, hitobject_radius) //鼠标开始动作在物件内的
                );

                list.AddRange(cond_actions);
            }

            var select_action = list.Min();

            /*   |----0----| o  
             *        +    + + mouse_click
             *  hitobject  |
             *       miss_offset
             */
            if (select_action==null||select_action.StartTime > obj.StartTime + miss_offset)
            {
                //物件没被击打，将被当做miss处理
                return;
            }

            var hit_offset = Math.Abs(select_action.StartTime - obj.StartTime);
            var hit_result = obj.HitWindows.ResultFor(hit_offset);

            if (hit_result != HitResult.None) //HitResult.None是过于提前以至于没被当做击打
            {
                ApplyTrigHitObject(param.ResultCollection, obj, select_action, hit_result);
            }
        }

        #region Slider

        private static float CalculateHitObjectRadius(JudgementParam param)
        {
            var mods = param.Score.Mods;
            var cs = param.Score.Beatmap.BaseDifficulty.CircleSize;

            var SpriteDisplaySize = (float)(VirtualWindow.Width / 8f * (1f - 0.7f * ModApplyHelper.AdjustDifficulty(cs, mods)));

            var HitObjectRadius = SpriteDisplaySize / 2f / VirtualWindow.Ratio * VirtualWindow.broken_gamefield_rounding_allowance;

            return HitObjectRadius;
        }

        public static void Judge(Slider slider, JudgementParam param)
        {
            var list = new List<WrapperMouseAction>();
            var miss_offset = slider.HitWindows.HalfWindowFor(HitResult.Miss);

            foreach (var actions in param.MouseActions.Values)
            {
                //选取时间范围内的鼠标动作
                var cond_actions = actions.Where(a => a.StartTime+miss_offset>=slider.StartTime&&a.StartTime-miss_offset<slider.EndTime);

                list.AddRange(cond_actions);
            }

            list.Sort();

            //过滤一开始连续已经处理的鼠标动作,那列表第一个鼠标动作便是要钦定滑条头
            list.SkipWhile(a => a.TriggedHitObject != null);

            //储存此滑条子物件的击打结果，留到最后统一计算
            List<(OsuHitObject obj, HitResult result,WrapperMouseAction action)> hit_results = new List<(OsuHitObject obj, HitResult result, WrapperMouseAction action)>();

            //跟踪圈大小
            var hitobject_radius = CalculateHitObjectRadius(param);
            var track_radius = hitobject_radius * 2.4f;

            /*
             Sliders have an end, a beginning and ticks. 
             If you miss everything, you will get a miss. 
             If you miss more than 50% of the total parts, you will get a 50. 
             If you miss between 1 and 50% of the parts, you will get a 100. 
             Missing the start or a tick will reduce your combo to 0. 
             Missing the end will not. 
             Ticks will also increase your combo.
             */
            foreach (OsuHitObject sub_object in slider.NestedHitObjects)
            {
                switch (sub_object)
                {
                    /*少一个tail就当100*/
                    case SliderTailCircle tail:
                        var a = list.Where(r => r.Contains(sub_object, sub_object.StartTime, track_radius)).FirstOrDefault();

                        if (a==null)
                        {
                            //没鼠标指针钦定这个滑条尾，那就返回100
                            //ApplyTrigHitObject(param.ResultCollection, sub_object, null, HitResult.Good);
                            hit_results.Add((tail, HitResult.Good,a));
                        }
                        else
                            hit_results.Add((tail, HitResult.Great, a));

                        break;
                    /*少一个tick就当Miss*/
                    case SliderTick tick:
                        var b = list.Where(r => r.Contains(sub_object, sub_object.StartTime, track_radius)).FirstOrDefault();
                        
                        hit_results.Add((tick, b==null?HitResult.Miss:HitResult.Great,b));
                        break;
                   /*少一个head就当Miss*/
                    case SliderCircle head:
                        var c = list.Where(r => r.Contains(sub_object, r.StartTime, hitobject_radius)).FirstOrDefault();

                        //钦定一下
                        c.TriggedHitObject = slider;

                        hit_results.Add((head, c == null ? HitResult.Miss : HitResult.Great,c));
                        break;
                    /*少一个repeat就当Miss*/
                    case RepeatPoint repeat:
                        var d = list.Where(r => r.Contains(sub_object, sub_object.StartTime, track_radius)).FirstOrDefault();
                        hit_results.Add((repeat, d == null ? HitResult.Miss : HitResult.Great,d));
                        break;
                    default:
                        break;
                }
            }

            /*少一个head就当Miss,在ScoreV1中，滑条头不计入判断，也就是说滑条头要不就是300，要不就是Miss*/

            Func<HitObject,bool> filter_func = p => !(p is SliderTailCircle);

            //如果滑条尾之外的物件有Miss,或者没处理的，一律Miss
            if (hit_results.Where(p => !(p.obj is SliderTailCircle)).Any(hit => hit.result == HitResult.Miss) || 
                slider.NestedHitObjects.Where(filter_func).Except(
                    hit_results.Select(p => p.obj).Where(filter_func)
                    ).Any())
            {
                param.ResultCollection.AddResult(new HitResultRecord(HitResult.Miss,slider,null));
                return;
            }


            var tail_result = hit_results.FirstOrDefault(p => p.obj is SliderTailCircle);
            if (tail_result.obj == null)
            {
                param.ResultCollection.AddResult(new HitResultRecord(HitResult.Miss, slider, null));
                return;
            }

            param.ResultCollection.AddResult(new HitResultRecord(tail_result.result, slider, null));

            /*断尾
            if (tail_result.result == HitResult.Good)
                param.ResultCollection.AddResult(new HitResultRecord(HitResult.Miss, slider, null));
                */
        }

        #endregion

        public static void Judge(Spinner spinner, JudgementParam param)
        {
            SpinnerCounter counter = new SpinnerCounter(spinner, param.Score);

            //这次不用candidate_actions的东西，因为甩盘牵扯到所有动作
            IEnumerable<WrapReplayFrame> select_frames = param.RawFrames.Where(f => f.Time >= spinner.StartTime && f.Time < spinner.EndTime);

            //counter.AddFrame(select_frames.First().PreviousFrame);

            foreach (var frame in select_frames)
            {
                counter.AddFrame(frame);
            }

            //counter.AddFrame(select_frames.Last().NextFrame);

            HitResult hit_result = counter.Hit;

            ApplyTrigHitObject(param.ResultCollection, spinner, null, hit_result);
        }

        private static void ApplyTrigHitObject(HitResultRecordCollection result_collection, OsuHitObject hit_object,WrapperMouseAction action,HitResult reuslt)
        {
            if (action!=null)
                action.TriggedHitObject = hit_object;
            HitResultRecord record = new HitResultRecord(reuslt, hit_object, action);
            result_collection.AddResult(record);

            Debug.WriteLine($"Apply hit:{record}");
        }
    }
}
