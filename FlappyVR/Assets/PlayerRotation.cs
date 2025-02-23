using UnityEngine;
using UnityEngine.XR;

public class PlayerRotation : MonoBehaviour
{
    public XRNode rightHandNode = XRNode.RightHand;
    public float rotationSpeed = 20f;

    private InputDevice rightHandDevice;

    void Start()
    {
        // Get the right-hand device for tracking thumbstick input
        rightHandDevice = InputDevices.GetDeviceAtXRNode(rightHandNode);
    }

    void Update()
    {
        Vector2 thumbstickInput;
        
        // Get the thumbstick input (horizontal and vertical)
        if (rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickInput))
        {
            // Only rotate if the thumbstick is being moved (not pressed in the neutral position)
            if (thumbstickInput.magnitude > 0.1f)  // Deadzone to avoid unwanted rotation from slight movements
            {
                // Rotate the player along the Y-axis based on horizontal input
                float rotationAmount = thumbstickInput.x * rotationSpeed * Time.deltaTime;
                transform.Rotate(Vector3.up, rotationAmount);
            }
        }
    }
}
