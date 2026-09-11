using UnityEngine;
using SafetyTraining.AR; // Using the namespace we saw in ARPlacementController

public enum ScenarioState
{
    IDLE,
    PLACING,
    PLACED,   // Warehouse placed — waiting for "Begin" dialog confirmation
    READY,
    INSTRUCTION,
    AWAITING_ACTION,
    VALIDATION,
    COMPLETE
}

public class ScenarioEngine : MonoBehaviour
{
    [Header("Scenario Data")]
    public TrainingScenarioData activeScenario;

    [Header("AR References")]
    [Tooltip("Reference to handle placement logic")]
    public ARPlacementController placementController;
    
    // Using Behaviour so we can toggle .enabled even if we don't know the exact namespace/class name right now
    [Tooltip("Reference to handle interactions (ARInteractionHandler)")]
    public Behaviour interactionHandler; 

    public ScenarioState CurrentState { get; private set; }

    // UI Events
    public event System.Action<ScenarioState> OnStateChanged;
    public event System.Action<string> OnInstructionUpdated;
    // Fired when the warehouse is placed — UIManager listens to show the Begin dialog
    public event System.Action OnWarehousePlacedEvent;

    public int CurrentStepIndex { get; private set; } = 0;

    private void Start()
    {
        ChangeState(ScenarioState.IDLE);
    }

    public void ChangeState(ScenarioState newState)
    {
        CurrentState = newState;
        Debug.Log($"[ScenarioEngine] State Changed to: {newState}");
        OnStateChanged?.Invoke(newState);

        switch (newState)
        {
            case ScenarioState.IDLE:
                HandleIdleState();
                break;
            case ScenarioState.PLACING:
                HandlePlacingState();
                break;
            case ScenarioState.PLACED:
                // Just wait — UIManager will show the Begin dialog
                break;
            case ScenarioState.READY:
                HandleReadyState();
                break;
            case ScenarioState.INSTRUCTION:
                HandleInstructionState();
                break;
            case ScenarioState.AWAITING_ACTION:
                HandleAwaitingActionState();
                break;
            case ScenarioState.VALIDATION:
                HandleValidationState();
                break;
            case ScenarioState.COMPLETE:
                HandleCompleteState();
                break;
        }
    }

    private void HandleIdleState()
    {
        // For the MVP, automatically transition to placing when we start
        ChangeState(ScenarioState.PLACING);
    }

    private void HandlePlacingState()
    {
        if (placementController != null)
        {
            placementController.enabled = true;
        }
        
        // Wait here. ARPlacementController will call OnWarehousePlaced() once the floor is tapped.
    }

    /// <summary>
    /// Called by ARPlacementController after the warehouse is moved to the AR tap point.
    /// Transitions to PLACED and fires the event so UIManager shows the Begin dialog.
    /// </summary>
    public void OnWarehousePlaced()
    {
        // No dialog — start immediately when the warehouse is placed
        Debug.Log("[ScenarioEngine] Warehouse placed — starting scenario immediately.");
        BeginScenario();
    }

    /// <summary>
    /// Called by the Begin button on the dialog. Starts the scenario.
    /// </summary>
    public void BeginScenario()
    {
        ChangeState(ScenarioState.READY);
    }

    private void HandleReadyState()
    {
        // Scenario is placed. Prepare to show first instruction.
        CurrentStepIndex = 0;
        ChangeState(ScenarioState.INSTRUCTION);
    }

    private void HandleInstructionState()
    {
        if (activeScenario == null || activeScenario.steps == null || CurrentStepIndex >= activeScenario.steps.Length)
        {
            ChangeState(ScenarioState.COMPLETE);
            return;
        }

        TrainingStepData currentStep = activeScenario.steps[CurrentStepIndex];
        
        // Update UI Canvas with currentStep.instructionText
        Debug.Log($"[ScenarioEngine] INSTRUCTION: {currentStep.instructionText}");
        OnInstructionUpdated?.Invoke(currentStep.instructionText);

        // Sequence interactivity: Enable only the target object for this step
        ARInteractiveObject[] allObjects = FindObjectsByType<ARInteractiveObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            bool isTarget = (obj.ObjectID == currentStep.targetObjectID);
            obj.SetInteractable(isTarget);
            
            // Specific logic for fire states
            if (obj.ObjectID == "fire_suppressed_zone")
            {
                obj.gameObject.SetActive(currentStep.targetObjectID == "fire_suppressed_zone");
            }
            if (obj.ObjectID == "fire_hazard" && currentStep.targetObjectID == "fire_suppressed_zone")
            {
                obj.gameObject.SetActive(false); // Hide the fire when suppressed
            }
        }

        ChangeState(ScenarioState.AWAITING_ACTION);
    }

    private void HandleAwaitingActionState()
    {
        // Enable interactions so the worker can tap/inspect objects
        if (interactionHandler != null)
        {
            interactionHandler.enabled = true;
        }
        
        // Wait here. ARInteractionHandler will report back, triggering VALIDATION.
    }

    private void HandleValidationState()
    {
        // We will validate the action here against activeScenario.steps[currentStepIndex]
        // And call LocalEventLogger.Instance.LogAction(...)
        
        // Disable interactions during validation
        if (interactionHandler != null)
        {
            interactionHandler.enabled = false;
        }

        CurrentStepIndex++;
        
        if (CurrentStepIndex >= activeScenario.steps.Length)
        {
            ChangeState(ScenarioState.COMPLETE);
        }
        else
        {
            ChangeState(ScenarioState.INSTRUCTION);
        }
    }

    private void HandleCompleteState()
    {
        Debug.Log("[ScenarioEngine] Scenario Complete!");
        
        if (interactionHandler != null) interactionHandler.enabled = false;
        if (placementController != null) placementController.enabled = false;
        
        // Save logs to the Android device storage
        if (LocalEventLogger.Instance != null)
        {
            LocalEventLogger.Instance.SaveLogsToJSON();
        }

        // Automatically trigger offline assessment and log results without needing UI
        AssessmentEngine assessmentEngine = FindObjectOfType<AssessmentEngine>();
        if (assessmentEngine != null)
        {
            assessmentEngine.TriggerEvaluation();
        }
    }
}
