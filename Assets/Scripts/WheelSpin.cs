using UnityEngine;

public class WheelSpin : MonoBehaviour
{
    public enum SpinAxis { X, Y, Z }

    [Header("Teker Ayarları")]
    public float wheelRadius = 0.3f;      // metre cinsinden teker yarıçapı

    [Header("Eksen Ayarları")]
    [Tooltip("Tekerleğin hangi local ekseni etrafında döneceği. Yanlış görünüyorsa burayı değiştir.")]
    public SpinAxis spinAxis = SpinAxis.X;

    [Tooltip("Dönüş yönü ters görünüyorsa işaretle.")]
    public bool invert = false;

    [Header("Referanslar")]
    public Rigidbody carRigidbody; // aracın rigidbody'si

    void Update()
    {
        if (carRigidbody == null) return;

        // Aracın ileri yöndeki hızını al
        float speed = Vector3.Dot(carRigidbody.linearVelocity, carRigidbody.transform.forward);

        // Açısal hızı hesapla (derece/saniye)
        float rotationSpeed = (speed / wheelRadius) * Mathf.Rad2Deg;

        if (invert) rotationSpeed *= -1f;

        Vector3 axis = spinAxis switch
        {
            SpinAxis.X => Vector3.right,
            SpinAxis.Y => Vector3.up,
            SpinAxis.Z => Vector3.forward,
            _ => Vector3.right
        };

        // Tekerleği seçilen local eksen etrafında döndür
        transform.Rotate(axis, rotationSpeed * Time.deltaTime, Space.Self);
    }
}