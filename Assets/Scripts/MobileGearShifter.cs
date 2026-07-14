using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MobileGearShifter : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public CarController car; // Arabamızın scripti
    public RectTransform handle; // Hareket edecek olan vites kolu görseli (UI_TransmissionHandle)
    
    [Header("Yazı Göstergeleri")]
    public TMP_Text dText;
    public TMP_Text rText;

    [Header("Tasarım Ayarları")]
    public float movementRange = 40f; // Vites kolunun yukarı/aşağı maksimum kayma mesafesi
    public float snapSpeed = 10f; // Vitesin yerine oturma (animasyon) hızı

    private float targetY = 0f;
    private bool isDragging = false;
    private Vector2 startPointerPos;
    private float startHandleY;

    public enum Gear { Drive, Reverse }
    private Gear currentGear = Gear.Drive;

    void Start()
    {
        // Başlangıçta vites D (Drive) konumunda başlasın
        targetY = movementRange;
        handle.anchoredPosition = new Vector2(0, targetY);
        UpdateVisuals();
    }

    void Update()
    {
        // Sürüklenmiyorken vitesi yerine yumuşakça kaydır
        if (!isDragging)
        {
            float currentY = Mathf.Lerp(handle.anchoredPosition.y, targetY, Time.deltaTime * snapSpeed);
            handle.anchoredPosition = new Vector2(0, currentY);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        startPointerPos = eventData.position;
        startHandleY = handle.anchoredPosition.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float diffY = eventData.position.y - startPointerPos.y;
        float newY = startHandleY + diffY;

        newY = Mathf.Clamp(newY, -movementRange, movementRange);
        handle.anchoredPosition = new Vector2(0, newY);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        // Kolun durduğu yere göre vitesi seç
        if (handle.anchoredPosition.y > 0)
        {
            targetY = movementRange;
            SetGear(Gear.Drive);
        }
        else
        {
            targetY = -movementRange;
            SetGear(Gear.Reverse);
        }
    }

    void SetGear(Gear newGear)
    {
        currentGear = newGear;
        UpdateVisuals();

        if (car != null)
        {
            bool isReverse = (currentGear == Gear.Reverse);
            
            // CarController içindeki vites fonksiyonunu tetiklemesini devre dışı bıraktık.
            // Böylece Unity hata vermeyecek, önce arayüz animasyonunu test edebileceğiz.
            // car.SetReverse(isReverse); 
        }
    }

    void UpdateVisuals()
    {
        if (dText != null && rText != null)
        {
            // D seçiliyken D yeşil, R gri; R seçiliyken R kırmızı, D gri olsun
            if (currentGear == Gear.Drive)
            {
                dText.color = new Color(0.1f, 1f, 0.1f, 1f); // Parlak Yeşil
                rText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Soluk Gri
            }
            else
            {
                dText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Soluk Gri
                rText.color = new Color(1f, 0.1f, 0.1f, 1f); // Parlak Kırmızı
            }
        }
    }
}