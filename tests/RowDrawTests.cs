using LemonRun.Rules;
using Xunit;

public class RowDrawTests
{
    /// <summary>
    /// The guarantee the whole system rests on: whatever the seed and however hard the setting,
    /// no drawn row can kill a player who reads it.
    /// </summary>
    [Fact]
    public void Every_drawn_row_is_survivable_from_the_previous_opening()
    {
        for (uint seed = 1; seed <= 200; seed++)
        {
            uint state = seed;
            int safe = Lanes.Start;

            for (int row = 0; row < 300; row++)
            {
                var drawn = RowDraw.Next(ref state, safe, blockedPercent: 100, fullPercent: 100);

                Assert.True(ObstacleRules.IsPassable(drawn.Lanes),
                            $"seed {seed}, row {row}: no way through at all");
                Assert.True(ObstacleRules.IsSurvivable(drawn.Lanes, safe),
                            $"seed {seed}, row {row}: opening out of reach from lane {safe}");

                safe = drawn.SafeLane;
            }
        }
    }

    [Fact]
    public void The_opening_never_moves_by_more_than_one_lane()
    {
        uint state = 12345u;
        int safe = Lanes.Start;

        for (int row = 0; row < 2000; row++)
        {
            var drawn = RowDraw.Next(ref state, safe, 70, 40);
            Assert.InRange(System.Math.Abs(drawn.SafeLane - safe), 0, 1);
            safe = drawn.SafeLane;
        }
    }

    [Fact]
    public void The_opening_lane_is_always_left_free()
    {
        uint state = 777u;
        int safe = Lanes.Start;

        for (int row = 0; row < 500; row++)
        {
            var drawn = RowDraw.Next(ref state, safe, 100, 100);
            Assert.Equal(Blocker.None, drawn.Lanes[drawn.SafeLane]);
            safe = drawn.SafeLane;
        }
    }

    [Fact]
    public void The_same_seed_draws_the_same_road()
    {
        static string Course(uint seed)
        {
            uint state = seed;
            int safe = Lanes.Start;
            var text = new System.Text.StringBuilder();

            for (int row = 0; row < 50; row++)
            {
                var drawn = RowDraw.Next(ref state, safe, 60, 35);
                foreach (var lane in drawn.Lanes) text.Append((int)lane);
                safe = drawn.SafeLane;
            }
            return text.ToString();
        }

        Assert.Equal(Course(42u), Course(42u));
        Assert.NotEqual(Course(42u), Course(43u));
    }

    [Fact]
    public void A_zero_percent_setting_leaves_the_road_empty()
    {
        uint state = 5u;
        int safe = Lanes.Start;

        for (int row = 0; row < 100; row++)
        {
            var drawn = RowDraw.Next(ref state, safe, blockedPercent: 0, fullPercent: 100);
            Assert.All(drawn.Lanes, lane => Assert.Equal(Blocker.None, lane));
            safe = drawn.SafeLane;
        }
    }

    [Fact]
    public void A_zero_state_does_not_freeze_the_generator()
    {
        uint state = 0u;
        Assert.NotEqual(RowDraw.NextRandom(ref state), RowDraw.NextRandom(ref state));
    }
}
