using UnityEngine;

namespace LemonRun.Gameplay
{
    /// <summary>
    /// Endless road made of a handful of tiles: whatever falls behind the runner is pushed back
    /// in front of it.
    /// </summary>
    /// <remarks>
    /// Why recycle rather than lay a very long road: the run has no end, so a road of any finite
    /// length is a cliff waiting at the far side of a play session -- and the session that finds
    /// it is the one nobody was watching.
    ///
    /// The tiles are this object's children, placed by <c>SceneBuilder</c>: the recycling only
    /// moves them, it never creates any. Reading their count from the hierarchy is what stops the
    /// two sides from disagreeing about how long the road is.
    /// </remarks>
    public class GroundTreadmill : MonoBehaviour
    {
        public Transform Target;
        public float TileLength = 40f;

        /// <summary>How far behind the runner a tile must fall before it is recycled.</summary>
        public float BehindMargin = 24f;

        Transform[] _tiles;

        void Awake()
        {
            _tiles = new Transform[transform.childCount];
            for (int i = 0; i < _tiles.Length; i++) _tiles[i] = transform.GetChild(i);
        }

        void Update()
        {
            if (Target == null || _tiles == null || _tiles.Length == 0) return;

            float span = _tiles.Length * TileLength;
            float threshold = Target.position.z - BehindMargin;

            foreach (var tile in _tiles)
            {
                if (tile.position.z + TileLength * 0.5f < threshold)
                    tile.position += new Vector3(0f, 0f, span);
            }
        }
    }
}
