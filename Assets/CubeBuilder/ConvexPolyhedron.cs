using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ConvexPolyhedronFace
{
    public ConvexPolyhedron polyhedron;
    public Plane plane;

    public ConvexPolyhedronFace(ConvexPolyhedron cp, Plane pl) { polyhedron = cp; plane = pl; }

    /***********************************************************************************************/

    const float MAX_DISTANCE = 128f;
    const float EPSILON_SQR = 1e-10f;

    static Vector3 IntersectThreePlanes(Plane plane1, Plane plane2, Plane plane3)
    {
        Matrix4x4 mat = new Matrix4x4();
        mat.SetRow(0, plane1.normal);
        mat.SetRow(1, plane2.normal);
        mat.SetRow(2, plane3.normal);
        mat.SetRow(3, new Vector4(0, 0, 0, 1));
        mat = mat.inverse;
        return mat.MultiplyPoint3x4(-new Vector3(plane1.distance, plane2.distance, plane3.distance));
    }

    static bool AlmostParallelPlanes(Plane plane1, Plane plane2)
    {
        return (plane1.normal - plane2.normal).sqrMagnitude < EPSILON_SQR ||
               (plane1.normal + plane2.normal).sqrMagnitude < EPSILON_SQR;
    }

    static Vector3 AnyNormalOf(Vector3 v)
    {
        if (Mathf.Abs(v.z) < 0.7f)
            return new Vector3(v.y, -v.x, 0).normalized;
        else
            return new Vector3(v.z, 0, -v.x).normalized;
    }

    public List<Vector3> ClosedPolygon(List<ConvexPolyhedronFace> other_faces)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> new_vertices = new List<Vector3>();

        Vector3 v1 = AnyNormalOf(plane.normal);
        Vector3 v2 = Vector3.Cross(v1, plane.normal);
        v1 *= MAX_DISTANCE;
        v2 *= MAX_DISTANCE;

        Vector3 pt = plane.normal * -plane.distance;
        vertices.Add(pt + v1 + v2);
        vertices.Add(pt + v1 - v2);
        vertices.Add(pt - v1 - v2);
        vertices.Add(pt - v1 + v2);

        foreach (var other_face in other_faces)
        {
            if (other_face == this)
                continue;

            if (vertices.Count < 3)
            {
                vertices.Clear();
                return vertices;
            }

            Vector3 prev_point = vertices[vertices.Count - 1];
            float prev_distance = other_face.plane.GetDistanceToPoint(prev_point);
            new_vertices.Clear();

            foreach (Vector3 v in vertices)
            {
                float new_distance = other_face.plane.GetDistanceToPoint(v);
                if (new_distance > 0)
                {
                    /* new point is behind the plane */
                    if (prev_distance <= 0)
                    {
                        /* previous point was not behind the plane.  Cut in the middle */
                        new_vertices.Add(Vector3.Lerp(v, prev_point, new_distance / (new_distance - prev_distance)));
                    }
                }
                else
                {
                    /* new point is not behind the plane */
                    if (prev_distance > 0)
                    {
                        /* previous point was behind the plane.  Cut in the middle */
                        new_vertices.Add(Vector3.Lerp(prev_point, v, prev_distance / (prev_distance - new_distance)));
                    }
                    new_vertices.Add(v);
                }
                prev_point = v;
                prev_distance = new_distance;
            }

            var tmp = vertices;
            vertices = new_vertices;
            new_vertices = tmp;
        }
        return vertices;
    }

    static public void LogVertex(Vector3 v)
    {
        Debug.Log(v.x + " " + v.y + " " + v.z);
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = v;
        go.transform.localScale = Vector3.one * 0.07f;
    }
}


public class ConvexPolyhedron : MonoBehaviour {

    public Material default_material;
    public List<ConvexPolyhedronFace> faces;

	void Start()
    {
		if (faces == null)
        {
            /* assume a cube */
            faces = new List<ConvexPolyhedronFace>();
            Vector3 center = transform.position;
            Vector3 edges = transform.lossyScale;

            Vector3 p;
            p = new Vector3(edges.x * 0.5f, 0, 0);
            faces.Add(new ConvexPolyhedronFace(this, new Plane(new Vector3(1, 0, 0), center + p)));
            faces.Add(new ConvexPolyhedronFace(this, new Plane(new Vector3(-1, 0, 0), center - p)));
            p = new Vector3(0, edges.y * 0.5f, 0);
            faces.Add(new ConvexPolyhedronFace(this, new Plane(new Vector3(0, 1, 0), center + p)));
            faces.Add(new ConvexPolyhedronFace(this, new Plane(new Vector3(0, -1, 0), center - p)));
            p = new Vector3(0, 0, edges.z * 0.5f);
            faces.Add(new ConvexPolyhedronFace(this, new Plane(new Vector3(0, 0, 1), center + p)));
            faces.Add(new ConvexPolyhedronFace(this, new Plane(new Vector3(0, 0, -1), center - p)));

            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            if (default_material == null)
                default_material = GetComponent<MeshRenderer>().sharedMaterial;
        }
        RecomputeMesh();
    }

    public void RecomputeMesh()
    {
        List<Vector3> all_vertices = new List<Vector3>();
        List<Vector3> all_normals = new List<Vector3>();
        List<int[]> all_faces = new List<int[]>();
        List<Material> all_materials = new List<Material>();

		foreach (ConvexPolyhedronFace face in faces)
        {
            List<Vector3> vertices = face.ClosedPolygon(faces);
            if (vertices.Count < 3)
                continue;
            int b = all_vertices.Count;
            foreach (var v in vertices)
            {
                all_vertices.Add(v);
                all_normals.Add(face.plane.normal);
            }
            int[] triangles = new int[(vertices.Count - 2) * 3];
            int j = 0;
            for (int i = 2; i < vertices.Count; i++)
            {
                triangles[j++] = b;
                triangles[j++] = b + i - 1;
                triangles[j++] = b + i;
            }
            all_faces.Add(triangles);
            all_materials.Add(default_material);
        }

        var mesh = new Mesh();
        mesh.vertices = all_vertices.ToArray();
        mesh.normals = all_normals.ToArray();
        mesh.subMeshCount = all_faces.Count;
        for (int i = 0; i < all_faces.Count; i++)
            mesh.SetTriangles(all_faces[i], i);

        GetComponent<MeshRenderer>().sharedMaterials = all_materials.ToArray();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
