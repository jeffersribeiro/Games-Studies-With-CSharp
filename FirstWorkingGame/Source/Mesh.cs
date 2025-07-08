using OpenTK.Graphics.OpenGL4;

namespace FirstWorkingGame.Source
{
    public class Mesh : IDisposable
    {
        private readonly int _vao;
        private readonly int _vbo;
        private readonly int _ebo;
        private readonly int _indexCount;

        public Mesh(float[] vertices, uint[] indices)
        {
            _indexCount = indices.Length;

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            // VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // posição no location = 0
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
                                   6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false,
                                   6 * sizeof(float),
                                   3 * sizeof(float));
            // EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }

        public void Render()
        {
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_ebo);
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
        }
    }
}