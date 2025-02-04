using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

[System.Serializable]
public class LeftTriggerEvent : UnityEvent<Vector2> { }

[System.Serializable]
public class RightTriggerEvent : UnityEvent<Vector2> { }

public class TriggerWatcher : MonoBehaviour
{
    public PrimaryLeft2DAxisEvent primaryLeft2DAxisMovement;
    public PrimaryRight2DAxisEvent primaryRight2DAxisMovement;

    private List<InputDevice> leftDevicesWithPrimaryJoystick;
    private List<InputDevice> rightDevicesWithPrimaryJoystick;
    private Vector2 lastPrimaryLeft2DAxisState = Vector2.zero;
    private Vector2 lastPrimaryRight2DAxisState = Vector2.zero;
    private void Awake()
    {
        if (primaryLeft2DAxisMovement == null)
        {
            primaryLeft2DAxisMovement = new PrimaryLeft2DAxisEvent();
        }
        if (primaryRight2DAxisMovement == null)
        {
            primaryRight2DAxisMovement = new PrimaryRight2DAxisEvent();
        }
        leftDevicesWithPrimaryJoystick = new List<InputDevice>();
        rightDevicesWithPrimaryJoystick = new List<InputDevice>();
    }

    void OnEnable()
    {
        List<InputDevice> leftHandedControllers = new List<InputDevice>();
        var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);
        foreach (InputDevice device in leftHandedControllers)
            InputDevices_deviceConnected(device);

        List<InputDevice> rightHandedControllers = new List<InputDevice>();
        desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, rightHandedControllers);
        foreach (InputDevice device in rightHandedControllers)
            InputDevices_deviceConnected(device);

        InputDevices.deviceConnected += InputDevices_deviceConnected;
        InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_deviceConnected;
        InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
        leftDevicesWithPrimaryJoystick.Clear();
        rightDevicesWithPrimaryJoystick.Clear();
    }

    private void InputDevices_deviceConnected(InputDevice device)
    {
        Vector2 discardedValue;
        if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out discardedValue))
        {
            if ((device.characteristics & InputDeviceCharacteristics.Left) == InputDeviceCharacteristics.Left)
            {
                leftDevicesWithPrimaryJoystick.Add(device);
            }

            if ((device.characteristics & InputDeviceCharacteristics.Right) == InputDeviceCharacteristics.Right)
            {
                rightDevicesWithPrimaryJoystick.Add(device);
            }
        }
    }

    private void InputDevices_deviceDisconnected(InputDevice device)
    {
        if (leftDevicesWithPrimaryJoystick.Contains(device))
            leftDevicesWithPrimaryJoystick.Remove(device);

        if (rightDevicesWithPrimaryJoystick.Contains(device))
            rightDevicesWithPrimaryJoystick.Remove(device);
    }

    void Update()
    {
        bool newRead = false;
        Vector2 primaryLeft2DAxisState = Vector2.zero;
        Vector2 primaryRight2DAxisState = Vector2.zero;

        foreach (var device in leftDevicesWithPrimaryJoystick)
        {
            newRead = device.TryGetFeatureValue(CommonUsages.primary2DAxis, out primaryLeft2DAxisState);
        }
        if (newRead)
        {
            primaryLeft2DAxisMovement.Invoke(primaryLeft2DAxisState);
            // lastPrimary2DAxisState = primary2DAxisState;
        }

        foreach (var device in rightDevicesWithPrimaryJoystick)
        {
            newRead = device.TryGetFeatureValue(CommonUsages.primary2DAxis, out primaryRight2DAxisState);
        }
        if (newRead)
        {
            primaryRight2DAxisMovement.Invoke(primaryRight2DAxisState);
            // lastPrimary2DAxisState = primary2DAxisState;
        }
    }
}
