using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public class AcquisitionEvent : UnityEvent<GameData> { }

public class SceneDirector : MonoBehaviour
{
    public int scenarioID;
    public string nextScene;
    public Transform pilotSailplane;
    public bool debug = false;
    public AcquisitionEvent acquisitionEvent;

    private AirTrafficController airTrafficController;
    private Camera m_MainCamera;
    private AudioSource m_AudioSource;
    // private (int, Dictionary<GameObject, Vector3>, Vector3) lastModerateRecord = (-1, null, Vector3.zero);
    private bool isNew = true;
    private bool isPlaying = false;
    // Start is called before the first frame update
    void Start()
    {
        m_MainCamera = Camera.main;
        m_AudioSource = GameObject.FindWithTag("AudioEffect").GetComponent<AudioSource>();

        // Time.timeScale = 0.0f;
        if (m_AudioSource != null)
            m_AudioSource.Pause();

        try
        {
            airTrafficController = GameObject.FindWithTag("AirTrafficController").GetComponent<AirTrafficController>();
        }
        catch (Exception e)
        {
            Debug.LogError("No Air Traffic Controller defined." + e.Message);
        }
        /*
        airTrafficController.lowThreatEvent.AddListener(onLowThreatEvent);
        airTrafficController.moderateThreatEvent.AddListener(onModerateThreatEvent);
        airTrafficController.highThreatEvent.AddListener(onHighThreatEvent);
        airTrafficController.severeThreatEvent.AddListener(onSevereThreatEvent);
        */
        if (debug)
            StartCoroutine(CatchAcquisitionRange());
    }

    private void Awake()
    {
        if (acquisitionEvent == null)
        {
            acquisitionEvent = new AcquisitionEvent();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // OVRInput.Update();
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            if (Time.timeScale == 0.0f)
            {
                Time.timeScale = 1.0f;
                if (m_AudioSource != null)
                    m_AudioSource.GetComponent<AudioSource>().Play();
            }
            else if (Time.timeScale == 1.0f)
            {
                Time.timeScale = 0.0f;
                if (m_AudioSource != null)
                    m_AudioSource.GetComponent<AudioSource>().Pause();
            }
        }
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            // SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
            StartCoroutine(LoadNextScene());
        }
        if (OVRInput.GetDown(OVRInput.Button.Three) || OVRInput.GetDown(OVRInput.Button.Four))
        {
            GameData gameData = GetAcquisitionData();
            if (gameData != null)
                acquisitionEvent.Invoke(gameData);
        }
    }

    private GameData GetAcquisitionData()
    {
        /*
        if (lastModerateRecord.Item1 == -1)
        {
            return null;
        }
        */

        /*
        GameObject[] sailplanes = GameObject.FindGameObjectsWithTag("Sailplane");
        GameObject closestSailplane = null;
        float closest = 10000f;
        foreach (GameObject sailplane in sailplanes)
        {
            Vector3 relativePosition = transform.InverseTransformDirection(sailplane.transform.position - pilotSailplane.position);
            float distance = relativePosition.magnitude;
            if (distance < closest)
            {
                closest = distance;
                closestSailplane = sailplane;
            }
        }
        */
        GameObject intruder = GetIntruder();
        if (intruder == null)
            return null;
        else
        {
            (int, Vector3, Vector3) intrudersFirstPossibilityAcquisition = airTrafficController.GetIntruderFirstPossibilityAcquisition(intruder);
            if (intrudersFirstPossibilityAcquisition.Item1 != -1)
            {
                int currentTime = GetCurrentTime();
                float acquisitionTime = ((float)(currentTime - intrudersFirstPossibilityAcquisition.Item1) / 1000f);
                Vector3 intruderAverageVelocity = (intruder.transform.position - intrudersFirstPossibilityAcquisition.Item2) / acquisitionTime ;
                Vector3 pilotAverageVelocity = (pilotSailplane.transform.position - intrudersFirstPossibilityAcquisition.Item3) / acquisitionTime;
                Vector3 relativeVelocity = (intruderAverageVelocity - pilotAverageVelocity);
                Vector3 closingDirection = (intruder.transform.position - pilotSailplane.transform.position).normalized;
                float closingRate = Vector3.Dot(relativeVelocity, closingDirection);
                float acquisitionRange = (intruder.transform.position - pilotSailplane.position).magnitude;

                GameData acquisitionData = new GameData(
                    timestamp: (long)currentTime,
                    interceptorID: intruder.name,
                    closingRate: closingRate,
                    acquisitionRange: acquisitionRange,
                    acquisitionTime: acquisitionTime,
                    position: intruder.transform.position);

                return acquisitionData;
            }
            else
                return null;
        }
    }

    IEnumerator LoadNextScene()
    {
        // yield return new WaitForSeconds(1.0f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    IEnumerator CatchAcquisitionRange()
    {
        for(; ; )
        {
            yield return new WaitForSeconds(5f);
            GameData gameData = GetAcquisitionData();
            if (gameData != null)
            {
                acquisitionEvent.Invoke(gameData);
            }
        }
    }

    /*
    private void onLowThreatEvent(List<GameObject> lowThreats)
    {
        isNew = true;
    }

    private void onModerateThreatEvent(List<GameObject> moderateThreats)
    {
        if (isNew)
        {
            isNew = false;
            Dictionary<GameObject, Vector3> moderateThreatsDictionary = new Dictionary<GameObject, Vector3>();
            foreach (GameObject sailplane in moderateThreats)
            {
                moderateThreatsDictionary.Add(sailplane, sailplane.transform.position);
            }
            var timestamp = getCurrentTime();
            lastModerateRecord = (timestamp, moderateThreatsDictionary, pilotSailplane.transform.position);
        }
    }

    private void onHighThreatEvent(List<GameObject> highThreats)
    {
        isNew = true;
    }

    private void onSevereThreatEvent(GameObject severeThreat)
    {
        isNew = true;
    }
    */

    private int GetCurrentTime()
    {
        return (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private GameObject GetIntruder()
    {
        GameObject[] sailplanes = GameObject.FindGameObjectsWithTag("Sailplane");
        GameObject intruder = null;
        float smallestLookingAngularDistance = (float)Math.PI;
        foreach (GameObject sailplane in sailplanes)
        {
            Vector3 sailplanePos = sailplane.transform.position;
            Vector3 cameraPos = m_MainCamera.transform.position;
            Vector3 relativeDirection = (sailplanePos - cameraPos).normalized;
            Vector3 lookingDirection = m_MainCamera.transform.rotation * Vector3.forward;
            float lookingAngularDistance = (float)Math.Acos((double)Vector3.Dot(relativeDirection, lookingDirection));
            Debug.Log("Sight angle: " + lookingAngularDistance);
            if (lookingAngularDistance < smallestLookingAngularDistance)
            {
                smallestLookingAngularDistance = lookingAngularDistance;
                intruder = sailplane;
            }
        }
        return intruder;
    }
}
