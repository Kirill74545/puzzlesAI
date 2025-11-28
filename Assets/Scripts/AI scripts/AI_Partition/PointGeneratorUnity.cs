using UnityEngine;
using Unity.Barracuda;

public class PointGeneratorUnity : MonoBehaviour
{
    public NNModel onnxModel;

    private IWorker worker;
    private Model runtimeModel;

    private const int LATENT_DIM = 128;
    private const int N_POINTS = 20;

    void Awake()
    {
        runtimeModel = ModelLoader.Load(onnxModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
    }

    public Vector2[] GeneratePoints()
    {
        Tensor z = new Tensor(1, LATENT_DIM);

        for (int i = 0; i < LATENT_DIM; i++)
            z[0, i] = Random.Range(-1f, 1f);

        worker.Execute(z);

        Tensor output = worker.PeekOutput("points");
        Debug.Log(
            $"output.shape => batch={output.shape.batch}, height={output.shape.height}, width={output.shape.width}, channels={output.shape.channels}"
        );

        Vector2[] pts = new Vector2[N_POINTS];

        // ВАЖНО: твоя модель возвращает shape [1, 1, 2, 20]
        for (int i = 0; i < N_POINTS; i++)
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
