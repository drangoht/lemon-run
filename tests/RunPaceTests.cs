using LemonRun.Rules;
using Xunit;

public class RunPaceTests
{
    const float Start = 12f;
    const float Top = 26f;
    const float Ramp = 600f;

    [Fact]
    public void The_run_opens_at_the_starting_speed()
        => Assert.Equal(Start, RunPace.SpeedAt(0f, Start, Top, Ramp), 4);

    [Fact]
    public void Halfway_up_the_ramp_the_speed_is_halfway_too()
        => Assert.Equal((Start + Top) / 2f, RunPace.SpeedAt(Ramp / 2f, Start, Top, Ramp), 4);

    [Fact]
    public void Past_the_ramp_the_speed_stops_climbing()
    {
        Assert.Equal(Top, RunPace.SpeedAt(Ramp, Start, Top, Ramp), 4);
        Assert.Equal(Top, RunPace.SpeedAt(Ramp * 100f, Start, Top, Ramp), 4);
    }

    [Fact]
    public void The_speed_never_goes_backwards_along_the_run()
    {
        float previous = 0f;
        for (int metre = 0; metre <= 1200; metre += 10)
        {
            float speed = RunPace.SpeedAt(metre, Start, Top, Ramp);
            Assert.True(speed >= previous, $"speed dropped at {metre} m");
            previous = speed;
        }
    }

    [Fact]
    public void A_ramp_of_zero_is_top_speed_from_the_first_metre()
        => Assert.Equal(Top, RunPace.SpeedAt(0f, Start, Top, 0f), 4);
}
