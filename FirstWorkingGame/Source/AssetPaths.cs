using System.Reflection;

namespace FirstWorkingGame.Source
{
    public static class AssetPaths
    {
        private static readonly string BaseDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static readonly string AssetsDir = Path.Combine(BaseDir, "Assets");

        public static string Shaders(string filename) => Path.Combine(BaseDir, "Assets", "Shaders", filename);
        public static string Models(string fileName) => Path.Combine(AssetsDir, "Models", fileName);
        public static string Gltf(string fileName) => Path.Combine(AssetsDir, "Models", "GLTF", fileName);
        public static string GltfCharacters(string fileName) => Path.Combine(AssetsDir, "Characters", "gltf", fileName);
    }
}