using OpenTK;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Legacy;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using ReplayAnalyserLib.Base;
using ReplayAnalyserLib.Base.HitResultRecord;
using ReplayAnalyserLib.Judgement;
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

            PreprocessHitObjects(converter_beatmap.HitObjects);

            //mumally parse
            foreach (var hitObject in converter_beatmap.HitObjects)
                hitObject.ApplyDefaults(converter_beatmap.ControlPointInfo, converter_beatmap.BeatmapInfo.BaseDifficulty);

            return converter_beatmap;
        }

        public static IEnumerable<WrapReplayFrame> BuildWrapReplyFrames(Replay replay)
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

        public static Dictionary<OsuAction,List<WrapperMouseAction>> BuildWrapMouseActions(IEnumerable<WrapReplayFrame> wrapReplayFrames)
        {
            WrapperMouseAction current_action=null;
            Dictionary<OsuAction, List<WrapperMouseAction>> dic = new Dictionary<OsuAction, List<WrapperMouseAction>>();

            foreach (OsuAction action in Enum.GetValues(typeof(OsuAction)))
            {
                current_action = null;
                dic[action] = new List<WrapperMouseAction>();

                foreach (var frame in wrapReplayFrames)
                {
                    var btn_state = frame.GetButtonState(action);

                    switch (btn_state)
                    {
                        case ButtonState.Click:
                            if (current_action != null)
                                dic[action].Add(current_action);
                            current_action = new WrapperMouseAction();
                            current_action.Frames.Add(frame);
                            break;

                        case ButtonState.Hold:
                            current_action.Frames.Add(frame);
                            break;

                        case ButtonState.Release:
                        case ButtonState.None:
                        default:
                            break;
                    }
                }
                
                if (current_action != null)
                    dic[action].Add(current_action);
            }

            return dic;
        }

        public static void PreprocessHitObjects(IEnumerable<HitObject> objects)
        {
            foreach (var obj in objects)
            {
                obj.HitWindows = new LengacyHitWindow();
            }
        }
        
        public static void Analyser(string osr_path,string osu_path)
        {
            var beatmap = GetBeatmap(osu_path);

            ReplayDecoder decoder = new ReplayDecoder(osr_path, beatmap);
            var score=decoder.Parse(File.OpenRead(osr_path));

            var frames = BuildWrapReplyFrames(score.Replay);
            var mouse_actions = BuildWrapMouseActions(frames);

            int left = mouse_actions[OsuAction.LeftButton].Count;
            int right = mouse_actions[OsuAction.RightButton].Count;

            HitResultRecordCollection result_collection = new HitResultRecordCollection();

            Console.WriteLine($"left:{left} right:{right}");

            JudgementParam judgement_param = new JudgementParam()
            {
                Score=score,
                RawFrames=frames,
                MouseActions=mouse_actions,
                ResultCollection=result_collection
            };

            foreach (OsuHitObject obj in beatmap.HitObjects)
            {
                OsuHitObjectJudgement.Judge(obj,judgement_param);
            }
        }
    }
}
