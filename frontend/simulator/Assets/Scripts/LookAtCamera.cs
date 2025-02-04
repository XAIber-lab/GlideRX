using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class LookAtCamera : MonoBehaviour
{
    Camera m_MainCamera;
    public bool fixedDistanceFromCamera = false;
    public float distanceFromCamera = 0.75f;
    public float minimumScale = 1500f;
    public float maximumScale = 10000f;
    public Vector3 rotationOffset = new Vector3(0f, 90f, 0f);

    private GameObject pilotSailplane;
    private GameObject selfSailplane;
    void Start()
    {
        m_MainCamera = Camera.main;
        try
        {
            pilotSailplane = GameObject.FindWithTag("PilotSailplane");
            selfSailplane = transform.parent.GetChild(transform.GetSiblingIndex() - 1).gameObject;
        }
        catch (Exception e)
        {
            Debug.LogError("No sailplane sibling of this object" + e);
        }
    }

    void LateUpdate()
    {
        if (fixedDistanceFromCamera)
        {
            transform.LookAt(selfSailplane.transform);
            transform.position = m_MainCamera.transform.position + (distanceFromCamera * (selfSailplane.transform.position - m_MainCamera.transform.position).normalized);
        }
        else
        {
            transform.LookAt(pilotSailplane.transform);
            transform.Rotate(rotationOffset, Space.Self);
        }
    }
}
