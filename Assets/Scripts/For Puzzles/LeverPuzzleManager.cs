using UnityEngine;
using UnityEngine.Events;

public class LeverPuzzle : MonoBehaviour
{
    [Header("Levers")]
    public Interactable[] levers = new Interactable[3];
    
    [Header("Correct Sequence")]
    public bool[] correctSequence = { true, false, true }; // ON, OFF, ON
    
    [Header("Puzzle Settings")]
    public float solveDelay = 2f;
    
    [Header("Events")]
    public UnityEvent OnPuzzleSolved;
    public UnityEvent OnPuzzleReset;
    
    [Header("Audio")]
    public AudioClip successSound;
    public AudioClip failSound;
    
    [Header("UI")]
    public TMPro.TextMeshProUGUI puzzleText;
    public string waitingText = "Activate the levers in the correct order...";
    public string solvedText = "PUZZLE SOLVED!";
    public string resetText = "Reset - Try again!";
    
    private AudioSource audioSource;
    private int currentStep = 0;
    private bool puzzleSolved = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        ResetPuzzle();
    }

    public void OnLeverActivated(Interactable lever)
    {
        if (puzzleSolved) return;

        // Find which lever was activated
        int leverIndex = System.Array.IndexOf(levers, lever);
        if (leverIndex == -1) return;

        // Check if correct state for this step
        if (lever.GetState() == correctSequence[currentStep])
        {
            currentStep++;
            
            if (currentStep >= levers.Length)
            {
                SolvePuzzle();
            }
            else
            {
                UpdateUIText(waitingText);
                PlaySound(successSound);
            }
        }
        else
        {
            FailPuzzle();
        }
    }

    void SolvePuzzle()
    {
        puzzleSolved = true;
        UpdateUIText(solvedText);
        PlaySound(successSound);
        OnPuzzleSolved?.Invoke();
        
        // Optional: Lock levers after solve
        foreach (var lever in levers)
        {
            lever.enabled = false;
        }
    }

    void FailPuzzle()
    {
        ResetPuzzle();
        PlaySound(failSound);
    }

    void ResetPuzzle()
    {
        currentStep = 0;
        puzzleSolved = false;
        UpdateUIText(waitingText);
        OnPuzzleReset?.Invoke();
    }

    void UpdateUIText(string text)
    {
        if (puzzleText != null)
            puzzleText.text = text;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    // Public reset method (for buttons/doors/etc)
    public void Reset()
    {
        ResetPuzzle();
        foreach (var lever in levers)
        {
            lever.enabled = true;
            lever.SetState(false);
        }
    }

    // Check if puzzle is solved
    public bool IsSolved() => puzzleSolved;
}