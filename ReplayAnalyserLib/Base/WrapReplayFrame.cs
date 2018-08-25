using OpenTK;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Replays;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ReplayAnalyserLib.Base
{
    public class WrapReplayFrame : OsuReplayFrame
    {
        public WrapReplayFrame PreviousFrame { get; set; }
        public WrapReplayFrame NextFrame { get; set; }

        public bool LeftButton { get => Actions.Contains(OsuAction.LeftButton); }
        public bool RightButton { get => Actions.Contains(OsuAction.RightButton); }

        public WrapReplayFrame(OsuReplayFrame frame) : base(frame.Time, frame.Position, frame.Actions.ToArray())
        {

        }

        /// <summary>
        /// 检查这一帧某个按钮的状态
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ButtonState GetButtonState(OsuAction action)
        {
            var prev_btn = (action == OsuAction.LeftButton ? PreviousFrame?.LeftButton : PreviousFrame?.RightButton) ?? false;
            var now_btn = action == OsuAction.LeftButton ? LeftButton :RightButton;

            if (prev_btn && now_btn)
                return ButtonState.Hold;
            if (!prev_btn && now_btn)
                return ButtonState.Click;
            if (prev_btn && !now_btn)
                return ButtonState.Release;

            //if (!prev_btn && !now_btn)
            return ButtonState.None;
        }

        public void SetNextFrame(WrapReplayFrame frame)
        {
            this.NextFrame = frame;
            if (frame != null)
                frame.PreviousFrame = this;
        }

        public void SetPreviousFrame(WrapReplayFrame frame)
        {
            this.PreviousFrame = frame;
            if (frame != null)
                frame.NextFrame = this;
        }

        public override string ToString() => $"time:{Time} pos:{Position} {(LeftButton ? "LEFT" : string.Empty)} {(RightButton ? "RIGHT" : string.Empty)}";
    }
}
