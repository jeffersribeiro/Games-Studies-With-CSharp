using OpenTK.Mathematics;

namespace FirstWorkingGame.Source
{
    public class OrbitalCamera
    {
        // Spherical coords
        private float _azimuth;    // around Y axis (radians)
        private float _elevation;  // up/down (radians)
        private float _distance;   // distance from target

        public Vector3 Target { get; set; } = Vector3.Zero;
        public float MinDistance { get; set; } = 0.5f;
        public float MaxDistance { get; set; } = 20f;

        public OrbitalCamera(float initialDistance = 5f)
        {
            _distance = initialDistance;
            _azimuth = MathHelper.DegreesToRadians(45f);
            _elevation = MathHelper.DegreesToRadians(30f);
        }

        /// <summary>
        /// Call this to rotate the camera around the target.
        /// dx, dy in pixels or normalized input units.
        /// </summary>
        public void Rotate(float dx, float dy)
        {
            const float ROTATE_SPEED = 0.005f;
            _azimuth += dx * ROTATE_SPEED;
            _elevation += dy * ROTATE_SPEED;
            _elevation = MathHelper.Clamp(_elevation, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
        }

        /// <summary>
        /// Call this to dolly in/out.
        /// delta > 0 → zoom out, < 0 → zoom in.
        /// </summary>
        public void Zoom(float delta)
        {
            const float ZOOM_SPEED = 0.1f;
            _distance += delta * ZOOM_SPEED;
            _distance = MathHelper.Clamp(_distance, MinDistance, MaxDistance);
        }

        /// <summary>
        /// Returns the view matrix for use as your “uView” uniform.
        /// </summary>
        public Matrix4 GetViewMatrix()
        {
            // Spherical → Cartesian
            var x = _distance * MathF.Cos(_elevation) * MathF.Cos(_azimuth);
            var y = _distance * MathF.Sin(_elevation);
            var z = _distance * MathF.Cos(_elevation) * MathF.Sin(_azimuth);
            var position = Target + new Vector3(x, y, z);

            return Matrix4.LookAt(position, Target, Vector3.UnitY);
        }
    }
}