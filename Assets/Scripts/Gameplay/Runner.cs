using LemonRun.Rules;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LemonRun.Gameplay
{
    /// <summary>
    /// The runner: goes forward on its own, changes lane, jumps. Every number it uses comes from
    /// <c>Assets/Scripts/Rules/</c> -- this class does the engine work and nothing else.
    /// </summary>
    /// <remarks>
    /// WARNING: arrow keys and Space, never WASD. <c>Key</c> designates a POSITION on a QWERTY
    /// keyboard: on AZERTY the key marked Z is reached as <c>Key.W</c>. The arrows and Space sit
    /// at the same physical place on both layouts, which is why the GDD chose them -- see
    /// <c>docs/pitfalls/input.md</c>.
    /// </remarks>
    public class Runner : MonoBehaviour
    {
        public RunnerTuning Tuning = new RunnerTuning();

        int _lane = Lanes.Start;
        float _fromX;
        float _toX;
        float _travel = 1f;        // 1 = settled on its lane
        float _jumpElapsed = -1f;  // negative = on the ground
        float _groundY;
        float _distance;

        public int Lane => _lane;
        public float Distance => _distance;
        public float Speed { get; private set; }

        void Awake()
        {
            Tuning = TuningLoader.Load();

            _groundY = transform.position.y;
            _fromX = _toX = Lanes.CenterX(_lane, Tuning.LaneWidth);
            transform.position = new Vector3(_toX, _groundY, 0f);
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;

            ReadInput();

            Speed = RunPace.SpeedAt(_distance, Tuning.StartSpeed, Tuning.TopSpeed, Tuning.RampDistance);
            _distance += Speed * deltaTime;

            _travel = LaneTravel.Advance(_travel, deltaTime, Tuning.LaneChangeDuration);
            float x = LaneTravel.PositionX(_fromX, _toX, _travel);

            float height = 0f;
            if (_jumpElapsed >= 0f)
            {
                _jumpElapsed += deltaTime;
                if (JumpArc.IsAirborne(_jumpElapsed, Tuning.JumpDuration))
                    height = JumpArc.Height(_jumpElapsed, Tuning.JumpDuration, Tuning.JumpPeak);
                else
                    _jumpElapsed = -1f;   // landed
            }

            transform.position = new Vector3(x, _groundY + height, _distance);
        }

        bool _keyboardReported;

        /// <remarks>
        /// WARNING: a null <c>Keyboard.current</c> is not "no keyboard plugged in", it is almost
        /// always <c>activeInputHandler</c> still set to the old Input Manager -- the package is
        /// installed, the code compiles, and the game simply never answers a key. Returning
        /// quietly there cost a full debugging pass: the failure is reported once, loudly.
        /// </remarks>
        void ReadInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                if (!_keyboardReported)
                {
                    _keyboardReported = true;
                    Debug.LogError("No keyboard from the Input System: check that Active Input " +
                                   "Handling is set to the Input System package. The run goes on, " +
                                   "but it answers nothing.");
                }
                return;
            }

            if (keyboard.leftArrowKey.wasPressedThisFrame) ChangeLane(-1);
            if (keyboard.rightArrowKey.wasPressedThisFrame) ChangeLane(+1);
            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) Jump();
        }

        /// <remarks>
        /// A change already under way blocks the next one. Whether that is the right answer is an
        /// open question of the GDD (buffer the second input, or drop it?) and it gets settled
        /// with the game in hand; until then, the simplest behaviour is the one that can be judged.
        /// </remarks>
        void ChangeLane(int direction)
        {
            if (_travel < 1f) return;
            if (Lanes.IsRefused(_lane, direction)) return;

            _lane = Lanes.Step(_lane, direction);
            _fromX = transform.position.x;
            _toX = Lanes.CenterX(_lane, Tuning.LaneWidth);
            _travel = 0f;
        }

        void Jump()
        {
            if (_jumpElapsed >= 0f) return;   // no second jump in mid-air
            _jumpElapsed = 0f;
        }
    }
}
