## Step 1: Scaffold the Project

### 1.1 Create a new .NET project
Open a terminal and run:
```bash
mkdir My3DViewer
cd My3DViewer
dotnet new console -n My3DViewer
cd My3DViewer
```  
This creates a console app named **My3DViewer**.

### 1.2 Add necessary NuGet packages
```bash
dotnet add package OpenTK
dotnet add package SharpGLTF
```  
- **OpenTK** provides windowing, input, and OpenGL bindings.  
- **SharpGLTF** lets us load `.gltf` models.

### 1.3 Define folder structure
```
My3DViewer/
├── My3DViewer.csproj        # project file
├── Program.cs               # entry point
├── Assets/
│   ├── Shaders/
│   │   ├── shader.vert
│   │   └── shader.frag
│   └── Models/
│       └── GLTF/
│           └── nave.gltf
└── Source/
    ├── Game.cs
    ├── Shader.cs
    ├── Mesh.cs
    ├── GltfMeshLoader.cs
    ├── GameObject.cs
    └── OrbitalCamera.cs
```

### 1.4 Inspect the project file `My3DViewer.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="OpenTK" Version="4.*" />
    <PackageReference Include="SharpGLTF" Version="2.*" />
  </ItemGroup>
  <ItemGroup>
    <None Remove="Assets\**" />
    <None Update="Assets\**">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

> This `.csproj` sets up .NET 7.0, references OpenTK and SharpGLTF, and ensures all files in `Assets/` are copied to the build output.

---

*Next up: we’ll create **Program.cs** and implement the `Game : GameWindow` class, wiring everything together.*

## Step 2: OrbitalCamera Class

Before wiring up the `Game`, let’s define the camera you’ll use for orbiting around your model.

**File:** `Source/OrbitalCamera.cs`
```csharp
using OpenTK.Mathematics;
using System;

namespace My3DViewer
{
    /// <summary>
    /// An arcball/orbit camera that orbits around a fixed target using spherical coordinates.
    /// </summary>
    public class OrbitalCamera
    {
        // Spherical coords
        private float _azimuth;    // horizontal angle in radians
        private float _elevation;  // vertical angle in radians
        private float _distance;   // distance from the target

        public Vector3 Target { get; set; } = Vector3.Zero;
        public float MinDistance { get; set; } = 0.5f;
        public float MaxDistance { get; set; } = 20f;

        public OrbitalCamera(float initialDistance = 5f)
        {
            _distance = initialDistance;
            _azimuth   = MathHelper.DegreesToRadians(45f);
            _elevation = MathHelper.DegreesToRadians(30f);
        }

        /// <summary>
        /// Rotate around the target. dx, dy in input units (e.g. pixels).
        /// </summary>
        public void Rotate(float dx, float dy)
        {
            const float ROTATE_SPEED = 0.005f;
            _azimuth   += dx * ROTATE_SPEED;
            _elevation += dy * ROTATE_SPEED;
            _elevation = MathHelper.Clamp(_elevation, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
        }

        /// <summary>
        /// Zoom in/out. Positive delta → zoom out, negative → zoom in.
        /// </summary>
        public void Zoom(float delta)
        {
            const float ZOOM_SPEED = 0.1f;
            _distance += delta * ZOOM_SPEED;
            _distance = MathHelper.Clamp(_distance, MinDistance, MaxDistance);
        }

        /// <summary>
        /// Builds and returns the view matrix (`LookAt`) based on current spherical coords.
        /// </summary>
        public Matrix4 GetViewMatrix()
        {
            float x = _distance * MathF.Cos(_elevation) * MathF.Cos(_azimuth);
            float y = _distance * MathF.Sin(_elevation);
            float z = _distance * MathF.Cos(_elevation) * MathF.Sin(_azimuth);
            var position = Target + new Vector3(x, y, z);
            return Matrix4.LookAt(position, Target, Vector3.UnitY);
        }

        /// <summary>
        /// Exposes the current world-space camera position.
        /// </summary>
        public Vector3 Position
            => Target + new Vector3(
                _distance * MathF.Cos(_elevation) * MathF.Cos(_azimuth),
                _distance * MathF.Sin(_elevation),
                _distance * MathF.Cos(_elevation) * MathF.Sin(_azimuth)
            );
    }
}
```

This class:
- Encapsulates all orbit camera math in one place.
- Exposes `Rotate`, `Zoom`, and `GetViewMatrix()`.
- Provides a `Position` property you can use for `uViewPos` uniforms.

## Step 3: Program Entry & Game Class

### 3.1 Program.cs
Create the entry point in `Program.cs`:
```csharp
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace My3DViewer
{
    class Program
    {
        static void Main()
        {
            var nativeSettings = new NativeWindowSettings
            {
                Size = new Vector2i(800, 600),
                Title = "My 3D Viewer"
            };

            using (var game = new Game(800, 600, "My 3D Viewer"))
            {
                game.Run();
            }
        }
    }
}
```

### 3.2 Game.cs
Add `Source/Game.cs` with your `Game` subclass (now that `OrbitalCamera` exists):
```csharp
using System;
using System.IO;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Game : GameWindow
{
    private Shader _shader;
    private OrbitalCamera _camera;
    private bool _rightMouseDown = false;
    private Vector2 _lastMousePos;
    private GameObject _triangle3d;

    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            ClientSize = new Vector2i(width, height),
            Title = title
        }) { }

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

        _triangle3d = GameObject.LoadFromGltf(
            Path.Combine(baseDir, "Assets", "Models", "GLTF", "nave.gltf"),
            _shader);
        _triangle3d.Scale = new Vector3(0.5f);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);
        var input = KeyboardState;
        float dt = (float)e.Time;
        if (input.IsKeyDown(Keys.W)) _camera.ProcessKeyboard(CameraMovement.Forward, dt);
        if (input.IsKeyDown(Keys.S)) _camera.ProcessKeyboard(CameraMovement.Backward, dt);
        if (input.IsKeyDown(Keys.A)) _camera.ProcessKeyboard(CameraMovement.Left, dt);
        if (input.IsKeyDown(Keys.D)) _camera.ProcessKeyboard(CameraMovement.Right, dt);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();
        var view = _camera.GetViewMatrix();
        var proj = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45f),
            ClientSize.X / (float)ClientSize.Y,
            0.1f, 100f);

        _shader.SetMatrix4("uView", view);
        _shader.SetMatrix4("uProj", proj);
        _shader.SetVector3("uLightPos", new Vector3(1.2f, 3.4f, 5.6f));
        _shader.SetVector3("uViewPos", _camera.Position);

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
```
