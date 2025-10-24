using UnityEngine;

public static class PuzzlePieceMaskGenerator
{
    public static Texture2D GeneratePuzzleMask(int width = 20, int height = 20)
    {
        Texture2D mask = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Color[] pixels = new Color[width * height];

        // Заполняем прозрачным фоном
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        DrawBezierPuzzleShape(pixels, width, height);

        mask.SetPixels(pixels);
        mask.Apply();
        return mask;
    }

    private static void DrawBezierPuzzleShape(Color[] pixels, int w, int h)
    {
        // Заполняем базовую область (прямоугольник с отступами)
        float margin = 2f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x >= margin && x <= w - margin && y >= margin && y <= h - margin)
                {
                    pixels[y * w + x] = Color.white;
                }
            }
        }

        // === Добавляем выступы/впадины по краям ===
        // Правый край — выступ
        AddBump(pixels, w, h, Edge.Right);
        // Левый край — впадина
        AddIndent(pixels, w, h, Edge.Left);
        // Верх — выступ
        AddBump(pixels, w, h, Edge.Top);
        // Низ — впадина
        AddIndent(pixels, w, h, Edge.Bottom);
    }

    private enum Edge { Left, Right, Top, Bottom }

    private static void AddBump(Color[] pixels, int w, int h, Edge edge)
    {
        int resolution = 20;
        float size = Mathf.Min(w, h) * 0.3f;

        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            Vector2 p = GetBezierControlPoints(edge, w, h, size, isBump: true);
            Vector2 q = GetBezierControlPoints(edge, w, h, size, isBump: true, index: 1);
            Vector2 r = GetBezierControlPoints(edge, w, h, size, isBump: true, index: 2);
            Vector2 s = GetBezierControlPoints(edge, w, h, size, isBump: true, index: 3);

            Vector2 point = Bezier(p, q, r, s, t);
            FillCircle(pixels, w, h, point, radius: 1.5f);
        }
    }

    private static void AddIndent(Color[] pixels, int w, int h, Edge edge)
    {
        // Впадина = не рисуем ничего, просто удаляем пиксели из базовой области
        int resolution = 20;
        float size = Mathf.Min(w, h) * 0.3f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].a > 0) // если пиксель был белым
                {
                    Vector2 pixelPos = new Vector2(x, y);
                    if (IsInsideIndent(pixelPos, edge, w, h, size))
                    {
                        pixels[y * w + x] = Color.clear;
                    }
                }
            }
        }
    }

    private static bool IsInsideIndent(Vector2 pos, Edge edge, int w, int h, float size)
    {
        Vector2 center = GetEdgeCenter(edge, w, h);
        Vector2 dir = GetEdgeNormal(edge);
        float distAlong = Vector2.Dot(pos - center, dir);
        Vector2 perp = new Vector2(-dir.y, dir.x);
        float distPerp = Vector2.Dot(pos - center, perp);

        // Простая круглая впадина
        return distAlong > -size * 0.7f && distAlong < size * 0.7f &&
               distPerp * distPerp + distAlong * distAlong < size * size;
    }

    private static Vector2 GetEdgeCenter(Edge edge, int w, int h)
    {
        switch (edge)
        {
            case Edge.Left: return new Vector2(0, h / 2f);
            case Edge.Right: return new Vector2(w, h / 2f);
            case Edge.Top: return new Vector2(w / 2f, h);
            case Edge.Bottom: return new Vector2(w / 2f, 0);
            default: return Vector2.zero;
        }
    }

    private static Vector2 GetEdgeNormal(Edge edge)
    {
        switch (edge)
        {
            case Edge.Left: return Vector2.right;
            case Edge.Right: return Vector2.left;
            case Edge.Top: return Vector2.down;
            case Edge.Bottom: return Vector2.up;
            default: return Vector2.zero;
        }
    }

    private static Vector2 GetBezierControlPoints(Edge edge, int w, int h, float size, bool isBump, int index = 0)
    {
        Vector2 center = GetEdgeCenter(edge, w, h);
        Vector2 normal = GetEdgeNormal(edge);
        Vector2 tangent = new Vector2(-normal.y, normal.x);

        if (isBump)
        {
            // Простая дуга как Bezier (аппроксимация)
            Vector2 p0 = center - tangent * size;
            Vector2 p3 = center + tangent * size;
            Vector2 p1 = p0 + normal * size * 0.5f;
            Vector2 p2 = p3 + normal * size * 0.5f;

            return index switch
            {
                0 => p0,
                1 => p1,
                2 => p2,
                3 => p3,
                _ => p0
            };
        }
        else
        {
            return center;
        }
    }

    private static Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
    }

    private static void FillCircle(Color[] pixels, int w, int h, Vector2 center, float radius)
    {
        int cx = Mathf.RoundToInt(center.x);
        int cy = Mathf.RoundToInt(center.y);
        int r = Mathf.CeilToInt(radius);

        for (int dy = -r; dy <= r; dy++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int x = cx + dx;
                int y = cy + dy;
                if (x >= 0 && x < w && y >= 0 && y < h)
                {
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        pixels[y * w + x] = Color.white;
                    }
                }
            }
        }
    }
}