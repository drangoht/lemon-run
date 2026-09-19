using LemonRun.Gameplay;
using LemonRun.Rules;
using UnityEngine;
using UnityEngine.UI;

namespace LemonRun.UI
{
    /// <summary>
    /// The lead bar: the only honest reading of how close the pursuer is.
    /// </summary>
    /// <remarks>
    /// It is not a decoration doubling what is on screen. The pursuer is drawn inside a band so
    /// that it always stays in frame (<see cref="Lead.DrawGap"/>), so the distance the player sees
    /// behind them is <b>not</b> the distance in the rules. This bar is.
    ///
    /// It goes red near the end rather than only shrinking: a bar that merely gets shorter is
    /// read at a glance as "still some left" right up to the moment it is not.
    /// </remarks>
    public class LeadGauge : MonoBehaviour
    {
        public Pursuer Pursuer;
        public RectTransform Fill;
        public Image FillImage;

        public Color Safe = new Color(0.45f, 0.82f, 0.45f);
        public Color Danger = new Color(0.92f, 0.28f, 0.25f);

        void Update()
        {
            if (Pursuer == null || Fill == null) return;

            float fraction = Lead.Fraction(Pursuer.CurrentLead, Pursuer.MaximumLead);

            var max = Fill.anchorMax;
            max.x = fraction;
            Fill.anchorMax = max;
            Fill.offsetMax = new Vector2(0f, Fill.offsetMax.y);
            Fill.offsetMin = new Vector2(0f, Fill.offsetMin.y);

            if (FillImage != null)
                FillImage.color = Color.Lerp(Danger, Safe, Mathf.Clamp01(fraction * 2f));
        }
    }
}
