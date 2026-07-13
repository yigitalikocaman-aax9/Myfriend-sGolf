using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Wheel Meshes (visual rims, optional)")]
    public Transform wheelMeshFL;
    public Transform wheelMeshFR;
    public Transform wheelMeshRL;
    public Transform wheelMeshRR;

    [Header("Settings")]
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 30f;

    [Header("Steering Feel (donusu iyilestirir)")]
    [Tooltip("Direksiyonun hedef acisina ulasma hizi. Dusuk = yumusak/tembel, Yuksek = ani/sert")]
    public float steerSpeed = 5f;
    [Tooltip("Hizlandikca direksiyon hassasiyeti dusurulur mu (gercekci his icin onerilir)")]
    public bool reduceSteerAtSpeed = true;
    [Tooltip("Bu hizin (km/s) uzerinde direksiyon en dusuk hassasiyete iner")]
    public float speedForMinSteer = 100f;
    [Tooltip("Yuksek hizda direksiyon acisinin carpani (orn 0.4 = %40'ina iner)")]
    [Range(0.1f, 1f)]
    public float minSteerMultiplier = 0.4f;

    [Header("Controls")]
    [Tooltip("W/S ters calisiyorsa bunu isaretle")]
    public bool invertThrottle = false;
    [Tooltip("A/D ters calisiyorsa bunu isaretle")]
    public bool invertSteer = false;

    [Header("Drivetrain")]
    public bool frontWheelDrive = true;
    public bool rearWheelDrive = true;

    [Header("Center of Mass (important for stability)")]
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);

    [Header("Tire Grip (kayma hissini azaltmak icin)")]
    public float forwardFrictionStiffness = 1.5f;
    public float sidewaysFrictionStiffness = 1.5f;

    [Header("Brake Light Status (read-only, for other scripts)")]
    [Tooltip("True whenever the brake lights should be lit: Space is held, or S is pressed while the car still moves forward")]
    public bool isBraking;
    public enum Gear
{
    Drive,
    Reverse
}

[Header("Transmission")]
public Gear currentGear = Gear.Drive;

    private Rigidbody rb;
    private float throttleInput;
    private float steerInput;
    private bool brakeInput;
    private float currentSteerAngle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMassOffset;

        ApplyFrictionSettings(wheelFL);
        ApplyFrictionSettings(wheelFR);
        ApplyFrictionSettings(wheelRL);
        ApplyFrictionSettings(wheelRR);
    }

    void ApplyFrictionSettings(WheelCollider wc)
    {
        if (wc == null) return;

        WheelFrictionCurve forward = wc.forwardFriction;
        forward.stiffness = forwardFrictionStiffness;
        wc.forwardFriction = forward;

        WheelFrictionCurve sideways = wc.sidewaysFriction;
        sideways.stiffness = sidewaysFrictionStiffness;
        wc.sidewaysFriction = sideways;
    }

    void Update()
    {
        ReadInput();
        UpdateWheelMeshes();
    }

    void ReadInput()
{
    var keyboard = Keyboard.current;
    if (keyboard == null) return;

    if (keyboard.rKey.wasPressedThisFrame)
{
    float speed = rb.linearVelocity.magnitude * 3.6f; // km/h

    if (speed < 2f)
    {
        currentGear = currentGear == Gear.Drive
            ? Gear.Reverse
            : Gear.Drive;
    }
    Debug.Log("Gear Changed: " + currentGear);
}

    float horizontal = 0f;

    if (keyboard.dKey.isPressed)
        horizontal += 1;

    if (keyboard.aKey.isPressed)
        horizontal -= 1;

    steerInput = invertSteer ? -horizontal : horizontal;

    brakeInput = keyboard.spaceKey.isPressed;

    throttleInput = 0;

    if (currentGear == Gear.Drive)
    {
        if (keyboard.wKey.isPressed)
            throttleInput = 1;

        if (keyboard.sKey.isPressed)
            throttleInput = 0;
    }
    else
    {
        if (keyboard.sKey.isPressed)
            throttleInput = -1;

        if (keyboard.wKey.isPressed)
            throttleInput = 0;
    }
}

    void FixedUpdate()
    {
        // Hiza gore direksiyon hassasiyetini ayarla
        float steerMultiplier = 1f;
        if (reduceSteerAtSpeed)
        {
            float speedKmh = rb.linearVelocity.magnitude * 3.6f;
            float t = Mathf.Clamp01(speedKmh / speedForMinSteer);
            steerMultiplier = Mathf.Lerp(1f, minSteerMultiplier, t);
        }

        float targetSteerAngle = steerInput * maxSteerAngle * steerMultiplier;

        // Direksiyonu aninda degil, yumusak bir gecisle hedef aciya getir
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, steerSpeed * Time.fixedDeltaTime);

        wheelFL.steerAngle = currentSteerAngle;
        wheelFR.steerAngle = currentSteerAngle;

        float appliedMotorTorque = throttleInput * -motorForce;

        if (frontWheelDrive)
        {
            wheelFL.motorTorque = appliedMotorTorque;
            wheelFR.motorTorque = appliedMotorTorque;
        }

        if (rearWheelDrive)
        {
            wheelRL.motorTorque = appliedMotorTorque;
            wheelRR.motorTorque = appliedMotorTorque;
        }

        float appliedBrakeForce = 0f;

if (brakeInput)
{
    appliedBrakeForce = brakeForce; // Space = el freni
}
else
{
    if (currentGear == Gear.Drive && Keyboard.current.sKey.isPressed)
    {
        appliedBrakeForce = brakeForce;
    }

    if (currentGear == Gear.Reverse && Keyboard.current.wKey.isPressed)
    {
        appliedBrakeForce = brakeForce;
    }
}
        wheelFL.brakeTorque = appliedBrakeForce;
        wheelFR.brakeTorque = appliedBrakeForce;
        wheelRL.brakeTorque = appliedBrakeForce;
        wheelRR.brakeTorque = appliedBrakeForce;

        // Brake lights should light up when the handbrake (Space) is held,
        // or when the player presses the "backward" input while the car is still moving forward
        // (i.e. actively slowing down rather than reversing from a standstill)
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        bool keyboardBrake = false;

        if (currentGear == Gear.Drive)
        {
          keyboardBrake = Keyboard.current.sKey.isPressed;
        }
        else
        {
          keyboardBrake = Keyboard.current.wKey.isPressed;
        }

        isBraking = brakeInput || keyboardBrake;
    }

    void UpdateWheelMeshes()
    {
        UpdateWheelPose(wheelFL, wheelMeshFL);
        UpdateWheelPose(wheelFR, wheelMeshFR);
        UpdateWheelPose(wheelRL, wheelMeshRL);
        UpdateWheelPose(wheelRR, wheelMeshRR);
    }

    void UpdateWheelPose(WheelCollider collider, Transform mesh)
    {
        if (mesh == null) return;

        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);

        mesh.position = pos;
        mesh.rotation = rot;
    }
}