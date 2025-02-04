using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Waypoint
{
    public int timestamp;
    public string waypointID;
    public float altitude;
    public Waypoint(int timestamp = -1, string waypointID = "", float altitude = -1f)
    {
        this.timestamp = timestamp;
        this.waypointID = waypointID;
        this.altitude = altitude;
    }
}

[System.Serializable]
public class ScenarioWaypoints
{
    public int scenarioID;
    public List<Waypoint> waypoints;
    public ScenarioWaypoints(int scenarioID = -1)
    {
        this.scenarioID = scenarioID;
        this.waypoints = new List<Waypoint>();
    }
}

[System.Serializable]
public class Waypoints
{
    public List<ScenarioWaypoints> scenarioWaypoints;
    public Waypoints()
    {
        this.scenarioWaypoints = new List<ScenarioWaypoints>();
    }
}