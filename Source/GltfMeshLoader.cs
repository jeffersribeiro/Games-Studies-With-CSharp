using SharpGLTF.Schema2;

namespace FirstWorkingGame.Source
{
    public static class GltfMeshLoader
    {
        public static Mesh LoadFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is null or empty", nameof(path));
            if (!File.Exists(path))
                throw new FileNotFoundException("GLTF file not found", path);

            ModelRoot model = ModelRoot.Load(path);
            Scene scene = model.DefaultScene
                     ?? model.LogicalScenes.FirstOrDefault()
                     ?? throw new InvalidOperationException("No scene or logical scenes found");

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

            // pick first node that has a mesh, then first primitive
            var prim = logicalMesh.Primitives
                   .FirstOrDefault()
               ?? throw new InvalidOperationException("Mesh has no primitives");

            return prim.ToEngineMesh();
        }
    }
}