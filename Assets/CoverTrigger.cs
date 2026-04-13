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
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        Gizmos.color = Color.yellow;
        Vector3 crawlBox = new Vector3(transform.localScale.x, 0.6f, transform.localScale.z);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.3f, crawlBox);
    }
}