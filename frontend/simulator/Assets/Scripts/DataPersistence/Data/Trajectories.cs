using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;

[System.Serializable]
public class StateVector
{
    public int timestamp;
    public Vector3 position;
    public float speed;
    public Vector2 gazeDirection;

    public StateVector(int timestamp = -1, Vector3 position = default, float speed = -1f, Vector2 gazeDirection = default)
    {
        this.timestamp = timestamp;
        this.position = position;
        this.speed = speed;
        this.gazeDirection = gazeDirection;
    }
}

[System.Serializable]
public class Trajectory
{
    public string sailplaneID = "";
    public int scenarioID = -1;
    public List<StateVector> stateVectors;

    public Trajectory(string sailplaneID = "", int scenarioID = -1)
    {
        this.sailplaneID = sailplaneID;
        this.scenarioID = scenarioID;
        this.stateVectors = new List<StateVector>();
    }
}

[System.Serializable]
public class Trajectories
{
    public List<Trajectory> trajectories;

    public Trajectories()
    {
        this.trajectories = new List<Trajectory>();
    }
}

public class StateVectorWithID
{
    public string sailplaneID;
    public StateVector stateVector;

    public StateVectorWithID(string sailplaneID, StateVector stateVector)
    {
        this.sailplaneID = sailplaneID;
        this.stateVector = stateVector;
    }
}