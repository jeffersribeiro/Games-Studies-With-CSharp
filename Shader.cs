using OpenTK.Graphics.OpenGL4;
using System.IO;

public class Shader
{
    private int _handle;

    public Shader(string vertexPath, string fragmentPath)
    {
        string vertexCode = File.ReadAllText(vertexPath);
        string fragmentCode = File.ReadAllText(fragmentPath);

        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, vertexCode);
        GL.CompileShader(vertex);

        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, fragmentCode);
        GL.CompileShader(fragment);

        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vertex);
        GL.AttachShader(_handle, fragment);
        GL.LinkProgram(_handle);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
    }

    public void Use() => GL.UseProgram(_handle);

    public void Dispose() => GL.DeleteProgram(_handle);
}
