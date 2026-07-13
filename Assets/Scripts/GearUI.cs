using TMPro;
using UnityEngine;

public class GearUI : MonoBehaviour
{
    public CarController car;
    public TextMeshProUGUI gearText;

    void Update()
    {
        if (car.currentGear == CarController.Gear.Drive)
        {
            gearText.text = "Vites : D";
            gearText.color = Color.green;
        }
        else
        {
            gearText.text = "Vites : R";
            gearText.color = Color.red;
        }
    }
}