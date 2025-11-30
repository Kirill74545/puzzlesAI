using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

public static class TriangulatePoints
{
    // ”величим минимальное рассто€ние между точками дл€ уменьшени€ плотности
    private const float MIN_SEP = 15f;

    public static (Vector2[], int[]) Triangulate(List<Vector2> pts, float mapSize)
    {
        // Ѕолее агрессивное удаление близких точек
        pts = RemoveClose(pts, MIN_SEP);

        // ќграничиваем точки границами с большим отступом
        float margin = 20f;
        for (int i = 0; i < pts.Count; i++)
        {
            Vector2 p = pts[i];
            p.x = Mathf.Clamp(p.x, margin, mapSize - margin);
            p.y = Mathf.Clamp(p.y, margin, mapSize - margin);
            pts[i] = p;
        }

        Polygon poly = new Polygon();

        // ƒобавл€ем углы карты
        var A = new Vertex(0, 0);
        var B = new Vertex(mapSize, 0);
        var C = new Vertex(mapSize, mapSize);
        var D = new Vertex(0, mapSize);

        poly.Add(A); poly.Add(B); poly.Add(C); poly.Add(D);

        // ƒобавл€ем границы
        poly.Add(new Segment(A, B));
        poly.Add(new Segment(B, C));
        poly.Add(new Segment(C, D));
        poly.Add(new Segment(D, A));

        // ƒобавл€ем точки
        foreach (var pt in pts)
            poly.Add(new Vertex(pt.x, pt.y));

        // »спользуем менее строгие настройки триангул€ции
        var mesh = poly.Triangulate(
            new ConstraintOptions() { ConformingDelaunay = true },
            new QualityOptions() { MinimumAngle = 25 } // ”величиваем минимальный угол дл€ менее плотной триангул€ции
        );

        List<Vector2> verts = new List<Vector2>();
        Dictionary<int, int> idmap = new Dictionary<int, int>();

        foreach (var v in mesh.Vertices)
        {
            idmap[v.ID] = verts.Count;
            verts.Add(new Vector2((float)v.X, (float)v.Y));
        }

        List<int> tri = new List<int>();
        foreach (var t in mesh.Triangles)
        {
            tri.Add(idmap[t.vertices[0].ID]);
            tri.Add(idmap[t.vertices[1].ID]);
            tri.Add(idmap[t.vertices[2].ID]);
        }

        return (verts.ToArray(), tri.ToArray());
    }

    private static List<Vector2> RemoveClose(List<Vector2> pts, float sep)
    {
        List<Vector2> result = new List<Vector2>();
        foreach (var p in pts)
        {
            bool ok = true;
            foreach (var q in result)
            {
                if (Vector2.Distance(p, q) < sep)
                {
                    ok = false;
                    break;
                }
            }
            if (ok) result.Add(p);
        }
        return result;
    }
}
