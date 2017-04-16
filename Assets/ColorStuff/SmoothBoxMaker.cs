using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothBoxMaker : MonoBehaviour {

	private void Start()
    {
        Vector3[] newVertices = new Vector3[8];
        Vector3[] newNormals = new Vector3[8];
        int[] newTriangles = new int[36];

        int index = 0;
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    newVertices[index] = new Vector3(x, y, z);
                    newNormals[index] = new Vector3(x, y, z).normalized;
                    index++;
                }

        AddFace(newTriangles, 0, 0, 1, 2);
        AddFace(newTriangles, 6, 1, 4, 2);
        AddFace(newTriangles, 12, 5, 1, 2);
        AddFace(newTriangles, 18, 4, 4, 2);
        AddFace(newTriangles, 24, 0, 4, 1);
        AddFace(newTriangles, 30, 2, 1, 4);

        Mesh mesh = new Mesh();
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
        mesh.triangles = newTriangles;

        GetComponent<MeshFilter>().mesh = mesh;
	}

    void AddFace(int[] triangles, int index, int org, int dx, int dy)
    {
        triangles[index++] = org;
        triangles[index++] = org ^ dx;
        triangles[index++] = org ^ dy;
        triangles[index++] = org ^ dy;
        triangles[index++] = org ^ dx;
        triangles[index++] = org ^ dx ^ dy;
    }
}
