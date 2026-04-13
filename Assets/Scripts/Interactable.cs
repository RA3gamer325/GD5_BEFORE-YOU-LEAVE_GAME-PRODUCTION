using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Components")]
    private Animator animator;
    private AudioSource audioSource;

    [Header("Events")]
    public UnityEvent OnInteraction;
    public UnityEvent OnTurnOn;
    public UnityEvent OnTurnOff;

    [Header("Switch Settings")]
    public bool isToggle = true;
    [SerializeField] private bool leverState = false;
    
    [Header("Interaction")]
    [SerializeField] private float interactionCooldown = 0.1f;
    private float lastInteractionTime = 0f;

    [Header("Audio")]
    public AudioClip onSound;
    public AudioClip offSound;
    public float volume = 0.7f;

    [Header("UI")]
    public TMPro.TextMeshProUGUI stateText;
    public string onText = "ON";
    public string offText = "OFF";

    [Header("Interaction UI")]
    [TextArea(2, 4)]
    public string interactionText = "Press E to interact";

    [Header("Debug")]
    public bool enableDebug = true;

    void Start()
    {
        GetComponents();
        InitializeState();
        DebugLog("✅ Interactable Ready!");
    }

    void GetComponents()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void InitializeState()
    {
        UpdateVisualState();
    }

    public void Interact()
    {
        if (Time.time < lastInteractionTime + interactionCooldown) return;
        
        lastInteractionTime = Time.time;
        DebugLog("👆 Interacted!");

        bool previousState = leverState;
        
        // Toggle Logic
        if (isToggle)
        {
            leverState = !leverState;
        }
        else
        {
            if (!leverState) leverState = true;
            else return;
        }

        UpdateVisualState();

        // Events
        TriggerStateEvents(previousState);
        OnInteraction?.Invoke();
    }

    void UpdateVisualState()
    {
        // Animation
        if (animator != null)
            animator.SetBool("isOn", leverState);

        // Audio
        PlaySound();

        // UI Text
        UpdateUIText();

        DebugLog($"🔄 State: {(leverState ? "ON" : "OFF")}");
    }

    void PlaySound()
    {
        if (audioSource == null) return;

        AudioClip clip = leverState ? onSound : offSound;
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }

    void UpdateUIText()
    {
        if (stateText != null)
            stateText.text = leverState ? onText : offText;
    }

    void TriggerStateEvents(bool previousState)
    {
        if (leverState && !previousState)
        {
            DebugLog("🟢 ON Event!");
            OnTurnOn?.Invoke();
        }
        else if (!leverState && previousState)
        {
            DebugLog("🔴 OFF Event!");
            OnTurnOff?.Invoke();
        }
    }

    // Public API
    public void SetState(bool state) => SetLeverState(state, true);
    public void SetLeverState(bool state, bool triggerEvents = true)
    {
        bool previous = leverState;
        leverState = state;
        UpdateVisualState();

        if (triggerEvents)
            TriggerStateEvents(previous);
    }

    public bool GetState() => leverState;

    private void DebugLog(string message)
    {
        if (enableDebug)
            Debug.Log($"[Interactable: {gameObject.name}] {message}", this);
    }
}