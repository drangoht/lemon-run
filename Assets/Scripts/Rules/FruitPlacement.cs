namespace LemonRun.Rules
{
    /// <summary>
    /// Where a fruit may be laid in a row -- and, above all, where it may not.
    /// </summary>
    /// <remarks>
    /// The rule comes straight from GDD section 5 and it is the one thing this file exists to
    /// enforce: <b>never on the lane that is already free</b>. A fruit on the opening hands the
    /// player back the antidote to the very constraint being applied; the run collapses into
    /// "stay on the empty lane and be fed for it", and the pillar of section 2 -- collecting IS
    /// how you survive -- stops meaning anything.
    ///
    /// So a fruit only ever sits on a lane blocked <b>low</b>. Taking it means leaving the
    /// opening, entering a blocked lane, and clearing it with a jump. Not on a full-height lane
    /// either: that one cannot be entered at all, and a reward nobody can reach is not a choice,
    /// it is decoration.
    /// </remarks>
    public static class FruitPlacement
    {
        /// <summary>No lane: the row carries no fruit.</summary>
        public const int None = -1;

        /// <summary>
        /// The lane a fruit goes on, or <see cref="None"/>.
        /// </summary>
        /// <param name="chancePercent">Chance a suitable row actually carries one.</param>
        public static int LaneFor(Blocker[] row, ref uint state, int chancePercent)
        {
            if (row == null) return None;
            if ((int)(RowDraw.NextRandom(ref state) % 100u) >= chancePercent) return None;

            // Count first, then pick: building a list would allocate on every row of an endless
            // run, and the collection would land at the worst possible moment.
            int candidates = 0;
            for (int lane = 0; lane < row.Length; lane++)
                if (row[lane] == Blocker.Low) candidates++;

            if (candidates == 0) return None;

            int chosen = (int)(RowDraw.NextRandom(ref state) % (uint)candidates);
            for (int lane = 0; lane < row.Length; lane++)
            {
                if (row[lane] != Blocker.Low) continue;
                if (chosen == 0) return lane;
                chosen--;
            }

            return None;
        }

        /// <summary>
        /// Whether the runner is high enough, and not too high, to swallow the fruit.
        /// </summary>
        /// <remarks>
        /// A band rather than a point. The choice the player makes is the <b>lane</b>, taken a
        /// second earlier and in full view; asking them to also land the apex on a given
        /// centimetre would move the decision into the jump, where nothing is readable. Clearing
        /// the obstacle under it should very nearly always mean taking the fruit.
        /// </remarks>
        public static bool IsWithinReach(float runnerHeight, float fruitHeight, float reach)
        {
            float gap = runnerHeight - fruitHeight;
            if (gap < 0f) gap = -gap;
            return gap <= reach;
        }
    }
}
