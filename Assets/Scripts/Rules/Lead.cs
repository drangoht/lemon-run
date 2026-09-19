namespace LemonRun.Rules
{
    /// <summary>
    /// The lead separating the runner from its pursuer -- the only resource in the game.
    /// </summary>
    /// <remarks>
    /// Held in <b>seconds</b>, not in units. A lead of eight units means something very different
    /// at twelve units per second and at twenty-six: expressed in distance, the threat would
    /// quietly change meaning all the way up the speed ramp, exactly as the row spacing would
    /// (see <see cref="RowSpacing"/>). In seconds, "he is two seconds behind" says the same thing
    /// at the start of the run and at the end of it.
    ///
    /// WARNING: nothing here makes the lead come back on its own, and that is deliberate -- GDD
    /// section 7 turned down passive regeneration, which splits the game into two loops and makes
    /// the fruit decorative. <see cref="AfterGain"/> exists for the fruit, and until the fruit
    /// exists the lead only ever falls.
    /// </remarks>
    public static class Lead
    {
        /// <summary>What a hit leaves, never below nothing.</summary>
        public static float AfterHit(float lead, float cost)
        {
            float left = lead - cost;
            return left < 0f ? 0f : left;
        }

        /// <summary>
        /// What swallowing a fruit buys back, capped: the pursuer can be pushed away, never lost.
        /// </summary>
        /// <remarks>
        /// The cap is what stops a good run from turning into a safe one. Without it a player
        /// ahead on fruit could bank an untouchable lead, and the threat -- the whole opposition
        /// of the game -- would simply stop existing for the rest of the session.
        /// </remarks>
        public static float AfterGain(float lead, float gain, float maximum)
        {
            float raised = lead + gain;
            return raised > maximum ? maximum : raised;
        }

        public static bool IsCaught(float lead) => lead <= 0f;

        /// <summary>Share of the maximum lead still held, 0 to 1 -- what the gauge shows.</summary>
        public static float Fraction(float lead, float maximum)
        {
            if (maximum <= 0f) return 0f;
            float share = lead / maximum;
            return share < 0f ? 0f : share > 1f ? 1f : share;
        }

        /// <summary>
        /// How far behind the runner to DRAW the pursuer, in world units.
        /// </summary>
        /// <remarks>
        /// WARNING: this is deliberately NOT the lead converted into distance. At full lead the
        /// true gap would put the pursuer behind the camera, where the player cannot see it -- and
        /// a threat nobody sees does not exist, whatever the gauge says. So the gauge carries the
        /// truth and the pursuer carries the reading of it: it is mapped into a band that always
        /// stays in frame, far edge at full lead, breathing down the runner's neck at nothing.
        ///
        /// The decoupling is a real cost: the distance on screen is not a distance in the rules,
        /// so it cannot be used to judge whether a hit is survivable. The gauge is the only thing
        /// that may be read for that.
        /// </remarks>
        public static float DrawGap(float lead, float maximum, float nearest, float furthest)
            => nearest + (furthest - nearest) * Fraction(lead, maximum);
    }
}
