using UnityEngine;
using UnityEngine.UI;

public class RotateUI : MonoBehaviour
{
    public float rotationSpeed = 30f; 

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}

