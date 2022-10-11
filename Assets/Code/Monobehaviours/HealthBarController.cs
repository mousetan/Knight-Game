using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI tmpgui;

    private void Start()
    {
        //slider = GetComponent<Slider>();
        //tmpgui = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateMaxHP(float maxHP)
    {
        var oldMax = slider.maxValue;
        slider.maxValue = maxHP;
        tmpgui.text = "HP: " + slider.value.ToString("F0") + "/" + slider.maxValue.ToString("F0");
    }

    public void UpdateCurrentHP(float hp)
    {
        slider.value = hp;
        tmpgui.text = "HP: " + slider.value.ToString("F0") + "/" + slider.maxValue.ToString("F0");
    }

}
