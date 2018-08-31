using System;

namespace ReplayAnalyserTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ReplayAnalyserLib.ReplayAnalyser.Analyser(@"G:\osu!\Replays\DarkProjector - 3R2 - Bunny Panic!!! [Hard] (2018-08-31) Osu.osr",
                @"G:\osu!\Songs\573894 3R2 - Bunny Panic!!!\3R2 - Bunny Panic!!! (Kyubey) [Hard].osu");
        }
    }
}
