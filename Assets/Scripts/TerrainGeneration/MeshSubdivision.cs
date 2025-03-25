using UnityEngine;
using System.Collections.Generic;

public static class MeshSubdivision
{
    public static void SubdivideTriangle(List<Vector3> vertices, List<int> triangles, int triIndex)
    {
        int v0 = triangles[triIndex];
        int v1 = triangles[triIndex + 1];
        int v2 = triangles[triIndex + 2];

        Vector3 V0 = vertices[v0];
        Vector3 V1 = vertices[v1];
        Vector3 V2 = vertices[v2];

        Vector3 M0 = (V0 + V1) * 0.5f;
        Vector3 M1 = (V1 + V2) * 0.5f;
        Vector3 M2 = (V2 + V0) * 0.5f;

        int m0Index = vertices.Count;
        vertices.Add(M0);

        int m1Index = vertices.Count;
        vertices.Add(M1);

        int m2Index = vertices.Count;
        vertices.Add(M2);

        triangles.RemoveRange(triIndex, 3);

        triangles.AddRange(new int[]
        {
            v0, m0Index, m2Index,
            m0Index, v1, m1Index,
            m2Index, m1Index, v2,
            m0Index, m1Index, m2Index
        });
    }
}
