using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
using System.Data.SqlTypes;

public class NewTrafficText : MonoBehaviour
{
    public AirTrafficController airTrafficController;
    Camera m_MainCamera;

    private bool alreadyDone = false;
    private GameObject newTraffic;
    // Start is called before the first frame update
    void Start()
    {
        m_MainCamera = Camera.main;
        airTrafficController.newTrafficEvent.AddListener(onNewTrafficEvent);
        airTrafficController.lowThreatEvent.AddListener(onLowThreatEvent);
    }

    private void onNewTrafficEvent(GameObject sailplane)
    {
        Debug.Log("New traffic event received");
        newTraffic = sailplane;
    }

    private void onLowThreatEvent(List<GameObject> lowThreats)
    {
        Vector3 relativePosition = newTraffic.transform.position - m_MainCamera.transform.position;
        float rightAngle = Mathf.Rad2Deg * (float)Math.Atan2((double)relativePosition.z, (float)relativePosition.x);

        if (rightAngle < 45f && rightAngle > -45f)
        {
            gameObject.GetComponent<TMP_Text>().text = "NEW AIRCRAFT NORTH";
        }
        else if (rightAngle < 135f && rightAngle > 45f)
        {
            gameObject.GetComponent<TMP_Text>().text = "NEW AIRCRAFT WEST";
        }
        else if (rightAngle > 135f || rightAngle < -135f)
        {
            gameObject.GetComponent<TMP_Text>().text = "NEW AIRCRAFT SOUTH";
        }
        else if (rightAngle < -45f && rightAngle > -135f)
        {
            gameObject.GetComponent<TMP_Text>().text = "NEW AIRCRAFT EAST";
        }
        if (!alreadyDone)
        {
            alreadyDone = true;
            StartCoroutine(FadeInOut(gameObject.GetComponent<TextMeshProUGUI>(), 0.05f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FadeInOut(TextMeshProUGUI textToFade, float morphingSpeed)
    {
        Debug.Log("FADING");
        Color color = textToFade.color;
        for (float alpha = 0f; alpha <= 1; alpha += morphingSpeed)
        {
            color.a = alpha;
            textToFade.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        for (float alpha = 1f; alpha >= 0; alpha -= morphingSpeed)
        {
            color.a = alpha;
            textToFade.color = color;
            yield return null;
        }
        color.a = 0f;
        textToFade.color = color;
    }
}
