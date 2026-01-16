using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

public sealed class Mesh : IDisposable
{
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _ebo;
    private readonly int _indexCount;

    private readonly Texture2D? _diffuse;

    public Mesh(Vertex[] vertices, uint[] indices, Texture2D? diffuse)
    {
        _diffuse = diffuse;
        _indexCount = indices.Length;

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Marshal.SizeOf<Vertex>(), vertices, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        var stride = Marshal.SizeOf<Vertex>();
        int offset = 0;

        // layout(location=0) pos (vec3)
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, offset);
        offset += sizeof(float) * 3;

        // location=1 norm (vec3)
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, offset);
        offset += sizeof(float) * 3;

        // location=2 tex (vec2)
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, offset);
        offset += sizeof(float) * 2;

        // location=3 tangent (vec3)
        GL.EnableVertexAttribArray(3);
        GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, stride, offset);
        offset += sizeof(float) * 3;

        // location=4 bitangent (vec3)
        GL.EnableVertexAttribArray(4);
        GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, stride, offset);
        offset += sizeof(float) * 3;

        // location=5 boneIds (ivec4)
        GL.EnableVertexAttribArray(5);
        GL.VertexAttribIPointer(5, 4, VertexAttribIntegerType.Int, stride, offset);
        offset += sizeof(int) * 4;

        // location=6 weights (vec4)
        GL.EnableVertexAttribArray(6);
        GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, stride, offset);
        offset += sizeof(float) * 4;

        GL.BindVertexArray(0);
    }

    public void Draw(Shader shader)
    {
        if (_diffuse != null)
        {
            _diffuse.Bind(TextureUnit.Texture0);
            shader.SetInt("texture_diffuse1", 0);
        }

        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        _diffuse?.Dispose();
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        GL.DeleteVertexArray(_vao);
    }
}
