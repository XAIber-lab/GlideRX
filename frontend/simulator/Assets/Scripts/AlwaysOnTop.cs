using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysOnTop : MonoBehaviour
{
    public int renderQueue = 3001;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.renderQueue = renderQueue;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
