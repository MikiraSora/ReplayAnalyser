using System;

namespace ReplayAnalyserTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ReplayAnalyserLib.ReplayAnalyser.Analyser(@"G:\osu!\Replays\ - DJ TECHNORCH - Sol Cosine Job 2 [Hyper] (2018-08-24) Osu.osr",
                @"G:\osu!\Songs\83919 DJ TECHNORCH - Sol Cosine Job 2\DJ TECHNORCH - Sol Cosine Job 2 (ignorethis) [Hyper].osu");
        }
    }
}
