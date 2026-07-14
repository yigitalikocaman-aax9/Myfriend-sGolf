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

            // ONEMLI DUZELTME:
            // World rotation'i degil, LOCAL rotation'i sifirla.
            // Boylece collider, parent'inin (arac govdesinin) o anki
            // yonelimine gore hizalanir; araba sahnede acili dursa bile
            // suspansiyon dogru yonde (parent'in "yukari" ekseninde) calisir.
            w.wheelCollider.transform.localRotation = Quaternion.identity;

            Debug.Log($"[WheelAligner] '{w.label}' hizalandi -> {worldCenter}");
        }

        Debug.Log("[WheelAligner] Tum tekerlekler hizalandi. Simdi Play'e basip test edebilirsin.");
    }

    [ContextMenu("Fix Wheel Pivots (Merkezi Sarmalayici Olustur)")]
    public void FixWheelPivots()
    {
        foreach (var w in wheels)
        {
            if (w.wheelMesh == null)
            {
                Debug.LogWarning($"[WheelAligner] '{w.label}' icin mesh atanmamis, atlaniyor.");
                continue;
            }

            Renderer rend = w.wheelMesh.GetComponentInChildren<Renderer>();
            if (rend == null)
            {
                Debug.LogWarning($"[WheelAligner] '{w.label}' icin Renderer bulunamadi.");
                continue;
            }

            // Meshin GERCEK gorsel merkezini hesapla (world space)
            Vector3 trueCenter = rend.bounds.center;

            // Mesh'in su anki parent'ini bul (yeni pivot'u onun altina, ayni seviyeye koyacagiz)
            Transform originalParent = w.wheelMesh.parent;

            // Yeni bos "pivot" objesini olustur, TAM merkeze yerlestir
            GameObject pivotGO = new GameObject($"Pivot_{w.label}");
            Undo.RegisterCreatedObjectUndo(pivotGO, "Create Wheel Pivot");
            pivotGO.transform.SetParent(originalParent, false);
            pivotGO.transform.position = trueCenter;
            pivotGO.transform.rotation = Quaternion.identity;

            // Asil mesh'i bu yeni pivot'un ALTINA al, DUNYA POZISYONUNU KORUYARAK
            // (worldPositionStays: true -> mesh oldugu yerde gorunmeye devam eder,
            // sadece parent'i degisir ve pivot merkeze tasinir)
            Undo.SetTransformParent(w.wheelMesh, pivotGO.transform, "Reparent Wheel Mesh");

            // WheelPair referansini artik yeni pivot'a guncelle,
            // CarController Inspector'ina BU objeyi (Pivot_XX) surukleyeceksin
            w.wheelMesh = pivotGO.transform;

            Debug.Log($"[WheelAligner] '{w.label}' icin Pivot_{w.label} olusturuldu ve merkeze yerlestirildi -> {trueCenter}. " +
                      $"CarController'daki Wheel Mesh alanina bu YENI objeyi (Pivot_{w.label}) surukle.");
        }

        Debug.Log("[WheelAligner] Pivot duzeltmesi tamamlandi. Simdi CarController Inspector'inda ilgili Wheel Mesh alanlarini yeni Pivot_XX objeleriyle guncelle.");
    }
#endif
}