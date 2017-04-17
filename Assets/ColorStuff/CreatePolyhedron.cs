using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CreatePolyhedron {

    public bool enabled = true;

    List<Vector3> vertices;
    List<Color> colors;
    List<int> triangles;
    List<Vector3> cut_vertices;
    List<Color> cut_colors;
    Plane plane;

    public CreatePolyhedron()
    {
        vertices = new List<Vector3>();
        colors = new List<Color>();
        triangles = new List<int>();
        cut_vertices = new List<Vector3>();
        cut_colors = new List<Color>();
    }

    public void Setup(Plane cut_plane)
    {
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
        cut_vertices.Clear();
        cut_colors.Clear();
        plane = cut_plane;
    }

    public Mesh BuildMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.triangles = triangles.ToArray();
        return mesh;
    }

    void _CutLine(Vector3 pt1, Vector3 pt2, Color col1, Color col2, float d1, float d2, out Vector3 pt12, out Color col12)
    {
        pt12 = Vector3.Lerp(pt1, pt2, d1 / (d1 - d2));
        col12 = Color.Lerp(col1, col2, d1 / (d1 - d2));
        cut_vertices.Add(pt12);
        cut_colors.Add(col12);
    }

    void _AddPoint(Vector3 pt, Color col)
    {
        triangles.Add(vertices.Count);
        vertices.Add(pt);
        colors.Add(col);
    }

    void AddTriangle(Vector3 pt1, Vector3 pt2, Vector3 pt3,
                     Color col1, Color col2, Color col3)
    {
        if (enabled)
        {
            _AddPoint(pt1, col1);
            _AddPoint(pt2, col2);
            _AddPoint(pt3, col3);
        }
    }

    public void AddTriangleCut(Vector3 pt1, Vector3 pt2, Vector3 pt3,
                               Color col1, Color col2, Color col3)
    {
        float d1 = Vector3.Dot(pt1, plane.normal) + plane.distance;
        float d2 = Vector3.Dot(pt2, plane.normal) + plane.distance;
        float d3 = Vector3.Dot(pt3, plane.normal) + plane.distance;

        if (d1 >= 0)
        {
            if (d2 >= 0)
            {
                if (d3 >= 0)
                    AddTriangle(pt1, pt2, pt3, col1, col2, col3);
                else
                {
                    Vector3 pt13, pt23;
                    Color col13, col23;
                    _CutLine(pt2, pt3, col2, col3, d2, d3, out pt23, out col23);
                    _CutLine(pt1, pt3, col1, col3, d1, d3, out pt13, out col13);
                    AddTriangle(pt1, pt2, pt23, col1, col2, col23);
                    AddTriangle(pt1, pt23, pt13, col1, col23, col13);
                }
            }
            else
            {
                if (d3 >= 0)
                    AddTriangleCut(pt3, pt1, pt2, col3, col1, col2);
                else
                {
                    Vector3 pt12, pt13;
                    Color col12, col13;
                    _CutLine(pt1, pt2, col1, col2, d1, d2, out pt12, out col12);
                    _CutLine(pt1, pt3, col1, col3, d1, d3, out pt13, out col13);
                    AddTriangle(pt1, pt12, pt13, col1, col12, col13);
                }
            }
        }
        else
        {
            if (d2 >= 0)
                AddTriangleCut(pt2, pt3, pt1, col2, col3, col1);
            else if (d3 >= 0)
                AddTriangleCut(pt3, pt1, pt2, col3, col1, col2);
        }
    }

    public void AddClosingCap()
    {
        for (int i = 2; i < cut_vertices.Count; i += 2)
        {
            AddTriangle(cut_vertices[0], cut_vertices[i + 1], cut_vertices[i],
                        cut_colors[0], cut_colors[i + 1], cut_colors[i]);
        }
        cut_vertices.Clear();
        cut_colors.Clear();
    }
}
