using UnityEngine;
using UnityEngine.UI;  // <- required for Text
using TMPro;

public class HTTPFlow : MonoBehaviour
{
    public Transform client, server, packet, beam;
    [SerializeField] private TMP_Text statusText;
       // <- public UnityEngine.UI.Text

    private float speed = 2f;
    private bool goingToServer = true;
    private bool https = false;

    void Update()
    {
        // movement logic (same as before)
        if (goingToServer)
            packet.position = Vector3.MoveTowards(packet.position, server.position, speed * Time.deltaTime);
        else
            packet.position = Vector3.MoveTowards(packet.position, client.position, speed * Time.deltaTime);

        if (Vector3.Distance(packet.position, server.position) < 0.1f && goingToServer)
        {
            goingToServer = false;
            packet.GetComponent<Renderer>().material.color = Color.red;
            if (statusText != null) statusText.text = "Response: 200 OK";
        }
        else if (Vector3.Distance(packet.position, client.position) < 0.1f && !goingToServer)
        {
            goingToServer = true;
            packet.GetComponent<Renderer>().material.color = Color.green;
            if (statusText != null) statusText.text = "Request: GET /index.html";
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            https = !https;
            var beamMat = beam.GetComponent<Renderer>().material;
            beamMat.color = https ? new Color(0, 0.6f, 1f, 0.6f) : new Color(0.2f, 0.2f, 0.2f, 0.4f);
            if (statusText != null) statusText.text = https ? "Protocol: HTTPS (Encrypted)" : "Protocol: HTTP";
        }
    }
}
