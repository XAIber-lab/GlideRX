using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestInput : MonoBehaviour
{
    private TMP_Text m_Text;
    // Start is called before the first frame update
    void Start()
    {
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);

        if (leftHandDevices.Count == 1)
        {
            UnityEngine.XR.InputDevice device = leftHandDevices[0];
            string txt = string.Format("Device name '{0}' with role '{1}'", device.name, device.characteristics.ToString());
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.characteristics.ToString()));
        }
        else if (leftHandDevices.Count > 1)
        {
            Debug.Log("Found more than one left hand!");
        }
    }

    private void Awake()
    {
        m_Text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
