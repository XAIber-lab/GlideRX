using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Airbrakes : MonoBehaviour
{
    private TMP_Text m_Text;
    public AeroSurfaceConfig mainWingSurfaceConfiguration = null;
    public float minimumMainWingSpan = 2.5f;
    float maximumMainWingSpan = 4.45f;
    [Range(0, 1)]
    public float airbrakesValue;

    // Start is called before the first frame update
    void Start()
    {
        if (mainWingSurfaceConfiguration != null)
        {
            maximumMainWingSpan = mainWingSurfaceConfiguration.span;
        }
    }
    private void Awake()
    {
        m_Text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        airbrakesValue = Input.GetAxis("Airbrakes");

        float airbrakesDeploymentPercentage = 0f;
        // Airbrakes deployment
        if (mainWingSurfaceConfiguration != null)
        {
            airbrakesDeploymentPercentage = (airbrakesValue * minimumMainWingSpan) + ((1 - airbrakesValue) * maximumMainWingSpan);
        }

        m_Text.text = "Airbrakes: " + airbrakesDeploymentPercentage;
    }
}
