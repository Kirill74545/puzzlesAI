using UnityEngine;


[System.Serializable]
public struct TriangleData
{
    // Координаты вершин для отрисовки
    public Vector2 uvA;
    public Vector2 uvB;
    public Vector2 uvC;

    public Vector2 boundingBoxMinUV;
    public Vector2 boundingBoxMaxUV;
    public Vector2 boundingBoxCenterUV;
    public Vector2 boundingBoxSizeUV;

    public int index;

    public TriangleData(Vector2 a, Vector2 b, Vector2 c, int i)
    {
        uvA = a;
        uvB = b;
        uvC = c;

        // Рассчитываем границы прямоугольника, который описывает треугольник
        float minX = Mathf.Min(uvA.x, uvB.x, uvC.x);
        float maxX = Mathf.Max(uvA.x, uvB.x, uvC.x);
        float minY = Mathf.Min(uvA.y, uvB.y, uvC.y);
        float maxY = Mathf.Max(uvA.y, uvB.y, uvC.y);

        boundingBoxMinUV = new Vector2(minX, minY);
        boundingBoxMaxUV = new Vector2(maxX, maxY);
        boundingBoxSizeUV = new Vector2(maxX - minX, maxY - minY);
        boundingBoxCenterUV = boundingBoxMinUV + boundingBoxSizeUV / 2f;

        index = i;
    }
}