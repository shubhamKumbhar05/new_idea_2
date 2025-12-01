using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TransmissionController : MonoBehaviour
{
    public TMP_InputField textInput;
    public TMP_Dropdown parityDropdown;
    public Toggle errorToggle;
    public TextMeshProUGUI originalBitsText;
    public TextMeshProUGUI senderBitsText;
    public TextMeshProUGUI receiverBitsText;
    public TextMeshProUGUI parityBitsText;
    public Transform sender;
    public Transform receiver;

    private string bitString = "";

    public void OnSendClicked()
    {
        if (string.IsNullOrEmpty(textInput.text))
        {
            Debug.Log("Enter text first!");
            return;
        }

        // Convert text to bits
        string bits = ConvertToBits(textInput.text);

        originalBitsText.text = "Original Bits: " + bits;

        // Apply parity
        string parityType = parityDropdown.value == 0 ? "Even" : "Odd";
        string bitsWithParity = ApplyParity(bits, parityType);

        // Show sender bits
        senderBitsText.text = "Sender Bits: " + bitsWithParity;

        // Simulate transfer (with or without error)
        StartCoroutine(Transfer(bitsWithParity));
    }

    string ConvertToBits(string text)
    {
        string result = "";
        foreach (char c in text)
        {
            result += System.Convert.ToString(c, 2).PadLeft(8, '0') + " ";
        }
        return result.Trim();
    }

    // string ApplyParity(string bits, string parityType)
    // {
    //     string[] bytes = bits.Split(' ');
    //     string result = "";

    //     foreach (string b in bytes)
    //     {
    //         int ones = 0;
    //         foreach (char bit in b)
    //             if (bit == '1') ones++;

    //         int parityBit = (parityType == "Even")
    //             ? (ones % 2 == 0 ? 0 : 1)
    //             : (ones % 2 == 0 ? 1 : 0);

    //         result += b + parityBit + " ";
    //     }

    //     return result.Trim();
    // }

    string ApplyParity(string bits, string parityType)
{
    string[] bytes = bits.Split(' ');
    string result = "";
    string parityBits = "";  // store only parity bits

    foreach (string b in bytes)
    {
        int ones = 0;
        foreach (char bit in b)
            if (bit == '1') ones++;

        int parityBit = (parityType == "Even")
            ? (ones % 2 == 0 ? 0 : 1)
            : (ones % 2 == 0 ? 1 : 0);

        parityBits += parityBit.ToString() + " ";
        result += b + parityBit + " ";
    }

    // display the parity bits used
    parityBitsText.text = "Parity Bits (" + parityType + "): " + parityBits.Trim();

    return result.Trim();
}


    System.Collections.IEnumerator Transfer(string bits)
    {
        yield return new WaitForSeconds(1f);

        string transmitted = bits;

        // If toggle ON → introduce random error
        if (errorToggle.isOn)
        {
            transmitted = IntroduceError(transmitted);
        }

        yield return new WaitForSeconds(1f);

        receiverBitsText.text = "Receiver Bits: " + transmitted;
    }

    string IntroduceError(string bits)
    {
        char[] chars = bits.ToCharArray();
        List<int> bitPositions = new List<int>();

        for (int i = 0; i < chars.Length; i++)
            if (chars[i] == '0' || chars[i] == '1')
                bitPositions.Add(i);

        if (bitPositions.Count > 0)
        {
            int randomIndex = bitPositions[Random.Range(0, bitPositions.Count)];
            chars[randomIndex] = chars[randomIndex] == '0' ? '1' : '0';
            Debug.Log($"Error introduced at bit {randomIndex}");
        }

        return new string(chars);
    }
}
