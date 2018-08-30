using OpenTK;
using osu.Framework.Input.States;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReplayAnalyserLib.Base
{
    //记录转盘
    //尝试抄旧版屙屎的，咕咕
    //8/28 操你妈算不对 先鸽着
    public class SpinnerCounter
    {
        private double lastMouseAngle;
        private double totalScoreFrameVariance;
        private double velocityTheoretical;
        private int zeroCount;
        private int rotationCount => (int)floatRotationCount;
        private int lastRotationCount;

        public HitResult Hit { get {
                if (scoringRotationCount <= rotationRequirement - 1)
                    return HitResult.Miss;

                if (scoringRotationCount > rotationRequirement + 1) return HitResult.Great;
                if (scoringRotationCount > rotationRequirement) return HitResult.Good;
                return HitResult.Meh;
            }
        }

        public int Spins => (int)TotalAngle / 360;

        public double TotalAngle { get; private set; } = 0;

        public bool IsRX { get; private set; }
        public bool IsDT { get; private set; }
        public bool IsSO { get; private set; }
        public bool IsAP { get; private set; }
        public bool IsHT { get; private set; }
        public Spinner Spinner { get; }
        public double SpinnerRotationRatio { get; }

        private List<WrapReplayFrame> RecordFrames { get; } = new List<WrapReplayFrame>();

        private const double SIXTY_FRAME_TIME = (double)1000 / 60;

        public SpinnerCounter(Spinner spinner,Score score)
        {
            IsRX = score.Mods.Any(m => m.ShortenedName == "RX");
            IsDT = score.Mods.Any(m => m.ShortenedName == "DT");
            IsSO = score.Mods.Any(m => m.ShortenedName == "SO");
            IsAP = score.Mods.Any(m => m.ShortenedName == "AP");
            IsHT = score.Mods.Any(m => m.ShortenedName == "HT");
            Spinner = spinner;

            SpinnerRotationRatio= BeatmapDifficulty.DifficultyRange(score.Beatmap.BaseDifficulty.OverallDifficulty, 3, 5, 7.5);

            rotationRequirement = (int)((float)spinner.Duration / 1000 * SpinnerRotationRatio);
            
            maxAccel = 0.00008 + Math.Max(0, (5000 - (double)spinner.Duration) / 1000 / 2000);
        }

        public double ApplyModsToTime(double time)
        {
            if (IsDT)
            {
                return time / 1.5;
            }
            else if (IsHT)
            {
                return time / 0.75;
            }
            else
            {
                return time;
            }
        }

        public void AddFrame(WrapReplayFrame frame)
        {
            ProcessRotation();
            ProcessMove(frame);
        }

        static readonly Vector2 adjust_center = new Vector2(240,160);

        private void ProcessMove(WrapReplayFrame frame)
        {
            RecordFrames.Add(frame);
            var currentMousePos = frame.Position;

            //将鼠标坐标换成相对于中间的坐标
            Vector2 mouseVector = currentMousePos - adjust_center;

            double mouseAngle = Math.Atan2(mouseVector.Y, mouseVector.X);
            double mouseAngleDiff = mouseAngle - lastMouseAngle;

            if (mouseAngle - lastMouseAngle < -Math.PI)
                mouseAngleDiff = (2 * Math.PI) + mouseAngle - lastMouseAngle;
            else if (lastMouseAngle - mouseAngle < -Math.PI)
                mouseAngleDiff = (-2 * Math.PI) - lastMouseAngle + mouseAngle;

            double timeDiff = frame.PreviousFrame!=null ? frame.Time - frame.PreviousFrame.Time : 0;

            double decay = Math.Pow(0.999, timeDiff);
            totalScoreFrameVariance = decay * totalScoreFrameVariance + (1 - decay) * timeDiff;

            if (mouseAngleDiff == 0)
                velocityTheoretical = zeroCount++ < 1 ? velocityTheoretical / 3 : 0;
            else
            {
                zeroCount = 0;

                if (! IsRX&& ((frame.LeftButton && frame.RightButton) || frame.Time < Spinner.StartTime || frame.Time > Spinner.EndTime))
                    mouseAngleDiff = 0;

                if (Math.Abs(mouseAngleDiff) < Math.PI)
                {
                    if (ApplyModsToTime(totalScoreFrameVariance) > SIXTY_FRAME_TIME * 1.04f)
                    {
                        //after a certain lenience we need to stop allowing for SIXTY_FRAMEs and take frames for their actual elapsed time.
                        //this is to handle the case where users are running at sub-60fps.
                        //in a simple world, we could always use this timeDiff calculation, but due to historical reasons,
                        //we were always slightly in the user's favour when calculating velocity here.
                        velocityTheoretical = mouseAngleDiff / ApplyModsToTime(timeDiff);
                    }
                    else
                        velocityTheoretical = mouseAngleDiff / SIXTY_FRAME_TIME;
                }
                else
                    velocityTheoretical = 0;
            }

            lastMouseAngle = mouseAngle;
            
            if (rotationCount != lastRotationCount)
            {
                scoringRotationCount++;
                lastRotationCount = rotationCount;

            }
        }

        private void ProcessRotation()
        {
            var elapsed_frame_time = 0d;

            var last_frame = RecordFrames.LastOrDefault();
            elapsed_frame_time = last_frame?.PreviousFrame == null ? SIXTY_FRAME_TIME : last_frame.Time - last_frame.PreviousFrame.Time;

            double decay = Math.Pow(0.9, elapsed_frame_time / SIXTY_FRAME_TIME);
            rpm = rpm * decay + (1.0 - decay) * (Math.Abs(velocityCurrent) * 1000) / (Math.PI * 2) * 60;

            // Mod time is applied here to keep discrepancies between DT, HT and nomod to preserve integrity of older scores. :(
            double maxAccelThisFrame = ApplyModsToTime(maxAccel * elapsed_frame_time);

            if (IsSO || IsRX)
                velocityCurrent = 0.03;
            else if (velocityTheoretical > velocityCurrent)
                velocityCurrent += Math.Min(velocityTheoretical - velocityCurrent, velocityCurrent < 0 && IsRX ? maxAccelThisFrame / RELAX_BONUS_ACCEL : maxAccelThisFrame);
            else
                velocityCurrent += Math.Max(velocityTheoretical - velocityCurrent, velocityCurrent > 0 && IsRX ? -maxAccelThisFrame / RELAX_BONUS_ACCEL : -maxAccelThisFrame);


            velocityCurrent = Math.Max(-0.05, Math.Min(velocityCurrent, 0.05));

            float rotationAddition = (float)(velocityCurrent * elapsed_frame_time);
            floatRotationCount += Math.Abs((float)(rotationAddition / Math.PI));
        }

        #region IncreaseScoreType

        [Flags]
        internal enum IncreaseScoreType
        {
            MissManiaHoldBreak = -1048576,
            MissHpOnlyNoCombo = -524288,
            MissHpOnly = -262144,
            Miss = -131072,
            MissMania = -65536,
            Ignore = 0,
            MuAddition = 1,
            KatuAddition = 2,
            GekiAddition = 4,
            SliderTick = 8,
            FruitTickTiny = 16,
            FruitTickTinyMiss = 32,
            SliderRepeat = 64,
            SliderEnd = 128,
            Hit50 = 256,
            Hit100 = 512,
            Hit300 = 1024,
            Hit50m = Hit50 | MuAddition,
            Hit100m = Hit100 | MuAddition,
            Hit300m = Hit300 | MuAddition,
            Hit100k = Hit100 | KatuAddition,
            Hit300k = Hit300 | KatuAddition,
            Hit300g = Hit300 | GekiAddition,
            FruitTick = 2048,
            SpinnerSpin = 4096,
            SpinnerSpinPoints = 8192,
            SpinnerBonus = 16384,
            TaikoDrumRoll = 32768,
            TaikoLargeHitBoth = 65536,
            TaikoDenDenHit = 1048576,
            TaikoDenDenComplete = 2097152,
            TaikoLargeHitFirst = 4194304,
            TaikoLargeHitSecond = 8388608,
            ManiaHit50 = 16777216,
            ManiaHit100 = 33554432,
            ManiaHit200 = 67108864,
            ManiaHit300 = 134217728,
            ManiaHit300g = 268435456,    //g means glow, so there's no GekiAddition
            BaseHitValuesOnly = Hit50 | Hit100 | Hit300,
            HitValuesOnly = Hit50 | Hit100 | Hit300 | GekiAddition | KatuAddition | ManiaHit300g | ManiaHit300 | ManiaHit200 | ManiaHit100 | ManiaHit50,
            ComboAddition = MuAddition | KatuAddition | GekiAddition,
            NonScoreModifiers = TaikoLargeHitBoth | TaikoLargeHitFirst | TaikoLargeHitSecond
        }

        List<IncreaseScoreType> record_list = new List<IncreaseScoreType>();
        private int scoringRotationCount;
        private int rotationRequirement;
        private double maxAccel;
        private double rpm;
        const int RELAX_BONUS_ACCEL = 4;
        private double velocityCurrent;
        private float floatRotationCount;

        /// <summary>
        /// 总感觉没啥用
        /// </summary>
        /// <param name="type"></param>
        private void AddBound(IncreaseScoreType type)
        {
            switch (type)
            {
                case IncreaseScoreType.SpinnerSpin:
                case IncreaseScoreType.SpinnerSpinPoints:
                case IncreaseScoreType.SpinnerBonus:
                    record_list.Add(type);
                    break;
                
                default:
                    throw new Exception("Unknown IncreaseScoreType for Spinner.");
            }
        }

        #endregion

    }

        /*
    public class SpinnerCounter
    {
        private readonly Spinner spinner;

        public SpinnerCounter(Spinner s)
        {
            spinner = s;
        }
        
        public float Rotation { get; set; }

        private float lastAngle;
        private float currentRotation;
        public float RotationAbsolute;
        private int completeTick;

        private bool updateCompleteTick() => completeTick != (completeTick = (int)(RotationAbsolute / 360));

        private bool rotationTransferred = false;

        public void Update(WrapReplayFrame frame)
        {
            var mousePosition = frame.Position-new Vector2(320,240);

            var thisAngle = -(float)MathHelper.RadiansToDegrees(Math.Atan2(mousePosition.X, mousePosition.Y));

            bool validAndTracking = spinner.StartTime <= frame.Time && spinner.EndTime > frame.Time;

            if (validAndTracking)
            {
                if (!rotationTransferred)
                {
                    currentRotation = Rotation * 2;
                    rotationTransferred = true;
                }

                if (thisAngle - lastAngle > 180)
                    lastAngle += 360;
                else if (lastAngle - thisAngle > 180)
                    lastAngle -= 360;

                currentRotation += thisAngle - lastAngle;
                RotationAbsolute += Math.Abs(thisAngle - lastAngle);
            }

            lastAngle = thisAngle;
        }
    }
    */
}
