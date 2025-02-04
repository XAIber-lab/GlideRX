using System.Collections;
using System.Collections.Generic;
// using System.Drawing;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class RadarManager : MonoBehaviour
{
    public AirTrafficController airTrafficController;
    public Transform pilotSailplane;
    public float moderateLowMorphingSpeed = 0.0075f;
    public float moderateHighMorphingSpeed = 0.01f;
    public float severeMorphingSpeed = 0.025f;
    public bool FLARM = false;
    public GameObject dotPrefab;
    private Color moderateColor = new Color(255f, 69f, 0f, 0f);
    private Color highColor = Color.red;

    private RiskSeverity previousRiskSeverity = RiskSeverity.NONE;
    private RiskSeverity currentRiskSeverity = RiskSeverity.NONE;
    private RiskSeverity updatedRiskSeverity = RiskSeverity.NONE;

    Dictionary<GameObject, GameObject> m_DotsID = new Dictionary<GameObject, GameObject>();

    private float moderateThreashold;
    private float widthHeight = 150f;
    private float standardScale;
    private string shaderColorPropertyName = "_TintColor";
    private bool notTransitioning = true;
    // Start is called before the first frame update
    void Start()
    {
        airTrafficController.trafficEvent.AddListener(onTrafficEvent);
        airTrafficController.moderateThreatEvent.AddListener(onModerateThreat);
        airTrafficController.highThreatEvent.AddListener(onHighThreat);

        moderateThreashold = airTrafficController.GetComponent<AirTrafficController>().moderateThreashold;

        widthHeight = transform.GetComponent<RectTransform>().rect.height;
        if (FLARM)
            standardScale = 1.0f;
        else
            standardScale = 0.7f;
        
        Image image = gameObject.GetComponent<Image>();
        Color color = image.color;
        color.a = 0f;
        image.color = color;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Image>() != null)
            {
                image = child.GetComponent<Image>();
                color = image.color;
                color.a = 0f;
                image.color = color;
            }
        }
    }

    private void Update()
    {
        // Update position of dots
        foreach (KeyValuePair<GameObject, GameObject> entry in m_DotsID)
        {
            // Update position of single dot
            // Vector3 relativePosition = transform.InverseTransformDirection(sailplaneObject.transform.position - pilotTransform.position);
            Vector3 relativePosition = (entry.Key.transform.position - pilotSailplane.position);
            float pilotHeading = -pilotSailplane.rotation.eulerAngles.y;
            Vector3 rotatedRelativePosition = Quaternion.Euler(0f, pilotHeading, 0f) * relativePosition;
            Vector3 newPosition = new Vector3(rotatedRelativePosition.x / moderateThreashold * (widthHeight / 2), rotatedRelativePosition.z / moderateThreashold * (widthHeight / 2), 0f);
            float threatHeading = 90f - entry.Key.transform.rotation.eulerAngles.y;
            entry.Value.transform.localPosition = newPosition;
            entry.Value.transform.localRotation = Quaternion.Euler(0f, 0f, threatHeading-pilotHeading);
        }
        if (!notTransitioning)
        {
            foreach (KeyValuePair<GameObject, GameObject> entry in m_DotsID)
            {
                entry.Value.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (KeyValuePair<GameObject, GameObject> entry in m_DotsID)
            {
                entry.Value.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    private void onTrafficEvent(int riskSeverity)
    {
        updatedRiskSeverity = (RiskSeverity)riskSeverity;
        if (updatedRiskSeverity != currentRiskSeverity)
        {
            previousRiskSeverity = currentRiskSeverity;
            currentRiskSeverity = updatedRiskSeverity;
            // morph in
            if (currentRiskSeverity == RiskSeverity.HIGH && (int)previousRiskSeverity <= (int)RiskSeverity.MODERATE)
            {
                StartCoroutine(Zoom(gameObject, standardScale, 1.3f, moderateHighMorphingSpeed));
            }
            if (currentRiskSeverity == RiskSeverity.SEVERE && (int)previousRiskSeverity == (int)RiskSeverity.HIGH)
            {
                foreach (KeyValuePair<GameObject, GameObject> dot in m_DotsID)
                {
                    dot.Value.transform.SetParent(null);
                    Destroy(dot.Value);
                }
                m_DotsID.Clear();
                StartCoroutine(Zoom(gameObject, 1.3f, 2.5f, severeMorphingSpeed));
                StartCoroutine(FadeOut(gameObject.GetComponent<Image>(), severeMorphingSpeed));
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<Image>() != null)
                        StartCoroutine(FadeOut(child.gameObject.GetComponent<Image>(), severeMorphingSpeed));
                    if (child.GetComponent<DotColor>() != null)
                        StartCoroutine(FadeOut(child.gameObject.GetComponent<Renderer>(), severeMorphingSpeed));
                }
            }
            // morph out
            if (currentRiskSeverity == RiskSeverity.HIGH && (int)previousRiskSeverity == (int)RiskSeverity.SEVERE)
            {
                StartCoroutine(Zoom(gameObject, 2.5f, 1.3f, moderateHighMorphingSpeed));
                StartCoroutine(FadeIn(gameObject.GetComponent<Image>(), moderateHighMorphingSpeed));
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<Image>() != null)
                        StartCoroutine(FadeIn(child.gameObject.GetComponent<Image>(), moderateHighMorphingSpeed));
                    if (child.GetComponent<DotColor>() != null)
                        StartCoroutine(FadeIn(child.gameObject.GetComponent<Renderer>(), moderateHighMorphingSpeed));
                }
            }
            if (currentRiskSeverity == RiskSeverity.MODERATE && (int)previousRiskSeverity >= (int)RiskSeverity.HIGH)
            {
                StartCoroutine(Zoom(gameObject, 1.3f, standardScale, moderateLowMorphingSpeed));
            }
            if (currentRiskSeverity == RiskSeverity.MODERATE && (int)previousRiskSeverity == (int)RiskSeverity.LOW)
            {
                notTransitioning = false;
                StartCoroutine(Zoom(gameObject, 2.5f, standardScale, moderateLowMorphingSpeed));
                StartCoroutine(FadeIn(gameObject.GetComponent<Image>(), moderateLowMorphingSpeed));
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<Image>() != null)
                        StartCoroutine(FadeIn(child.gameObject.GetComponent<Image>(), moderateLowMorphingSpeed));
                    if (child.GetComponent<DotColor>() != null)
                        StartCoroutine(FadeIn(child.gameObject.GetComponent<Renderer>(), moderateLowMorphingSpeed));
                }
            }
            if ((int)currentRiskSeverity <= (int)RiskSeverity.LOW)
            {
                foreach (KeyValuePair<GameObject, GameObject> dot in m_DotsID)
                {
                    dot.Value.transform.SetParent(null);
                    Destroy(dot.Value);
                }
                m_DotsID.Clear();
                /*
                StartCoroutine(FadeOut(gameObject.GetComponent<Image>(), morphingSpeed));
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<Image>() != null)
                        StartCoroutine(FadeOut(child.gameObject.GetComponent<Image>(), morphingSpeed));
                }
                */
            }
        }
    }

    private void onModerateThreat(List<GameObject> moderateThreats)
    {
        List<GameObject> toDestroy = new List<GameObject>(m_DotsID.Keys);
        foreach (GameObject moderateSailplane in moderateThreats)
        {

            if (!m_DotsID.ContainsKey(moderateSailplane))
            {
                GameObject dotObject = Instantiate(dotPrefab, transform);
                dotObject.transform.SetSiblingIndex(0);
                m_DotsID.Add(moderateSailplane, dotObject);
                m_DotsID[moderateSailplane].transform.GetChild(0).gameObject.SetActive(false);
                m_DotsID[moderateSailplane].transform.GetChild(0).GetComponent<DotColor>().SetSailplane(moderateSailplane.transform);
            }
            if (toDestroy.Contains(moderateSailplane))
                toDestroy.Remove(moderateSailplane);
            m_DotsID[moderateSailplane].transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            m_DotsID[moderateSailplane].transform.GetChild(0).GetComponent<Renderer>().material.SetColor(shaderColorPropertyName, moderateColor);
        }
        foreach (GameObject sailplaneDotToDestroy in toDestroy)
        {
            var dot = m_DotsID[sailplaneDotToDestroy];
            dot.transform.SetParent(null);
            Destroy(dot);
            m_DotsID.Remove(sailplaneDotToDestroy);
        }
    }

    private void onHighThreat(List<GameObject> highThreat)
    {
        List<GameObject> toDestroy = new List<GameObject>(m_DotsID.Keys);
        foreach (GameObject highSailplane in highThreat)
        {
            
            if (!m_DotsID.ContainsKey(highSailplane))
            {
                GameObject dotObject = Instantiate(dotPrefab, transform);
                dotObject.transform.SetSiblingIndex(0);
                m_DotsID.Add(highSailplane, dotObject);
            }
            if (toDestroy.Contains(highSailplane))
                toDestroy.Remove(highSailplane);
            m_DotsID[highSailplane].transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            m_DotsID[highSailplane].transform.GetChild(0).GetComponent<Renderer>().material.SetColor(shaderColorPropertyName, highColor);
            m_DotsID[highSailplane].transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.SetColor(shaderColorPropertyName, highColor);
        }
        foreach (GameObject sailplaneDotToDestroy in toDestroy)
        {
            var dot = m_DotsID[sailplaneDotToDestroy];
            dot.transform.SetParent(null);
            Destroy(dot);
            m_DotsID.Remove(sailplaneDotToDestroy);
        }
    }

    IEnumerator FadeIn(Image image, float morphingSpeed)
    {
        Color color = image.color;
        for (float alpha = 0f; alpha <= 1; alpha += morphingSpeed)
        {
            color.a = alpha;
            image.color = color;
            yield return null;
        }
        color.a = 1f;
        image.color = color;
    }

    IEnumerator FadeOut(Image image, float morphingSpeed)
    {
        Color color = image.color;
        for (float alpha = 1f; alpha >= 0; alpha -= morphingSpeed)
        {
            color.a = alpha;
            image.color = color;
            yield return null;
        }
        color.a = 0f;
        image.color = color;
    }

    IEnumerator FadeIn(Renderer renderer, float morphingSpeed)
    {
        Color color = Color.red;
        for (float alpha = 0f; alpha <= 1; alpha += morphingSpeed)
        {
            color.a = alpha;
            renderer.material.SetColor(shaderColorPropertyName, color);
            yield return null;
        }
        color.a = 1f;
        renderer.material.SetColor(shaderColorPropertyName, color);
    }

    IEnumerator FadeOut(Renderer renderer, float morphingSpeed)
    {
        Color color = Color.red;
        for (float alpha = 1f; alpha >= 0; alpha -= morphingSpeed)
        {
            color.a = alpha;
            renderer.material.SetColor(shaderColorPropertyName, color);
            yield return null;
        }
        color.a = 0f;
        renderer.material.SetColor(shaderColorPropertyName, color);
    }

    IEnumerator Zoom(GameObject obj, float initialScale, float finalScale, float morphingSpeed)
    {
        Debug.Log("ZOOM initialScale: " + initialScale);
        for (float alpha = 0f; alpha <= 1; alpha += morphingSpeed)
        {
            float scale = (alpha) * finalScale + (1 - alpha) * initialScale;
            obj.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
        notTransitioning = true;
    }
}
