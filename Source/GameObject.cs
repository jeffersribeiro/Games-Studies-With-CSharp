using OpenTK.Mathematics;

namespace FirstWorkingGame.Source
{
    public class GameObject : IDisposable
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 RotationEuler { get; set; }
        public Vector3 Scale { get; set; }

        private readonly GltfModel _gltf;

        public GameObject(string path, Shader shader)
        {
            Name = Path.GetFileNameWithoutExtension(path);
            Position = Vector3.Zero;
            RotationEuler = Vector3.Zero;
            Scale = Vector3.One;
            _gltf = new GltfModel(path, shader);
        }

        public void Update(double dt)
        {
            // aqui só delega
            _gltf.Update(dt);
        }

        public void Render(Matrix4 view, Matrix4 proj)
        {
            var world = Matrix4.CreateScale(Scale)
                      * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(RotationEuler.X))
                      * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotationEuler.Y))
                      * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(RotationEuler.Z))
                      * Matrix4.CreateTranslation(Position);

            _gltf.Render(world, view, proj);
        }

        public void Dispose() => _gltf.Dispose();
    }
}