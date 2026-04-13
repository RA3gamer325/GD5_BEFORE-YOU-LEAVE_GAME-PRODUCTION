using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    [Header("Components")]
    public Transform doorTransform;
    
    [Header("Transform Settings")]
    public Vector3 openPosition = new Vector3(0, 5, 0);
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public Vector3 openScale = Vector3.one;
    
    [Header("Interaction")]
    [SerializeField] private float moveSpeed = 2f;
    [TextArea(2, 4)]
    public string interactionTextClosed = "Press E to open door";
    public string interactionTextOpen = "Press E to close door";
    
    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    
    [Header("Debug")]
    public bool enableDebug = true;

    private Interactable interactable;
    private AudioSource audioSource;
    private bool isOpen = false;
    private Vector3 startPosition;
    private Vector3 startRotation;
    private Vector3 startScale;
    private string currentInteractionText;

    void Start()
    {
        // Auto-setup
        if (doorTransform == null) doorTransform = transform;
        interactable = GetComponent<Interactable>();
        audioSource = GetComponent<AudioSource>();
        
        if (interactable == null)
        {
            DebugLog("❌ No Interactable component found! Adding one...");
            interactable = gameObject.AddComponent<Interactable>();
            interactable.interactionText = interactionTextClosed;
        }
        
        // Store starting transform
        startPosition = doorTransform.localPosition;
        startRotation = doorTransform.localEulerAngles;
        startScale = doorTransform.localScale;
        
        // Hook into Interactable events
        interactable.OnTurnOn.AddListener(OpenDoor);
        interactable.OnTurnOff.AddListener(CloseDoor);
        
        UpdateInteractionText();
        DebugLog("✅ InteractableDoor Ready!");
    }

    public void OpenDoor()
    {
        if (isOpen) return;
        StartCoroutine(OpenDoorCoroutine());
    }

    public void CloseDoor()
    {
        if (!isOpen) return;
        StartCoroutine(CloseDoorCoroutine());
    }

    System.Collections.IEnumerator OpenDoorCoroutine()
    {
        isOpen = true;
        PlaySound(openSound);
        UpdateInteractionText();
        
        float elapsed = 0f;
        Vector3 startPos = doorTransform.localPosition;
        Vector3 startRot = doorTransform.localEulerAngles;
        Vector3 startScale = doorTransform.localScale;
        
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * moveSpeed;
            
            doorTransform.localPosition = Vector3.Lerp(startPos, openPosition, elapsed);
            doorTransform.localEulerAngles = Vector3.Lerp(startRot, openRotation, elapsed);
            doorTransform.localScale = Vector3.Lerp(startScale, openScale, elapsed);
            
            yield return null;
        }
        
        DebugLog("🚪 Door Opened!");
    }

    System.Collections.IEnumerator CloseDoorCoroutine()
    {
        isOpen = false;
        PlaySound(closeSound);
        UpdateInteractionText();
        
        float elapsed = 0f;
        Vector3 startPos = doorTransform.localPosition;
        Vector3 startRot = doorTransform.localEulerAngles;
        Vector3 startScale = doorTransform.localScale;
        
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * moveSpeed;
            
            doorTransform.localPosition = Vector3.Lerp(startPos, startPosition, elapsed);
            doorTransform.localEulerAngles = Vector3.Lerp(startRot, startRotation, elapsed);
            doorTransform.localScale = Vector3.Lerp(startScale, startScale, elapsed);
            
            yield return null;
        }
        
        DebugLog("🚪 Door Closed!");
    }

    void UpdateInteractionText()
    {
        if (interactable != null)
        {
            interactable.interactionText = isOpen ? interactionTextOpen : interactionTextClosed;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void DebugLog(string message)
    {
        if (enableDebug)
            Debug.Log($"[InteractableDoor: {gameObject.name}] {message}", this);
    }

    // Public API
    public void SetOpen(bool open)
    {
        if (open) OpenDoor();
        else CloseDoor();
    }

    public bool IsOpen() => isOpen;
}