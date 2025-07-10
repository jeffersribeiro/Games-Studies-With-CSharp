using OpenTK.Mathematics;
using SharpGLTF.Schema2;

namespace FirstWorkingGame.Source
{
    public class GltfModel : IDisposable
    {
        private readonly ModelRoot _model;
        private readonly Scene _scene;
        private readonly Shader _shader;
        private readonly List<(Node, Mesh)> _meshes;
        private readonly Animation[] _animations;
        private float _time;

        public GltfModel(string path, Shader shader)
        {
            // 1) Load + pick scene
            _model = ModelRoot.Load(path);
            _scene = _model.DefaultScene ?? _model.LogicalScenes.First();
            _shader = shader;
            _animations = _model.LogicalAnimations.ToArray();

            // 2) Extrair meshes
            _meshes = new List<(Node, Mesh)>();
            foreach (var root in _scene.VisualChildren)
                Collect(root);

            PlayAnimation(0);
        }

        public IReadOnlyList<Animation> Animations => _animations;
        public Animation CurrentAnimation { get; private set; }

        public void PlayAnimation(int idx)
        {
            if (idx < 0 || idx >= _animations.Length)
                throw new ArgumentOutOfRangeException();
            CurrentAnimation = _animations[idx];
            _time = 0;
        }

        public void Update(double dt)
        {
            if (CurrentAnimation == null) return;
            _time = (_time + (float)dt) % CurrentAnimation.Duration;

            foreach (var ch in CurrentAnimation.Channels)
                ch.ApplyAtTime(_time);     // encapsula o sampler + LERP/SLERP
        }

        public void Render(Matrix4 world, Matrix4 view, Matrix4 proj)
        {
            _shader.Use();
            _shader.SetMatrix4("uView", view);
            _shader.SetMatrix4("uProj", proj);

            void draw(Node node, Matrix4 parent)
            {
                var local = ToMatrix4(node.LocalMatrix) * parent;
                foreach (var (n, mesh) in _meshes)
                {
                    if (n == node)
                    {
                        _shader.SetMatrix4("uModel", local);
                        var nm = new Matrix3(local); nm.Invert(); nm.Transpose();
                        _shader.SetMatrix3("uNormalMatrix", nm);
                        mesh.Render();
                    }
                }
                foreach (var c in node.VisualChildren)
                    draw(c, local);
            }

            // Em vez de passar a Scene, percorra cada nó raiz:
            foreach (var root in _scene.VisualChildren)
                draw(root, world);
        }

        public void Dispose()
        {
            foreach (var (_, m) in _meshes) m.Dispose();
        }

        // ——— auxiliares ———
        private void Collect(Node node)
        {
            if (node.Mesh != null)
                foreach (var prim in node.Mesh.Primitives)
                    _meshes.Add((node, prim.ToEngineMesh()));

            foreach (var c in node.VisualChildren)
                Collect(c);
        }
        private static Matrix4 ToMatrix4(System.Numerics.Matrix4x4 m)
            => new Matrix4(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44);
    }
}