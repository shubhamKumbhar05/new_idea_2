using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class HTTPBeamController : MonoBehaviour
{
    [Header("References (drag in Inspector)")]
    [SerializeField] private Transform clientTransform;
    [SerializeField] private Transform serverTransform;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject packetPrefab;   // small sphere or cube
    [SerializeField] private TMP_Text infoText;

    [Header("Visuals")]
    [SerializeField] private Color httpColor = new Color(1f, 0.15f, 0.15f, 1f);   // red-ish
    [SerializeField] private Color httpsColor = new Color(0f, 0.8f, 0.2f, 1f);    // green-ish
    [SerializeField] private float lineWidth = 0.06f;

    [Header("Animation")]
    [SerializeField] private float requestDuration = 1.2f;   // time client -> server
    [SerializeField] private float responseDuration = 1.0f;  // time server -> client
    [SerializeField] private bool useHTTPS = true;

    private GameObject packetInstance;
    private bool isRunning = false;

    void Reset()
    {
        // try to auto-assign LineRenderer if not set
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
    }

    void Awake()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

        // make sure line renderer is setup
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.enabled = false;

        // create packet if prefab provided
        if (packetPrefab != null)
        {
            packetInstance = Instantiate(packetPrefab);
            packetInstance.SetActive(false);
        }
    }

    void Start()
    {
        // validate
        if (clientTransform == null || serverTransform == null)
            Debug.LogWarning("[HTTPBeamController] Client or Server transform is not assigned.");

        UpdateLinePositions(); // set initial positions
        UpdateInfo("Ready");
    }

    void UpdateLinePositions()
    {
        if (clientTransform == null || serverTransform == null) return;
        lineRenderer.SetPosition(0, clientTransform.position);
        lineRenderer.SetPosition(1, serverTransform.position);
    }

    // Public interface to start a single request-response cycle
    public void StartRequestResponseCycle(bool https)
    {
        if (isRunning) return;
        useHTTPS = https;
        StartCoroutine(RequestResponseCoroutine());
    }

    private IEnumerator RequestResponseCoroutine()
    {
        if (clientTransform == null || serverTransform == null)
        {
            Debug.LogError("[HTTPBeamController] Missing transforms.");
            yield break;
        }

        isRunning = true;
        lineRenderer.enabled = true;
        lineRenderer.startColor = useHTTPS ? httpsColor : httpColor;
        lineRenderer.endColor = useHTTPS ? httpsColor : httpColor;

        // show DNS step if you want (brief)
        UpdateInfo("DNS: resolving domain...");
        yield return new WaitForSeconds(0.8f);

        // TCP handshake animation (two quick small pings)
        UpdateInfo("TCP: 3-way handshake (SYN, SYN-ACK, ACK)...");
        yield return StartCoroutine(PlayHandshakePings());

        // TLS handshake if https
        if (useHTTPS)
        {
            UpdateInfo("TLS: performing handshake (certificate exchange)...");
            yield return StartCoroutine(PlayTLSHandshake());
        }

        // Send request (client -> server)
        UpdateInfo(useHTTPS ? "HTTPS: Sending encrypted GET /index.html" : "HTTP: Sending GET /index.html");
        yield return StartCoroutine(MovePacket(clientTransform.position, serverTransform.position, requestDuration));

        // Simulate server processing
        UpdateInfo("Server: processing request...");
        // flash server (brief)
        yield return StartCoroutine(FlashObject(serverTransform.gameObject, 0.25f, 2));

        // Send response (server -> client)
        UpdateInfo("Server: responding with HTTP/1.1 200 OK");
        yield return StartCoroutine(MovePacket(serverTransform.position, clientTransform.position, responseDuration));

        // finalize
        UpdateInfo("Transaction complete. Connection keep-alive.");
        lineRenderer.enabled = false;
        if (packetInstance) packetInstance.SetActive(false);
        isRunning = false;
    }

    private IEnumerator PlayHandshakePings()
    {
        // brief visualization: ping dot from client to server and back twice
        float pingTime = 0.12f;
        int repeats = 2;
        for (int i = 0; i < repeats; i++)
        {
            yield return StartCoroutine(MovePacket(clientTransform.position, serverTransform.position, pingTime, smallPacket: true));
            yield return StartCoroutine(MovePacket(serverTransform.position, clientTransform.position, pingTime, smallPacket: true));
        }
    }

    private IEnumerator PlayTLSHandshake()
    {
        // visual: particle swirl or short pause + lock icon toggle (you can add your lock icon logic)
        UpdateInfo("TLS: server sends certificate...");
        yield return new WaitForSeconds(0.6f);
        UpdateInfo("TLS: exchanging keys...");
        yield return new WaitForSeconds(0.6f);
        UpdateInfo("TLS: secure channel established.");
        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator MovePacket(Vector3 startPos, Vector3 endPos, float duration, bool smallPacket = false)
    {
        if (packetInstance == null)
        {
            // fallback: create a temporary sphere
            packetInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            packetInstance.transform.localScale = smallPacket ? Vector3.one * 0.08f : Vector3.one * 0.18f;
            var r = packetInstance.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Standard"));
        }

        packetInstance.SetActive(true);
        packetInstance.transform.position = startPos;
        float elapsed = 0f;

        // set packet color based on direction (request green, response cyan/red)
        var rend = packetInstance.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = smallPacket ? Color.yellow : (startPos == clientTransform.position ? Color.green : Color.cyan);
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // position along straight line
            packetInstance.transform.position = Vector3.Lerp(startPos, endPos, t);

            // update line visually so it looks like "beam" moving: set end point to packet
            // keep start fixed at client; show partial line from client to packet (for request)
            if (startPos == clientTransform.position)
            {
                lineRenderer.SetPosition(0, clientTransform.position);
                lineRenderer.SetPosition(1, packetInstance.transform.position);
            }
            else // for response show packet nearing client but line from server to packet
            {
                lineRenderer.SetPosition(0, serverTransform.position);
                lineRenderer.SetPosition(1, packetInstance.transform.position);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ensure final position
        packetInstance.transform.position = endPos;
        // reset line to full segment after arrival briefly
        lineRenderer.SetPosition(0, clientTransform.position);
        lineRenderer.SetPosition(1, serverTransform.position);

        yield return new WaitForSeconds(0.05f);
    }

    private IEnumerator FlashObject(GameObject obj, float flashDuration, int flashes)
    {
        var rend = obj.GetComponent<Renderer>();
        if (rend == null) yield break;
        Color orig = rend.material.color;
        for (int i = 0; i < flashes; i++)
        {
            rend.material.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
            rend.material.color = orig;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private void UpdateInfo(string s)
    {
        if (infoText != null) infoText.text = s;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // draw line in editor between client & server to help placement
        if (clientTransform != null && serverTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(clientTransform.position, serverTransform.position);
        }
    }
#endif
}
