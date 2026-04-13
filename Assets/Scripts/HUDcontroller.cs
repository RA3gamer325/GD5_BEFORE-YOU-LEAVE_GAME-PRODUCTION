using UnityEngine;
using TMPro;

public class HUDcontroller : MonoBehaviour
{
    [SerializeField] private TMP_Text interactionText;
    public static HUDcontroller instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void EnableInteractionText(string text)
    {
        if (interactionText != null)
        {
            interactionText.text = text + " Press E";
            interactionText.gameObject.SetActive(true);
        }
    }

    public void DisableInteractionText()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}