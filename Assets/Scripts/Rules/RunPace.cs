namespace LemonRun.Rules
{
    /// <summary>
    /// Forward speed as a function of the distance already run.
    /// </summary>
    /// <remarks>
    /// A straight ramp from <c>startSpeed</c> to <c>topSpeed</c> over <c>rampDistance</c>, then
    /// flat. Flat on purpose: an endless ramp ends in a speed no reaction time can follow, and the
    /// run then stops being lost on a decision. Where the ceiling belongs is a measurement, not a
    /// guess -- nothing here has been played yet.
    /// </remarks>
    public static class RunPace
    {
        public static float SpeedAt(float distance, float startSpeed, float topSpeed, float rampDistance)
        {
            if (rampDistance <= 0f) return topSpeed;
            if (distance <= 0f) return startSpeed;
            if (distance >= rampDistance) return topSpeed;
            return startSpeed + (topSpeed - startSpeed) * (distance / rampDistance);
        }
    }
}
