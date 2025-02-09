using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScreenSpaceCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Canvas m_Canvas = GetComponent<Canvas>();
        m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        m_Canvas.worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
