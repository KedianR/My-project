using UnityEngine;
using UnityEngine.InputSystem;

namespace SafetyTraining.AR
{
    public class ARInteractionHandler : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("The camera to use for raycasting. If null, Camera.main will be used.")]
        [SerializeField] private Camera mainCamera;

        [Tooltip("Layer mask to filter which objects can be interacted with. Default is all layers.")]
        [SerializeField] private LayerMask interactableLayer = Physics.DefaultRaycastLayers;

        [Tooltip("Maximum distance to raycast for interactions.")]
        [SerializeField] private float maxInteractionDistance = 20f;

        void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        void Update()
        {
            // Use New Input System for touch
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                // --- UI PASSTHROUGH: never consume taps that land on a UI element ---
                int fingerId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(fingerId))
                    return;

                HandleInteraction(Touchscreen.current.primaryTouch.position.ReadValue());
            }
            // Add mouse support for easy testing in the editor
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Mouse UI passthrough check (pointer ID -1 for mouse)
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
                    return;

                HandleInteraction(Mouse.current.position.ReadValue());
            }
        }

        private void HandleInteraction(Vector2 screenPosition)
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance, interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    interactable.OnInteract(ActionType.Tap);
                    return;
                }
            }

            // Fallback: If screen tap misses the collider but an interactive target object for the current step is awaiting action, trigger it
            ScenarioEngine scenarioEngine = FindObjectOfType<ScenarioEngine>();
            if (scenarioEngine != null && scenarioEngine.CurrentState == ScenarioState.AWAITING_ACTION && scenarioEngine.activeScenario != null)
            {
                if (scenarioEngine.CurrentStepIndex < scenarioEngine.activeScenario.steps.Length)
                {
                    string targetID = scenarioEngine.activeScenario.steps[scenarioEngine.CurrentStepIndex].targetObjectID;
                    ARInteractiveObject[] allObjects = FindObjectsOfType<ARInteractiveObject>();
                    foreach (var obj in allObjects)
                    {
                        if (obj.ObjectID == targetID && obj.CanInteract())
                        {
                            Debug.Log($"[ARInteractionHandler] Screen tap fallback triggered interaction on target object: {targetID}");
                            obj.OnInteract(ActionType.Tap);
                            break;
                        }
                    }
                }
            }
        }
    }
}