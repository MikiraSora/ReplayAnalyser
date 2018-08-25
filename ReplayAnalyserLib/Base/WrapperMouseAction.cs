using OpenTK;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// 判断某时刻指针是否在某物件上
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool Contains(OsuHitObject obj, double time)
        {
            var frame = Frames.LastOrDefault(f => time >= f.Time);
            var next_frame = frame?.NextFrame;

            if (next_frame == null)
                return false;

            //插值计算
            var cur_timestramp = (time - frame.Time) / (next_frame.Time - frame.Time);//归一化
            var temp_offset = (next_frame.Position - frame.Position);

            //指针位置
            var cur_position = Vector2.Clamp(
                new Vector2((float)(temp_offset.X * cur_timestramp), (float)(temp_offset.Y * cur_timestramp)),
                frame.Position,
                next_frame.Position
                );

            return Vector2.Distance(obj.Position, cur_position) <= obj.Radius;
        }

        public override string ToString() => $"time:{StartTime}~{EndTime} pos:{StartPosition}~{EndPosition} [{Frames.Count}] " +
            $"{(TriggedHitObject!=null?$"HIT in ({TriggedHitObject.GetType().Name})({TriggedHitObject.StartTime}:{Math.Abs(TriggedHitObject.StartTime-StartTime)})":string.Empty)}";
    }
}
