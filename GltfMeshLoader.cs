using glTFLoader;
using glTFLoader.Schema;

public static class GltfMeshLoader
{
    public static Mesh LoadFromFile(string path)
    {
        var model = Interface.LoadModel(path);
        var mesh = model.Meshes.First();
        var primitive = mesh.Value.Primitives[0];

        var posAccessor = model.Accessors[primitive.Attributes["POSITION"]];
        var indicesAccessor = model.Accessors[primitive.Indices];

        float[] vertices = GltfDataUtils.ExtractFloatAccessor(model, posAccessor);
        uint[] indices = GltfDataUtils.ExtractUIntAccessor(model, indicesAccessor);

        return new Mesh(vertices, indices);
    }
}
