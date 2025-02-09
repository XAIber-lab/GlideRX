using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextCanvasInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject text = gameObject.transform.GetChild(0).gameObject;
        Color c = text.GetComponent<TextMeshProUGUI>().color;
        c.a = 0f;
        text.GetComponent<TextMeshProUGUI>().color = c;

        Image leftFog = gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>();
        Image rightFog = gameObject.transform.GetChild(1).GetChild(1).gameObject.GetComponent<Image>();
        c = leftFog.color;
        c.a = 0f;
        leftFog.color = c;
        c = rightFog.color;
        c.a = 0f;
        rightFog.color = c;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
