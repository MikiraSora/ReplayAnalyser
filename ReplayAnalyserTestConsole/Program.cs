using System;

namespace ReplayAnalyserTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ReplayAnalyserLib.ReplayAnalyser.Analyser(@"G:\osu!\Replays\DarkProjector - MOMOIRO CLOVER Z - SANTA SAN [X-Mas] (2017-06-23) Osu.osr",
                @"G:\osu!\Songs\378183 MOMOIRO CLOVER Z - SANTA SAN\MOMOIRO CLOVER Z - SANTA SAN (monstrata) [X-Mas].osu");
        }
    }
}
