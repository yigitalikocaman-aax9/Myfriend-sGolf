using TMPro;
using UnityEngine;

public class GearUI : MonoBehaviour
{
    public CarController car;
    public TextMeshProUGUI gearText; // Sol üstteki yeşil/kırmızı "GEAR : D" yazısı
    public TextMeshProUGUI buttonText; // Sağdaki butonun üzerindeki "D" veya "R" yazısı

    void Update()
    {
        if (car == null) return;

        // Buton şeffaflığıyla tam eşleşen Alpha oranı (36 / 255)
        float targetAlpha = 36f / 255f; 

        if (car.currentGear == CarController.Gear.Drive)
        {
            // Sol üst HUD yazısı (Burası tam canlı yeşil kalabilir, istersen burayı da şeffaf yapabilirsin)
            if (gearText != null)
            {
                gearText.text = "GEAR : D";
                gearText.color = Color.green;
            }

            // Buton üzerindeki harf (Şeffaf Yeşil)
            if (buttonText != null)
            {
                buttonText.text = "D";
                // RGBA: Kırmızı=0, Yeşil=1, Mavi=0, Alpha=0.14f
                buttonText.color = new Color(0f, 1f, 0f, targetAlpha); 
            }
        }
        else
        {
            // Sol üst HUD yazısı (Tam canlı kırmızı)
            if (gearText != null)
            {
                gearText.text = "GEAR : R";
                gearText.color = Color.red;
            }

            // Buton üzerindeki harf (Şeffaf Kırmızı)
            if (buttonText != null)
            {
                buttonText.text = "R";
                // RGBA: Kırmızı=1, Yeşil=0, Mavi=0, Alpha=0.14f
                buttonText.color = new Color(1f, 0f, 0f, targetAlpha);
            }
        }
    }

    // Butona tıklandığında çalışacak fonksiyon
    public void OnGearButtonClicked()
    {
        if (car != null)
        {
            car.ToggleGear();
        }
    }
}