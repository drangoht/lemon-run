using LemonRun.Rules;
using Xunit;

public class LeadTests
{
    [Fact]
    public void A_hit_costs_exactly_what_it_says()
        => Assert.Equal(2.3f, Lead.AfterHit(3.0f, 0.7f), 4);

    [Fact]
    public void The_lead_never_goes_below_nothing()
        => Assert.Equal(0f, Lead.AfterHit(0.2f, 0.7f), 4);

    [Fact]
    public void Caught_is_exactly_when_there_is_nothing_left()
    {
        Assert.False(Lead.IsCaught(0.01f));
        Assert.True(Lead.IsCaught(0f));
        Assert.True(Lead.IsCaught(-1f));
    }

    /// <summary>
    /// The defect found by playing: the run opened AT the ceiling, so a fruit swallowed before
    /// the first hit bought nothing. The rule was right; the starting state made it a lie.
    /// </summary>
    [Fact]
    public void A_fruit_taken_before_any_hit_must_still_buy_something()
    {
        var tuning = new RunnerTuning();

        Assert.True(tuning.MaximumLead > tuning.StartLead,
                    "the ceiling must sit above the opening lead, or early fruit is wasted");

        float afterOne = Lead.AfterGain(tuning.StartLead, tuning.FruitGain, tuning.MaximumLead);
        Assert.True(afterOne > tuning.StartLead, "a fruit on a clean run changed nothing");
    }

    [Fact]
    public void The_headroom_is_worth_a_couple_of_mistakes()
    {
        var tuning = new RunnerTuning();
        float headroom = tuning.MaximumLead - tuning.StartLead;

        Assert.True(headroom >= tuning.HitCost * 2f,
                    "banking fruit should be worth at least two extra mistakes");
    }

    [Fact]
    public void A_fixed_number_of_hits_ends_the_run_when_nothing_buys_the_lead_back()
    {
        // The state the game is in until fruit exists: five hits and it is over.
        var tuning = new RunnerTuning();
        float lead = tuning.StartLead;
        int hits = 0;

        while (!Lead.IsCaught(lead))
        {
            lead = Lead.AfterHit(lead, tuning.HitCost);
            hits++;
        }

        Assert.Equal(5, hits);
    }

    [Fact]
    public void Fruit_buys_the_lead_back_but_never_past_the_cap()
    {
        Assert.Equal(2.0f, Lead.AfterGain(1.65f, 0.35f, 3.0f), 4);
        Assert.Equal(3.0f, Lead.AfterGain(2.9f, 0.35f, 3.0f), 4);   // capped
        Assert.Equal(3.0f, Lead.AfterGain(3.0f, 10f, 3.0f), 4);     // the threat never disappears
    }

    [Fact]
    public void The_gauge_reads_zero_to_one_and_nothing_outside()
    {
        Assert.Equal(0f, Lead.Fraction(0f, 3f), 4);
        Assert.Equal(0.5f, Lead.Fraction(1.5f, 3f), 4);
        Assert.Equal(1f, Lead.Fraction(3f, 3f), 4);
        Assert.Equal(1f, Lead.Fraction(99f, 3f), 4);
        Assert.Equal(0f, Lead.Fraction(-5f, 3f), 4);
    }

    [Fact]
    public void A_maximum_of_zero_does_not_divide_by_it()
        => Assert.Equal(0f, Lead.Fraction(1f, 0f), 4);

    [Fact]
    public void The_pursuer_is_drawn_inside_its_band_whatever_the_lead()
    {
        const float Near = 1.8f, Far = 6.0f;

        Assert.Equal(Far, Lead.DrawGap(3f, 3f, Near, Far), 4);      // full lead: far edge
        Assert.Equal(Near, Lead.DrawGap(0f, 3f, Near, Far), 4);     // caught: on top of the runner

        for (float lead = -1f; lead <= 4f; lead += 0.1f)
            Assert.InRange(Lead.DrawGap(lead, 3f, Near, Far), Near, Far);
    }

    [Fact]
    public void The_pursuer_draws_closer_as_the_lead_falls()
    {
        float far = Lead.DrawGap(3f, 3f, 1.8f, 6f);
        float mid = Lead.DrawGap(1.5f, 3f, 1.8f, 6f);
        float near = Lead.DrawGap(0.3f, 3f, 1.8f, 6f);

        Assert.True(far > mid && mid > near, "the gap must shrink as the lead falls");
    }
}
