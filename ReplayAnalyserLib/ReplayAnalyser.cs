using OpenTK;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Legacy;
using osu.Game.Rulesets.Scoring.Legacy;
using ReplayAnalyserLib.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReplayAnalyserLib
{
    public static class ReplayAnalyser
    {
        private static IBeatmap GetBeatmap(string osu_file)
        {
            var osu_streamReader = File.OpenText(osu_file);

            var beatmap = Decoder.GetDecoder<Beatmap>(osu_streamReader).Decode(osu_streamReader);

            var osu_converter = new OsuBeatmapConverter(beatmap);
            var converter_beatmap = osu_converter.Convert();

            //mumally parse
            foreach (var hitObject in converter_beatmap.HitObjects)
                hitObject.ApplyDefaults(converter_beatmap.ControlPointInfo, converter_beatmap.BeatmapInfo.BaseDifficulty);

            return converter_beatmap;
        }

        public static IEnumerable<WrapReplayFrame> BuildWrapReplyFrame(Replay replay)
        {
            WrapReplayFrame prev_frame = null;
            List<WrapReplayFrame> frames = new List<WrapReplayFrame>();

            foreach (OsuReplayFrame frame in replay.Frames)
            {
                var wrap = new WrapReplayFrame(frame);

                prev_frame?.SetNextFrame(wrap);
                wrap.SetPreviousFrame(prev_frame);

                frames.Add(wrap);
                prev_frame = wrap;
            }

            return frames;
        }

        public class WrapperMouseAction
        {
            public IEnumerable<WrapReplayFrame> Frames { get;private set; }

            public double StartTime => Frames.First().Time;
            public double EndTime => Frames.First().Time;

            public Vector2 StartPosition => Frames.First().Position;
            public Vector2 EndPosition => Frames.First().Position;
            
            /// <summary>
            /// 判断某时刻指针是否在某物件上
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="time"></param>
            /// <returns></returns>
            public bool Contains(HitObject obj,double time)
            {
                var frame = Frames.LastOrDefault(f => time > f.Time);
                var next_frame = frame.NextFrame;

                if (next_frame == null)
                    return false;

                //插值计算
                var cur_timestramp = (time - frame.Time) / (next_frame.Time - frame.Time);//归一化
                var temp_offset = (next_frame.Position - frame.Position);
                var cur_position = Vector2.Clamp(
                    new Vector2((float)(temp_offset.X * cur_timestramp), (float)(temp_offset.Y * cur_timestramp)),
                    frame.Position,
                    next_frame.Position
                    );

                if (true)
                {

                }

                return true;
            }
        }

        public static void Analyser(string osr_path,string osu_path)
        {
            osuElements.Replays.Replay replay = new osuElements.Replays.Replay(osr_path, true);

            var beatmap = GetBeatmap(osu_path);

            ReplayDecoder decoder = new ReplayDecoder(osr_path, beatmap);
            var score=decoder.Parse(File.OpenRead(osr_path));

            var frames = BuildWrapReplyFrame(score.Replay);

            int left = frames.Where(f => f.GetButtonState(OsuAction.LeftButton) == ButtonState.Click).Count();
            int right = frames.Where(f => f.GetButtonState(OsuAction.RightButton) == ButtonState.Click).Count();

            Console.WriteLine($"left:{left} right:{right}");
        }
    }
}
