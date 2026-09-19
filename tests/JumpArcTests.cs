using LemonRun.Rules;
using Xunit;

public class JumpArcTests
{
    const float Duration = 0.62f;
    const float Peak = 1.6f;

    [Fact]
    public void The_runner_is_on_the_ground_at_both_ends_of_the_jump()
    {
        Assert.Equal(0f, JumpArc.Height(0f, Duration, Peak), 4);
        Assert.Equal(0f, JumpArc.Height(Duration, Duration, Peak), 4);
    }

    [Fact]
    public void Outside_the_jump_there_is_no_height_at_all()
    {
        Assert.Equal(0f, JumpArc.Height(-1f, Duration, Peak), 4);
        Assert.Equal(0f, JumpArc.Height(Duration + 1f, Duration, Peak), 4);
    }

    [Fact]
    public void The_top_of_the_arc_is_reached_halfway_and_is_the_stated_peak()
        => Assert.Equal(Peak, JumpArc.Height(Duration / 2f, Duration, Peak), 4);

    [Fact]
    public void The_arc_never_climbs_above_its_peak()
    {
        for (int i = 0; i <= 50; i++)
        {
            float height = JumpArc.Height(Duration * i / 50f, Duration, Peak);
            Assert.InRange(height, 0f, Peak);
        }
    }

    [Fact]
    public void Airborne_is_true_only_strictly_inside_the_jump()
    {
        Assert.False(JumpArc.IsAirborne(0f, Duration));
        Assert.True(JumpArc.IsAirborne(Duration / 2f, Duration));
        Assert.False(JumpArc.IsAirborne(Duration, Duration));
    }
}
