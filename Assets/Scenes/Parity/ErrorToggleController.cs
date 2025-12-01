using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ErrorToggleController : MonoBehaviour
{
    public Toggle errorToggle;
    public TextMeshProUGUI toggleStatusText;

    void Start()
    {
        // Set default to "Without Error"
        errorToggle.isOn = false;
        UpdateToggleText(false);

        // Add listener to detect state change
        errorToggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(errorToggle);
        });
    }

    void ToggleValueChanged(Toggle toggle)
    {
        bool isErrorMode = toggle.isOn;
        UpdateToggleText(isErrorMode);

        // You can later use this flag in your transmission logic
        PlayerPrefs.SetInt("ErrorMode", isErrorMode ? 1 : 0);
    }

    void UpdateToggleText(bool isErrorMode)
    {
        if (toggleStatusText != null)
            toggleStatusText.text = isErrorMode ? "Mode: With Error" : "Mode: Without Error";
    }
}
