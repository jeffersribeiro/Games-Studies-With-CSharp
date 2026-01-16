using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public sealed class Shader : IDisposable
{
    public int Handle { get; }

    public Shader(string vertexPath, string fragmentPath)
    {
        var vertexSrc = File.ReadAllText(vertexPath);
        var fragSrc = File.ReadAllText(fragmentPath);

        var v = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(v, vertexSrc);
        GL.CompileShader(v);
        GL.GetShader(v, ShaderParameter.CompileStatus, out var vOk);
        if (vOk == 0) throw new Exception(GL.GetShaderInfoLog(v));

        var f = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(f, fragSrc);
        GL.CompileShader(f);
        GL.GetShader(f, ShaderParameter.CompileStatus, out var fOk);
        if (fOk == 0) throw new Exception(GL.GetShaderInfoLog(f));

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, v);
        GL.AttachShader(Handle, f);
        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out var linked);
        if (linked == 0) throw new Exception(GL.GetProgramInfoLog(Handle));

        GL.DetachShader(Handle, v);
        GL.DetachShader(Handle, f);
        GL.DeleteShader(v);
        GL.DeleteShader(f);
    }

    public void Use() => GL.UseProgram(Handle);

    public void SetInt(string name, int value)
    {
        var loc = GL.GetUniformLocation(Handle, name);
        if (loc >= 0) GL.Uniform1(loc, value);
    }

    public void SetMatrix4(string name, Matrix4 value)
    {
        var loc = GL.GetUniformLocation(Handle, name);
        if (loc >= 0) GL.UniformMatrix4(loc, false, ref value);
    }

    public void Dispose() => GL.DeleteProgram(Handle);
}
