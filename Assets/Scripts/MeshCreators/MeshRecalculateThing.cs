using System.Collections.Generic;
using UnityEngine;

public class MeshRecalculateThing : MeshCreator
{
    
    public Mesh InputMesh;

    private Vector3 WarpVertexUsingQuad(Vector3 vertex, List<Vector3> points){
        float c1 = (1-vertex.x) * (1- vertex.z);
        float c2 = (1-vertex.x) * vertex.z;
        float c3 = vertex.x * vertex.z;
        float c4 = vertex.x * (1-vertex.z);

        return c1 * points[0] + c2 * points[1] + c3 * points[2] + c4 * points[3];
        }

    public override void RecalculateMesh(){
		Curve spline = GetComponent<Curve>();
		if(spline == null){
			Debug.Log("warpMeshQuad : this game objects needs to have a curve component");
			return;
		}
		List<Vector3> points = spline.points;
		if(points.Count != 4){
			Debug.Log("WarpMeshQuad : the curve components needs to have four points");
			return;
		}
		Vector3[] warpedVertices = new Vector3[InputMesh.vertices.Length];

		for (int i = 0; i<InputMesh.vertices.Length; i++){
			warpedVertices[i] = WarpVertexUsingQuad(InputMesh.vertices[i], points);

		}
		Mesh newMesh = new Mesh();
		newMesh.vertices = warpedVertices;
		newMesh.uv = InputMesh.uv;
		newMesh.subMeshCount = InputMesh.subMeshCount;

		for (int i = 0; i<InputMesh.subMeshCount; i++){
			newMesh.SetTriangles(InputMesh.GetTriangles(i), i);
		}
		newMesh.RecalculateNormals();
		newMesh.RecalculateTangents();

		GetComponent<MeshFilter>().mesh = newMesh;

	}
  
}
