using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysAtFront : MonoBehaviour
{
    public string shaderColorPropertyName;
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Renderer>().material.SetColor(shaderColorPropertyName, Color.yellow);
    }
}
