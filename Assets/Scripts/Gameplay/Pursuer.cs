using LemonRun.Rules;
using UnityEngine;

namespace LemonRun.Gameplay
{
    /// <summary>
    /// What is chasing the runner, and the lead separating them -- the only resource in the game.
    /// </summary>
    /// <remarks>
    /// The lead is held in seconds and falls only on a hit. Nothing gives it back yet: the fruit
    /// that is supposed to buy it back does not exist, so a run is currently a countdown of five
    /// mistakes.
    ///
    /// WARNING: that countdown is **not** the design. GDD section 7 turned down "three hits, no
    /// refund" precisely because, with no way back, the pursuer is a life counter wearing a
    /// costume and the gauge stops saying anything during a run. It is where the game stands
    /// between two systems, and it must not be allowed to settle in by default.
    /// </remarks>
    public class Pursuer : MonoBehaviour
    {
        public Runner Runner;

        public float CurrentLead { get; private set; }
        public float MaximumLead { get; private set; }
        public bool IsCaught => Lead.IsCaught(CurrentLead);

        RunnerTuning _tuning;
        int _seenHits;

        void Start()
        {
            if (Runner == null) return;

            _tuning = Runner.Tuning;
            MaximumLead = _tuning.StartLead;
            CurrentLead = _tuning.StartLead;
            _seenHits = Runner.Hits;

            Place();
        }

        void LateUpdate()
        {
            if (Runner == null || _tuning == null) return;

            // Polled rather than pushed: one counter read per frame costs nothing, and an event
            // would have to be unsubscribed at exactly the right moment on a scene reload.
            if (Runner.Hits != _seenHits)
            {
                for (int i = _seenHits; i < Runner.Hits; i++)
                    CurrentLead = Lead.AfterHit(CurrentLead, _tuning.HitCost);

                _seenHits = Runner.Hits;
            }

            Place();
        }

        /// <summary>Buys lead back. Called by the fruit, the day there is any.</summary>
        public void Gain()
            => CurrentLead = Lead.AfterGain(CurrentLead, _tuning.FruitGain, MaximumLead);

        void Place()
        {
            float gap = Lead.DrawGap(CurrentLead, MaximumLead,
                                     _tuning.PursuerNearest, _tuning.PursuerFurthest);

            // Locked to the runner's lane rather than free to roam: a pursuer that also picks a
            // lane would be a second thing to read, and the road is already what has to be read.
            transform.position = new Vector3(Runner.transform.position.x * 0.6f,
                                             transform.localScale.y / 2f,
                                             Runner.Distance - gap);
        }
    }
}
