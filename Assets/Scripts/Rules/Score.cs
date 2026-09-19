namespace LemonRun.Rules
{
    /// <summary>
    /// The score: distance run plus fruit swallowed (GDD section 2).
    /// </summary>
    /// <remarks>
    /// Fruit counts twice over, and that is the point. It already buys back lead, so it lets the
    /// run go on and earns distance that way; counting it in the score as well says out loud that
    /// the greedy line is the one worth playing. A fruit worth nothing in points would still be
    /// worth taking, but only defensively -- and the game would be about surviving rather than
    /// about the choice section 2 is built on.
    /// </remarks>
    public static class Score
    {
        public static int Total(float distance, int fruit, int pointsPerFruit)
        {
            if (distance < 0f) distance = 0f;
            if (fruit < 0) fruit = 0;
            return (int)distance + fruit * pointsPerFruit;
        }
    }
}
