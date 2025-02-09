using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class FogBehaviour : MonoBehaviour
{
    public AirTrafficController airTrafficController;

    public GameObject target = null;
    public bool reactive = false;
    public int frontEdgeAngle = 17;
    public int backEdgeAngle = 163;

    float leftFrontEdgeAngle;
    float leftBackEdgeAngle;
    float rightBackEdgeAngle;
    float rightFrontEdgeAngle;
    float majorScope;
    float minorScope;
    bool lastReactiveValue = false;
    bool morphingComplete = false;
    Camera m_MainCamera;
    GameObject FogLeft;
    GameObject FogRight;


    
    private bool isVisible;
    // Start is called before the first frame update
    void Start()
    {
        isVisible = false;
        airTrafficController.trafficEvent.AddListener(trafficEvent);

        m_MainCamera = Camera.main;
        FogLeft = gameObject.transform.GetChild(0).gameObject;
        FogRight = gameObject.transform.GetChild(1).gameObject;
        Image leftImage = FogLeft.GetComponent<Image>();
        leftImage.color = new Color(leftImage.color.r, leftImage.color.g, leftImage.color.b, 0f);
        Image rightImage = FogRight.GetComponent<Image>();
        rightImage.color = new Color(rightImage.color.r, rightImage.color.g, rightImage.color.b, 0f);

        leftFrontEdgeAngle = (float)frontEdgeAngle;
        leftBackEdgeAngle = (float)backEdgeAngle;
        rightBackEdgeAngle = 360f - (float)backEdgeAngle;
        rightFrontEdgeAngle = 360f - (float)frontEdgeAngle;
        majorScope = (float)backEdgeAngle - (float)frontEdgeAngle;
        minorScope = (float)(frontEdgeAngle * 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (isVisible)
        {
            if (!reactive)
            {
                if (lastReactiveValue)
                {
                    lastReactiveValue = reactive;
                    StartCoroutine(DampReaction(false));
                    morphingComplete = false;
                }
            }
            else
            {
                if (!lastReactiveValue)
                {
                    lastReactiveValue = reactive;
                    StartCoroutine(DampReaction(true));
                    morphingComplete = false;
                }
                if (morphingComplete)
                {
                    React();
                }
            }
        }
        else
        {
            FogLeft = gameObject.transform.GetChild(0).gameObject;
            FogRight = gameObject.transform.GetChild(1).gameObject;
            Image leftImage = FogLeft.GetComponent<Image>();
            leftImage.color = new Color(leftImage.color.r, leftImage.color.g, leftImage.color.b, 0f);
            Image rightImage = FogRight.GetComponent<Image>();
            rightImage.color = new Color(rightImage.color.r, rightImage.color.g, rightImage.color.b, 0f);
        }
    }

    private void trafficEvent(int riskSeverity)
    {
        if (riskSeverity != (int)RiskSeverity.LOW)
        {
            isVisible = false;
            /*
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(false);       
                Image image = t.gameObject.GetComponent<Image>();
                Color color = image.color;
                color.a = 0f;
                image.color = color;
            }
            */
        }
        else
        {
            isVisible = true;
            /*
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(true);
                Image image = t.gameObject.GetComponent<Image>();
                Color color = image.color;
                color.a = 1f;
                image.color = color;
            }
            */
        }
    }


    IEnumerator DampReaction(bool isFadingIn)
    {
        float maximumValue = 10f;
        float minimumValue = 1f;
        float morphingSpeed = 0.05f;
        for (float alpha = maximumValue; alpha >= minimumValue; alpha -= morphingSpeed)
        {
            float r_alpha;
            if (isFadingIn)
            {
                r_alpha = alpha;
            }
            else
            {
                r_alpha = -(alpha - minimumValue) + maximumValue; 
            }
            React(r_alpha);
            yield return null;
        }
        if (!isFadingIn)
        {
            Image leftImage = FogLeft.GetComponent<Image>();
            leftImage.color = new Color(leftImage.color.r, leftImage.color.g, leftImage.color.b, 0f);
            Image rightImage = FogRight.GetComponent<Image>();
            rightImage.color = new Color(rightImage.color.r, rightImage.color.g, rightImage.color.b, 0f);
        }
        morphingComplete = true;
    }

    void React(float damper = 1f)
    {
        Vector3 relativePosition = target.transform.position - m_MainCamera.transform.position;
        float rightAngle = (float)Math.Atan2((double)relativePosition.z, (float)relativePosition.x);
        rightAngle *= 180f / (float)Math.PI;
        float currentAngle = m_MainCamera.transform.rotation.eulerAngles.y;
        currentAngle = (currentAngle + 270f) % 360f;
        currentAngle = -currentAngle + 360f;
        float divergence = ((currentAngle - rightAngle) + 360f) % 360f;

        float left_fog_divergence = 0f;
        float right_fog_divergence = 0f;
        Color color;
        if (divergence <= leftBackEdgeAngle && divergence > leftFrontEdgeAngle)
        {
            color = FogLeft.GetComponent<Image>().color;
            color.a = 0f;
            FogLeft.GetComponent<Image>().color = color;

            right_fog_divergence = divergence - leftFrontEdgeAngle;
            color = FogRight.GetComponent<Image>().color;
            color.a = right_fog_divergence / (majorScope * damper);
            FogRight.GetComponent<Image>().color = color;
        }
        else if (divergence <= rightBackEdgeAngle && divergence > leftBackEdgeAngle)
        {
            left_fog_divergence = divergence - leftBackEdgeAngle;
            color = FogLeft.GetComponent<Image>().color;
            color.a = left_fog_divergence / (minorScope * damper);
            FogLeft.GetComponent<Image>().color = color;

            right_fog_divergence = -(divergence - leftBackEdgeAngle) + minorScope;
            color = FogRight.GetComponent<Image>().color;
            color.a = right_fog_divergence / (minorScope * damper);
            FogRight.GetComponent<Image>().color = color;
        }
        else if (divergence <= rightFrontEdgeAngle && divergence > rightBackEdgeAngle)
        {
            left_fog_divergence = -(divergence - rightBackEdgeAngle) + majorScope;
            color = FogLeft.GetComponent<Image>().color;
            color.a = left_fog_divergence / (majorScope * damper);
            FogLeft.GetComponent<Image>().color = color;

            color = FogRight.GetComponent<Image>().color;
            color.a = 0f;
            FogRight.GetComponent<Image>().color = color;
        }
        else
        {
            color = FogLeft.GetComponent<Image>().color;
            color.a = 0f;
            FogLeft.GetComponent<Image>().color = color;

            color = FogRight.GetComponent<Image>().color;
            color.a = 0f;
            FogRight.GetComponent<Image>().color = color;
        }
    }

    IEnumerator FadeOut(Image image, float morphingSpeed)
    {
        Color color = image.color;
        for (float alpha = 1f; alpha >= 0; alpha -= morphingSpeed)
        {
            color.a = alpha;
            image.color = color;
            yield return null;
        }
        color.a = 0f;
        image.color = color;
    }
}