using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    private Outline outline;
    private Animator animator;
    public UnityEvent OnInteraction;
    
    [Header("Animation Settings")]
    public bool isLever = false;
    public bool leverState = false;
    public string interactionText;

    void Start()
    {
        outline = GetComponent<Outline>();
        animator = GetComponent<Animator>();
        
        if (outline != null) outline.enabled = false;
        if (animator != null) animator.SetBool("isOn", leverState);
    }

    public void Interact()
    {
        leverState = !leverState;
        animator?.SetBool("isOn", leverState);
        OnInteraction?.Invoke();
    }

    public void SetLeverState(bool state)
    {
        leverState = state;
        animator?.SetBool("isOn", state);
    }

    public bool GetLeverState() => leverState;

    // Use enabled property instead of Enable() and Disable()
    public void EnableOutline()
    {
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    public void DisableOutline()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}