using System.Collections.Generic;
using Unity.VRTemplate;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.UI;

public class MyAirplaneController : MonoBehaviour
{
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    List<WheelCollider> wheels = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    public AeroSurfaceConfig mainWingSurfaceConfiguration = null;

    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(0, 1)]
    public float Flap;
    [Range(0, 1)]
    public float Airbrakes;
    [SerializeField]
    Text displayText = null;
    public float minimumMainWingSpan = 2.5f;

    // public Primary2DAxisWatcher primary2DAxisWatcher;

    float thrustPercent;
    float brakesTorque;
    float maximumMainWingSpan = 4.45f;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    private void Start()
    {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();

        if (mainWingSurfaceConfiguration != null)
        {
            maximumMainWingSpan = mainWingSurfaceConfiguration.span;
        }
    }

    private void Update()
    {
        // Pitch = Input.GetAxis("Vertical");
        // Roll = Input.GetAxis("Horizontal");
        // Yaw = Input.GetAxis("Yaw");
        // Airbrakes = Input.GetAxis("Airbrakes");

        // primary2DAxisWatcher.primaryLeft2DAxisMovement.AddListener(onPrimaryLeft2DAxisChange);
        // primary2DAxisWatcher.primaryRight2DAxisMovement.AddListener(onPrimaryRight2DAxisChange);
        // OVRInput.Update();
        SetPitchRoll();
        SetYaw();
        SetAirbrakes();

        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            thrustPercent = thrustPercent > 0 ? 0 : 1f;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }
        */

        /*
        displayText.text = "V: " + ((int)rb.velocity.magnitude).ToString("D3") + " m/s\n";
        displayText.text += "A: " + ((int)transform.position.y).ToString("D4") + " m\n";
        displayText.text += "T: " + (int)(thrustPercent * 100) + "%\n";
        displayText.text += brakesTorque > 0 ? "B: ON" : "B: OFF";
        */
    }

    private void SetPitchRoll()
    {
        Vector2 secondaryThumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Pitch = secondaryThumbstick.y;
        Roll = secondaryThumbstick.x;
    }

    private void SetYaw()
    {
        float primaryTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
        float secondaryTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        Yaw = secondaryTrigger - primaryTrigger;
    }

    private void SetAirbrakes()
    {
        Vector2 primaryThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        float yValue = primaryThumbstick.y;
        if (yValue >= 0)
            Airbrakes = 0;
        else
        {
            Airbrakes = - yValue;
        }
    }

    public void onPrimaryLeft2DAxisChange(Vector2 newPosition)
    {
        Pitch = newPosition.y;
        Roll = newPosition.x;
    }

    public void onPrimaryRight2DAxisChange(Vector2 newPosition)
    {
        Airbrakes = -newPosition.x;
    }

    private void FixedUpdate()
    {
        SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);
        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakesTorque;
            // small torque to wake up wheel collider
            wheel.motorTorque = 0.01f;
        }

        // Airbrakes deployment
        if (mainWingSurfaceConfiguration != null)
        {
            mainWingSurfaceConfiguration.span = (Airbrakes * minimumMainWingSpan) + ((1 - Airbrakes) * maximumMainWingSpan);
        }
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap)
    {
        foreach (var surface in controlSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
    }
}
