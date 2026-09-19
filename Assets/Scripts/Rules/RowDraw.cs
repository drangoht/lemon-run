namespace LemonRun.Rules
{
    /// <summary>One drawn row: what stands in each lane, and the opening it was built around.</summary>
    public struct DrawnRow
    {
        public Blocker[] Lanes;
        public int SafeLane;
    }

    /// <summary>
    /// Draws the rows of obstacles, <b>around a guaranteed opening</b> rather than at random.
    /// </summary>
    /// <remarks>
    /// Drawing each lane independently and rejecting the bad rows looks simpler and is a trap:
    /// on a hard setting most draws are rejected, the loop runs an unbounded number of times, and
    /// the rows that do come out are the least varied ones. Here the opening is placed first, one
    /// lane at most away from the previous one, and the rest is filled in around it. Every row is
    /// therefore survivable by construction -- there is nothing to reject and nothing to check at
    /// run time.
    ///
    /// The generator is deterministic: same seed, same road. That is what lets a balancing pass
    /// compare two settings on the <b>same</b> obstacle course, which is the only way the
    /// comparison means anything (GDD section 6).
    /// </remarks>
    public static class RowDraw
    {
        /// <summary>xorshift32 -- deterministic, identical on every platform, and enough for this.</summary>
        public static uint NextRandom(ref uint state)
        {
            if (state == 0) state = 0x9E3779B9;   // a zero state would stay zero for ever
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        static int Percent(ref uint state) => (int)(NextRandom(ref state) % 100u);

        /// <summary>
        /// Draws the next row. <paramref name="fullPercent"/> is the chance that a blocked lane is
        /// a full-height one rather than a jumpable one.
        /// </summary>
        public static DrawnRow Next(ref uint state, int previousSafeLane, int blockedPercent, int fullPercent)
        {
            var lanes = new Blocker[Lanes.Count];

            // The opening moves by at most one lane, so it is always reachable with a single
            // change from wherever the previous row left the runner.
            int drift = (int)(NextRandom(ref state) % 3u) - 1;
            int safe = Lanes.Clamp(Lanes.Clamp(previousSafeLane) + drift);
            lanes[safe] = Blocker.None;

            for (int lane = 0; lane < Lanes.Count; lane++)
            {
                if (lane == safe) continue;
                if (Percent(ref state) >= blockedPercent) continue;

                lanes[lane] = Percent(ref state) < fullPercent ? Blocker.Full : Blocker.Low;
            }

            return new DrawnRow { Lanes = lanes, SafeLane = safe };
        }
    }
}
