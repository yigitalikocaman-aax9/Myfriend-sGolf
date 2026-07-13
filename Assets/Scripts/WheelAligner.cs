using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Bu script, tekerlek collider objelerini otomatik olarak
// gorsel tekerlek meshinin tam merkezine hizalar.
// Elle surukleme geregi kalmaz.
public class WheelAligner : MonoBehaviour
{
    [System.Serializable]
    public class WheelPair
    {
        public string label;
        public Transform wheelMesh;      // Gorsel tekerlek (R:Tire_FL_C7M_Tires_Mesh...)
        public WheelCollider wheelCollider; // O tekerlegin WheelCollider'i (SolTeker vs.)
    }

    [Header("Hizalanacak tekerlek ciftleri")]
    public WheelPair[] wheels = new WheelPair[4];

#if UNITY_EDITOR
    [ContextMenu("Align All Wheel Colliders To Mesh Center")]
    public void AlignAllWheels()
    {
        foreach (var w in wheels)
        {
            if (w.wheelMesh == null || w.wheelCollider == null)
            {
                Debug.LogWarning($"[WheelAligner] '{w.label}' icin mesh veya collider atanmamis, atlaniyor.");
                continue;
            }

            Renderer rend = w.wheelMesh.GetComponentInChildren<Renderer>();
            if (rend == null)
            {
                Debug.LogWarning($"[WheelAligner] '{w.label}' icin Renderer bulunamadi.");
                continue;
            }

            // Meshin gercek gorsel merkezini (world space) hesapla
            Vector3 worldCenter = rend.bounds.center;

            // WheelCollider'in transformunu bu merkeze tasi
            Undo.RecordObject(w.wheelCollider.transform, "Align Wheel Collider");
            w.wheelCollider.transform.position = worldCenter;

            // Rotasyonu sifirla (WheelCollider dikey eksende calismali)
            w.wheelCollider.transform.rotation = Quaternion.identity;

            Debug.Log($"[WheelAligner] '{w.label}' hizalandi -> {worldCenter}");
        }

        Debug.Log("[WheelAligner] Tum tekerlekler hizalandi. Simdi Play'e basip test edebilirsin.");
    }
#endif
}