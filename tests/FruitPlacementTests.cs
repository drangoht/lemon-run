using LemonRun.Rules;
using Xunit;

public class FruitPlacementTests
{
    static Blocker[] Row(Blocker a, Blocker b, Blocker c) => new[] { a, b, c };

    /// <summary>
    /// The rule of GDD section 5, and the reason this class exists: a fruit on the free lane
    /// would hand the player the antidote to the constraint being applied.
    /// </summary>
    [Fact]
    public void A_fruit_is_never_laid_on_a_free_lane_whatever_the_seed()
    {
        for (uint seed = 1; seed <= 300; seed++)
        {
            uint state = seed;
            int safe = Lanes.Start;

            for (int i = 0; i < 200; i++)
            {
                var drawn = RowDraw.Next(ref state, safe, blockedPercent: 60, fullPercent: 40);
                int lane = FruitPlacement.LaneFor(drawn.Lanes, ref state, chancePercent: 100);

                if (lane == FruitPlacement.None) continue;

                Assert.NotEqual(drawn.SafeLane, lane);
                Assert.Equal(Blocker.Low, drawn.Lanes[lane]);

                safe = drawn.SafeLane;
            }
        }
    }

    [Fact]
    public void A_fruit_never_goes_behind_a_full_blocker_which_cannot_be_entered()
    {
        uint state = 99u;
        for (int i = 0; i < 500; i++)
        {
            int lane = FruitPlacement.LaneFor(Row(Blocker.Full, Blocker.None, Blocker.Full),
                                              ref state, 100);
            Assert.Equal(FruitPlacement.None, lane);
        }
    }

    [Fact]
    public void An_empty_row_carries_no_fruit()
    {
        uint state = 7u;
        Assert.Equal(FruitPlacement.None,
                     FruitPlacement.LaneFor(Row(Blocker.None, Blocker.None, Blocker.None), ref state, 100));
    }

    [Fact]
    public void A_jumpable_lane_is_where_a_fruit_goes()
    {
        uint state = 3u;
        Assert.Equal(2, FruitPlacement.LaneFor(Row(Blocker.Full, Blocker.None, Blocker.Low), ref state, 100));
    }

    [Fact]
    public void Every_jumpable_lane_gets_its_turn_over_many_rows()
    {
        uint state = 555u;
        bool left = false, right = false;

        for (int i = 0; i < 400; i++)
        {
            int lane = FruitPlacement.LaneFor(Row(Blocker.Low, Blocker.None, Blocker.Low), ref state, 100);
            if (lane == 0) left = true;
            if (lane == 2) right = true;
        }

        Assert.True(left && right, "the draw always picked the same lane");
    }

    [Fact]
    public void A_zero_chance_lays_no_fruit_at_all()
    {
        uint state = 11u;
        for (int i = 0; i < 300; i++)
            Assert.Equal(FruitPlacement.None,
                         FruitPlacement.LaneFor(Row(Blocker.Low, Blocker.None, Blocker.Low), ref state, 0));
    }

    [Theory]
    [InlineData(1.05f, true)]    // right on it
    [InlineData(0.40f, true)]    // low in the arc, still within reach
    [InlineData(1.70f, true)]    // high in the arc
    [InlineData(0.00f, false)]   // on the ground: missed
    [InlineData(2.00f, false)]   // sailed over it
    public void The_fruit_is_swallowed_within_a_band_not_on_a_point(float height, bool taken)
        => Assert.Equal(taken, FruitPlacement.IsWithinReach(height, 1.05f, 0.75f));

    [Fact]
    public void Clearing_the_obstacle_under_it_is_enough_to_take_it()
    {
        // A jump that just clears a low blocker must not miss the fruit sitting on it.
        var tuning = new RunnerTuning();
        float justCleared = tuning.LowClearance + 0.01f;

        Assert.True(FruitPlacement.IsWithinReach(justCleared, tuning.FruitHeight, tuning.FruitReach));
    }
}
