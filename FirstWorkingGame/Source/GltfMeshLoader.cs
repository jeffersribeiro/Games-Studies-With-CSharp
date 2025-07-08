using System;
using System.IO;
using System.Linq;
using SharpGLTF.Schema2;
using SharpGLTF.Geometry;

namespace FirstWorkingGame.Source
{
    public static class GltfMeshLoader
    {
        public static Mesh LoadFromFile(string path)
        {
            // 1) Validate path
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is null or empty", nameof(path));
            if (!File.Exists(path))
                throw new FileNotFoundException("GLTF file not found", path);

            // 2) Load the model
            ModelRoot model = ModelRoot.Load(path);

            // 3) Attempt to grab meshes via the default scene
            Scene scene = model.DefaultScene
                       ?? model.LogicalScenes.FirstOrDefault();

            // collect all Mesh instances either from scene nodes or fallback to LogicalMeshes
            var meshesFromScene = scene != null
                ? scene.VisualChildren
                       .Where(n => n.Mesh != null)
                       .Select(n => n.Mesh)
                : Enumerable.Empty<SharpGLTF.Schema2.Mesh>();

            var allMeshes = meshesFromScene.Any()
                ? meshesFromScene
                : model.LogicalMeshes;   // fallback when no scene/instances defined

            var logicalMesh = allMeshes.FirstOrDefault()
               ?? throw new InvalidOperationException("No mesh found in glTF model");

            // 4) Grab the first primitive
            var prim = logicalMesh.Primitives
                       .FirstOrDefault()
                   ?? throw new InvalidOperationException("Mesh has no primitives");

            // 5) Extract POSITION attribute
            var posAccessor = prim.GetVertexAccessor("POSITION")
                             ?? throw new InvalidOperationException("POSITION attribute missing");
            var positions = posAccessor.AsVector3Array();   // Vector3[]

            var normAccessor = prim.GetVertexAccessor("NORMAL")
                   ?? throw new InvalidOperationException("NORMAL attribute missing");
            var normals = normAccessor.AsVector3Array(); // Vector3[]

            // 6) Extract indices
            var idxAccessor = prim.IndexAccessor
                            ?? throw new InvalidOperationException("Indices accessor missing");
            var indices = idxAccessor.AsIndicesArray();     // int[]

            uint[] indexBuf = indices
                               .Select(i => (uint)i)
                               .ToArray();

            int vertCount = positions.Count;
            float[] vertices = new float[vertCount * 6];
            for (int i = 0; i < vertCount; i++)
            {
                var p = positions[i];
                var n = normals[i];
                vertices[i * 6 + 0] = p.X;
                vertices[i * 6 + 1] = p.Y;
                vertices[i * 6 + 2] = p.Z;
                vertices[i * 6 + 3] = n.X;
                vertices[i * 6 + 4] = n.Y;
                vertices[i * 6 + 5] = n.Z;
            }

            // 8) Return your engine‐Mesh
            return new Mesh(vertices, indexBuf);
        }
    }
}