using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Color c = gameObject.GetComponent<TextMeshProUGUI>().color;
        c.a = 0f;
        gameObject.GetComponent<TextMeshProUGUI>().color = c;


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
