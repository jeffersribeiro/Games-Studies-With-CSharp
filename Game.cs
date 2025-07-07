using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Game : GameWindow
{
    private Mesh _mesh;
    private Shader _shader;

    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
    {
    }

    protected override void OnLoad()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        _shader = new Shader("shader.vert", "shader.frag");
        _mesh = GltfMeshLoader.LoadFromFile("C:\\Users\\Jeffe\\OneDrive\\Documents\\Projects\\GameStudiesWithCSharp\\nave.gltf");
    }

    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();
        _mesh.Render();

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        _mesh.Dispose();
        _shader.Dispose();
    }
}
