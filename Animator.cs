using System.Numerics;

public sealed class Animator
{
    private Animation _currentAnimation;
    private float _currentTime;

    private readonly Matrix4x4[] _finalBoneMatrices = new Matrix4x4[AnimConsts.MaxBones];

    public Animator(Animation animation)
    {
        _currentAnimation = animation;
        _currentTime = 0f;

        for (int i = 0; i < _finalBoneMatrices.Length; i++)
            _finalBoneMatrices[i] = Matrix4x4.Identity;
    }

    public void UpdateAnimation(float dt)
    {
        if (_currentAnimation == null) return;

        _currentTime += _currentAnimation.GetTicksPerSecond() * dt;
        _currentTime %= _currentAnimation.GetDuration();

        CalculateBoneTransform(_currentAnimation.GetRootNode(), Matrix4x4.Identity);
    }

    public void PlayAnimation(Animation animation)
    {
        _currentAnimation = animation;
        _currentTime = 0f;
    }

    private void CalculateBoneTransform(AssimpNodeData node, Matrix4x4 parentTransform)
    {
        var nodeName = node.Name;
        var nodeTransform = node.Transformation;

        var bone = _currentAnimation.FindBone(nodeName);
        if (bone != null)
        {
            bone.Update(_currentTime);
            nodeTransform = bone.GetLocalTransform();
        }

        var globalTransform = parentTransform * nodeTransform;

        var boneInfoMap = _currentAnimation.GetBoneIDMap();
        if (boneInfoMap.TryGetValue(nodeName, out var info))
        {
            int index = info.Id;
            if ((uint)index < (uint)_finalBoneMatrices.Length)
                _finalBoneMatrices[index] = globalTransform * info.Offset;
        }

        foreach (var child in node.Children)
            CalculateBoneTransform(child, globalTransform);
    }

    public OpenTK.Mathematics.Matrix4[] GetFinalBoneMatrices()
    {
        // converte pro tipo do OpenTK na hora de enviar
        var arr = new OpenTK.Mathematics.Matrix4[_finalBoneMatrices.Length];
        for (int i = 0; i < _finalBoneMatrices.Length; i++)
            arr[i] = _finalBoneMatrices[i].ToOpenTK();
        return arr;
    }
}
