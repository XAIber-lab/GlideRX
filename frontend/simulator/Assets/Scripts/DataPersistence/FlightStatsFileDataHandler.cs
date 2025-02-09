using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FlightStatsFileDataHandler : MonoBehaviour
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private int scenarioID = -1;
    public FlightStatsFileDataHandler(string dataDirPath, string dataFileName, SceneDirector sceneDirector)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.scenarioID = sceneDirector.scenarioID;
    }

    public ScenariosFlightStats Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        ScenariosFlightStats loadedData = null;
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
                loadedData = JsonUtility.FromJson<ScenariosFlightStats>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(FlightStats data)
    {
        ScenariosFlightStats currentContent = Load();
        if (currentContent == null)
        {
            currentContent = new ScenariosFlightStats();
            ScenarioFlightStats scenarioFlightStats= new ScenarioFlightStats(this.scenarioID);
            currentContent.scenariosFlightStats.Add(scenarioFlightStats);
        }
        else
        {
            if (!currentContent.scenariosFlightStats.Exists(x => (x.scenarioID == this.scenarioID)))
            {
                ScenarioFlightStats scenarioFlightStats = new ScenarioFlightStats(this.scenarioID);
                currentContent.scenariosFlightStats.Add(scenarioFlightStats);
            }
        }
        // Update content
        currentContent.scenariosFlightStats.Find(x => (x.scenarioID == this.scenarioID)).flightStats.Add(data);
        Debug.Log("currentContent: " + currentContent.scenariosFlightStats);

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
