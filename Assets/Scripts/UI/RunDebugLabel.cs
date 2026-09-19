using LemonRun.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace LemonRun.UI
{
    /// <summary>
    /// DEBUG readout: lane, speed and distance, refreshed every frame.
    /// </summary>
    /// <remarks>
    /// Not a HUD and not a score -- it exists so that a change can be <b>checked</b> rather than
    /// believed. A road of periodic dashes looks identical whether the runner advances at twelve
    /// units per second or stands still, and a screenshot cannot tell them apart. This label is
    /// what turns a capture into a measurement.
    ///
    /// It goes the day the real HUD arrives (distance is the score, GDD section 2).
    /// </remarks>
    public class RunDebugLabel : MonoBehaviour
    {
        public Runner Runner;
        public Pursuer Pursuer;

        Text _text;

        void Awake() => _text = GetComponent<Text>();

        void Update()
        {
            if (_text == null || Runner == null) return;

            string lead = Pursuer != null ? $"   lead {Pursuer.CurrentLead:0.00}s" : "";

            _text.text = $"lane {Runner.Lane}   speed {Runner.Speed:0.0}   " +
                         $"distance {Runner.Distance:0.0}   height {Runner.Height:0.00}   " +
                         $"hits {Runner.Hits}{lead}";
        }
    }
}
