// ФАЙЛ: TriangleMeshGraphic.cs

using UnityEngine;
using UnityEngine.UI;

public class TriangleMeshGraphic : MaskableGraphic
{
    private TriangleData _triangleData;
    private Texture2D _texture;

    // Создаем ОДИН общий материал для ВСЕХ пазлов. Это очень эффективно.
    private static Material _sharedUIMaterial;

    public TriangleData TriangleData
    {
        get => _triangleData;
        set
        {
            _triangleData = value;
            SetVerticesDirty();
        }
    }

    public Texture2D Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            // Устанавливаем текстуру в общий материал
            if (_sharedUIMaterial != null)
            {
                _sharedUIMaterial.mainTexture = _texture;
            }
            SetMaterialDirty();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // Гарантируем, что материал создан при включении объекта
        if (_sharedUIMaterial == null)
        {
            _sharedUIMaterial = new Material(Shader.Find("UI/Default"));
        }
        material = _sharedUIMaterial;

        // Если текстура уже была установлена, применяем ее
        if (_texture != null)
        {
            _sharedUIMaterial.mainTexture = _texture;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // Не уничтожаем общий материал при отключении одного объекта
    }

    // Этот метод остается без изменений, он рисует сам треугольник
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (_triangleData.index == -1) return;

        vh.Clear();
        Rect rect = rectTransform.rect;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = Color.white;

        // Преобразуем UV-координаты в локальные координаты
        Vector2 vertA = new Vector2(_triangleData.uvA.x * rect.width - rect.width / 2f, _triangleData.uvA.y * rect.height - rect.height / 2f);
        Vector2 vertB = new Vector2(_triangleData.uvB.x * rect.width - rect.width / 2f, _triangleData.uvB.y * rect.height - rect.height / 2f);
        Vector2 vertC = new Vector2(_triangleData.uvC.x * rect.width - rect.width / 2f, _triangleData.uvC.y * rect.height - rect.height / 2f);

        vertex.position = vertA; vertex.uv0 = _triangleData.uvA; vh.AddVert(vertex);
        vertex.position = vertB; vertex.uv0 = _triangleData.uvB; vh.AddVert(vertex);
        vertex.position = vertC; vertex.uv0 = _triangleData.uvC; vh.AddVert(vertex);

        vh.AddTriangle(0, 1, 2);
    }
}