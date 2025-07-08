using System.IO;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FirstWorkingGame.Source
{
    public class Game : GameWindow
    {
        private Shader _shader;
        private OrbitalCamera _camera;
        private bool _rightMouseDown = false;
        private Vector2 _lastMousePos;
        private GameObject _triangle3d;



        public Game(int width, int height, string title)
            : base(
                GameWindowSettings.Default,
                new NativeWindowSettings
                {
                    ClientSize = new Vector2i(width, height),
                    Title = title
                })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();
            _camera = new OrbitalCamera(initialDistance: 5f);

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            var baseDir = Path.GetDirectoryName(typeof(Game).Assembly.Location);

            _shader = new Shader(
                Path.Combine(baseDir, "Assets", "Shaders", "shader.vert"),
                Path.Combine(baseDir, "Assets", "Shaders", "shader.frag"));

            var triangle3dPath = Path.Combine(baseDir, "Assets", "Models", "GLTF", "nave.gltf");

            _triangle3d = GameObject.LoadFromGltf(triangle3dPath, _shader);
            _triangle3d.Position = new Vector3(0, 0, 0);
            _triangle3d.Scale = new Vector3(0.5f);

        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Right && e.IsPressed)
            {
                _rightMouseDown = true;
                // Get the current pointer pos from the window's MouseState
                _lastMousePos = MouseState.Position;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Right && !e.IsPressed)
            {
                _rightMouseDown = false;
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (_rightMouseDown)
            {
                // e.Position *is* valid here on MouseMoveEventArgs
                Vector2 delta = e.Position - _lastMousePos;
                _camera.Rotate(delta.X, delta.Y);
                _lastMousePos = e.Position;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _camera.Zoom(-e.OffsetY);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            // build your matrices:
            var model = Matrix4.Identity;
            var view = _camera.GetViewMatrix();
            var proj = Matrix4.CreatePerspectiveFieldOfView(
                                 MathHelper.DegreesToRadians(45f),
                                 ClientSize.X / (float)ClientSize.Y,
                                 0.1f, 100f);


            // send them:
            _shader.SetMatrix4("uModel", model);
            _shader.SetMatrix4("uView", view);
            _shader.SetMatrix4("uProj", proj);
            _shader.SetVector3("uLightPos", new Vector3(1.2f, 3.4f, 5.6f));

            // or if you used a single uMVP uniform:
            // var mvp = proj * view * model;
            // _shader.SetMatrix4("uMVP", mvp);

            _triangle3d.Render(view, proj);
            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            _triangle3d.Dispose();
            _shader.Dispose();
        }
    }
}