using Assimp;
using System.Numerics;

public static class AssimpHelpers
{
    public static System.Numerics.Matrix4x4 ToNumerics(this Assimp.Matrix4x4 m)
    {
        // Assimp Matrix4x4D é row-major; System.Numerics também é row-major.
        return new System.Numerics.Matrix4x4(
            (float)m.A1, (float)m.B1, (float)m.C1, (float)m.D1,
            (float)m.A2, (float)m.B2, (float)m.C2, (float)m.D2,
            (float)m.A3, (float)m.B3, (float)m.C3, (float)m.D3,
            (float)m.A4, (float)m.B4, (float)m.C4, (float)m.D4
        );
    }

    public static Vector3 ToVec3(this Vector3D v) => new((float)v.X, (float)v.Y, (float)v.Z);
    public static Vector2 ToVec2(this Vector3D v) => new((float)v.X, (float)v.Y);
    public static System.Numerics.Quaternion ToQuat(this Assimp.Quaternion q) => new((float)q.X, (float)q.Y, (float)q.Z, (float)q.W);

    public static OpenTK.Mathematics.Matrix4 ToOpenTK(this  System.Numerics.Matrix4x4 m)
    {
        // OpenTK.Matrix4 é column-major no upload; SetMatrix4 abaixo já lida (transpose=false com data column-major).
        // Aqui montamos preservando os campos.
        return new OpenTK.Mathematics.Matrix4(
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
        );
    }
}
