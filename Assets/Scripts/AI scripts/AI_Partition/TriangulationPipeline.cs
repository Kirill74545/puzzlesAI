using UnityEngine;
using System.Collections.Generic;

public static class TriangulationPipeline
{
    private const float MAP_SIZE = 512f;

    public static List<TriangleData> Generate(PointGeneratorUnity generator)
    {
        // 1. Генерация точек нейросетью
        Vector2[] generated = generator.GeneratePoints();

        // 2. Превращаем в список
        List<Vector2> inputPoints = new List<Vector2>(generated);

        // 3. Триангулируем
        var (verts, tris) = TriangulatePoints.Triangulate(inputPoints);

        // 4. Конвертация вершин в UV + центры
        List<TriangleData> results = new List<TriangleData>(tris.Length / 3);

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector2 A = verts[tris[i]];
            Vector2 B = verts[tris[i + 1]];
            Vector2 C = verts[tris[i + 2]];

            Vector2 uvA = A / MAP_SIZE;
            Vector2 uvB = B / MAP_SIZE;
            Vector2 uvC = C / MAP_SIZE;

            results.Add(new TriangleData(uvA, uvB, uvC));
        }

        return results;
    }
}
