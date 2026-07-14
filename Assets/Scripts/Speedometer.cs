using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    [Header("Bağlantılar")]
    public Rigidbody carRigidbody; // Arabanın Rigidbody bileşeni
    public TMP_Text speedText;     // UI_SpeedometerText objemiz
    public AudioSource engineAudio; // Arabaya eklediğimiz Audio Source

    [Header("Hız Ayarları")]
    public float speedMultiplier = 3.6f; // m/s -> KM/H çevirici
    public float speedLimit = 100f;      // Bu hızı geçince yazı kırmızı olacak

    [Header("Renk Ayarları")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.8f); 
    public Color warningColor = new Color(1f, 0.1f, 0.1f, 1f); 

    [Header("Ses ve Devir Ayarları")]
    public float minPitch = 0.8f;   // Vitesin en başındaki en düşük ses tonu (Rölanti/Yeni vites)
    public float maxPitch = 1.8f;   // Vitesin sonundaki en yüksek ses tonu (Bağırma noktası)

    // Senin belirttiğin vites geçiş hız sınırları
    private readonly float[] gearLimits = { 20f, 50f, 80f, 100f, 120f, 130f };

    void Start()
    {
        if (speedText != null)
        {
            speedText.color = normalColor;
        }

        if (engineAudio == null && carRigidbody != null)
        {
            engineAudio = carRigidbody.GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (carRigidbody != null)
        {
            // Arabanın anlık hızını KM/H olarak hesapla
            float currentSpeed = carRigidbody.linearVelocity.magnitude * speedMultiplier;
            int displaySpeed = Mathf.RoundToInt(currentSpeed);

            // 1. Hız Göstergesi Yazısı ve Renk Kontrolü
            if (speedText != null)
            {
                speedText.text = displaySpeed.ToString() + " KM/H";
                speedText.color = (currentSpeed >= speedLimit) ? warningColor : normalColor;
            }

            // 2. Vites ve Dinamik Motor Sesi Kontrolü
            if (engineAudio != null)
            {
                int currentGear = CalculateGear(currentSpeed);
                float pitchValue = CalculatePitchForGear(currentSpeed, currentGear);
                
                // Hesaplanan ses perdesini motora aktar
                engineAudio.pitch = pitchValue;
            }
        }
    }

    // Arabanın hızına göre şu an hangi viteste olduğunu bulur (0: 1. Vites, 1: 2. Vites...)
    int CalculateGear(float speed)
    {
        for (int i = 0; i < gearLimits.Length; i++)
        {
            if (speed < gearLimits[i])
            {
                return i; // Hız limitin altındaysa bu vitesteyiz
            }
        }
        return gearLimits.Length; // Limitlerin üstündeysek en son vitesteyiz
    }

    // Bulunan vitesin içindeki hıza göre motor devrini (pitch) hesaplar
    float CalculatePitchForGear(float speed, int gear)
    {
        float minSpeedForThisGear = 0f;
        float maxSpeedForThisGear = gearLimits[0];

        if (gear > 0)
        {
            minSpeedForThisGear = gearLimits[gear - 1];
            if (gear < gearLimits.Length)
            {
                maxSpeedForThisGear = gearLimits[gear];
            }
            else
            {
                maxSpeedForThisGear = 200f; // Son vitesin sınırını yüksek bir hız yapalım
            }
        }

        // Mevcut vitesin içindeki hız yüzdesini bul (0.0 ile 1.0 arasında)
        float gearSpeedRange = maxSpeedForThisGear - minSpeedForThisGear;
        float currentSpeedInGear = speed - minSpeedForThisGear;
        float gearProgress = Mathf.Clamp01(currentSpeedInGear / gearSpeedRange);

        // Bu yüzdeye göre minPitch ve maxPitch arasında yumuşakça geçiş yap
        return Mathf.Lerp(minPitch, maxPitch, gearProgress);
    }
}