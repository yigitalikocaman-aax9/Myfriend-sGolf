using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    [Header("Bağlantılar")]
    public Rigidbody carRigidbody; // Arabanın Rigidbody bileşeni
    public TMP_Text speedText;     // UI_SpeedometerText objemiz

    [Header("Hız Ayarları")]
    public float speedMultiplier = 3.6f; // m/s -> KM/H çevirici
    public float speedLimit = 100f;      // Bu hızı geçince yazı kırmızı olacak

    [Header("Renk Ayarları")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.8f); // Normal hızda yarı şeffaf beyaz
    public Color warningColor = new Color(1f, 0.1f, 0.1f, 1f); // Hız sınırını aşınca parlak kırmızı

    void Start()
    {
        // Başlangıçta yazıyı normal renge eşitleyelim
        if (speedText != null)
        {
            speedText.color = normalColor;
        }
    }

    void Update()
    {
        if (carRigidbody != null && speedText != null)
        {
            // Arabanın anlık hızını KM/H olarak hesapla
            float currentSpeed = carRigidbody.linearVelocity.magnitude * speedMultiplier;
            int displaySpeed = Mathf.RoundToInt(currentSpeed);

            // Ekrana yazdır
            speedText.text = displaySpeed.ToString() + " KM/H";

            // Hız kontrolü ve Renk değişimi
            if (currentSpeed >= speedLimit)
            {
                speedText.color = warningColor;
            }
            else
            {
                speedText.color = normalColor;
            }
        }
    }
}