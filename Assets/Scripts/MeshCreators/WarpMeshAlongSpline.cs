using System.Collections.Generic;
using UnityEngine;
using Handout;

public class WarpMeshAlongSpline : MeshCreator
{
    public Mesh InputMesh;
    public Vector3 MeshOrigin;
    public float MeshScale;
    public Vector2 TextureScale;
    public bool ComputeUVs;
    public bool ModifySharedMesh;

    public override void RecalculateMesh()
    {
        Curve curve = GetComponent<Curve>();
        if (curve == null)
            return;

        List<Vector3> points = curve.points;
        Debug.Log("Recalculating spline mesh");

        MeshBuilder builder = new MeshBuilder();

        if (points.Count < 2)
        {
            GetComponent<MeshFilter>().mesh = builder.CreateMesh(true);
            return;
        }

        Bounds bounds = InputMesh.bounds;
        Vector3 max = bounds.max;
        Vector3 min = bounds.min;

        // ✅ Compute smoothed orientations (using average of incident directions)
        var localOrientation = new List<Quaternion>();
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 forward;
            if (i == 0)
            {
                forward = (points[i + 1] - points[i]).normalized;
            }
            else if (i == points.Count - 1)
            {
                forward = (points[i] - points[i - 1]).normalized;
            }
            else
            {
                Vector3 dir1 = (points[i] - points[i - 1]).normalized;
                Vector3 dir2 = (points[i + 1] - points[i]).normalized;
                forward = (dir1 + dir2).normalized;
            }
            localOrientation.Add(Quaternion.LookRotation(forward, Vector3.up));
        }

        int vertexCount = InputMesh.vertexCount;
        Vector3[] vertices = InputMesh.vertices;
        Vector2[] uvs = InputMesh.uv;
        int subMeshCount = InputMesh.subMeshCount;

        // ✅ Warp mesh along spline
        for (int seg = 0; seg < points.Count - 1; seg++)
        {
            for (int v = 0; v < vertexCount; v++)
            {
                float t = (vertices[v].z - min.z) / (max.z - min.z);
                Vector3 inputV = (vertices[v] - MeshOrigin) * MeshScale;
                inputV.z = 0;

                Vector3 position = Vector3.Lerp(points[seg], points[seg + 1], t);

                // ✅ Interpolate orientation
                Quaternion orientation = Quaternion.Slerp(localOrientation[seg], localOrientation[seg + 1], t);
                Vector3 rotated = orientation * inputV;

                builder.AddVertex(position + rotated, uvs[v] / TextureScale);
            }

            // ✅ Add submesh triangles with correct offset
            for (int s = 0; s < subMeshCount; s++)
            {
                int[] tris = InputMesh.GetTriangles(s);
                for (int t = 0; t < tris.Length; t += 3)
                {
                    builder.AddTriangle(
                        tris[t] + seg * vertexCount,
                        tris[t + 1] + seg * vertexCount,
                        tris[t + 2] + seg * vertexCount,
                        s
                    );
                }
            }
        }

        Mesh mesh = builder.CreateMesh(true);
        var autoUV = GetComponent<AutoUv>();
        if (autoUV != null && autoUV.enabled && ComputeUVs)
        {
            autoUV.UpdateUVs(mesh);
        }

        ReplaceMesh(mesh, ModifySharedMesh);
    }
}