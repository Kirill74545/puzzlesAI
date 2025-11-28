using UnityEngine;

[System.Serializable]
public struct TriangleData
{
    public Vector2 uvA;
    public Vector2 uvB;
    public Vector2 uvC;

    public Vector2 center;

    public TriangleData(Vector2 a, Vector2 b, Vector2 c)
    {
        uvA = a;
        uvB = b;
        uvC = c;
        center = (a + b + c) / 3f;
    }
}
