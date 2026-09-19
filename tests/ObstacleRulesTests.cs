using LemonRun.Rules;
using Xunit;

public class ObstacleRulesTests
{
    static Blocker[] Row(Blocker a, Blocker b, Blocker c) => new[] { a, b, c };

    [Fact]
    public void A_full_blocker_cannot_be_passed_a_low_one_can()
    {
        Assert.True(ObstacleRules.CanPass(Blocker.None));
        Assert.True(ObstacleRules.CanPass(Blocker.Low));
        Assert.False(ObstacleRules.CanPass(Blocker.Full));
    }

    [Fact]
    public void A_row_full_across_every_lane_is_not_passable()
        => Assert.False(ObstacleRules.IsPassable(Row(Blocker.Full, Blocker.Full, Blocker.Full)));

    [Fact]
    public void A_row_of_low_blockers_is_passable_by_jumping()
        => Assert.True(ObstacleRules.IsPassable(Row(Blocker.Low, Blocker.Low, Blocker.Low)));

    [Fact]
    public void An_opening_two_lanes_away_is_passable_but_NOT_survivable()
    {
        var row = Row(Blocker.None, Blocker.Full, Blocker.Full);

        Assert.True(ObstacleRules.IsPassable(row));
        Assert.False(ObstacleRules.IsSurvivable(row, fromLane: 2));
        Assert.True(ObstacleRules.IsSurvivable(row, fromLane: 1));
    }

    [Fact]
    public void Staying_put_counts_as_an_answer()
        => Assert.True(ObstacleRules.IsSurvivable(Row(Blocker.Full, Blocker.None, Blocker.Full), 1));

    [Theory]
    [InlineData(Blocker.None, 0f, false)]
    [InlineData(Blocker.None, 2f, false)]
    [InlineData(Blocker.Full, 0f, true)]
    [InlineData(Blocker.Full, 2f, true)]   // a full blocker is not cleared by jumping
    [InlineData(Blocker.Low, 0f, true)]    // low, but on the ground: hit
    [InlineData(Blocker.Low, 1.2f, false)] // low, and high enough: cleared
    public void A_hit_depends_on_the_blocker_and_on_the_height(Blocker blocker, float height, bool hit)
        => Assert.Equal(hit, ObstacleRules.Hits(blocker, height, lowClearance: 0.45f));

    [Fact]
    public void A_low_blocker_is_cleared_exactly_at_the_clearance()
        => Assert.False(ObstacleRules.Hits(Blocker.Low, 0.45f, 0.45f));

    [Fact]
    public void A_row_is_crossed_once_and_only_once()
    {
        Assert.True(ObstacleRules.Crossed(9.6f, 10.1f, 10f));
        Assert.False(ObstacleRules.Crossed(10.1f, 10.6f, 10f));   // already behind
        Assert.False(ObstacleRules.Crossed(8.0f, 9.0f, 10f));     // not there yet
    }

    [Fact]
    public void A_row_jumped_over_in_a_single_frame_is_still_crossed()
    {
        // At top speed a frame covers nearly half a unit; a big hitch covers several.
        Assert.True(ObstacleRules.Crossed(5f, 40f, 10f));
    }
}
