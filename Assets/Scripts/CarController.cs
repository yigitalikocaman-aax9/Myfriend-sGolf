using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Wheel Colliders (fizik - gorunmez)")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Wheel Pivots (Hiyerarşideki PivotFL, PivotFR vb.)")]
    public Transform wheelMeshFL;
    public Transform wheelMeshFR;
    public Transform wheelMeshRL;
    public Transform wheelMeshRR;

    [Header("Araba Gövdesi Ayarı (Arabayı Kaldırmak İçin)")]
    public Transform[] carPartsToRaise;
    public float carBodyHeightOffset = 0f;

    [Header("Settings")]
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 30f;

    [Header("Steering Feel (donusu iyilestirir)")]
    public float steerSpeed = 5f;
    public bool reduceSteerAtSpeed = true;
    public float speedForMinSteer = 100f;
    [Range(0.1f, 1f)]
    public float minSteerMultiplier = 0.4f;

    [Header("Tire Grip")]
    public float forwardFrictionStiffness = 1.5f;
    public float sidewaysFrictionStiffness = 1.5f;

    [Header("Brake Light Status (read-only)")]
    public bool isBraking;

    public enum Gear { Drive, Reverse }
    [Header("Transmission")]
    public Gear currentGear = Gear.Drive;

    [Header("Model Açı Düzeltmesi")]
    public Vector3 meshRotationOffset = new Vector3(0, 90, 0);
    [Header("Model Yükseklik Düzeltmesi")]
    public Vector3 meshPositionOffset = new Vector3(0, 0, 0);
    [Header("Collider Pozisyon Düzeltmesi")]
    public Vector3 colliderPositionOffset = new Vector3(0, 0, 0);

    // Mobil Arayüzden Tetiklenecek Giriş Değerleri
    private float throttleInput; // Gaz pedalından gelecek (0 ile 1 arası)
    private float steerInput;    // Direksiyondan gelecek (-1 ile 1 arası)
    private bool isHoldingBrake; // Fren pedalından gelecek (true/false)

    private Rigidbody rb;
    private float currentSteerAngle;
    private Vector3[] originalBodyLocalPositions;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        ApplyFrictionSettings(wheelFL);
        ApplyFrictionSettings(wheelFR);
        ApplyFrictionSettings(wheelRL);
        ApplyFrictionSettings(wheelRR);

        if (carPartsToRaise != null && carPartsToRaise.Length > 0)
        {
            originalBodyLocalPositions = new Vector3[carPartsToRaise.Length];
            for (int i = 0; i < carPartsToRaise.Length; i++)
            {
                if (carPartsToRaise[i] != null)
                    originalBodyLocalPositions[i] = carPartsToRaise[i].localPosition;
            }
        }
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

    // --- MOBİL BUTONLARIN TETİKLEYECEĞİ FONKSİYONLAR (Dışarıdan çağrılacak) ---
    
    // Gaz pedalına basıldığında (value = 1), bırakıldığında (value = 0)
    public void SetThrottleInput(float value)
    {
        throttleInput = value;
    }

    // Direksiyon döndürüldüğünde (-1 tam sol, 1 tam sağ)
    public void SetSteerInput(float value)
    {
        steerInput = value;
    }

    // Fren pedalına basıldığında (state = true), bırakıldığında (state = false)
    public void SetBrakeInput(bool state)
    {
        isHoldingBrake = state;
    }

    // Vites butonuna her basıldığında D ve R arasında geçiş yapar
    public void ToggleGear()
    {
        float speed = rb.linearVelocity.magnitude * 3.6f;
        if (speed < 5f) // Araba durmaya yakınken vites değişsin
        {
            currentGear = currentGear == Gear.Drive ? Gear.Reverse : Gear.Drive;
            Debug.Log("Yeni Vites: " + currentGear);
        }
    }

    void FixedUpdate()
    {
        ApplySteering();
        ApplyMotorTorque();
        ApplyBrakes();
        
        isBraking = isHoldingBrake; // Fren lambası durumu

        SyncAllWheelMeshes();
        UpdateCarBodyHeight();
    }

    void ApplySteering()
    {
        float steerMultiplier = 1f;
        if (reduceSteerAtSpeed)
        {
            float speedKmh = rb.linearVelocity.magnitude * 3.6f;
            float t = Mathf.Clamp01(speedKmh / speedForMinSteer);
            steerMultiplier = Mathf.Lerp(1f, minSteerMultiplier, t);
        }

        float targetSteerAngle = steerInput * maxSteerAngle * steerMultiplier;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, steerSpeed * Time.fixedDeltaTime);

        wheelFL.steerAngle = currentSteerAngle;
        wheelFR.steerAngle = currentSteerAngle;
    }

    void ApplyMotorTorque()
    {
        // Gaz inputunu vites durumuna göre yönlendiriyoruz
        float gearMultiplier = (currentGear == Gear.Drive) ? -1f : 1f;
        float appliedMotorTorque = throttleInput * motorForce * gearMultiplier;

        // Eğer frene basılıyorsa gaza basılsa bile motor torkunu sıfırla (güvenlik için)
        if (isHoldingBrake) appliedMotorTorque = 0f;

        wheelFL.motorTorque = appliedMotorTorque;
        wheelFR.motorTorque = appliedMotorTorque;
        wheelRL.motorTorque = appliedMotorTorque;
        wheelRR.motorTorque = appliedMotorTorque;
    }

    void ApplyBrakes()
    {
        // UI butonundan gelen fren komutuna göre fren kuvveti uygula
        float appliedBrakeForce = isHoldingBrake ? brakeForce : 0f;

        wheelFL.brakeTorque = appliedBrakeForce;
        wheelFR.brakeTorque = appliedBrakeForce;
        wheelRL.brakeTorque = appliedBrakeForce;
        wheelRR.brakeTorque = appliedBrakeForce;
    }

    void SyncAllWheelMeshes()
    {
        SyncWheelMesh(wheelFL, wheelMeshFL, true);
        SyncWheelMesh(wheelFR, wheelMeshFR, false);
        SyncWheelMesh(wheelRL, wheelMeshRL, true);
        SyncWheelMesh(wheelRR, wheelMeshRR, false);
    }

    void SyncWheelMesh(WheelCollider collider, Transform mesh, bool isLeftWheel)
    {
        if (collider == null || mesh == null) return;

        collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        
        Vector3 directionOffset = colliderPositionOffset;
        if (!isLeftWheel) directionOffset.x = -directionOffset.x;

        Vector3 finalPosition = pos + (rot * meshPositionOffset) + (transform.TransformDirection(directionOffset));
        Quaternion finalRotation = rot * Quaternion.Euler(meshRotationOffset);

        mesh.SetPositionAndRotation(finalPosition, finalRotation);
        mesh.localScale = Vector3.one; 
    }

    void UpdateCarBodyHeight()
    {
        if (carPartsToRaise == null || originalBodyLocalPositions == null) return;

        for (int i = 0; i < carPartsToRaise.Length; i++)
        {
            if (carPartsToRaise[i] != null && i < originalBodyLocalPositions.Length)
            {
                Vector3 targetLocalPos = originalBodyLocalPositions[i];
                targetLocalPos.y += carBodyHeightOffset;
                carPartsToRaise[i].localPosition = targetLocalPos;
            }
        }
    }
}