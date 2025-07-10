using SharpGLTF.Schema2;
using SharpGLTF.Scenes;
using System;
using System.Linq;
using System.Numerics;

namespace FirstWorkingGame.Source
{
    public static class AnimationChannelExtensions
    {
        public static void ApplyAtTime(this AnimationChannel channel, float time)
        {
            switch (channel.TargetNodePath)
            {
                case PropertyPath.translation:
                    ApplyTranslation(channel, time);
                    break;
                case PropertyPath.scale:
                    ApplyScale(channel, time);
                    break;
                case PropertyPath.rotation:
                    ApplyRotation(channel, time);
                    break;
            }
        }

        private static void ApplyTranslation(AnimationChannel channel, float t)
        {
            var sampler = channel.GetTranslationSampler();
            if (sampler == null) return;

            var keys = sampler.GetLinearKeys().ToArray();
            Interpolate(keys.Select(k => k.Key).ToArray(),
                        keys.Select(k => k.Value).ToArray(),
                        t,
                        v => channel.TargetNode.WithLocalTranslation(v));
        }

        private static void ApplyScale(AnimationChannel channel, float t)
        {
            var sampler = channel.GetScaleSampler();
            if (sampler == null) return;

            var keys = sampler.GetLinearKeys().ToArray();
            Interpolate(keys.Select(k => k.Key).ToArray(),
                        keys.Select(k => k.Value).ToArray(),
                        t,
                        v => channel.TargetNode.WithLocalScale(v));
        }

        private static void ApplyRotation(AnimationChannel channel, float t)
        {
            var sampler = channel.GetRotationSampler();
            if (sampler == null) return;

            var keys = sampler.GetLinearKeys().ToArray();
            Interpolate(keys.Select(k => k.Key).ToArray(),
                        keys.Select(k => k.Value).ToArray(),
                        t,
                        v => channel.TargetNode.WithLocalRotation(v),
                        isRotation: true);
        }

        // genérico pra translation/scale (Vector3) e rotação (Quaternion)
        private static void Interpolate<T>(float[] times, T[] values, float t, Action<T> apply, bool isRotation = false)
        {
            if (times.Length == 0) return;
            // loop around duration
            float duration = times.Last();
            t %= duration;

            int idx = Array.BinarySearch(times, t);
            if (idx < 0) idx = ~idx;
            int i1 = Math.Clamp(idx, 1, times.Length - 1);
            int i0 = i1 - 1;

            float t0 = times[i0], t1 = times[i1];
            float f = (t1 > t0) ? (t - t0) / (t1 - t0) : 0f;

            if (!isRotation)
            {
                // Vector3 lerp
                var v0 = (Vector3)(object)values[i0];
                var v1 = (Vector3)(object)values[i1];
                apply((T)(object)Vector3.Lerp(v0, v1, f));
            }
            else
            {
                // Quaternion slerp
                var q0 = (Quaternion)(object)values[i0];
                var q1 = (Quaternion)(object)values[i1];
                apply((T)(object)Quaternion.Slerp(q0, q1, f));
            }
        }
    }
}
