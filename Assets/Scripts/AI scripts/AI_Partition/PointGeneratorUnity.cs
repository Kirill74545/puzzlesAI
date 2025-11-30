using UnityEngine;
using Unity.Barracuda;

public class PointGeneratorUnity : MonoBehaviour
{
    public NNModel onnxModel;
    private IWorker worker;
    private Model runtimeModel;
    private const int LATENT_DIM = 128;

    // Замените константу на настраиваемое поле
    [Header("Настройки количества точек")]
    public int pointsCount = 10; // По умолчанию для Easy

    void Awake()
    {
        runtimeModel = ModelLoader.Load(onnxModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
    }

    public Vector2[] GeneratePoints(int customPointsCount = -1)
    {
        // Используем переданное количество или значение по умолчанию
        int actualPointsCount = customPointsCount > 0 ? customPointsCount : pointsCount;

        Tensor z = new Tensor(1, LATENT_DIM);

        for (int i = 0; i < LATENT_DIM; i++)
            z[0, i] = Random.Range(-1f, 1f);

        worker.Execute(z);

        Tensor output = worker.PeekOutput("points");

        Vector2[] pts = new Vector2[actualPointsCount];

        for (int i = 0; i < actualPointsCount; i++)
        {
            float x = output[0, 0, 0, i];
            float y = output[0, 0, 1, i];
            pts[i] = new Vector2(x, y);
        }

        z.Dispose();
        output.Dispose();

        return pts;
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}
