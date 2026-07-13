using UnityEngine;

public class CarCameraFollow : MonoBehaviour
{
    [Header("Reverse Camera")]
public CarController carController;

public Transform rearCameraPoint;
public Transform frontCameraPoint;
    [Header("Target")]
    [Tooltip("The car's main object (the one with the Rigidbody, e.g. FINAL_MODEL_R_BE)")]
    public Transform target;

    [Header("Position Settings")]
    [Tooltip("Camera offset relative to the car's local axes (X=side, Y=height, Z=forward/back)")]
    public Vector3 offset = new Vector3(0f, 3.5f, -7f);

    [Tooltip("Where the camera looks at, relative to the car's position (up/down offset)")]
    public Vector3 lookAtOffset = new Vector3(0f, 1f, 0f);

    [Header("Orientation Fix")]
    [Tooltip("Enable this if the camera ends up in front of the car instead of behind it")]
    public bool invertForward = false;

    [Header("Smoothing Settings")]
    [Tooltip("How fast the camera reaches its position. Lower = smoother/laggier, Higher = snappier/instant follow")]
    public float positionSmoothTime = 0.15f;

    [Tooltip("How fast the camera's rotation smooths toward the target rotation")]
    public float rotationSmoothSpeed = 6f;

    [Header("Advanced")]
    [Tooltip("If enabled, the camera fully follows the car's rotation (turns behind the car as it steers)")]
    public bool followCarRotation = true;

    [Tooltip("If enabled, the camera aligns to the car's actual movement direction instead of its facing direction, smoothing out sharp steering jitter")]
    public bool useVelocityDirection = false;

    private Vector3 currentVelocity = Vector3.zero;
    private Rigidbody targetRb;

    void Start()
    {
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
        }
    }

  void LateUpdate()
{
    if (target == null) return;

    Vector3 desiredPosition;

    if (carController != null)
    {
        if (carController.currentGear == CarController.Gear.Drive)
        {
            desiredPosition = rearCameraPoint.position;
        }
        else
        {
            desiredPosition = frontCameraPoint.position;
        }
    }
    else
    {
        Vector3 usedOffset = offset;

        if (invertForward)
            usedOffset.z = -usedOffset.z;

        desiredPosition = target.position + target.TransformDirection(usedOffset);
    }

    transform.position = Vector3.SmoothDamp(
        transform.position,
        desiredPosition,
        ref currentVelocity,
        positionSmoothTime);

    Vector3 lookTarget = target.position + lookAtOffset;
    Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);

    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        desiredRotation,
        rotationSmoothSpeed * Time.deltaTime);

}
}