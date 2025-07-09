using SharpGLTF.Schema2;

namespace FirstWorkingGame.Source
{
    public static class GltfExtensions
    {
        /// <summary>
        /// Converts a glTF mesh-primitive into your engine’s Mesh (positions + normals + indices).
        /// </summary>
        public static Mesh ToEngineMesh(this MeshPrimitive prim)
        {
            // POSITION
            var posAcc = prim.GetVertexAccessor("POSITION")
                         ?? throw new InvalidOperationException("POSITION attribute missing");
            var positions = posAcc.AsVector3Array();

            // NORMAL
            var normAcc = prim.GetVertexAccessor("NORMAL")
                          ?? throw new InvalidOperationException("NORMAL attribute missing");
            var normals = normAcc.AsVector3Array();

            // INDICES
            var idxAcc = prim.IndexAccessor
                        ?? throw new InvalidOperationException("Index accessor missing");
            var indices = idxAcc.AsIndicesArray();

            // interleave into [ x, y, z, nx, ny, nz, … ]
            float[] vertices = new float[positions.Count * 6];
            for (int i = 0; i < positions.Count; i++)
            {
                vertices[6 * i + 0] = positions[i].X;
                vertices[6 * i + 1] = positions[i].Y;
                vertices[6 * i + 2] = positions[i].Z;
                vertices[6 * i + 3] = normals[i].X;
                vertices[6 * i + 4] = normals[i].Y;
                vertices[6 * i + 5] = normals[i].Z;
            }

            uint[] indexBuf = indices.Select(i => (uint)i).ToArray();

            return new Mesh(vertices, indexBuf);
        }
    }
}
