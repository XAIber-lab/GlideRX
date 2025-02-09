using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WaypointsFileDataHandler : MonoBehaviour
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private int scenarioID = -1;
    public WaypointsFileDataHandler(string dataDirPath, string dataFileName, SceneDirector sceneDirector)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.scenarioID = sceneDirector.scenarioID;
    }

    public Waypoints Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        Waypoints loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                loadedData = JsonUtility.FromJson<Waypoints>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(Waypoint waypoint)
    {
        Waypoints currentContent = Load();
        if (currentContent == null)
        {
            currentContent = new Waypoints();
            ScenarioWaypoints newScenarioWaypoints = new ScenarioWaypoints(this.scenarioID);
            currentContent.scenarioWaypoints.Add(newScenarioWaypoints);
        }
        else
        {
            if (!currentContent.scenarioWaypoints.Exists(x => x.scenarioID == this.scenarioID))
            {
                ScenarioWaypoints newScenarioWaypoints = new ScenarioWaypoints(this.scenarioID);
                currentContent.scenarioWaypoints.Add(newScenarioWaypoints);
            }
        }
        // Update content
        currentContent.scenarioWaypoints.Find(x => x.scenarioID == this.scenarioID).waypoints.Add(waypoint);

        string fullPath = Path.Combine(dataDirPath, dataFileName);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        string dataToStore = JsonUtility.ToJson(currentContent, true);

        using (FileStream stream = new FileStream(fullPath, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(dataToStore);
            }
        }
    }
}
