namespace LemonRun.Rules
{
    /// <summary>
    /// The three lanes: numbering, clamping and lateral position.
    /// </summary>
    /// <remarks>
    /// Lane 0 is the leftmost. <see cref="Step"/> never wraps around: asking to go left from lane 0
    /// keeps lane 0. That refusal is deliberate -- wrapping would teleport the runner across the
    /// whole road on a mistyped key, and a death would stop being attributable to a readable
    /// decision (GDD section 2).
    ///
    /// WARNING: a refused input is invisible unless something says so on screen. The refusal is
    /// silent here on purpose -- it is the caller's job to show it -- but as long as nothing does,
    /// the player reads it as a dropped key rather than as an edge.
    /// </remarks>
    public static class Lanes
    {
        public const int Count = 3;

        /// <summary>Lane the player starts on: the middle one, so both sides cost the same.</summary>
        public const int Start = 1;

        public static int Clamp(int lane)
        {
            if (lane < 0) return 0;
            if (lane > Count - 1) return Count - 1;
            return lane;
        }

        /// <summary>Lane reached by moving <paramref name="direction"/> (negative = left).</summary>
        public static int Step(int lane, int direction)
        {
            int offset = direction < 0 ? -1 : direction > 0 ? 1 : 0;
            return Clamp(Clamp(lane) + offset);
        }

        /// <summary>True when the step would change nothing -- the runner is already on the edge.</summary>
        public static bool IsRefused(int lane, int direction) => Step(lane, direction) == Clamp(lane);

        /// <summary>Lateral position of a lane's centre, the middle lane sitting on x = 0.</summary>
        public static float CenterX(int lane, float laneWidth)
            => (Clamp(lane) - (Count - 1) * 0.5f) * laneWidth;
    }
}
