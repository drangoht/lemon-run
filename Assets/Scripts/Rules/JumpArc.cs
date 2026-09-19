namespace LemonRun.Rules
{
    /// <summary>
    /// Height of the runner during a jump: a fixed-duration arc, not a simulation.
    /// </summary>
    /// <remarks>
    /// Why an arc rather than physics: a jump whose length depends on gravity, mass and the frame
    /// rate cannot be placed against an obstacle with any certainty. Here the jump lasts exactly
    /// <see cref="RunnerTuning.JumpDuration"/> seconds whatever happens, so an obstacle either
    /// fits under it or does not -- and that is a design decision rather than a physics outcome.
    /// </remarks>
    public static class JumpArc
    {
        /// <summary>Height above the ground, zero outside the jump and at both its ends.</summary>
        public static float Height(float elapsed, float duration, float peak)
        {
            if (duration <= 0f || elapsed <= 0f || elapsed >= duration) return 0f;
            float t = elapsed / duration;
            return peak * 4f * t * (1f - t);
        }

        public static bool IsAirborne(float elapsed, float duration)
            => duration > 0f && elapsed > 0f && elapsed < duration;
    }
}
