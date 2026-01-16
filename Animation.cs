using System.Numerics;

public sealed class AssimpNodeData
{
    public string Name = "";
    public Matrix4x4 Transformation = Matrix4x4.Identity;
    public List<AssimpNodeData> Children = new();
}

public sealed class Animation
{
    private readonly float _duration;
    private readonly float _ticksPerSecond;

    private readonly List<Bone> _bones = new();
    private readonly Dictionary<string, BoneInfo> _boneInfoMap = new();

    private readonly AssimpNodeData _rootNode = new();

    public Animation(string animationPath, Model model)
    {
        using var importer = new Assimp.AssimpContext();
        var scene = importer.ImportFile(animationPath, Assimp.PostProcessSteps.Triangulate);
        if (scene == null || scene.RootNode == null || scene.Animations.Count == 0)
            throw new Exception("ASSIMP: animação não encontrada.");

        var anim = scene.Animations[0];

        _duration = (float)anim.DurationInTicks;
        _ticksPerSecond = anim.TicksPerSecond == 0 ? 25f : (float)anim.TicksPerSecond;

        ReadHierarchyData(_rootNode, scene.RootNode);
        ReadMissingBones(anim, model);
    }

    public Bone? FindBone(string name)
        => _bones.FirstOrDefault(b => b.Name == name);

    public float GetTicksPerSecond() => _ticksPerSecond;
    public float GetDuration() => _duration;
    public AssimpNodeData GetRootNode() => _rootNode;
    public Dictionary<string, BoneInfo> GetBoneIDMap() => _boneInfoMap;

    private void ReadMissingBones(Assimp.Animation animation, Model model)
    {
        var boneInfoMap = model.GetBoneInfoMap();
        ref int boneCount = ref model.GetBoneCount();

        foreach (var channel in animation.NodeAnimationChannels)
        {
            var boneName = channel.NodeName;

            if (!boneInfoMap.ContainsKey(boneName))
            {
                boneInfoMap[boneName] = new BoneInfo { Id = boneCount, Offset = Matrix4x4.Identity };
                boneCount++;
            }

            _bones.Add(new Bone(boneName, boneInfoMap[boneName].Id, channel));
        }

        // cópia local (igual ao LearnOpenGL)
        foreach (var kv in boneInfoMap)
            _boneInfoMap[kv.Key] = kv.Value;
    }

    private static void ReadHierarchyData(AssimpNodeData dest, Assimp.Node src)
    {
        dest.Name = src.Name;
        dest.Transformation = src.Transform.ToNumerics();

        foreach (var child in src.Children)
        {
            var childData = new AssimpNodeData();
            ReadHierarchyData(childData, child);
            dest.Children.Add(childData);
        }
    }
}
