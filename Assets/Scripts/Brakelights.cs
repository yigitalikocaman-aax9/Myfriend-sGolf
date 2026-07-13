using UnityEngine;

public class BrakeLights : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("The CarController script on the car, used to know when the brakes are active")]
    public CarController carController;

    [Header("Brake Light Objects")]
    [Tooltip("The GameObjects that visually represent the lit brake lights (e.g. a red glowing overlay on top of the taillights). They will be enabled while braking and disabled otherwise.")]
    public GameObject[] brakeLightObjects;

    void Update()
    {
        if (carController == null) return;

        bool shouldBeOn = carController.isBraking;

        foreach (var obj in brakeLightObjects)
        {
            if (obj != null && obj.activeSelf != shouldBeOn)
            {
                obj.SetActive(shouldBeOn);
            }
        }
    }
}
