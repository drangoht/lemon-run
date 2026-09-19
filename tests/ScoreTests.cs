using LemonRun.Rules;
using Xunit;

public class ScoreTests
{
    [Fact]
    public void Distance_alone_scores_the_metres_run()
        => Assert.Equal(250, Score.Total(250.9f, 0, 10));

    [Fact]
    public void Fruit_adds_on_top_of_the_distance()
        => Assert.Equal(370, Score.Total(250.9f, 12, 10));

    [Fact]
    public void A_run_that_went_nowhere_scores_nothing()
        => Assert.Equal(0, Score.Total(0f, 0, 10));

    [Fact]
    public void Nonsense_inputs_do_not_produce_a_negative_score()
    {
        Assert.Equal(0, Score.Total(-50f, 0, 10));
        Assert.Equal(0, Score.Total(-50f, -3, 10));
    }

    [Fact]
    public void Two_fruit_are_worth_more_than_the_metres_lost_stopping_for_them()
    {
        // Sanity on the shipped defaults: two fruit undo one hit (0.7 s of lead) AND pay points.
        var tuning = new RunnerTuning();

        Assert.Equal(tuning.HitCost, tuning.FruitGain * 2f, 4);
        Assert.True(tuning.PointsPerFruit > 0, "a fruit worth no points is only ever defensive");
    }
}
