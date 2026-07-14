using UnityEngine;
using UnityEngine.EventSystems;

public class MobileSteeringWheel : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public CarController car;
    public RectTransform wheelImage; 
    
    [Header("Direksiyon Ayarları")]
    public float maxSteerAngle = 200f; 
    public float releaseSpeed = 400f;  

    private float wheelAngle = 0f;
    private float lastWheelAngle = 0f;
    private bool isDragging = false;
    private Vector2 centerPoint;

    void Start()
    {
        if (wheelImage == null)
            wheelImage = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!isDragging && wheelAngle != 0f)
        {
            wheelAngle = Mathf.MoveTowards(wheelAngle, 0f, releaseSpeed * Time.deltaTime);
            ApplyRotation();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        centerPoint = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, wheelImage.position);
        lastWheelAngle = Vector2.SignedAngle(Vector2.up, eventData.position - centerPoint);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPoint = eventData.position;
        float currentAngle = Vector2.SignedAngle(Vector2.up, currentPoint - centerPoint);
        
        float angleDifference = Mathf.DeltaAngle(lastWheelAngle, currentAngle);
        
        // DEĞİŞEN KISIM BURASI: += yerine -= yaptık!
        // Böylece fareyi sağa (saat yönüne) sürüklediğinde değerler doğru şekilde pozitif artacak.
        wheelAngle -= angleDifference; 
        
        wheelAngle = Mathf.Clamp(wheelAngle, -maxSteerAngle, maxSteerAngle);
        lastWheelAngle = currentAngle;

        ApplyRotation();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    void ApplyRotation()
    {
        if (wheelImage != null)
        {
            // Direksiyon görselini tam merkezinden saat yönünde/tersinde döndürür
            wheelImage.localRotation = Quaternion.Euler(0, 0, -wheelAngle);
        }

        if (car != null)
        {
            // Artık yönler eşitlendiği için temiz bir bölme işlemi yeterli!
            float steerInput = wheelAngle / maxSteerAngle; 
            car.SetSteerInput(steerInput);
        }
    }
}