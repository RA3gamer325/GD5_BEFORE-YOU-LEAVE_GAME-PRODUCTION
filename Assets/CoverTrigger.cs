using UnityEngine;

public class CoverTrigger : MonoBehaviour
{
    [Header("Cover Settings")]
    public bool forceCrawl = true;
    
    [Header("Visual Debug")]
    public Color triggerColor = Color.red;
    public bool showGizmos = true;
    
    private PlayerMovement playerMovement;
    private bool playerInside = false;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null && forceCrawl)
            {
                playerInside = true;
                playerMovement.ForceCrawl(true);
                Debug.Log("🐛 Player entered cover - FORCED CRAWL!");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerMovement != null)
        {
            playerInside = false;
            playerMovement.ForceCrawl(false);  // 👈 CRITICAL: Explicitly disable
            Debug.Log("✅ Player exited cover - Can stand up!");
        }
    }

    void OnDrawGizmos()
{
    if (!showGizmos) return;

    Gizmos.color = triggerColor;

    // 🔥 Proper rotation-aware box
    Matrix4x4 rotationMatrix = Matrix4x4.TRS(
        transform.position,
        transform.rotation,
        transform.localScale
    );

    Gizmos.matrix = rotationMatrix;
    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

    // Reset matrix (VERY IMPORTANT)
    Gizmos.matrix = Matrix4x4.identity;

    // Optional crawl box preview
    Gizmos.color = Color.yellow;

    Matrix4x4 crawlMatrix = Matrix4x4.TRS(
        transform.position + Vector3.up * 0.3f,
        transform.rotation,
        new Vector3(transform.localScale.x, 0.6f, transform.localScale.z)
    );

    Gizmos.matrix = crawlMatrix;
    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

    Gizmos.matrix = Matrix4x4.identity;
}
}