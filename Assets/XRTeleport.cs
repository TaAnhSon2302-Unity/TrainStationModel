using UnityEngine;
using UnityEngine.XR;

public class TeleportManager : MonoBehaviour
{
    [Header("XR Origin")]
    public Transform xrOrigin;

    [Header("Teleport Points")]
    public Transform[] teleportPoints;

    private int currentIndex = 0;

    private InputDevice rightHand;
    private bool previousButtonState;

    private void Start()
    {
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        if (!rightHand.isValid)
            rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        bool pressed;

        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out pressed))
        {
            if (pressed && !previousButtonState)
            {
                TeleportNext();
            }

            previousButtonState = pressed;
        }
    }

    private void TeleportNext()
    {
        if (teleportPoints == null || teleportPoints.Length == 0)
            return;

        xrOrigin.SetPositionAndRotation(
            teleportPoints[currentIndex].position,
            teleportPoints[currentIndex].rotation
        );

        currentIndex++;

        if (currentIndex >= teleportPoints.Length)
        {
            currentIndex = 0;
        }
    }
}