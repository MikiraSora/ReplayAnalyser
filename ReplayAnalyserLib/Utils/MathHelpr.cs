using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReplayAnalyserLib.Utils
{
    public static class MathHelper
    {
        //我他妈洗的什么gaybar
        public static bool IsLineInCircle(Vector2 line_a,Vector2 line_b, Vector2 circle_p,double circle_r)
        {
            if (Vector2.Distance(line_a, circle_p) < circle_r || Vector2.Distance(line_b, circle_p) < circle_r)//任意一点在圈内
                return true;

            if (line_a.X == line_b.X)//竖线，直接x判断
                return Math.Abs(circle_p.X - line_a.X) <= circle_r;

            if (line_a.Y == line_b.Y)//横线，直接y判断
                return Math.Abs(circle_p.Y - line_a.Y) <= circle_r;

            var A = line_a.Y - line_b.Y;
            var B = line_b.X - line_a.X;
            var C = line_a.X * line_b.Y - line_b.X * line_a.Y;

            var d = Math.Abs(A * circle_p.X + B * circle_p.Y + C) / Math.Sqrt(A * A + B * B);
            return d <= circle_r;
        }
    }
}
