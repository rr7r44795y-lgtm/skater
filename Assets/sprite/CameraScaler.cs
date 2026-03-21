using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    public float targetWidth = 1080f;

    void Start()
    {
        float ratio = (float)Screen.width / (float)Screen.height;
        Camera.main.orthographicSize = targetWidth / ratio / 2f;
    }
}