namespace LemonRun.Rules
{
    /// <summary>
    /// A lane change travels: it is not a teleport.
    /// </summary>
    /// <remarks>
    /// Why travel rather than snap: at speed, a runner that jumps sideways from one lane to the
    /// next gives no reading of WHERE it passed, and a collision then looks arbitrary. The travel
    /// is short enough not to feel like steering, long enough for the eye to follow the path.
    ///
    /// The duration is tuning, not a constant: it is exactly the kind of value that has to be
    /// tried several times (<see cref="RunnerTuning.LaneChangeDuration"/>).
    /// </remarks>
    public static class LaneTravel
    {
        /// <summary>Advances a 0..1 progress by <paramref name="deltaTime"/>, never past 1.</summary>
        public static float Advance(float progress, float deltaTime, float duration)
        {
            if (duration <= 0f) return 1f;
            float next = progress + deltaTime / duration;
            return next > 1f ? 1f : next < 0f ? 0f : next;
        }

        /// <summary>Smoothstep: the change starts and ends softly, so it reads as a move, not a jerk.</summary>
        public static float Ease(float progress)
        {
            float t = progress < 0f ? 0f : progress > 1f ? 1f : progress;
            return t * t * (3f - 2f * t);
        }

        public static float PositionX(float fromX, float toX, float progress)
            => fromX + (toX - fromX) * Ease(progress);
    }
}
