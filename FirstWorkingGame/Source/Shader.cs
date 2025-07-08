using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FirstWorkingGame.Source
{
    public class Shader : IDisposable
    {
        private readonly int _handle;

        // <- THIS is the two‐argument constructor your Game code expects
        public Shader(string vertexPath, string fragmentPath)
        {
            // 1) Load and compile vertex shader
            string vertexCode = File.ReadAllText(vertexPath);
            int vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex, vertexCode);
            GL.CompileShader(vertex);
            GL.GetShader(vertex, ShaderParameter.CompileStatus, out int vStatus);
            if (vStatus == 0)
                throw new Exception($"Vertex shader compile error: {GL.GetShaderInfoLog(vertex)}");

            // 2) Load and compile fragment shader
            string fragmentCode = File.ReadAllText(fragmentPath);
            int fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fragmentCode);
            GL.CompileShader(fragment);
            GL.GetShader(fragment, ShaderParameter.CompileStatus, out int fStatus);
            if (fStatus == 0)
                throw new Exception($"Fragment shader compile error: {GL.GetShaderInfoLog(fragment)}");

            // 3) Link into a program
            _handle = GL.CreateProgram();
            GL.AttachShader(_handle, vertex);
            GL.AttachShader(_handle, fragment);
            GL.LinkProgram(_handle);
            GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
                throw new Exception($"Program link error: {GL.GetProgramInfoLog(_handle)}");

            // 4) Cleanup
            GL.DetachShader(_handle, vertex);
            GL.DetachShader(_handle, fragment);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
        }

        public void Use() => GL.UseProgram(_handle);

        /// <summary>
        /// Sets a 4x4 matrix uniform.
        /// </summary>
        public void SetMatrix4(string name, Matrix4 mat)
        {
            int loc = GL.GetUniformLocation(_handle, name);
            if (loc < 0) throw new Exception($"Uniform '{name}' not found in shader");
            GL.UniformMatrix4(loc, false, ref mat);
        }


        public void SetVector3(string name, Vector3 v)
        {
            int location = GL.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                Console.WriteLine($"Warning: uniform '{name}' not found.");
                return;
            }
            GL.Uniform3(location, v);
        }

        public void SetMatrix3(string name, Matrix3 m)
        {
            int loc = GL.GetUniformLocation(_handle, name);
            if (loc < 0) throw new Exception($"Uniform '{name}' not found");
            GL.UniformMatrix3(loc, false, ref m);
        }

        public void SetFloat(string name, float f)
        {
            int loc = GL.GetUniformLocation(_handle, name);
            if (loc < 0) throw new Exception($"Uniform '{name}' not found");
            GL.Uniform1(loc, f);
        }


        public void Dispose()
        {
            GL.DeleteProgram(_handle);
        }
    }
}