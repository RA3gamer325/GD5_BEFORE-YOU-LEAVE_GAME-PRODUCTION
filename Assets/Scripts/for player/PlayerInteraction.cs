using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    private Interactable currentInteractable;
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        CheckInteraction();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void CheckInteraction()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                Interactable newInteractable = hit.collider.GetComponent<Interactable>();
                
                if (newInteractable != null && newInteractable.enabled && newInteractable != currentInteractable)
                {
                    SetNewCurrentInteractable(newInteractable);
                }
            }
            else
            {
                DisableCurrentInteractable();
            }
        }
        else
        {
            DisableCurrentInteractable();
        }
    }

    void SetNewCurrentInteractable(Interactable newInteractable)
    {
        currentInteractable = newInteractable;
        
        if (currentInteractable != null)
        {
            currentInteractable.EnableOutline();
            HUDcontroller.instance?.EnableInteractionText(currentInteractable.interactionText);
        }
    }

    void DisableCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.DisableOutline();
            currentInteractable = null;
        }
        
        HUDcontroller.instance?.DisableInteractionText();
    }
}