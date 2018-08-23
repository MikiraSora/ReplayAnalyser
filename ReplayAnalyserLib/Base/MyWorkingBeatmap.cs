using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReplayAnalyserLib.Base
{
    public class MyWorkingBeatmap : WorkingBeatmap
    {
        IBeatmap beatmap;

        public MyWorkingBeatmap(IBeatmap beatmap) : base(beatmap.BeatmapInfo)
        {
            this.beatmap = beatmap;
        }

        protected override Texture GetBackground()
        {
            throw new NotImplementedException();
        }

        protected override IBeatmap GetBeatmap()
        {
            return beatmap;
        }

        protected override Track GetTrack()
        {
            throw new NotImplementedException();
        }
    }
}
