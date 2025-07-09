using OpenTK.Mathematics;
using SharpGLTF.Schema2;

namespace FirstWorkingGame.Source
{
    public class AnimatedGameObject : IDisposable
    {
        public string Name { get; set; }

        // Simple transform for the whole object
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 RotationEuler { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        private readonly ModelRoot _model;
        private readonly Scene _scene;
        private readonly Shader _shader;
        private readonly List<(Node node, Mesh mesh)> _nodeMeshes = new();

        private readonly Animation[] _animations;
        private float _time;
        public IReadOnlyList<Animation> Animations => _animations;
        public Animation CurrentAnimation { get; private set; }

        public AnimatedGameObject(string path, Shader shader, Vector3 position, Vector3 scale, string name = null)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("glb path required", nameof(path));
            _shader = shader ?? throw new ArgumentNullException(nameof(shader));

            Position = position;
            Scale = scale;
            Name = name ?? Path.GetFileNameWithoutExtension(path);

            // 1) Load the glTF and pick a scene
            _model = ModelRoot.Load(path);
            _scene = _model.DefaultScene
                  ?? _model.LogicalScenes.FirstOrDefault()
                  ?? throw new InvalidOperationException("No scene found in glb");

            // 2) Cache the available animations
            _animations = _model.LogicalAnimations.ToArray();

            // 3) Extract every mesh primitive into your Mesh wrapper
            foreach (var root in _scene.VisualChildren)
                CollectNodeMeshes(root);

            PlayAnimation(73);
        }

        // World matrix from this object’s Position/RotationEuler/Scale
        public Matrix4 WorldMatrix
        {
            get
            {
                var s = Matrix4.CreateScale(Scale);
                var r = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(RotationEuler.X))
                       * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotationEuler.Y))
                       * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(RotationEuler.Z));
                var p = Matrix4.CreateTranslation(Position);
                return s * r * p;
            }
        }

        private void CollectNodeMeshes(Node node)
        {
            if (node.Mesh != null)
            {
                foreach (var prim in node.Mesh.Primitives)
                {
                    // POSITION
                    var posAcc = prim.GetVertexAccessor("POSITION")
                                 ?? throw new InvalidOperationException("POSITION attribute missing");
                    var positions = posAcc.AsVector3Array();

                    // NORMAL
                    var normAcc = prim.GetVertexAccessor("NORMAL")
                                  ?? throw new InvalidOperationException("NORMAL attribute missing");
                    var normals = normAcc.AsVector3Array();

                    // INDICES
                    var idxAcc = prim.IndexAccessor
                                ?? throw new InvalidOperationException("Index accessor missing");
                    var indices = idxAcc.AsIndicesArray();

                    float[] vertices = new float[positions.Count * 6];
                    for (int i = 0; i < positions.Count; i++)
                    {
                        vertices[6 * i + 0] = positions[i].X;
                        vertices[6 * i + 1] = positions[i].Y;
                        vertices[6 * i + 2] = positions[i].Z;
                        vertices[6 * i + 3] = normals[i].X;
                        vertices[6 * i + 4] = normals[i].Y;
                        vertices[6 * i + 5] = normals[i].Z;
                    }
                    uint[] idxBuf = indices.Select(i => (uint)i).ToArray();

                    var mesh = new Mesh(vertices, idxBuf);

                    _nodeMeshes.Add((node, mesh));
                }
            }

            foreach (var child in node.VisualChildren)
                CollectNodeMeshes(child);
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

            _time += (float)deltaTime;
            float duration = CurrentAnimation.Duration;
            float t = _time % duration;

            foreach (var channel in CurrentAnimation.Channels)
            {
                switch (channel.TargetNodePath)
                {
                    case PropertyPath.translation:
                        {
                            var sampler = channel.GetTranslationSampler();
                            if (sampler == null) break;

                            var frames = sampler.GetLinearKeys().ToArray();
                            float[] times = frames.Select(f => f.Key).ToArray();
                            var values = frames.Select(f => f.Value).ToArray();

                            int idx = Array.BinarySearch(times, t);
                            if (idx < 0) idx = ~idx;
                            int i1 = Math.Clamp(idx, 1, times.Length - 1);
                            int i0 = i1 - 1;

                            float t0 = times[i0], t1 = times[i1];
                            float f = (t1 > t0) ? (t - t0) / (t1 - t0) : 0f;

                            var p0 = values[i0];
                            var p1 = values[i1];
                            var sample = System.Numerics.Vector3.Lerp(p0, p1, f);

                            channel.TargetNode.WithLocalTranslation(sample);
                        }
                        break;

                    case PropertyPath.scale:
                        {
                            var sampler = channel.GetScaleSampler();
                            if (sampler == null) break;

                            var frames = sampler.GetLinearKeys().ToArray();
                            float[] times = frames.Select(f => f.Key).ToArray();
                            var values = frames.Select(f => f.Value).ToArray();

                            int idx = Array.BinarySearch(times, t);
                            if (idx < 0) idx = ~idx;
                            int i1 = Math.Clamp(idx, 1, times.Length - 1);
                            int i0 = i1 - 1;

                            float t0 = times[i0], t1 = times[i1];
                            float f = (t1 > t0) ? (t - t0) / (t1 - t0) : 0f;

                            var s0 = values[i0];
                            var s1 = values[i1];
                            var sample = System.Numerics.Vector3.Lerp(s0, s1, f);

                            channel.TargetNode.WithLocalScale(sample);
                        }
                        break;

                    case PropertyPath.rotation:
                        {
                            var sampler = channel.GetRotationSampler();
                            if (sampler == null) break;

                            var frames = sampler.GetLinearKeys().ToArray();
                            float[] times = frames.Select(f => f.Key).ToArray();
                            var values = frames.Select(f => f.Value).ToArray();

                            int idx = Array.BinarySearch(times, t);
                            if (idx < 0) idx = ~idx;
                            int i1 = Math.Clamp(idx, 1, times.Length - 1);
                            int i0 = i1 - 1;

                            float t0 = times[i0], t1 = times[i1];
                            float f = (t1 > t0) ? (t - t0) / (t1 - t0) : 0f;

                            var q0 = values[i0];
                            var q1 = values[i1];
                            var sample = System.Numerics.Quaternion.Slerp(q0, q1, f);

                            channel.TargetNode.WithLocalRotation(sample);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Render the entire scene graph of this object.
        /// </summary>
        public void Render(Matrix4 view, Matrix4 proj)
        {
            _shader.Use();

            // Apply this root’s world matrix
            var rootWorld = WorldMatrix;
            _shader.SetMatrix4("uModel", rootWorld);

            var nm = new Matrix3(rootWorld);
            nm.Invert();
            nm.Transpose();
            _shader.SetMatrix3("uNormalMatrix", nm);

            _shader.SetMatrix4("uView", view);
            _shader.SetMatrix4("uProj", proj);

            foreach (var root in _scene.VisualChildren)
                DrawNodeRecursive(root, rootWorld, view, proj);
        }

        private void DrawNodeRecursive(Node node, Matrix4 parentWorld, Matrix4 view, Matrix4 proj)

        {
            // Local → world
            var localN = node.LocalMatrix;              // System.Numerics.Matrix4x4
            var local = ToMatrix4(localN);              // convert to OpenTK.Matrix4
            var world = local * parentWorld;

            foreach (var (n, mesh) in _nodeMeshes)
            {
                if (n == node)
                {
                    _shader.SetMatrix4("uModel", world);
                    _shader.SetMatrix4("uView", view);
                    _shader.SetMatrix4("uProj", proj);


                    var nm = new Matrix3(world);
                    nm.Invert();
                    nm.Transpose();
                    _shader.SetMatrix3("uNormalMatrix", nm);

                    mesh.Render();
                }
            }

            foreach (var child in node.VisualChildren)
                DrawNodeRecursive(child, world, view, proj);
        }

        private static Matrix4 ToMatrix4(System.Numerics.Matrix4x4 m) => new(
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
        );

        public void Dispose()
        {
            foreach (var (_, mesh) in _nodeMeshes)
                mesh.Dispose();
        }
    }
}
