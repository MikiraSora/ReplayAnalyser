using OpenTK;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ReplayAnalyserLib.Base
{
    public class WrapperMouseAction:IComparable<WrapperMouseAction>
    {
        public List<WrapReplayFrame> Frames { get; private set; } = new List<WrapReplayFrame>();

        public double StartTime => Frames.First().Time;
        public double EndTime => Frames.Last().Time;

        public Vector2 StartPosition => Frames.First().Position;
        public Vector2 EndPosition => Frames.Last().Position;

        public OsuHitObject TriggedHitObject { get; set; } = null;

        public int CompareTo(WrapperMouseAction other) => Math.Sign(StartTime - other.StartTime);

        bool l = false;

        /// <summary>
        /// 判断某时刻指针是否在某物件上
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool Contains(OsuHitObject obj, double time,double? judgement_radius=null)
        {
            if (obj.StartTime==10752)
            {

            }

            var frame = Frames.LastOrDefault(f => time >= f.Time);

            if (frame==null)
                frame = Frames.First().PreviousFrame;

            var next_frame = frame?.NextFrame;

            if (next_frame == null)
                return false;
            
            //插值计算
            var cur_timestramp = (time - frame.Time) / (next_frame.Time - frame.Time);//归一化
            var temp_offset = (next_frame.Position - frame.Position)* (float)cur_timestramp;

            //指针位置
            var cur_position = frame.Position + temp_offset;

            cur_position=Vector2.Clamp(
                cur_position,
                frame.Position,
                next_frame.Position
                );

            var dist = Vector2.Distance(obj.Position, cur_position);

            return dist <= (judgement_radius??obj.Radius);
        }

        public override string ToString() => $"time:{StartTime}~{EndTime} pos:{StartPosition}~{EndPosition} [{Frames.Count}] " +
            $"{(TriggedHitObject!=null?$"HIT in ({TriggedHitObject.GetType().Name})({TriggedHitObject.StartTime}:{Math.Abs(TriggedHitObject.StartTime-StartTime)})":string.Empty)}";
    }
}
