using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class FrameGenerator : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public TMP_Dropdown framingDropdown;
    public TMP_Text frameOutputText;
    public TMP_Text receiverOutputText;
    public TMP_InputField stuffingThresholdInput;

    [Header("Framing Settings")]
    public int stuffingThreshold = 5;

    [Header("Scene References")]
    public GameObject bitPrefab;
    public Transform channelStart;
    public Transform channelEnd;
    public float bitSpeed = 200f;
    public float bitSpacing = 5f;

    [Header("Stacking Settings")]
    public Transform frameStackStart;           // Base position for first frame stack
    public GameObject terminationBlockPrefab;   // Red block prefab
    public float frameVerticalSpacing = 1f;     // Vertical distance between frames

    [Header("Receiver Buffer")]
    public Transform receiverBufferPos;   // receiver slot
    public float receiverBitSpacing = 0.4f; // distance between stacked bits

    private List<GameObject> allFrameBits = new List<GameObject>();
    private List<GameObject> currentFrameBits = new List<GameObject>(); // NEW: class-level variable
    private float currentStackHeight = 0f;      // Tracks vertical stacking
    private List<GameObject> spawnedBits = new List<GameObject>();

    private string lastOriginalMessage;
    private string lastFramedBits;

    void Start()
    {
        // Hide stuffing input box at start
        stuffingThresholdInput.gameObject.SetActive(false);

        // Hook dropdown listener
        framingDropdown.onValueChanged.AddListener(OnFramingOptionChanged);
    }

    public void OnFramingOptionChanged(int optionIndex)
    {
        // If Bit Stuffing selected (index = 2)
        stuffingThresholdInput.gameObject.SetActive(optionIndex == 2);
    }

    // ---------------- BUTTON 1: GENERATE ----------------
    public void OnGenerateFrame()
{
    string userText = inputField.text;
    if (string.IsNullOrEmpty(userText))
    {
        frameOutputText.text = "⚠ Enter some text first!";
        return;
    }

    string binaryData = StringToBinary(userText);

    string framedData = "";
    switch (framingDropdown.value)
    {
        case 0: framedData = CharacterCountFraming(binaryData); break;
        case 1: framedData = ByteStuffingFraming(binaryData); break;
        case 2:
            if (!string.IsNullOrEmpty(stuffingThresholdInput.text))
            {
                int userValue;
                if (int.TryParse(stuffingThresholdInput.text, out userValue))
                    stuffingThreshold = Mathf.Max(1, userValue);
            }
            framedData = BitStuffingFraming(binaryData); break;
    }

    frameOutputText.text = "Frame Output:\n" + framedData;

    lastOriginalMessage = userText;
    lastFramedBits = framedData;

    // Clear previous currentFrameBits
    currentFrameBits.Clear();

    // Spawn bits at channel start (animation will move them)
    Vector3 spawnPos = channelStart.position;

    foreach (char bit in framedData)
    {
        GameObject bitObj = Instantiate(bitPrefab, spawnPos, Quaternion.identity);
        bitObj.GetComponentInChildren<TMP_Text>().text = bit.ToString();

        Renderer rend = bitObj.GetComponent<Renderer>();
        rend.material.color = (bit == '0') ? new Color(1f, 1f, 0.6f) : new Color(0.2f, 0.5f, 1f);

        currentFrameBits.Add(bitObj);
        allFrameBits.Add(bitObj);
    }

    // Termination block will be added after transfer, not now
    receiverOutputText.text = "Receiver Output: (waiting transfer)";
}


    // ---------------- BUTTON 2: TRANSFER ----------------
    public void OnTransferFrame()
    {
        // Transfer only newly generated frame
        spawnedBits = new List<GameObject>(currentFrameBits);

        if (spawnedBits.Count == 0)
        {
            frameOutputText.text += "\n⚠ No frame generated yet!";
            return;
        }

        StartCoroutine(TransferBits());
    }

    private System.Collections.IEnumerator TransferBits()
{
    // Move bits along the channel first
    for (int i = 0; i < currentFrameBits.Count; i++)
    {
        GameObject bitObj = currentFrameBits[i];

        Vector3 targetPos = channelEnd.position;
        while (Vector3.Distance(bitObj.transform.position, targetPos) > 0.05f)
        {
            bitObj.transform.position = Vector3.MoveTowards(bitObj.transform.position, targetPos, 5*bitSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(bitSpacing);
    }

    // Arrange bits horizontally for this frame at receiver
    Vector3 frameStartPos = frameStackStart.position + new Vector3(0, currentStackHeight, 0);
    float bitOffset = 0f;

    foreach (GameObject bitObj in currentFrameBits)
    {
        bitObj.transform.position = frameStartPos + new Vector3(bitOffset, 0, 0);
        bitOffset += receiverBitSpacing; // horizontal spacing
    }

    // Add termination block at the end of this frame
    Vector3 termPos = frameStartPos + new Vector3(bitOffset + terminationBlockPrefab.transform.localScale.x / 2f, 0, 0);
    GameObject termBlock = Instantiate(terminationBlockPrefab, termPos, Quaternion.identity);
    allFrameBits.Add(termBlock);

    // Update stack height for next frame (vertical shift)
    currentStackHeight += receiverBitSpacing + terminationBlockPrefab.transform.localScale.y;

    receiverOutputText.text = "Receiver Output: " + lastOriginalMessage;
}



    // ---------------- Utils ----------------
    private string StringToBinary(string data)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in data)
            sb.Append(System.Convert.ToString(c, 2).PadLeft(8, '0'));
        return sb.ToString();
    }

    private string CharacterCountFraming(string bits)
    {
        int count = bits.Length;
        string countField = System.Convert.ToString(count, 2).PadLeft(8, '0');
        return countField + bits;
    }

    private string ByteStuffingFraming(string bits)
    {
        string FLAG = "01111110";
        string ESC = "11100011";
        string stuffed = bits.Replace(ESC, ESC + ESC).Replace(FLAG, ESC + FLAG);
        return FLAG + stuffed + FLAG;
    }

    private string BitStuffingFraming(string bits)
    {
        string FLAG = "01111110";
        string stuffed = "";
        int ones = 0;

        foreach (char b in bits)
        {
            stuffed += b;
            if (b == '1')
            {
                ones++;
                if (ones == stuffingThreshold)
                {
                    stuffed += '0';
                    ones = 0;
                }
            }
            else ones = 0;
        }
        return FLAG + stuffed + FLAG;

    }
}
