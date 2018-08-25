using osu.Game.Rulesets.Osu.Objects;
using ReplayAnalyserLib.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReplayAnalyserLib
{
    public static class OsuHitObjectJudgement
    {
        /// <summary>
        /// 将在符合此物件时间范围内的鼠标动作全都提供给物件使用判断
        /// </summary>
        /// <param name="hitObject">要判断的物件</param>
        /// <param name="candidate_actions">没被其他物件处理过的符合物件击打时间内的鼠标动作</param>
        /// <returns>鼠标动作，其内元素可能已经标记物件</returns>
        public static IEnumerable<WrapperMouseAction> JudgeHitObject(OsuHitObject hitObject,IEnumerable<WrapperMouseAction> candidate_actions)
        {
            return null;
        }
    }
}
