using UnityEngine;
using UnityEngine.UI;

public class FadePulse : MonoBehaviour
{
    public float minAlpha = 0.7f;
    public float maxAlpha = 1f;
    public float speed = 1.5f;

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        float t = Mathf.Sin(Time.time * speed * Mathf.PI * 2) * 0.5f + 0.5f;
        Color c = image.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        image.color = c;
    }
}