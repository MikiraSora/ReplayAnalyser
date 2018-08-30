using System;

namespace ReplayAnalyserTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ReplayAnalyserLib.ReplayAnalyser.Analyser(@"G:\osu!\Replays\DarkProjector - forget you [spinner test] (2018-08-28) Osu.osr",
                @"G:\osu!\Songs\yuki - forget you\yuki - forget you\audio (DarkProjector) [spinner test].osu");
        }
    }
}
