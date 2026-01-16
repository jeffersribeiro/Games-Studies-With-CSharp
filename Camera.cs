using OpenTK.Mathematics;

public enum CameraMovement { Forward, Backward, Left, Right }

public sealed class Camera
{
    public Vector3 Position;
    public Vector3 Front = -Vector3.UnitZ;
    public Vector3 Up = Vector3.UnitY;
    public Vector3 Right = Vector3.UnitX;
    public Vector3 WorldUp = Vector3.UnitY;

    public float Yaw = -90f;
    public float Pitch = 0f;

    public float MovementSpeed = 2.5f;
    public float MouseSensitivity = 0.1f;
    public float Zoom = 45f;

    public Camera(Vector3 position)
    {
        Position = position;
        UpdateCameraVectors();
    }

    public Matrix4 GetViewMatrix()
        => Matrix4.LookAt(Position, Position + Front, Up);

    public void ProcessKeyboard(CameraMovement dir, float dt)
    {
        var vel = MovementSpeed * dt;
        if (dir == CameraMovement.Forward) Position += Front * vel;
        if (dir == CameraMovement.Backward) Position -= Front * vel;
        if (dir == CameraMovement.Left) Position -= Right * vel;
        if (dir == CameraMovement.Right) Position += Right * vel;
    }

    public void ProcessMouseMovement(float xoffset, float yoffset, bool constrainPitch = true)
    {
        xoffset *= MouseSensitivity;
        yoffset *= MouseSensitivity;

        Yaw += xoffset;
        Pitch += yoffset;

        if (constrainPitch)
        {
            if (Pitch > 89f) Pitch = 89f;
            if (Pitch < -89f) Pitch = -89f;
        }

        UpdateCameraVectors();
    }

    public void ProcessMouseScroll(float yoffset)
    {
        Zoom -= yoffset;
        if (Zoom < 1f) Zoom = 1f;
        if (Zoom > 45f) Zoom = 45f;
    }

    private void UpdateCameraVectors()
    {
        Vector3 front;
        front.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
        front.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
        Front = front.Normalized();

        Right = Vector3.Cross(Front, WorldUp).Normalized();
        Up = Vector3.Cross(Right, Front).Normalized();
    }
}
