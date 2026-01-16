using System.Numerics;

public static class AnimConsts
{
    public const int MaxBones = 100;
    public const int MaxBoneInfluence = 4;
}

public struct BoneInfo
{
    public int Id;
    public Matrix4x4 Offset;
}

public struct Vertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoords;
    public Vector3 Tangent;
    public Vector3 Bitangent;

    public int BoneId0, BoneId1, BoneId2, BoneId3;
    public float Weight0, Weight1, Weight2, Weight3;

    public void SetBoneDataToDefault()
    {
        BoneId0 = BoneId1 = BoneId2 = BoneId3 = -1;
        Weight0 = Weight1 = Weight2 = Weight3 = 0f;
    }

    public void SetBoneData(int boneId, float weight)
    {
        if (BoneId0 < 0) { BoneId0 = boneId; Weight0 = weight; return; }
        if (BoneId1 < 0) { BoneId1 = boneId; Weight1 = weight; return; }
        if (BoneId2 < 0) { BoneId2 = boneId; Weight2 = weight; return; }
        if (BoneId3 < 0) { BoneId3 = boneId; Weight3 = weight; return; }
        // Ignora extras (igual ao padrão do tutorial)
    }
}
