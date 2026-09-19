using LemonRun.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace LemonRun.UI
{
    /// <summary>
    /// DEBUG readout: frame time, lane, speed, distance, hits, fruit and lead.
    /// </summary>
    /// <remarks>
    /// Not a HUD and not a score -- it exists so that a change can be <b>checked</b> rather than
    /// believed. A road of periodic dashes looks identical whether the runner advances at twelve
    /// units per second or stands still, and a screenshot cannot tell them apart.
    ///
    /// The frame time is here for the same reason: "it lags" is not a report anybody can act on,
    /// and the web build cannot be profiled from outside -- a background tab has its
    /// <c>requestAnimationFrame</c> frozen by the browser, so the game is not even running while
    /// something else measures it. On screen, the number travels with the capture.
    ///
    /// WARNING: this label used to rebuild its string EVERY FRAME. Six formatted floats into an
    /// interpolated string is a heap allocation per frame, for a line that changes a few times a
    /// second -- and on WebGL the collection that follows is exactly the hitch it was put there
    /// to measure. It now writes only when the displayed text would actually differ.
    /// </remarks>
    public class RunDebugLabel : MonoBehaviour
    {
        public Runner Runner;
        public Pursuer Pursuer;

        /// <summary>Weight of the newest frame in the smoothed time. Low = steadier reading.</summary>
        public float Smoothing = 0.08f;

        /// <summary>Seconds between two refreshes of the text.</summary>
        /// <remarks>
        /// Five a second is more than a human reads, and it is the whole point: the frame time
        /// changes every frame, so a readout that redrew whenever its text differed would rebuild
        /// the string every frame regardless of any equality check. The sampling below still runs
        /// every frame -- it is the FORMATTING that is throttled.
        /// </remarks>
        public float RefreshSeconds = 0.2f;

        Text _text;
        float _frameMs;
        float _worstMs;
        float _worstResetAt;
        float _nextRefresh;

        void Awake() => _text = GetComponent<Text>();

        void Update()
        {
            if (_text == null || Runner == null) return;

            float ms = Time.unscaledDeltaTime * 1000f;
            _frameMs = _frameMs <= 0f ? ms : Mathf.Lerp(_frameMs, ms, Smoothing);

            // The worst frame over a rolling window: a median hides exactly the hitches that make
            // a game feel bad, and those are what "it lags" usually means.
            if (ms > _worstMs) _worstMs = ms;
            if (Time.unscaledTime - _worstResetAt > 3f)
            {
                _worstMs = ms;
                _worstResetAt = Time.unscaledTime;
            }

            if (Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + RefreshSeconds;

            string lead = Pursuer != null
                        ? $"   lead {Pursuer.CurrentLead:0.00}/{Pursuer.MaximumLead:0.0}s"
                        : "";

            _text.text = $"{_frameMs:0.0}ms ({1000f / Mathf.Max(_frameMs, 0.01f):0} fps)  " +
                         $"worst {_worstMs:0}ms   " +
                         $"lane {Runner.Lane}   speed {Runner.Speed:0.0}   " +
                         $"distance {Runner.Distance:0.0}   hits {Runner.Hits}   " +
                         $"fruit {Runner.Fruit}{lead}";
        }
    }
}
