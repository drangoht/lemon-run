using LemonRun.Rules;
using Xunit;

public class LaneTravelTests
{
    [Fact]
    public void Progress_never_goes_past_one_however_long_the_frame()
        => Assert.Equal(1f, LaneTravel.Advance(0.9f, 10f, 0.14f), 4);

    [Fact]
    public void A_zero_duration_lands_immediately_rather_than_dividing_by_zero()
        => Assert.Equal(1f, LaneTravel.Advance(0f, 0.016f, 0f), 4);

    [Fact]
    public void Progress_reaches_exactly_one_after_the_full_duration()
        => Assert.Equal(1f, LaneTravel.Advance(0f, 0.14f, 0.14f), 4);

    [Fact]
    public void Easing_keeps_both_ends_and_the_middle_in_place()
    {
        Assert.Equal(0f, LaneTravel.Ease(0f), 4);
        Assert.Equal(0.5f, LaneTravel.Ease(0.5f), 4);
        Assert.Equal(1f, LaneTravel.Ease(1f), 4);
    }

    [Fact]
    public void Easing_stays_inside_the_segment_it_travels()
    {
        for (int i = 0; i <= 20; i++)
        {
            float eased = LaneTravel.Ease(i / 20f);
            Assert.InRange(eased, 0f, 1f);
        }
    }

    [Fact]
    public void The_travel_ends_exactly_on_the_target_lane()
    {
        Assert.Equal(-2f, LaneTravel.PositionX(-2f, 2f, 0f), 4);
        Assert.Equal(2f, LaneTravel.PositionX(-2f, 2f, 1f), 4);
    }
}
