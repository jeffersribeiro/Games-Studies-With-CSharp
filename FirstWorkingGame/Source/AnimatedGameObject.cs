using OpenTK.Mathematics;
using SharpGLTF.Schema2;

namespace FirstWorkingGame.Source
{
    public class AnimatedGameObject : IDisposable
    {
        // -- Fields & Properties ----------------------------------------
        public string Name { get; set; }

        // Transform simples
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 RotationEuler { get; set; } = Vector3.Zero;  // graus
        public Vector3 Scale { get; set; } = Vector3.One;

        private readonly ModelRoot _model;
        private readonly Scene _scene;
        private readonly Shader _shader;
        private readonly List<(Node node, Mesh mesh)> _nodeMeshes = new();

        private readonly Animation[] _animations;
        private float _time;
        public IReadOnlyList<Animation> Animations => _animations;
        public Animation CurrentAnimation { get; private set; }

        // -- Constructor -------------------------------------------------

        public AnimatedGameObject(string path, Shader shader, Vector3 position, Vector3 scale, string name = null)
        {

            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("glb path required", nameof(path));
            _shader = shader ?? throw new ArgumentNullException(nameof(shader));
            Position = position;
            Scale = scale;
            Name = name ?? Path.GetFileNameWithoutExtension(path);

            // 1) Load model + pick a scene
            _model = ModelRoot.Load(path);
            _scene = _model.DefaultScene
                  ?? _model.LogicalScenes.FirstOrDefault()
                  ?? throw new InvalidOperationException("No scene found in glb");

            // 2) Cache animations
            _animations = _model.LogicalAnimations.ToArray();

            // 3) Extract every mesh primitive into VAO/VBO/EBO
            foreach (var root in _scene.VisualChildren)
                CollectNodeMeshes(root);
        }

        public Matrix4 WorldMatrix
        {
            get
            {
                var t = Matrix4.CreateScale(Scale);
                var r = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(RotationEuler.X)) *
                        Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotationEuler.Y)) *
                        Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(RotationEuler.Z));
                var p = Matrix4.CreateTranslation(Position);
                return t * r * p;
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

                    // Flatten to interleaved [x,y,z, nx,ny,nz]
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

            // recurse children
            foreach (var child in node.VisualChildren)
                CollectNodeMeshes(child);
        }

        // -- Animation Control -------------------------------------------

        /// <summary>Start playing the animation at the given index.</summary>
        public void PlayAnimation(int index)
        {
            if (index < 0 || index >= _animations.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            CurrentAnimation = _animations[index];
            _time = 0f;
        }

        /// <summary>Advance the animation playhead and apply transforms into the node graph.</summary>
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

                            // get (time, value) pairs
                            var frames = sampler.GetLinearKeys().ToArray();
                            float[] times = frames.Select(f => f.Key).ToArray();
                            System.Numerics.Vector3[] values = frames.Select(f => f.Value).ToArray();

                            // find our keyframe interval
                            int idx = Array.BinarySearch(times, t);
                            if (idx < 0) idx = ~idx;
                            int i1 = Math.Clamp(idx, 1, times.Length - 1);
                            int i0 = i1 - 1;

                            float t0 = times[i0], t1 = times[i1];
                            float f = (t1 > t0) ? (t - t0) / (t1 - t0) : 0f;

                            // lerp in System.Numerics space
                            var p0 = values[i0];
                            var p1 = values[i1];
                            var sample = System.Numerics.Vector3.Lerp(p0, p1, f);

                            // write it back to the node
                            channel.TargetNode.WithLocalTranslation(sample);
                        }
                        break;

                    case PropertyPath.scale:
                        {
                            var sampler = channel.GetScaleSampler();
                            if (sampler == null) break;

                            var frames = sampler.GetLinearKeys().ToArray();
                            float[] times = frames.Select(f => f.Key).ToArray();
                            System.Numerics.Vector3[] values = frames.Select(f => f.Value).ToArray();

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
                            System.Numerics.Quaternion[] values = frames.Select(f => f.Value).ToArray();

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

        // -- Rendering ---------------------------------------------------

        /// <summary>Recursively draw every mesh, using each node’s world matrix.</summary>
        public void Render(Matrix4 view, Matrix4 proj)
        {
            _shader.Use();
            var model = WorldMatrix;
            _shader.SetMatrix4("uModel", model);

            // 2) normal matrix = inverse-transpose of model's top-left 3×3
            var nm = new Matrix3(model);
            nm.Invert();
            nm.Transpose();
            _shader.SetMatrix3("uNormalMatrix", nm);

            // 3) view & proj (shared across objects)
            _shader.SetMatrix4("uView", view);
            _shader.SetMatrix4("uProj", proj);

            foreach (var root in _scene.VisualChildren)
                DrawNodeRecursive(root, Matrix4.Identity, view, proj);
        }

        private void DrawNodeRecursive(Node node, Matrix4 parent, Matrix4 view, Matrix4 proj)
        {
            // 1) Compute this node’s world matrix:
            var localN = node.LocalMatrix;               // System.Numerics.Matrix4x4
            var local = ToMatrix4(localN);                   // convert to OpenTK.Matrix4
            var world = local * parent;

            // 2) If this node has meshes, draw them:
            foreach (var (n, mesh) in _nodeMeshes)
            {
                if (n == node)
                {
                    _shader.SetMatrix4("uModel", world);
                    _shader.SetMatrix4("uView", view);
                    _shader.SetMatrix4("uProj", proj);

                    // normal matrix = inverse-transpose of model’s upper-left 3×3
                    var nm = new Matrix3(world);
                    nm.Invert();
                    nm.Transpose();
                    _shader.SetMatrix3("uNormalMatrix", nm);

                    mesh.Render();
                }
            }

            // 3) Recurse into children
            foreach (var child in node.VisualChildren)
                DrawNodeRecursive(child, world, view, proj);
        }

        // Helper to convert System.Numerics.Matrix4x4 → OpenTK.Mathematics.Matrix4
        private static Matrix4 ToMatrix4(System.Numerics.Matrix4x4 m) => new(
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
        );

        // -- Cleanup -----------------------------------------------------

        public void Dispose()
        {
            foreach (var (_, mesh) in _nodeMeshes)
                mesh.Dispose();
        }
    }
}