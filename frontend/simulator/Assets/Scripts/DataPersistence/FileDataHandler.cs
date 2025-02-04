using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private int scenarioID = 0;

    public FileDataHandler(string dataDirPath, string dataFileName, int scenarioID)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.scenarioID = scenarioID;
    }

    public TestSamples Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        TestSamples loadedData = null;
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
                loadedData = JsonUtility.FromJson<TestSamples>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        TestSamples currentContent = Load();
        if(currentContent == null)
        {
            currentContent = new TestSamples();
            ScenarioSamples scenarioSamples = new ScenarioSamples(scenarioID);
            currentContent.scenarioSamples.Add(scenarioSamples);
        }
        else
        {
            if (!currentContent.scenarioSamples.Exists(x => x.scenarioID == scenarioID))
            {
                ScenarioSamples scenarioSamples = new ScenarioSamples(scenarioID);
                currentContent.scenarioSamples.Add(scenarioSamples);
            }
        }
        // Update content
        currentContent.scenarioSamples.Find(x => x.scenarioID == scenarioID).samples.Add(data);

        string fullPath = Path.Combine(dataDirPath, dataFileName);
        // try
        // {
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        string dataToStore = JsonUtility.ToJson(currentContent, true);

        using (FileStream stream = new FileStream(fullPath, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(dataToStore);
            }
        }
        // }
        /*
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
        */
    }
}
