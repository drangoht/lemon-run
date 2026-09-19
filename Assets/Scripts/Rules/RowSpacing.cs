namespace LemonRun.Rules
{
    /// <summary>
    /// How far apart two rows of obstacles must stand.
    /// </summary>
    /// <remarks>
    /// The spacing is <b>not</b> a distance, it is a duration. A gap of fifteen units is
    /// comfortable at twelve units per second and nearly unreadable at twenty-six: expressed in
    /// units, the road silently gets harder as the speed ramps, and the difficulty ends up coming
    /// from a number nobody chose. Expressed in seconds, the reaction window stays what it was
    /// decided to be, and any tightening is a deliberate act (GDD section 5).
    ///
    /// WARNING: the window must also cover the lane change itself, not only the decision. A
    /// window shorter than <c>LaneChangeDuration</c> leaves the runner still travelling when it
    /// reaches the row, and the hit then lands on a decision that was taken correctly.
    /// </remarks>
    public static class RowSpacing
    {
        /// <summary>Distance between two rows at this speed, floored by <paramref name="minimumGap"/>.</summary>
        public static float Gap(float speed, float reactionSeconds, float minimumGap)
        {
            float wanted = speed * reactionSeconds;
            return wanted < minimumGap ? minimumGap : wanted;
        }

        /// <summary>
        /// Seconds the runner would have to read a row met at this distance and speed.
        /// </summary>
        public static float WindowSeconds(float distanceAhead, float speed)
            => speed <= 0f ? float.PositiveInfinity : distanceAhead / speed;

        /// <summary>
        /// True when the window still leaves time to decide <b>and</b> to finish the lane change.
        /// </summary>
        public static bool LeavesTimeToAct(float distanceAhead, float speed, float laneChangeDuration)
            => WindowSeconds(distanceAhead, speed) >= laneChangeDuration;
    }
}
