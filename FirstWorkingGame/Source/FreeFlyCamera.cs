using OpenTK.Mathematics;
using System;

namespace FirstWorkingGame.Source
{
    public class FreeFlyCamera
    {
        // Public so you can read/write externally if you like
        public Vector3 Position { get; set; }
        public Vector3 Front { get; private set; } = -Vector3.UnitZ;
        public Vector3 Up { get; private set; } = Vector3.UnitY;
        public Vector3 Right { get; private set; } = Vector3.UnitX;
        public Vector3 WorldUp { get; } = Vector3.UnitY;

        // Euler angles
        public float Yaw { get; private set; } = -90f;  // looking down -Z
        public float Pitch { get; private set; } = 0f;

        // Options
        public float MovementSpeed { get; set; } = 2.5f;
        public float MouseSensitivity { get; set; } = 0.1f;
        public float Zoom { get; private set; } = 45f;

        public FreeFlyCamera(Vector3 startPosition)
        {
            Position = startPosition;
            UpdateVectors();
        }

        // Returns the view matrix for your shader:
        public Matrix4 GetViewMatrix()
            => Matrix4.LookAt(Position, Position + Front, Up);

        // Call this from your OnUpdateFrame
        public void ProcessKeyboard(CameraMovement dir, float deltaTime)
        {
            float velocity = MovementSpeed * deltaTime;
            if (dir == CameraMovement.Forward) Position += Front * velocity;
            if (dir == CameraMovement.Backward) Position -= Front * velocity;
            if (dir == CameraMovement.Left) Position -= Right * velocity;
            if (dir == CameraMovement.Right) Position += Right * velocity;
            if (dir == CameraMovement.Up) Position += WorldUp * velocity;
            if (dir == CameraMovement.Down) Position -= WorldUp * velocity;
        }

        // Call this from your OnMouseMove
        public void ProcessMouseMovement(float deltaX, float deltaY, bool constrainPitch = true)
        {
            deltaX *= MouseSensitivity;
            deltaY *= MouseSensitivity;

            Yaw += deltaX;
            Pitch -= deltaY; // invert Y if you like

            if (constrainPitch)
                Pitch = MathHelper.Clamp(Pitch, -89f, 89f);

            UpdateVectors();
        }

        // Call this from your OnMouseWheel (to zoom FOV)
        public void ProcessMouseScroll(float offset)
        {
            Zoom -= offset;
            Zoom = MathHelper.Clamp(Zoom, 1f, 90f);
        }

        private void UpdateVectors()
        {
            // Recompute Front, Right, Up from yaw & pitch
            Vector3 f;
            f.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            f.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
            f.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            Front = f.Normalized();
            Right = Vector3.Cross(Front, WorldUp).Normalized();
            Up = Vector3.Cross(Right, Front).Normalized();
        }
    }

    public enum CameraMovement
    {
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down
    }
}