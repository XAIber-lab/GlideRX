using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public long timestamp;
    public string interceptorID;
    public float closingRate;
    public float acquisitionRange;
    public float acquisitionTime;
    public Vector3 position;

    public GameData(
        long timestamp = 0,
        string interceptorID = "",
        float closingRate = -1f,
        float acquisitionRange = -1f,
        float acquisitionTime = -1f,
        Vector3 position = default
        )
    {
        this.timestamp = timestamp;
        this.interceptorID = interceptorID;
        this.closingRate = closingRate;
        this.acquisitionRange = acquisitionRange;
        this.acquisitionTime = acquisitionTime;
        this.position = position;
    }
}

[System.Serializable]
public class ScenarioSamples
{
    public int scenarioID;
    public List<GameData> samples;

    public ScenarioSamples(int scenarioID = -1)
    {
        this.scenarioID = scenarioID;
        this.samples = new List<GameData>();
    }
}

[System.Serializable]
public class TestSamples
{
    public List<ScenarioSamples> scenarioSamples;

    public TestSamples()
    {
        this.scenarioSamples = new List<ScenarioSamples>();
    }
}