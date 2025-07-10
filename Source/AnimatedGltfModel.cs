using FirstWorkingGame.Source;
using OpenTK.Mathematics;
using SharpGLTF.Schema2;

namespace FirstWorkingGame.Source
{

    /// <summary>
    /// Encapsula carregamento, animação e renderização de um modelo glTF animado.
    /// </summary>
    public class AnimatedGltfModel : IDisposable
    {
        private readonly ModelRoot _model;
        private readonly Scene _scene;
        private readonly Shader _shader;
        private readonly List<(Node Node, Mesh Mesh)> _nodeMeshes = new();
        private readonly Animation[] _animations;
        private float _time;

        public IReadOnlyList<Animation> Animations => _animations;
        public Animation CurrentAnimation { get; private set; }

        public AnimatedGltfModel(string path, Shader shader)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("GLB path required", nameof(path));
            _shader = shader ?? throw new ArgumentNullException(nameof(shader));

            // 1) Carrega o glTF e seleciona a cena
            _model = ModelRoot.Load(path);
            _scene = _model.DefaultScene ?? _model.LogicalScenes.FirstOrDefault()
                          ?? throw new InvalidOperationException("No scene found in glb");

            // 2) Cache das animações disponíveis
            _animations = _model.LogicalAnimations.ToArray();

            // 3) Extrai as primitivas de malha
            foreach (var root in _scene.VisualChildren)
                CollectNodeMeshes(root);

            // Inicia primeira animação (se existir)
            if (_animations.Length > 0) PlayAnimation(0);
        }

        public void PlayAnimation(int index)
        {
            if (index < 0 || index >= _animations.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            CurrentAnimation = _animations[index];
            _time = 0f;
        }

        public void Update(double deltaTime)
        {
            if (CurrentAnimation == null) return;

            // Loop dentro da duração
            _time = (_time + (float)deltaTime) % CurrentAnimation.Duration;
            float t = _time;

            // Aplica cada canal de animação
            foreach (var channel in CurrentAnimation.Channels)
            {
                channel.ApplyAtTime(t);
            }
        }

        public void Render(Matrix4 world, Matrix4 view, Matrix4 proj)
        {
            _shader.Use();
            _shader.SetMatrix4("uView", view);
            _shader.SetMatrix4("uProj", proj);

            void DrawNode(Node node, Matrix4 parentWorld)
            {
                var local = ToMatrix4(node.LocalMatrix) * parentWorld;

                // Renderiza meshes associadas
                foreach (var (n, mesh) in _nodeMeshes)
                {
                    if (n == node)
                    {
                        _shader.SetMatrix4("uModel", local);
                        var nm = new Matrix3(local); nm.Invert(); nm.Transpose();
                        _shader.SetMatrix3("uNormalMatrix", nm);
                        mesh.Render();
                    }
                }

                // Recursão para filhos
                foreach (var child in node.VisualChildren)
                    DrawNode(child, local);
            }

            // <<--- aqui: itera sobre todos os nós raiz da cena
            foreach (var root in _scene.VisualChildren)
                DrawNode(root, world);
        }

        private void CollectNodeMeshes(Node node)
        {
            if (node.Mesh != null)
            {
                foreach (var prim in node.Mesh.Primitives)
                    _nodeMeshes.Add((node, prim.ToEngineMesh()));
            }

            foreach (var child in node.VisualChildren)
                CollectNodeMeshes(child);
        }

        private static Matrix4 ToMatrix4(System.Numerics.Matrix4x4 m)
            => new Matrix4(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44);

        public void Dispose()
        {
            foreach (var (_, mesh) in _nodeMeshes)
                mesh.Dispose();
        }
    }
}