﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AircraftPhysics : MonoBehaviour
{
    const float PREDICTION_TIMESTEP_FRACTION = 0.5f;

    public WindAtCurrentPosition wind;
    public Canvas logCanvas;
    private TextMeshProUGUI windLog;

    [SerializeField] 
    float thrust = 0;
    [SerializeField] 
    List<AeroSurface> aerodynamicSurfaces = null;

    Rigidbody rb;
    float thrustPercent;
    BiVector3 currentForceAndTorque;

    private Vector3 windVelocity = Vector3.zero;
    void Start()
    {
        windLog = logCanvas.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        windVelocity = wind.windStrengthAtCurrentPosition;
        wind.weatherEvent.AddListener(onWeatherEvent);
    }

    private void onWeatherEvent(Vector3 wind)
    {
        windVelocity = wind;
        windLog.GetComponent<TMP_Text>().text = "Wind:\nx = " + windVelocity.x + "\ny = " + windVelocity.y + "\nz = " + windVelocity.z;
    }

    public void SetThrustPercent(float percent)
    {
        thrustPercent = percent;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        BiVector3 forceAndTorqueThisFrame = 
            CalculateAerodynamicForces(rb.velocity, rb.angularVelocity, windVelocity, 1.2f, rb.worldCenterOfMass);

        Vector3 velocityPrediction = PredictVelocity(forceAndTorqueThisFrame.p
            + transform.forward * thrust * thrustPercent + Physics.gravity * rb.mass);
        Vector3 angularVelocityPrediction = PredictAngularVelocity(forceAndTorqueThisFrame.q);

        BiVector3 forceAndTorquePrediction = 
            CalculateAerodynamicForces(velocityPrediction, angularVelocityPrediction, windVelocity, 1.2f, rb.worldCenterOfMass);

        currentForceAndTorque = (forceAndTorqueThisFrame + forceAndTorquePrediction) * 0.5f;
        rb.AddForce(currentForceAndTorque.p);
        rb.AddTorque(currentForceAndTorque.q);

        rb.AddForce(transform.forward * thrust * thrustPercent);
    }

    private BiVector3 CalculateAerodynamicForces(Vector3 velocity, Vector3 angularVelocity, Vector3 windVelocity, float airDensity, Vector3 centerOfMass)
    {
        BiVector3 forceAndTorque = new BiVector3();
        foreach (var surface in aerodynamicSurfaces)
        {
            Vector3 relativePosition = surface.transform.position - centerOfMass;
            forceAndTorque += surface.CalculateForces(-velocity + windVelocity
                -Vector3.Cross(angularVelocity,
                relativePosition),
                airDensity, relativePosition);
        }
        return forceAndTorque;
    }

    private Vector3 PredictVelocity(Vector3 force)
    {
        return rb.velocity + Time.fixedDeltaTime * PREDICTION_TIMESTEP_FRACTION * force / rb.mass;
    }

    private Vector3 PredictAngularVelocity(Vector3 torque)
    {
        Quaternion inertiaTensorWorldRotation = rb.rotation * rb.inertiaTensorRotation;
        Vector3 torqueInDiagonalSpace = Quaternion.Inverse(inertiaTensorWorldRotation) * torque;
        Vector3 angularVelocityChangeInDiagonalSpace;
        angularVelocityChangeInDiagonalSpace.x = torqueInDiagonalSpace.x / rb.inertiaTensor.x;
        angularVelocityChangeInDiagonalSpace.y = torqueInDiagonalSpace.y / rb.inertiaTensor.y;
        angularVelocityChangeInDiagonalSpace.z = torqueInDiagonalSpace.z / rb.inertiaTensor.z;

        return rb.angularVelocity + Time.fixedDeltaTime * PREDICTION_TIMESTEP_FRACTION
            * (inertiaTensorWorldRotation * angularVelocityChangeInDiagonalSpace);
    }

#if UNITY_EDITOR
    // For gizmos drawing.
    public void CalculateCenterOfLift(out Vector3 center, out Vector3 force, Vector3 displayAirVelocity, float displayAirDensity)
    {
        Vector3 com;
        BiVector3 forceAndTorque;
        if (aerodynamicSurfaces == null)
        {
            center = Vector3.zero;
            force = Vector3.zero;
            return;
        }

        if (rb == null)
        {
            com = GetComponent<Rigidbody>().worldCenterOfMass;
            forceAndTorque = CalculateAerodynamicForces(-displayAirVelocity, Vector3.zero, windVelocity, displayAirDensity, com);
        }
        else
        {
            com = rb.worldCenterOfMass;
            forceAndTorque = currentForceAndTorque;
        }

        force = forceAndTorque.p;
        center = com + Vector3.Cross(forceAndTorque.p, forceAndTorque.q) / forceAndTorque.p.sqrMagnitude;
    }
#endif
}


