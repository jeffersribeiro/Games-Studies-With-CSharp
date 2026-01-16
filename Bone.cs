using System.Numerics;

public struct KeyPosition { public Vector3 Position; public float TimeStamp; }
public struct KeyRotation { public Quaternion Orientation; public float TimeStamp; }
public struct KeyScale    { public Vector3 Scale; public float TimeStamp; }

public sealed class Bone
{
    private readonly List<KeyPosition> _positions = new();
    private readonly List<KeyRotation> _rotations = new();
    private readonly List<KeyScale> _scales = new();

    public string Name { get; }
    public int Id { get; }

    private Matrix4x4 _localTransform = Matrix4x4.Identity;

    public Bone(string name, int id, Assimp.NodeAnimationChannel channel)
    {
        Name = name;
        Id = id;

        foreach (var k in channel.PositionKeys)
            _positions.Add(new KeyPosition { Position = k.Value.ToVec3(), TimeStamp = (float)k.Time });

        foreach (var k in channel.RotationKeys)
            _rotations.Add(new KeyRotation { Orientation = k.Value.ToQuat(), TimeStamp = (float)k.Time });

        foreach (var k in channel.ScalingKeys)
            _scales.Add(new KeyScale { Scale = k.Value.ToVec3(), TimeStamp = (float)k.Time });
    }

    public void Update(float animationTime)
    {
        var translation = InterpolatePosition(animationTime);
        var rotation = InterpolateRotation(animationTime);
        var scale = InterpolateScaling(animationTime);
        _localTransform = translation * rotation * scale;
    }

    public Matrix4x4 GetLocalTransform() => _localTransform;

    private static float GetScaleFactor(float last, float next, float time)
        => (time - last) / (next - last);

    private int GetPositionIndex(float time)
    {
        for (int i = 0; i < _positions.Count - 1; i++)
            if (time < _positions[i + 1].TimeStamp) return i;
        return _positions.Count - 2;
    }

    private int GetRotationIndex(float time)
    {
        for (int i = 0; i < _rotations.Count - 1; i++)
            if (time < _rotations[i + 1].TimeStamp) return i;
        return _rotations.Count - 2;
    }

    private int GetScaleIndex(float time)
    {
        for (int i = 0; i < _scales.Count - 1; i++)
            if (time < _scales[i + 1].TimeStamp) return i;
        return _scales.Count - 2;
    }

    private Matrix4x4 InterpolatePosition(float time)
    {
        if (_positions.Count == 1)
            return Matrix4x4.CreateTranslation(_positions[0].Position);

        int p0 = GetPositionIndex(time);
        int p1 = p0 + 1;

        float factor = GetScaleFactor(_positions[p0].TimeStamp, _positions[p1].TimeStamp, time);
        var finalPos = Vector3.Lerp(_positions[p0].Position, _positions[p1].Position, factor);

        return Matrix4x4.CreateTranslation(finalPos);
    }

    private Matrix4x4 InterpolateRotation(float time)
    {
        if (_rotations.Count == 1)
        {
            var q = Quaternion.Normalize(_rotations[0].Orientation);
            return Matrix4x4.CreateFromQuaternion(q);
        }

        int r0 = GetRotationIndex(time);
        int r1 = r0 + 1;

        float factor = GetScaleFactor(_rotations[r0].TimeStamp, _rotations[r1].TimeStamp, time);
        var finalRot = Quaternion.Slerp(_rotations[r0].Orientation, _rotations[r1].Orientation, factor);
        finalRot = Quaternion.Normalize(finalRot);

        return Matrix4x4.CreateFromQuaternion(finalRot);
    }

    private Matrix4x4 InterpolateScaling(float time)
    {
        if (_scales.Count == 1)
            return Matrix4x4.CreateScale(_scales[0].Scale);

        int s0 = GetScaleIndex(time);
        int s1 = s0 + 1;

        float factor = GetScaleFactor(_scales[s0].TimeStamp, _scales[s1].TimeStamp, time);
        var finalScale = Vector3.Lerp(_scales[s0].Scale, _scales[s1].Scale, factor);

        return Matrix4x4.CreateScale(finalScale);
    }
}
