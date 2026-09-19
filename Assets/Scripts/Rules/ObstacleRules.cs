namespace LemonRun.Rules
{
    /// <summary>What occupies a lane at a given row.</summary>
    public enum Blocker
    {
        None = 0,

        /// <summary>Low enough to be jumped -- or simply gone round.</summary>
        Low = 1,

        /// <summary>Too tall to jump: the only answer is another lane.</summary>
        Full = 2,
    }

    /// <summary>
    /// What a row of obstacles allows, and what it costs to read it wrong.
    /// </summary>
    /// <remarks>
    /// Everything here serves one rule of the GDD (section 2): a death must be attributable to
    /// <b>one readable decision</b>. A row that cannot be passed at all, or whose only opening is
    /// two lane changes away when there is time for one, breaks that rule -- and it breaks it
    /// silently, since the player simply dies without ever seeing the mistake.
    /// </remarks>
    public static class ObstacleRules
    {
        /// <summary>A lane can be got through when it is free, or low enough to jump.</summary>
        public static bool CanPass(Blocker blocker) => blocker != Blocker.Full;

        /// <summary>True when at least one lane of the row can be got through.</summary>
        public static bool IsPassable(Blocker[] row)
        {
            if (row == null) return true;
            for (int lane = 0; lane < row.Length; lane++)
                if (CanPass(row[lane])) return true;
            return false;
        }

        /// <summary>
        /// True when the row can be got through from <paramref name="fromLane"/> using at most one
        /// lane change.
        /// </summary>
        /// <remarks>
        /// Passable is not enough. An opening on lane 0 while the runner is on lane 2 is a death
        /// the player could not avoid, and it looks exactly like a death they could have -- which
        /// is worse than an obvious unfairness, because they will look for their own mistake.
        /// </remarks>
        public static bool IsSurvivable(Blocker[] row, int fromLane)
        {
            if (row == null) return true;

            int from = Lanes.Clamp(fromLane);
            for (int lane = from - 1; lane <= from + 1; lane++)
            {
                if (lane < 0 || lane >= row.Length) continue;
                if (CanPass(row[lane])) return true;
            }
            return false;
        }

        /// <summary>
        /// Whether the runner is hit, given what stands in its lane and how high it is.
        /// </summary>
        /// <remarks>
        /// Analytic rather than a physics collision: at top speed a frame covers nearly half a
        /// unit, and a thin trigger is exactly the kind of thing a fast object walks straight
        /// through without ever touching it. The rule is decided here, once, and tested without
        /// an engine.
        /// </remarks>
        public static bool Hits(Blocker blocker, float runnerHeight, float lowClearance)
        {
            if (blocker == Blocker.None) return false;
            if (blocker == Blocker.Full) return true;
            return runnerHeight < lowClearance;
        }

        /// <summary>
        /// True when the runner went past <paramref name="rowZ"/> during this frame.
        /// </summary>
        /// <remarks>
        /// WARNING: comparing the current position with the row's is not enough. The runner
        /// advances by up to half a unit per frame at top speed: an obstacle tested only on
        /// "am I level with it" is missed on most frames, and the hit lands or not depending on
        /// the frame rate. The crossing is what has to be tested.
        /// </remarks>
        public static bool Crossed(float previousZ, float currentZ, float rowZ)
            => previousZ < rowZ && currentZ >= rowZ;
    }
}
