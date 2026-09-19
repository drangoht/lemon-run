using System.Collections.Generic;
using LemonRun.Rules;
using UnityEngine;

namespace LemonRun.Gameplay
{
    /// <summary>
    /// Lays the rows of obstacles in front of the runner, and decides what it runs into.
    /// </summary>
    /// <remarks>
    /// The spacing between rows is re-derived from the CURRENT speed at every draw: what is held
    /// constant is the reaction window in seconds, not the distance (see
    /// <see cref="RowSpacing"/>). Without that, the road silently gets harder as the speed ramps,
    /// and the difficulty ends up coming from a number nobody chose.
    ///
    /// The rows are drawn around a guaranteed opening one lane at most from the previous one, so
    /// nothing has to be rejected or re-rolled here -- <see cref="RowDraw"/> holds that guarantee,
    /// and the tests hold <see cref="RowDraw"/>.
    /// </remarks>
    public class ObstacleField : MonoBehaviour
    {
        public Runner Runner;
        public Material LowMaterial;
        public Material FullMaterial;

        /// <summary>How far ahead of the runner the road is kept laid.</summary>
        public float LookAhead = 140f;

        /// <summary>How far behind the runner a row is kept before being taken back.</summary>
        public float KeepBehind = 20f;

        class Row
        {
            public float Z;
            public Blocker[] Lanes;
            public readonly List<GameObject> Pieces = new List<GameObject>();
        }

        readonly List<Row> _rows = new List<Row>();
        readonly Stack<GameObject> _pool = new Stack<GameObject>();

        RunnerTuning _tuning;
        uint _state;
        int _safeLane;
        float _nextZ;
        float _previousZ;

        void Start()
        {
            if (Runner == null) return;

            _tuning = Runner.Tuning;
            _state = _tuning.CourseSeed;
            _safeLane = Lanes.Start;
            _previousZ = Runner.Distance;

            // The first row is placed a full look-ahead away: opening a run already inside an
            // obstacle would be a death nobody could read.
            _nextZ = Runner.Distance + LookAhead;
        }

        void Update()
        {
            if (Runner == null || _tuning == null) return;

            float z = Runner.Distance;

            LayAhead(z);
            CheckCrossings(_previousZ, z);
            TakeBack(z);

            _previousZ = z;
        }

        void LayAhead(float runnerZ)
        {
            while (_nextZ < runnerZ + LookAhead)
            {
                var drawn = RowDraw.Next(ref _state, _safeLane, _tuning.BlockedPercent, _tuning.FullPercent);
                _safeLane = drawn.SafeLane;

                Place(drawn.Lanes, _nextZ);

                _nextZ += RowSpacing.Gap(Runner.Speed, _tuning.ReactionSeconds, _tuning.MinimumRowGap);
            }
        }

        void Place(Blocker[] lanes, float z)
        {
            var row = new Row { Z = z, Lanes = lanes };

            for (int lane = 0; lane < lanes.Length; lane++)
            {
                if (lanes[lane] == Blocker.None) continue;

                bool full = lanes[lane] == Blocker.Full;
                float height = full ? 1.8f : _tuning.LowClearance;

                var piece = Take();
                piece.transform.localScale = new Vector3(1.4f, height, 0.9f);
                piece.transform.position = new Vector3(Lanes.CenterX(lane, _tuning.LaneWidth),
                                                       height / 2f, z);
                piece.GetComponent<MeshRenderer>().sharedMaterial = full ? FullMaterial : LowMaterial;
                piece.SetActive(true);

                row.Pieces.Add(piece);
            }

            _rows.Add(row);
        }

        /// <remarks>
        /// WARNING: tested on the CROSSING, never on "am I level with the row". At top speed a
        /// frame covers nearly half a unit, so a row tested for proximity is missed on most
        /// frames and the hit lands or not depending on the frame rate.
        /// </remarks>
        void CheckCrossings(float previousZ, float currentZ)
        {
            foreach (var row in _rows)
            {
                if (!ObstacleRules.Crossed(previousZ, currentZ, row.Z)) continue;

                int lane = Lanes.NearestLane(Runner.transform.position.x, _tuning.LaneWidth);
                if (ObstacleRules.Hits(row.Lanes[lane], Runner.Height, _tuning.LowClearance))
                    Runner.TakeHit();
            }
        }

        void TakeBack(float runnerZ)
        {
            for (int i = _rows.Count - 1; i >= 0; i--)
            {
                if (_rows[i].Z >= runnerZ - KeepBehind) continue;

                foreach (var piece in _rows[i].Pieces)
                {
                    piece.SetActive(false);
                    _pool.Push(piece);
                }
                _rows.RemoveAt(i);
            }
        }

        /// <remarks>
        /// Pooled rather than created and destroyed: a row goes by roughly every second for the
        /// whole run, and the garbage of an endless game is collected at the worst possible
        /// moment -- a hitch right when the player is reading the next row.
        /// </remarks>
        GameObject Take()
        {
            if (_pool.Count > 0) return _pool.Pop();

            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = "Obstacle";
            piece.transform.SetParent(transform, false);
            Destroy(piece.GetComponent<BoxCollider>());
            return piece;
        }
    }
}
