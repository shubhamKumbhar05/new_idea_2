using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BitNode : MonoBehaviour
{
    Renderer rend;
    Color colorOne = new Color(0.0f, 0.9f, 0.9f);   // cyan
    Color colorZero = new Color(0.6f, 0.6f, 0.6f);  // gray
    Color colorParity = new Color(0.25f, 0.5f, 1.0f);// blue
    Color colorError = new Color(1.0f, 0.25f, 0.25f);// red

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void SetBitVisual(char bit, bool isParity = false)
    {
        if (isParity) rend.material.color = colorParity;
        else if (bit == '1') rend.material.color = colorOne;
        else rend.material.color = colorZero;
    }

    public void MarkFlipped()
    {
        rend.material.color = colorError;
    }
}
