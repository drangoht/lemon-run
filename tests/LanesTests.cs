using LemonRun.Rules;
using Xunit;

public class LanesTests
{
    [Theory]
    [InlineData(-5, 0)]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    public void Clamp_keeps_the_lane_on_the_road(int lane, int expected)
        => Assert.Equal(expected, Lanes.Clamp(lane));

    [Fact]
    public void Step_does_not_wrap_around_at_the_edges()
    {
        Assert.Equal(0, Lanes.Step(0, -1));
        Assert.Equal(2, Lanes.Step(2, +1));
    }

    [Fact]
    public void Step_moves_one_lane_at_a_time_whatever_the_magnitude()
    {
        Assert.Equal(0, Lanes.Step(1, -4));
        Assert.Equal(2, Lanes.Step(1, +9));
    }

    [Fact]
    public void A_step_into_the_edge_is_reported_as_refused()
    {
        Assert.True(Lanes.IsRefused(0, -1));
        Assert.True(Lanes.IsRefused(2, +1));
        Assert.False(Lanes.IsRefused(1, -1));
        Assert.False(Lanes.IsRefused(1, +1));
    }

    [Fact]
    public void The_middle_lane_sits_on_the_axis_and_the_sides_are_symmetric()
    {
        Assert.Equal(0f, Lanes.CenterX(1, 2f), 4);
        Assert.Equal(-2f, Lanes.CenterX(0, 2f), 4);
        Assert.Equal(+2f, Lanes.CenterX(2, 2f), 4);
    }

    [Fact]
    public void The_run_starts_on_the_middle_lane()
        => Assert.Equal(0f, Lanes.CenterX(Lanes.Start, 2f), 4);
}
