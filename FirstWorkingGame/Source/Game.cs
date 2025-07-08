using System.IO;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FirstWorkingGame.Source
{
    public class Game : GameWindow
    {
        private Shader _shader;
        private FreeFlyCamera _camera;
        private bool _rightMouseDown = false;
        private Vector2 _lastMousePos;
        private List<AnimatedGameObject> _objects;

        public Game(int width, int height, string title)
            : base(
                GameWindowSettings.Default,
                new NativeWindowSettings
                {
                    ClientSize = new Vector2i(width, height),
                    Title = title,

                })
        {
            UpdateFrame += OnUpdateFrame;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            _camera = new FreeFlyCamera(startPosition: new Vector3(0f, 0f, 3f));

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            var baseDir = Path.GetDirectoryName(typeof(Game).Assembly.Location);

            _shader = new Shader(
                AssetPaths.Shaders("shader.vert"),
                AssetPaths.Shaders("shader.frag"));

            _objects = new List<AnimatedGameObject>
        {
            new AnimatedGameObject(
                path: AssetPaths.GltfCharacters("Barbarian.glb"),
                shader: _shader,
                position: new Vector3(0, 0, 0),
                scale:    new Vector3(0.5f)
            ),
            new AnimatedGameObject(
                path: AssetPaths.GltfCharacters("Knight.glb"),
                shader: _shader,
                position: new Vector3(100, 100, 100),
                scale:    new Vector3(0.7f)
            ),
        };


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
                _camera.ProcessMouseMovement(e.DeltaX, e.DeltaY);
                _lastMousePos = e.Position;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _camera.ProcessMouseScroll(e.OffsetY);
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


            var view = _camera.GetViewMatrix();
            var proj = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(_camera.Zoom),
                Size.X / (float)Size.Y,
                0.1f, 100f
            );
            _shader.SetMatrix4("uView", view);
            _shader.SetMatrix4("uProj", proj);

            // Global light uniforms
            _shader.SetVector3("uLightPos", new Vector3(1.2f, 3.4f, 5.6f));
            _shader.SetVector3("uLightAmbient", new Vector3(0.1f));
            _shader.SetVector3("uLightDiffuse", new Vector3(0.8f));
            _shader.SetVector3("uLightSpecular", new Vector3(1.0f));

            // Material is same for all; if different per object, move inside loop
            _shader.SetVector3("uMaterialAmbient", new Vector3(1.0f));
            _shader.SetVector3("uMaterialDiffuse", new Vector3(0.6f, 0.7f, 0.8f));
            _shader.SetVector3("uMaterialSpecular", new Vector3(1.0f));
            _shader.SetFloat("uMaterialShininess", 32.0f);

            foreach (var obj in _objects)
            {
                obj.Render(view, proj);
            }

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {

            // how much time has passed since last update:
            float deltaTime = (float)e.Time;
            foreach (var obj in _objects)
            {
                obj.Update(deltaTime);
            }


            // grab the current keyboard state:
            var input = KeyboardState;

            // move the free‐fly camera:
            if (input.IsKeyDown(Keys.W)) _camera.ProcessKeyboard(CameraMovement.Forward, deltaTime);
            if (input.IsKeyDown(Keys.S)) _camera.ProcessKeyboard(CameraMovement.Backward, deltaTime);
            if (input.IsKeyDown(Keys.A)) _camera.ProcessKeyboard(CameraMovement.Left, deltaTime);
            if (input.IsKeyDown(Keys.D)) _camera.ProcessKeyboard(CameraMovement.Right, deltaTime);
            if (input.IsKeyDown(Keys.Space)) _camera.ProcessKeyboard(CameraMovement.Up, deltaTime);
            if (input.IsKeyDown(Keys.LeftControl)) _camera.ProcessKeyboard(CameraMovement.Down, deltaTime);
        }

        protected override void OnUnload()
        {
            foreach (var obj in _objects)
                obj.Dispose();
            _shader.Dispose();
            base.OnUnload();
        }
    }
}