using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FlightStats
{
    public string intruderID;
    public float heading;
    public float variometer;
    public FlightStats(string intruderID, float heading, float variometer)
    {
        this.intruderID = intruderID;
        this.heading = heading;
        this.variometer = variometer;
    }
}

[System.Serializable]
public class ScenarioFlightStats
{
    public int scenarioID;
    public List<FlightStats> flightStats;
    public ScenarioFlightStats(int scenarioID)
    {
        this.scenarioID = scenarioID;
        this.flightStats = new List<FlightStats>();
    }
}

[System.Serializable]
public class ScenariosFlightStats
{
    public List<ScenarioFlightStats> scenariosFlightStats;
    public ScenariosFlightStats()
    {
        this.scenariosFlightStats = new List<ScenarioFlightStats>();
    }
}
