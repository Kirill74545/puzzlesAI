using UnityEngine;
using UnityEngine.UI;

public class PulseAnimation : MonoBehaviour
{
    public float minScale = 0.95f;
    public float maxScale = 1.05f;
    public float speed = 2f; 

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        float t = Mathf.Sin(Time.time * speed * Mathf.PI * 2) * 0.5f + 0.5f;
        float scale = Mathf.Lerp(minScale, maxScale, t);
        rectTransform.localScale = Vector3.one * scale;
    }
}