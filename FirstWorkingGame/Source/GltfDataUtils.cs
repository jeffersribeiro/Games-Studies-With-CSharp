using glTFLoader.Schema;

namespace GameStudiesWithCSharp.FirstWorkingGame.Source
{
    public static class GltfDataUtils
    {
        public static float[] ExtractFloatAccessor(Gltf model, Accessor accessor)
        {
            var view = model.BufferViews[accessor.BufferView];
            var buffer = model.Buffers[view.Buffer];
            var data = File.ReadAllBytes(buffer.Uri.FirstOrDefault().ToString());

            int offset = view.ByteOffset + accessor.ByteOffset;
            int count = accessor.Count * GetTypeCount(accessor.Type.ToString());
            float[] result = new float[count];

            System.Buffer.BlockCopy(data, offset, result, 0, count * sizeof(float));
            return result;
        }

        public static uint[] ExtractUIntAccessor(Gltf model, Accessor accessor)
        {
            var view = model.BufferViews[accessor.BufferView];
            var buffer = model.Buffers[view.Buffer];
            var data = File.ReadAllBytes(buffer.Uri.FirstOrDefault().ToString());

            int offset = view.ByteOffset + accessor.ByteOffset;
            int count = accessor.Count;
            uint[] result = new uint[count];

            const int GLTF_UNSIGNED_INT = 5125;

            if (accessor.ComponentType == Accessor.ComponentTypeEnum.UNSIGNED_SHORT)
            {
                ushort[] temp = new ushort[count];
                System.Buffer.BlockCopy(data, offset, temp, 0, count * sizeof(ushort));
                for (int i = 0; i < count; i++) result[i] = temp[i];
            }
            else if ((int)accessor.ComponentType == GLTF_UNSIGNED_INT)
            {
                System.Buffer.BlockCopy(data, offset, result, 0, count * sizeof(uint));

            }
            else
            {
                throw new NotSupportedException("Index component type not supported");
            }

            return result;
        }

        private static int GetTypeCount(string type)
        {
            return type switch
            {
                "SCALAR" => 1,
                "VEC2" => 2,
                "VEC3" => 3,
                "VEC4" => 4,
                "MAT2" => 4,
                "MAT3" => 9,
                "MAT4" => 16,
                _ => throw new Exception("Unknown accessor type")
            };
        }
    }
}