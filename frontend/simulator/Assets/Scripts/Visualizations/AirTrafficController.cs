using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.Networking;
using Unity.VisualScripting;
using UnityEditor;

[System.Serializable]
public class TrafficEvent : UnityEvent<int> { }

[System.Serializable]
public class SevereThreatEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class ModerateThreatEvent : UnityEvent<List<GameObject>> { }

[System.Serializable]
public class HighThreatEvent: UnityEvent<List<GameObject>> { }

[System.Serializable]
public class LowThreatEvent : UnityEvent<List<GameObject>> { }

[System.Serializable]
public class NewTrafficEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class ClosestRangeEvent : UnityEvent<float> { }

[System.Serializable]
public class TrackingEvent : UnityEvent<StateVectorWithID> { }

enum RiskSeverity
{
    NONE,
    LOW,
    MODERATE,
    HIGH,
    SEVERE
}

public class AirTrafficController : MonoBehaviour
{
    public Transform pilotSailplane;

    public TrafficEvent trafficEvent;
    public NewTrafficEvent newTrafficEvent;
    public LowThreatEvent lowThreatEvent;
    public ModerateThreatEvent moderateThreatEvent;
    public HighThreatEvent highThreatEvent;
    public SevereThreatEvent severeThreatEvent;
    
    public ClosestRangeEvent closestRangeEvent;
    public TrackingEvent trackingEvent;

    List<GameObject> traffic;
    List<GameObject> lowThreatTraffic = new List<GameObject>();
    List<GameObject> moderateThreatTraffic = new List<GameObject>();
    List<GameObject> highThreatTraffic = new List<GameObject>();
    GameObject severeThreatTraffic = null;

    /*
        * ### RELEASE VALUES ###
        * 
    const float lowThreashold = 9260.0f;
    const float moderateThreashold = 2778.0f;
    const float severeThreashold = 926.0f;

    red circle scale: 1655
    */
    // ### DEBUG VALUES ###
    public float lowThreashold = 2000.0f;
    public float moderateThreashold = 1000.0f;
    public float highThreashold = 666.6f;
    public float severeThreashold = 333.3f;

    public bool enableTrackingEvents = false;
    public bool logOnServer = false;

    private Camera m_MainCamera;

    private int[] statesWindow = new int[] {
        (int)RiskSeverity.NONE,
        (int)RiskSeverity.NONE,
        (int)RiskSeverity.NONE,
        (int)RiskSeverity.NONE,
        (int)RiskSeverity.NONE
    };

    RiskSeverity previousState = RiskSeverity.NONE;
    RiskSeverity currentState = RiskSeverity.NONE;
    RiskSeverity endingState = RiskSeverity.NONE;

    // Dictionary<intruder, (timestamp at distance moderateThreashold, intruder position at moderateThreashold, pilot position when intruder at moderateThreashold)>
    private Dictionary<GameObject, (int, Vector3, Vector3)> intrudersFirstPossibilityAcquisition;
    public int currentStateInt = (int)RiskSeverity.NONE;
    void Start()
    {
        m_MainCamera = Camera.main;
        traffic = new List<GameObject>();
        intrudersFirstPossibilityAcquisition = new Dictionary<GameObject, (int, Vector3, Vector3)>();
        StartCoroutine(CheckTraffic());
    }

    private void Awake()
    {
        if (trafficEvent == null)
        {
            trafficEvent = new TrafficEvent();
        }
        if (newTrafficEvent == null)
        {
            newTrafficEvent = new NewTrafficEvent();
        }
        if (lowThreatEvent == null)
        {
            lowThreatEvent = new LowThreatEvent();
        }
        if (moderateThreatEvent == null)
        {
            moderateThreatEvent = new ModerateThreatEvent();
        }
        if (severeThreatEvent == null)
        {
            severeThreatEvent = new SevereThreatEvent();
        }
        if (closestRangeEvent == null)
        {
            closestRangeEvent = new ClosestRangeEvent();
        }
        if (trackingEvent == null)
        {
            trackingEvent = new TrackingEvent();
        }
    }

    IEnumerator CheckTraffic()
    {
        for (; ; )
        {
            int currentTime = GetCurrentTime();

            if (enableTrackingEvents && Time.timeScale == 1.0f)
                InvokeTrackingEvent(currentTime, "SELF", pilotSailplane);

            GameObject[] sailplanes = GameObject.FindGameObjectsWithTag("Sailplane");

            // Debug.Log("HELLO Air Traffic Controller!");
            lowThreatTraffic.Clear();
            moderateThreatTraffic.Clear();
            highThreatTraffic.Clear();
            severeThreatTraffic = null;
            endingState = RiskSeverity.NONE;

            Debug.Log("Air Traffic Controller: Known sailplanes " + traffic);

            float closest = 100000f;
            foreach (GameObject sailplane in sailplanes)
            {
                // Distance in meters between the pilotTransform (pilot) and the other sailplane
                Vector3 relativePosition = transform.InverseTransformDirection(sailplane.transform.position - pilotSailplane.position);
                float distance = relativePosition.magnitude;

                if (!traffic.Contains(sailplane))
                {
                    Debug.Log("AIR TRAFFIC CONTROLLER: new traffic " + sailplane.name);
                    newTrafficEvent.Invoke(sailplane);
                    Debug.Log("AIR TRAFFIC CONTROLLER: event sent");
                    traffic.Add(sailplane);
                }

                // Debug.Log("Sailplane found at distance: " + distance.ToString());
                if (distance <= lowThreashold && distance > moderateThreashold)
                {
                    // Between 5NM and 1.5NM
                    lowThreatTraffic.Add(sailplane);
                    if ((int)endingState < 1)
                    {
                        endingState = RiskSeverity.LOW;
                    }
                    UpdateIntrudersFirstPossibilityAcquisition(
                        reset: true,
                        currentTime: currentTime,
                        sailplane);
                    if (enableTrackingEvents)
                        InvokeTrackingEvent(currentTime, sailplane.name, sailplane.transform);
                    // m_Fog.GetComponent<FogBehaviour>().target = sailplane.transform.GetChild(0).gameObject;
                }
                else if (distance <= moderateThreashold && distance > highThreashold)
                {
                    // Between 1.5NM and 1.0NM
                    moderateThreatTraffic.Add(sailplane);
                    if ((int)endingState < 2)
                    {
                        endingState = RiskSeverity.MODERATE;
                    }
                    UpdateIntrudersFirstPossibilityAcquisition(
                        reset: false,
                        currentTime: currentTime,
                        sailplane);
                    if (enableTrackingEvents)
                        InvokeTrackingEvent(currentTime, sailplane.name, sailplane.transform);
                }
                else if (distance <= highThreashold && distance > severeThreashold)
                {
                    // Between 1.0NM and 0.5NM
                    highThreatTraffic.Add(sailplane);
                    if ((int)endingState < 3)
                    {
                        endingState = RiskSeverity.HIGH;
                    }
                    UpdateIntrudersFirstPossibilityAcquisition(
                        reset: false,
                        currentTime: currentTime,
                        sailplane);
                    if (enableTrackingEvents)
                        InvokeTrackingEvent(currentTime, sailplane.name, sailplane.transform);
                }
                else if (distance <= severeThreashold)
                {
                    // Below 0.5NM
                    severeThreatTraffic = sailplane;
                    endingState = RiskSeverity.SEVERE;
                    UpdateIntrudersFirstPossibilityAcquisition(
                        reset: false,
                        currentTime: currentTime,
                        sailplane);
                    if (enableTrackingEvents)
                        InvokeTrackingEvent(currentTime, sailplane.name, sailplane.transform);
                    // arrow.GetComponent<LookAtSailplane>().sailplane = sailplane;
                }
                else
                {
                    UpdateIntrudersFirstPossibilityAcquisition(
                        reset: true,
                        currentTime: currentTime,
                        sailplane);
                    traffic.Remove(sailplane);
                }
                if (distance < closest)
                {
                    closest = distance;
                }
            }
            closestRangeEvent.Invoke(closest);

            // UpdatePredictor();

            switch (endingState)
            {
                case RiskSeverity.LOW:
                    Debug.Log("Air Traffic Controller: severity LOW");
                    trafficEvent.Invoke((int)RiskSeverity.LOW);
                    lowThreatEvent.Invoke(lowThreatTraffic);
                    currentStateInt = (int)RiskSeverity.LOW;
                    break;

                case RiskSeverity.MODERATE:
                    Debug.Log("Air Traffic Controller: severity MODERATE");
                    trafficEvent.Invoke((int)RiskSeverity.MODERATE);
                    moderateThreatEvent.Invoke(moderateThreatTraffic);
                    currentStateInt = (int)RiskSeverity.MODERATE;
                    break;

                case RiskSeverity.HIGH:
                    Debug.Log("Air Traffic Controller: severity HIGH");
                    trafficEvent.Invoke((int)RiskSeverity.HIGH);
                    highThreatEvent.Invoke(highThreatTraffic);
                    // GetPredictedTrajectories();
                    currentStateInt = (int)RiskSeverity.HIGH;
                    break;

                case RiskSeverity.SEVERE:
                    Debug.Log("Air Traffic Controller: severity SEVERE");
                    trafficEvent.Invoke((int)RiskSeverity.SEVERE);
                    severeThreatEvent.Invoke(severeThreatTraffic);
                    currentStateInt = (int)RiskSeverity.SEVERE; 
                    break;
            }

            // printIntruders();
            yield return new WaitForSeconds(1f);
        }
    }

    private void InvokeTrackingEvent(int currentTime, string sailplaneID, Transform sailplane)
    {
        if (sailplane.GetComponent<Rigidbody>() != null)
        {
            Vector3 position = sailplane.position;

            StateVector stateVector = new StateVector(
                timestamp: currentTime,
                position: position,
                speed: sailplane.GetComponent<Rigidbody>().velocity.magnitude,
                gazeDirection: GetGazeDirection());
            trackingEvent.Invoke(new StateVectorWithID(sailplaneID: sailplaneID, stateVector: stateVector));
        }
    }

    private Vector2 GetGazeDirection()
    {
        var gazeDirectionEulerAngles = m_MainCamera.transform.localRotation.eulerAngles;
        
        var lrGaze = gazeDirectionEulerAngles.y;
        if (lrGaze > 180f)
            lrGaze -= 360f;

        var udGaze = - gazeDirectionEulerAngles.x + 360f;
        if (udGaze > 180f)
            udGaze -= 360f;

        return new Vector2(lrGaze, udGaze);
    }

    private int GetCurrentTime()
    {
        return (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private void UpdateIntrudersFirstPossibilityAcquisition(bool reset, int currentTime, GameObject intruder)
    {
        if (reset)
        {
            if (intrudersFirstPossibilityAcquisition.ContainsKey(intruder))
            {
                intrudersFirstPossibilityAcquisition.Remove(intruder);
            }
        }
        else
        {
            if (!intrudersFirstPossibilityAcquisition.ContainsKey(intruder))
            {
                intrudersFirstPossibilityAcquisition.Add(intruder, (currentTime, intruder.transform.position, pilotSailplane.position));
            }
        }
    }

    private void PrintIntruders()
    {
        string log = "Intruders Dictionary\n";
        foreach (KeyValuePair<GameObject, (int, Vector3, Vector3)> entry in intrudersFirstPossibilityAcquisition)
        {
            log += "name: " + entry.Key.name + "\nts: " + entry.Value.Item1 + "\nintruder pos: " + entry.Value.Item2 + "\npilot pos: " + entry.Value.Item3;
        }
        Debug.Log(log);
    }

    public (int, Vector3, Vector3) GetIntruderFirstPossibilityAcquisition(GameObject intruder)
    {
        if (intrudersFirstPossibilityAcquisition.ContainsKey(intruder))
        {
            return intrudersFirstPossibilityAcquisition[intruder];
        }
        else
        {
            return (-1, default, default);
        }
    }

    public Dictionary<GameObject, (int, Vector3, Vector3)> GetIntrudersFirstPossibilityAcquisition()
    {
        return this.intrudersFirstPossibilityAcquisition;
    }

    private void MaximumFilter()
    {

    }

    /*
    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }
    */

    /*
    private void UpdatePredictor()
    {
        string payload = "{\"aircrafts\": [";
        var pos = pilotSailplane.transform.position;
        var aircraftData = new AircraftData
        {
            id = 0,
            latitude = pos.z,
            longitude = pos.x,
            altitude = pos.y
        };
        payload += JsonSerializer.ToJsonString(aircraftData);
        if (traffic.Count > 0)
            payload += ", ";
        int i = 0;
        foreach (GameObject sailplane in traffic)
        {
            i++;
            pos = sailplane.transform.position;
            aircraftData = new AircraftData
            {
                id = i,
                latitude = pos.z,
                longitude = pos.x,
                altitude = pos.y
            };
            payload += JsonSerializer.ToJsonString(aircraftData);
            if(i < traffic.Count)
                payload += ", ";
        }
        payload += "]}";
        
        Debug.Log(payload);
        StartCoroutine(Upload(predictionServiceURI, payload));
        /*
        foreach (GameObject sailplane in traffic)
        {
            
        }
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    IEnumerator Upload(string uri, string data)
    {
        Debug.Log("Sending content");
        using (UnityWebRequest www = UnityWebRequest.Post(uri, data, "application/json"))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }

    private void GetPredictedTrajectories()
    {

    }
    */
}
