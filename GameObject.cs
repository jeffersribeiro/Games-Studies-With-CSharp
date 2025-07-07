using System;
using OpenTK.Mathematics;

public class GameObject : IDisposable
{
    public string Name { get; set; }

    // Transform simples
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 RotationEuler { get; set; } = Vector3.Zero;  // graus
    public Vector3 Scale { get; set; } = Vector3.One;

    private Mesh _mesh;
    private Shader _shader;

    /// <summary>
    /// Fábrica que cria um GameObject a partir de um arquivo glTF
    /// </summary>
    public static GameObject LoadFromGltf(string path, Shader shader, string name = null)
    {
        var go = new GameObject();
        go.Name = name ?? System.IO.Path.GetFileNameWithoutExtension(path);
        go._shader = shader;
        go._mesh = GltfMeshLoader.LoadFromFile(path);
        return go;
    }

    /// <summary>
    /// Constrói a matrix de mundo a partir de Pos/Rot/Escala
    /// </summary>
    public Matrix4 WorldMatrix
    {
        get
        {
            var t = Matrix4.CreateScale(Scale);
            var r = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(RotationEuler.X)) *
                    Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotationEuler.Y)) *
                    Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(RotationEuler.Z));
            var p = Matrix4.CreateTranslation(Position);
            return t * r * p;
        }
    }

    /// <summary>
    /// Renderiza usando o shader que foi passado no Load
    /// </summary>
    public void Render(Matrix4 view, Matrix4 proj)
    {
        _shader.Use();
        _shader.SetMatrix4("uModel", WorldMatrix);
        _shader.SetMatrix4("uView", view);
        _shader.SetMatrix4("uProj", proj);
        _mesh.Render();
    }

    public void Dispose()
    {
        _mesh?.Dispose();
        // geralmente o Shader é compartilhado, dispose apenas se for dono
    }
}
