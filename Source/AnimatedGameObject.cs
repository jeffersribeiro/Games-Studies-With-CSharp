using OpenTK.Mathematics;
using SharpGLTF.Schema2;

namespace FirstWorkingGame.Source
{

    /// <summary>
    /// Representa um objeto de jogo com transform e componente de glTF animado.
    /// </summary>
    public class AnimatedGameObject : IDisposable
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 RotationEuler { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        private readonly AnimatedGltfModel _gltf;

        public AnimatedGameObject(string path, Shader shader, Vector3 position, Vector3 scale, string name = null)
        {
            Name = name ?? Path.GetFileNameWithoutExtension(path);
            Position = position;
            Scale = scale;
            _gltf = new AnimatedGltfModel(path, shader);
        }

        public void PlayAnimation(int index) => _gltf.PlayAnimation(index);

        public void Update(double deltaTime)
        {
            _gltf.Update(deltaTime);
        }

        public void Render(Matrix4 view, Matrix4 proj)
        {
            // Construir matriz do objeto
            var s = Matrix4.CreateScale(Scale);
            var r = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(RotationEuler.X))
                   * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotationEuler.Y))
                   * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(RotationEuler.Z));
            var p = Matrix4.CreateTranslation(Position);
            var world = s * r * p;

            _gltf.Render(world, view, proj);
        }

        public void Dispose() => _gltf.Dispose();
    }
}
