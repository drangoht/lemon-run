using LemonRun.Rules;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LemonRun.Gameplay
{
    /// <summary>
    /// Watches for the catch, ends the run and starts the next one.
    /// </summary>
    /// <remarks>
    /// The restart is a scene reload: everything in this game is built by <c>SceneBuilder</c> and
    /// carries no state worth keeping between runs, so reloading is both the shortest way and the
    /// one that cannot leave a stale value behind -- which hand-resetting each system would, on
    /// the first system somebody forgets.
    /// </remarks>
    public class RunSession : MonoBehaviour
    {
        public Runner Runner;
        public Pursuer Pursuer;
        public GameObject EndPanel;

        public bool IsOver { get; private set; }

        /// <summary>Distance run when the pursuer caught up -- the score, until fruit counts too.</summary>
        public float FinalDistance { get; private set; }

        float _lockout;

        void Start()
        {
            if (EndPanel != null) EndPanel.SetActive(false);
        }

        void Update()
        {
            if (Runner == null || Pursuer == null) return;

            if (!IsOver)
            {
                if (Pursuer.IsCaught) End();
                return;
            }

            _lockout -= Time.deltaTime;
            if (_lockout > 0f) return;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <remarks>
        /// WARNING: the lockout is not politeness. Space is also the jump, so a player caught
        /// mid-jump is holding down the very key that restarts -- without the delay the score
        /// would flash past before it could be read, and the run would look like it restarted on
        /// its own.
        /// </remarks>
        void End()
        {
            IsOver = true;
            FinalDistance = Runner.Distance;
            Runner.Frozen = true;
            _lockout = Runner.Tuning != null ? Runner.Tuning.RestartLockout : 0.7f;

            if (EndPanel != null) EndPanel.SetActive(true);
        }
    }
}
