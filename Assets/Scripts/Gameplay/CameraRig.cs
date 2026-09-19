using UnityEngine;

namespace LemonRun.Gameplay
{
    /// <summary>
    /// Keeps the camera behind and above the runner.
    /// </summary>
    /// <remarks>
    /// The camera follows a lane change only <b>partially</b> (<see cref="LateralReach"/>):
    /// following it fully would keep the runner dead centre and make the change nearly invisible,
    /// while not following at all pushes it to the edge of the frame on the outer lanes. Half way
    /// keeps the road framed and still shows that something moved.
    ///
    /// WARNING: <c>LateUpdate</c>, not <c>Update</c>. In <c>Update</c> the camera would be placed
    /// from the runner's position of the PREVIOUS frame -- a jitter that reads as an unsteady
    /// frame rate rather than as a camera fault.
    /// </remarks>
    public class CameraRig : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new Vector3(0f, 4.5f, -7f);

        /// <summary>Share of the runner's lateral offset that the camera takes on.</summary>
        public float LateralReach = 0.55f;

        /// <summary>Higher catches up faster; too high and a lane change loses its weight.</summary>
        public float LateralDamping = 8f;

        void LateUpdate()
        {
            if (Target == null) return;

            float wanted = Target.position.x * LateralReach + Offset.x;
            float x = Mathf.Lerp(transform.position.x, wanted, LateralDamping * Time.deltaTime);

            transform.position = new Vector3(x, Offset.y, Target.position.z + Offset.z);
        }
    }
}
