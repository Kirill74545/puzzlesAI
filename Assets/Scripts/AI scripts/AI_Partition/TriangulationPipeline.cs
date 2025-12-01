using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

public static class TriangulationPipeline
{
    public static List<TriangleData> Generate(PointGeneratorUnity generator, string difficultyLevel)
    {
        int pointsCount = GetPointsCountByDifficulty(difficultyLevel);
        Vector2[] generated = generator.GeneratePoints(pointsCount);
        List<Vector2> inputPoints = new List<Vector2>(generated);

        float MAP_SIZE = 512f;
        var (verts, tris) = TriangulatePoints.Triangulate(inputPoints, MAP_SIZE);

        List<TriangleData> result = new List<TriangleData>();
        int index = 0;
        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector2 A = verts[tris[i]];
            Vector2 B = verts[tris[i + 1]];
            Vector2 C = verts[tris[i + 2]];

            Vector2 uvA = new Vector2(A.x / MAP_SIZE, A.y / MAP_SIZE);
            Vector2 uvB = new Vector2(B.x / MAP_SIZE, B.y / MAP_SIZE);
            Vector2 uvC = new Vector2(C.x / MAP_SIZE, C.y / MAP_SIZE);

            // Просто создаем TriangleData с новой логикой конструктора
            result.Add(new TriangleData(uvA, uvB, uvC, index));
            index++;
        }

        Debug.Log($"Сгенерировано {result.Count} треугольников для уровня {difficultyLevel}");
        return result;
    }

    private static int GetPointsCountByDifficulty(string difficultyLevel)
    {
        switch (difficultyLevel)
        {
            case "level1": return 4;
            case "level2": return 8;
            case "level3": return 15;
            case "level4": return 20;
            default: return 4;
        }
    }
}