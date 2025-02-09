using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Velocity : MonoBehaviour
{
    private TMP_Text m_Text;
    private GameObject pilotSailplane;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] pilotSailplanes = GameObject.FindGameObjectsWithTag("PilotSailplane");
        if (pilotSailplanes.Length > 0)
        {
            pilotSailplane = pilotSailplanes[0];
        }
    }
    private void Awake()
    {
        m_Text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        m_Text.text = "Velocity: " + (pilotSailplane.GetComponent<Rigidbody>().velocity.magnitude * 3.6f).ToString() + "\nAngle: " + -(((pilotSailplane.GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * (260f/360f)) - 90f);
    }
}
