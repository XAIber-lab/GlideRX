using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCircleScale : MonoBehaviour
{
    Camera m_MainCamera;

    private float minimumScale;
    private float maximumScale;
    private bool dynamicScaleActivated;

    private RedCircle redCircle;
    // public float maximumDistance = 2000f;

    // Start is called before the first frame update
    void Start()
    {
        dynamicScaleActivated = false;
        transform.localScale = Vector3.zero;

        m_MainCamera = Camera.main;
        minimumScale = gameObject.GetComponent<LookAtCamera>().minimumScale;
        maximumScale = gameObject.GetComponent<LookAtCamera>().maximumScale;

        redCircle = gameObject.GetComponent<RedCircle>();
        redCircle.zoomingInEvent.AddListener(onZoomIn);
    }

    private void onZoomIn(bool hasZoomedIn)
    {
        if (hasZoomedIn)
        {
            dynamicScaleActivated = true;
        }
        else
        {
            dynamicScaleActivated = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (dynamicScaleActivated)
        {
        */
        if (redCircle.GetIsActivated())
        {
            float distance = (m_MainCamera.transform.position - transform.position).magnitude;
            float scale = (distance * ((maximumScale - minimumScale) / 2000f)) + minimumScale;
            transform.localScale = new Vector3(scale, scale, scale);
        }
        // }
        /*
        if (distance > maximumDistance)
        {
            transform.localScale = Vector3.zero;
        }
        else
        {
            float scale = (distance * ((maximumScale - minimumScale) / 2000f)) + minimumScale;
            transform.localScale = new Vector3(scale, scale, scale);
        }
        */
    }
}
