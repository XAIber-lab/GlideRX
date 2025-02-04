using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Image radarImage = gameObject.GetComponent<Image>();
        radarImage.color = new Color(radarImage.color.r, radarImage.color.g, radarImage.color.b, 0f);
        foreach (Transform child in gameObject.transform)
        {
            Image childImage = child.gameObject.GetComponent<Image>();
            childImage.color = new Color(childImage.color.r, childImage.color.g, childImage.color.b, 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
