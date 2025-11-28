using UnityEngine;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

public static class TriangulatePoints
{
    private const float normDist = 30f;

    public static List<Vector2> NormalizePoints(List<Vector2> points)
    {
        List<Vector2> result = new List<Vector2>();

        foreach (var p in points)
        {
            Vector2 x = p;

            foreach (var pj in points)
            {
                if (p == pj) continue;

                float dx = x.x - pj.x;
                float dy = x.y - pj.y;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                if (distance <= normDist && distance > 0.0001f)
                {
                    float compens = (normDist - distance);
                    x += new Vector2(dx, dy) * (compens / distance);
                }
            }

            // Clamp
            x.x = Mathf.Clamp(x.x, normDist, 512 - normDist);
            x.y = Mathf.Clamp(x.y, normDist, 512 - normDist);

            result.Add(x);
        }

        return result;
    }

    public static List<Vector2> AddBorderPoints(List<Vector2> pts)
    {
        pts.AddRange(new[]
        {
            new Vector2(0,   0),
            new Vector2(0,   256),
            new Vector2(0,   512),
            new Vector2(256, 512),
            new Vector2(512, 512),
            new Vector2(512, 256),
            new Vector2(512, 0),
            new Vector2(256, 0),
        });

        return pts;
    }

    public static (Vector2[], int[]) DelaunayTriangulate(List<Vector2> pts)
    {
        Polygon poly = new Polygon();

        for (int i = 0; i < pts.Count; i++)
            poly.Add(new Vertex(pts[i].x, pts[i].y, i));

        var mesh = poly.Triangulate(new ConstraintOptions(), new QualityOptions());

        List<int> tri = new List<int>();
        foreach (var t in mesh.Triangles)
        {
            tri.Add(t.vertices[0].id);
            tri.Add(t.vertices[1].id);
            tri.Add(t.vertices[2].id);
        }

        return (pts.ToArray(), tri.ToArray());
    }

    public static (Vector2[], int[]) Triangulate(List<Vector2> points)
    {
        var norm = NormalizePoints(points);
        AddBorderPoints(norm);
        return DelaunayTriangulate(norm);
    }
}
