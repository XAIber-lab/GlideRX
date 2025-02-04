using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deactivate : MonoBehaviour
{
    public bool isActive = true;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(isActive);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
