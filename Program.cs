using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class Program
{
    public static void Main()
    {
        var native = new NativeWindowSettings()
        {
            Title = "Skeletal Animation (LearnOpenGL port)",
            Size = new Vector2i(800, 600),
            Flags = ContextFlags.ForwardCompatible
        };

        using var window = new Game(GameWindowSettings.Default, native);
        window.Run();
    }
}

public sealed class Game : GameWindow
{
    private Shader _shader = null!;
    private Camera _camera = null!;
    private Model _model = null!;
    private Animation _animation = null!;
    private Animator _animator = null!;

    private bool _firstMouse = true;
    private Vector2 _lastMouse;

    public Game(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.Enable(EnableCap.DepthTest);
        CursorState = CursorState.Grabbed;

        _shader = new Shader("Shaders/anim_model.vs", "Shaders/anim_model.fs");
        _camera = new Camera(new Vector3(0, 0, 0));

        // >>> AJUSTE AQUI: caminho para o root do LearnOpenGL (onde existe "resources/...")
        // Ex: @"C:\...\LearnOpenGL\"
        var learnOpenGlRoot = @"./LearnOpenGL/"; // exemplo
        var daePath = System.IO.Path.Combine(learnOpenGlRoot, "resources/objects/vampire/dancing_vampire.dae");

        _model = new Model(daePath);
        _animation = new Animation(daePath, _model);
        _animator = new Animator(_animation);

        _lastMouse = new Vector2(Size.X / 2f, Size.Y / 2f);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (!IsFocused) return;

        var dt = (float)e.Time;

        if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            Close();

        if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)) _camera.ProcessKeyboard(CameraMovement.Forward, dt);
        if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)) _camera.ProcessKeyboard(CameraMovement.Backward, dt);
        if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A)) _camera.ProcessKeyboard(CameraMovement.Left, dt);
        if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D)) _camera.ProcessKeyboard(CameraMovement.Right, dt);

        var mouse = MouseState.Position;
        if (_firstMouse)
        {
            _lastMouse = mouse;
            _firstMouse = false;
        }
        else
        {
            var delta = mouse - _lastMouse;
            _lastMouse = mouse;
            _camera.ProcessMouseMovement(delta.X, -delta.Y);
        }

        var scroll = MouseState.ScrollDelta;
        if (scroll.Y != 0) _camera.ProcessMouseScroll(scroll.Y);

        _animator.UpdateAnimation(dt);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.ClearColor(0.05f, 0.05f, 0.05f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();

        var projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(_camera.Zoom),
            Size.X / (float)Size.Y,
            0.1f,
            100f);

        var view = _camera.GetViewMatrix();

        _shader.SetMatrix4("projection", projection);
        _shader.SetMatrix4("view", view);

        // envia finalBonesMatrices[100]
        var transforms = _animator.GetFinalBoneMatrices();
        for (int i = 0; i < transforms.Length; i++)
            _shader.SetMatrix4($"finalBonesMatrices[{i}]", transforms[i]);

        var model = Matrix4.Identity;
        model *= Matrix4.CreateTranslation(0f, -0.4f, 0f);
        model *= Matrix4.CreateScale(0.05f);

        _shader.SetMatrix4("model", model);

        _model.Draw(_shader);

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _model.Dispose();
        _shader.Dispose();
    }
}
