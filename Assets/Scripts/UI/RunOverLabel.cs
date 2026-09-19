using LemonRun.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace LemonRun.UI
{
    /// <summary>
    /// What is shown once the pursuer has caught up: the distance run, and the way to go again.
    /// </summary>
    /// <remarks>
    /// The key is written on the panel rather than assumed known. Invisible reads as
    /// non-existent, and a player who does not know how to start again reads a finished game as a
    /// frozen one.
    /// </remarks>
    public class RunOverLabel : MonoBehaviour
    {
        public RunSession Session;

        Text _text;

        void Awake() => _text = GetComponent<Text>();

        void OnEnable()
        {
            if (_text == null || Session == null) return;

            _text.text = $"CAUGHT\n{Session.FinalScore}\n" +
                         $"{Session.FinalDistance:0} m + {Session.FinalFruit} fruit\n\n" +
                         "Space to run again";
        }
    }
}
