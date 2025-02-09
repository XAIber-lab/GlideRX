using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BankAngle : MonoBehaviour
{
    public TMP_Text m_Text;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        m_Text.text = "Angular velocity x: " + rb.angularVelocity.x
            + "\ny: " + rb.angularVelocity.y
            + "\nz: " + rb.angularVelocity.z;
        Debug.Log("Angular velocity x: " + rb.angularVelocity.x
            + "\ny: " + rb.angularVelocity.y
            + "\nz: " + rb.angularVelocity.z);
    }
}
