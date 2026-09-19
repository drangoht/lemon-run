namespace LemonRun.Rules
{
    /// <summary>
    /// Every value of the running system that deserves to be tried several times.
    /// </summary>
    /// <remarks>
    /// Kept in one plain serialisable object so the game can read it back from a
    /// <c>tuning.json</c> sitting next to the executable: changing a value must not require a
    /// recompilation, let alone a rebuild.
    ///
    /// WARNING: none of these numbers has been measured. They are set by eye to give something
    /// playable to react to -- the whole point of the file is that they get changed.
    /// </remarks>
    [System.Serializable]
    public class RunnerTuning
    {
        /// <summary>Distance between two lane centres, in world units.</summary>
        public float LaneWidth = 2f;

        /// <summary>Seconds a lane change takes end to end.</summary>
        public float LaneChangeDuration = 0.14f;

        public float StartSpeed = 12f;
        public float TopSpeed = 26f;

        /// <summary>Distance over which the speed climbs from start to top.</summary>
        public float RampDistance = 600f;

        public float JumpPeak = 1.6f;
        public float JumpDuration = 0.62f;
    }
}
