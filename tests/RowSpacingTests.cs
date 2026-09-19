using LemonRun.Rules;
using Xunit;

public class RowSpacingTests
{
    [Fact]
    public void The_gap_grows_with_the_speed_so_the_window_stays_put()
    {
        float slow = RowSpacing.Gap(12f, 0.9f, 0f);
        float fast = RowSpacing.Gap(26f, 0.9f, 0f);

        Assert.Equal(10.8f, slow, 3);
        Assert.Equal(23.4f, fast, 3);

        // The point of the whole class: the same number of seconds at either speed.
        Assert.Equal(RowSpacing.WindowSeconds(slow, 12f), RowSpacing.WindowSeconds(fast, 26f), 4);
    }

    [Fact]
    public void The_minimum_gap_wins_at_low_speed()
        => Assert.Equal(12f, RowSpacing.Gap(4f, 0.9f, 12f), 3);

    [Fact]
    public void A_row_met_at_the_tuned_gap_always_leaves_time_to_change_lane()
    {
        // Across the whole speed ramp, with the shipped defaults.
        var tuning = new RunnerTuning();

        for (float speed = tuning.StartSpeed; speed <= tuning.TopSpeed; speed += 0.5f)
        {
            float gap = RowSpacing.Gap(speed, tuning.ReactionSeconds, tuning.MinimumRowGap);
            Assert.True(RowSpacing.LeavesTimeToAct(gap, speed, tuning.LaneChangeDuration),
                        $"at {speed} u/s the gap leaves no time to finish a lane change");
        }
    }

    [Fact]
    public void A_gap_shorter_than_the_lane_change_is_reported_as_such()
    {
        // 2 units at 26 u/s is 0.077 s: shorter than a 0.14 s lane change.
        Assert.False(RowSpacing.LeavesTimeToAct(2f, 26f, 0.14f));
    }

    [Fact]
    public void A_standing_runner_has_all_the_time_in_the_world()
        => Assert.Equal(float.PositiveInfinity, RowSpacing.WindowSeconds(10f, 0f));
}
