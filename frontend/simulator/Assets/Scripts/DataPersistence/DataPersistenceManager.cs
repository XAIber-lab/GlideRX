using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string acquisitionsFileName;
    [SerializeField] private string trajectoriesFileName;
    [SerializeField] private string waypointsFileName;
    [SerializeField] private string flightStatsFileName;

    public SceneDirector sceneDirector;

    public GameData gameData;
    public Canvas logCanvas;

    public bool logTrackingData = true;
    public bool logOnlySelfTrack = true;

    private TextMeshProUGUI acquisitionLog;
    private TextMeshProUGUI gazeDirectionLog;
    private FileDataHandler dataHandler;
    private TrajectoriesFileDataHandler trajectoriesFileDataHandler;
    private WaypointsFileDataHandler waypointsFileDataHandler;
    private FlightStatsFileDataHandler flightStatsFileDataHandler;

    public static DataPersistenceManager instance { get; private set; }

    public void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        instance = this;
    }

    private void Start()
    {
        acquisitionLog = logCanvas.transform.GetChild(5).gameObject.GetComponent<TextMeshProUGUI>();
        if (logCanvas.transform.childCount == 7)
            gazeDirectionLog = logCanvas.transform.GetChild(6).gameObject.GetComponent<TextMeshProUGUI>();
        try
        {
            AirTrafficController airTrafficController = GameObject.FindWithTag("AirTrafficController").GetComponent<AirTrafficController>();
            airTrafficController.trackingEvent.AddListener(onTrackingEvent);
        }
        catch (Exception e)
        {
            Debug.LogError("No Air Traffic Controller defined." + e.Message);
        }
        sceneDirector.acquisitionEvent.AddListener(onAcquisitionEvent);

        GameObject[] intruders = GameObject.FindGameObjectsWithTag("IWaypointEvent");
        foreach (GameObject intruder in intruders)
        {
            intruder.GetComponent<IWaypointEvent>().waypointCheckedEvent.AddListener(OnWaypointCheckedEvent);
        }

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, acquisitionsFileName, sceneDirector.scenarioID);
        this.trajectoriesFileDataHandler = new TrajectoriesFileDataHandler(Application.persistentDataPath, trajectoriesFileName, sceneDirector);
        this.waypointsFileDataHandler = new WaypointsFileDataHandler(Application.persistentDataPath, waypointsFileName, sceneDirector);
        this.flightStatsFileDataHandler = new FlightStatsFileDataHandler(Application.persistentDataPath, flightStatsFileName, sceneDirector);
        // LoadGame();

        GameObject[] sailplanes = GameObject.FindGameObjectsWithTag("Sailplane");
        foreach (GameObject sailplane in sailplanes)
        {
            Debug.Log("Sailplane name: " + sailplane.name);
            sailplane.GetComponent<FlightStatsLog>().flightStatsLogEvent.AddListener(onFlightStatsLogEvent);
        }
    }

    private void onFlightStatsLogEvent(FlightStats flightStats)
    {
        Debug.Log("Saving log for intruder" + flightStats.intruderID);
        flightStatsFileDataHandler.Save(flightStats);
    }

    private void onAcquisitionEvent(GameData acquisitionData)
    {
        this.gameData = acquisitionData;
        SaveGame();
        StartCoroutine(FadeInOut(acquisitionLog, 0.05f));
    }

    private void onTrackingEvent(StateVectorWithID stateVectorWithID)
    {
        if (logTrackingData)
        {
            if (logOnlySelfTrack)
            {
                if (stateVectorWithID.sailplaneID == "SELF")
                {
                    if (gazeDirectionLog != null)
                        gazeDirectionLog.text = "Gaze direction: " + "\nl/r = " + stateVectorWithID.stateVector.gazeDirection.x + "\nu/d = " + stateVectorWithID.stateVector.gazeDirection.y;
                    SaveTrackingStateVector(stateVectorWithID);
                }
            }
        }
    }

    private void OnWaypointCheckedEvent(Waypoint waypoint)
    {
        waypointsFileDataHandler.Save(waypoint);
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    /*
    public void LoadGame()
    {
        this.gameData = dataHandler.Load();

        if (this.gameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults.");
            NewGame();
        }
    }
    */

    public void SaveGame()
    {
        dataHandler.Save(gameData);
    }

    public void SaveTrackingStateVector(StateVectorWithID stateVectorWithID)
    {
        trajectoriesFileDataHandler.Save(stateVectorWithID.sailplaneID, stateVectorWithID.stateVector);
    }
    IEnumerator FadeInOut(TextMeshProUGUI textToFade, float morphingSpeed)
    {
        Color color = textToFade.color;
        for (float alpha = 0f; alpha <= 1; alpha += morphingSpeed)
        {
            color.a = alpha;
            textToFade.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        for (float alpha = 1f; alpha >= 0; alpha -= morphingSpeed)
        {
            color.a = alpha;
            textToFade.color = color;
            yield return null;
        }
        color.a = 0f;
        textToFade.color = color;
    }
}
