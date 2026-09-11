using UnityEngine;

namespace SafetyTraining.AR
{
    public class ARInteractiveObject : MonoBehaviour, IInteractable
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this interactive object in the scenario.")]
        [SerializeField] private string objectID;

        [Header("State Configuration")]
        [Tooltip("The current safety state of the object.")]
        [SerializeField] private ObjectState currentState = ObjectState.Safe;

        [Tooltip("The expected action a user should take on this object.")]
        [SerializeField] private ActionType expectedAction = ActionType.Tap;

        // Implement IInteractable Property
        public string ObjectID => objectID;

        // Expose ExpectedAction for other systems (like the Assessment Engine)
        public ActionType ExpectedAction => expectedAction;

        private bool isInteractable = true;

        public bool CanInteract()
        {
            return isInteractable;
        }

        [Header("Held in Hand Settings")]
        [Tooltip("Local position relative to the main camera when picked up.")]
        [SerializeField] private Vector3 heldPosition = new Vector3(0.25f, -0.3f, 0.55f);

        [Tooltip("Local rotation (Euler angles) relative to the main camera when picked up.")]
        [SerializeField] private Vector3 heldRotation = new Vector3(10f, -15f, 0f);

        [Tooltip("Enable to set a custom scale when picked up in hand.")]
        [SerializeField] private bool useCustomHeldScale = false;

        [Tooltip("Custom local scale when held in hand.")]
        [SerializeField] private Vector3 heldScale = Vector3.one;

        private bool isHeldInHand = false;

        public void AttachToHand()
        {
            if (isHeldInHand) return;

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                transform.SetParent(mainCam.transform);
                transform.localPosition = heldPosition;
                transform.localRotation = Quaternion.Euler(heldRotation);
                if (useCustomHeldScale)
                {
                    transform.localScale = heldScale;
                }
                isHeldInHand = true;
                Debug.Log($"[ARInteractiveObject] '{objectID}' attached to Camera (held in hand).");
            }
        }

        public void OnInteract(ActionType action)
        {
            if (!isInteractable) return;

            // (#2) Find the ScenarioEngine — only process interaction if we are awaiting one
            ScenarioEngine scenarioEngine = FindObjectOfType<ScenarioEngine>();
            if (scenarioEngine == null || scenarioEngine.CurrentState != ScenarioState.AWAITING_ACTION)
            {
                Debug.Log($"[ARInteractiveObject] Interaction on '{objectID}' ignored — not in AWAITING_ACTION state.");
                return;
            }

            // ATTACH TO HAND ON PICK UP (fire_extinguisher)
            if (objectID == "fire_extinguisher" && !isHeldInHand)
            {
                AttachToHand();
            }

            // INTERCEPT STEP 8 (Trigger Foam Spray)
            if (scenarioEngine.activeScenario != null && scenarioEngine.CurrentStepIndex < scenarioEngine.activeScenario.steps.Length)
            {
                if (scenarioEngine.activeScenario.steps[scenarioEngine.CurrentStepIndex].stepID == "step_08_squeeze_handle")
                {
                    Transform spray = transform.Find("FoamSprayVFX");
                    if (spray != null)
                    {
                        var ps = spray.GetComponent<ParticleSystem>();
                        if (ps != null) ps.Play();
                    }
                }
            }

            // Determine if this is the correct action on the correct object
            bool isCorrect = (action == expectedAction);

            Debug.Log($"[ARInteractiveObject] '{objectID}' received action: {action}. Expected: {expectedAction}. Correct: {isCorrect}");

            // (#2) Log the action to LocalEventLogger
            if (LocalEventLogger.Instance != null)
            {
                LocalEventLogger.Instance.LogAction(
                    workerID: "worker_01",
                    objectID: objectID,
                    actionTaken: action,
                    isCorrect: isCorrect,
                    wasCritical: (currentState == ObjectState.CriticalHazard)
                );
            }

            // Lock this object so it cannot be interacted with again in this step
            isInteractable = false;

            // (#2) Notify ScenarioEngine to validate and advance to the next step
            scenarioEngine.ChangeState(ScenarioState.VALIDATION);
        }

        public ObjectState GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// Allows external systems (like the ScenarioEngine) to update this object's state.
        /// </summary>
        public void SetState(ObjectState newState)
        {
            currentState = newState;
            // Here you could add logic to change visuals (e.g., swapping materials) based on state.
        }

        /// <summary>
        /// Allows external systems to lock/unlock interactions.
        /// </summary>
        public void SetInteractable(bool state)
        {
            isInteractable = state;
        }
    }
}
