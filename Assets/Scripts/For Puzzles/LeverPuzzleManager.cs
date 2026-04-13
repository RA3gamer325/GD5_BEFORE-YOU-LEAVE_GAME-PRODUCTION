using UnityEngine;
using UnityEngine.Events;

public class LeverPuzzleManager : MonoBehaviour
{
    [Header("Assign Your Levers Here")]
    public Interactable[] allLevers;

    [Header("Correct Combination")]
    public bool[] correctStates;

    [Header("Animation Settings")]
    public string leverBoolName = "isOn";

    private bool puzzleSolved = false;
    private bool puzzleFailed = false;

    private void Start()
    {
        if (allLevers != null)
        {
            foreach (Interactable lever in allLevers)
            {
                if (lever != null)
                {
                    lever.OnInteraction.AddListener(CheckPuzzle);
                }
            }
        }

        Debug.Log("=== Lever Puzzle System Started ===");
        Debug.Log($"Total Levers: {allLevers?.Length ?? 0}");
        Debug.Log($"Correct States: {correctStates?.Length ?? 0}");
    }

    public void CheckPuzzle()
    {
        if (puzzleSolved)
        {
            Debug.Log("Puzzle Already Solved!");
            return;
        }

        if (puzzleFailed)
        {
            Debug.Log("Puzzle Failed - Resetting...");
            ResetAllLeverAnimations();
            puzzleFailed = false;
            return;
        }

        if (allLevers != null && correctStates != null && allLevers.Length == correctStates.Length)
        {
            bool allCorrect = true;

            for (int i = 0; i < allLevers.Length; i++)
            {
                if (allLevers[i] != null)
                {
                    bool currentState = allLevers[i].GetLeverState();
                    bool expectedState = correctStates[i];
                    
                    Debug.Log($"Lever {i + 1}: Current={currentState}, Expected={expectedState}");
                    
                    if (currentState != expectedState)
                    {
                        allCorrect = false;
                    }
                }
            }

            if (allCorrect)
            {
                SolvePuzzle();
            }
            else
            {
                FailPuzzle();
            }
        }
        else
        {
            Debug.LogError("Lever count doesn't match correct states count!");
        }
    }

    void SolvePuzzle()
    {
        puzzleSolved = true;
        Debug.Log("========================================");
        Debug.Log("🎉 PUZZLE SOLVED! 🎉");
        Debug.Log("========================================");
        Debug.Log("All levers are in correct positions!");
    }

    void FailPuzzle()
    {
        puzzleFailed = true;
        Debug.Log("========================================");
        Debug.Log("❌ PUZZLE FAILED ❌");
        Debug.Log("========================================");
        Debug.Log("Resetting all lever animations...");
        
        ResetAllLeverAnimations();
        
        Debug.Log("All levers reset to OFF state.");
    }

    public void ResetAllLeverAnimations()
    {
        if (allLevers != null)
        {
            foreach (Interactable lever in allLevers)
            {
                if (lever != null)
                {
                    lever.SetLeverState(false);
                    Debug.Log($"Lever reset to OFF");
                }
            }
        }
    }

    public void ResetPuzzleManually()
    {
        puzzleSolved = false;
        puzzleFailed = false;
        ResetAllLeverAnimations();
        Debug.Log("=== Puzzle Reset Manually ===");
    }

    void ResetPuzzle()
    {
        puzzleFailed = false;
        Debug.Log("=== Auto Reset After 3 Seconds ===");
    }

    private void OnDestroy()
    {
        if (allLevers != null)
        {
            foreach (Interactable lever in allLevers)
            {
                if (lever != null)
                {
                    lever.OnInteraction.RemoveListener(CheckPuzzle);
                }
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== Lever Puzzle Debug ===");
        
        if (allLevers != null)
        {
            for (int i = 0; i < allLevers.Length; i++)
            {
                if (allLevers[i] != null)
                {
                    GUILayout.Label($"Lever {i + 1}: {allLevers[i].GetLeverState()}");
                }
            }
        }
        
        GUILayout.Label($"Puzzle Solved: {puzzleSolved}");
        GUILayout.Label($"Puzzle Failed: {puzzleFailed}");
        
        if (GUILayout.Button("Reset Puzzle"))
        {
            ResetPuzzleManually();
        }
        
        GUILayout.EndArea();
    }
}