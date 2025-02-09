using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class TrajectoriesFileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private int scenarioID = -1;
    public TrajectoriesFileDataHandler(string dataDirPath, string dataFileName, SceneDirector sceneDirector)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.scenarioID = sceneDirector.scenarioID;
    }

    public Trajectories Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        Trajectories loadedData = null;
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
                loadedData = JsonUtility.FromJson<Trajectories>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(string sailplaneID, StateVector data)
    {
        Trajectories currentContent = Load();
        if(currentContent == null)
        {
            currentContent = new Trajectories();
            Trajectory trajectory = new Trajectory(sailplaneID, this.scenarioID);
            currentContent.trajectories.Add(trajectory);
        }
        else
        {
            if (!currentContent.trajectories.Exists(x => (x.sailplaneID == sailplaneID && x.scenarioID == this.scenarioID)))
            {
                Trajectory trajectory = new Trajectory(sailplaneID, this.scenarioID);
                currentContent.trajectories.Add(trajectory);
            }
        }
        // Update content
        currentContent.trajectories.Find(x => (x.sailplaneID == sailplaneID && x.scenarioID == this.scenarioID)).stateVectors.Add(data);

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
