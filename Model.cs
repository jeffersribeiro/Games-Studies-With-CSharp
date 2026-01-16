using Assimp;
using System.Numerics;

public sealed class Model : IDisposable
{
    private readonly List<Mesh> _meshes = new();
    private readonly string _directory;

    private readonly Dictionary<string, BoneInfo> _boneInfoMap = new();
    private int _boneCounter = 0;

    public Model(string path)
    {
        _directory = Path.GetDirectoryName(path) ?? "";
        LoadModel(path);
    }

    public Dictionary<string, BoneInfo> GetBoneInfoMap() => _boneInfoMap;
    public ref int GetBoneCount() => ref _boneCounter;

    private void LoadModel(string path)
    {
        using var importer = new AssimpContext();

        var flags =
            PostProcessSteps.Triangulate |
            PostProcessSteps.GenerateSmoothNormals |
            PostProcessSteps.CalculateTangentSpace;

        var scene = importer.ImportFile(path, flags);
        if (scene == null || scene.RootNode == null)
            throw new Exception("ASSIMP: falha ao carregar model/scene.");

        ProcessNode(scene.RootNode, scene);
    }

    private void ProcessNode(Node node, Scene scene)
    {
        foreach (var meshIndex in node.MeshIndices)
        {
            var mesh = scene.Meshes[meshIndex];
            _meshes.Add(ProcessMesh(mesh, scene));
        }

        foreach (var child in node.Children)
            ProcessNode(child, scene);
    }

    private Mesh ProcessMesh(Assimp.Mesh mesh, Scene scene)
    {
        var vertices = new Vertex[mesh.VertexCount];

        for (int i = 0; i < mesh.VertexCount; i++)
        {
            var v = new Vertex();
            v.SetBoneDataToDefault();

            v.Position = mesh.Vertices[i].ToVec3();
            v.Normal = (mesh.HasNormals ? mesh.Normals[i].ToVec3() : new Vector3(0, 1, 0));

            if (mesh.HasTextureCoords(0))
                v.TexCoords = mesh.TextureCoordinateChannels[0][i].ToVec2();
            else
                v.TexCoords = Vector2.Zero;

            if (mesh.HasTangentBasis)
            {
                v.Tangent = mesh.Tangents[i].ToVec3();
                v.Bitangent = mesh.BiTangents[i].ToVec3();
            }
            else
            {
                v.Tangent = Vector3.Zero;
                v.Bitangent = Vector3.Zero;
            }

            vertices[i] = v;
        }

        var indices = new List<uint>(mesh.FaceCount * 3);
        foreach (var face in mesh.Faces)
            foreach (var idx in face.Indices)
                indices.Add((uint)idx);

        Texture2D? diffuse = null;
        if (mesh.MaterialIndex >= 0)
        {
            var mat = scene.Materials[mesh.MaterialIndex];
            diffuse = LoadDiffuse(mat);
        }

        ExtractBoneWeightsForVertices(ref vertices, mesh);

        return new Mesh(vertices, indices.ToArray(), diffuse);
    }

    private Texture2D? LoadDiffuse(Material mat)
    {
        if (mat.GetMaterialTextureCount(TextureType.Diffuse) <= 0)
            return null;

        mat.GetMaterialTexture(TextureType.Diffuse, 0, out TextureSlot slot);

        // slot.FilePath geralmente é relativo (ex: "diffuse.png")
        var texPath = Path.Combine(_directory, slot.FilePath);
        if (!File.Exists(texPath))
        {
            // às vezes vem com subpasta; tenta relativo ao diretório do model
            var alt = Path.Combine(_directory, slot.FilePath.Replace('\\', Path.DirectorySeparatorChar));
            if (File.Exists(alt)) texPath = alt;
        }

        if (!File.Exists(texPath))
            return null;

        return new Texture2D(texPath);
    }

    private void ExtractBoneWeightsForVertices(ref Vertex[] vertices, Assimp.Mesh mesh)
    {
        // Igual ao LearnOpenGL: m_BoneInfoMap / m_BoneCounter
        foreach (var bone in mesh.Bones)
        {
            var boneName = bone.Name;

            if (!_boneInfoMap.TryGetValue(boneName, out var info))
            {
                info = new BoneInfo
                {
                    Id = _boneCounter,
                    Offset = bone.OffsetMatrix.ToNumerics()
                };
                _boneInfoMap[boneName] = info;
                _boneCounter++;
            }

            int boneId = _boneInfoMap[boneName].Id;

            foreach (var vw in bone.VertexWeights)
            {
                int vertexId = vw.VertexID;
                float weight = vw.Weight;

                if ((uint)vertexId >= (uint)vertices.Length) continue;
                vertices[vertexId].SetBoneData(boneId, weight);
            }
        }
    }

    public void Draw(Shader shader)
    {
        foreach (var m in _meshes)
            m.Draw(shader);
    }

    public void Dispose()
    {
        foreach (var m in _meshes)
            m.Dispose();
        _meshes.Clear();
    }
}
