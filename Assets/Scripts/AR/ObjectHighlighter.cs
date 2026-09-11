using UnityEngine;

namespace SafetyTraining.AR
{
    /// <summary>
    /// Applies an outline highlight to an ARInteractiveObject.
    /// SAFETY RULE: Hazards (MinorHazard / CriticalHazard) may NOT be highlighted
    /// before the ScenarioEngine enters AWAITING_ACTION for their specific step.
    /// Only force=true (post-interaction) bypasses this guard.
    /// </summary>
    [RequireComponent(typeof(ARInteractiveObject))]
    public class ObjectHighlighter : MonoBehaviour
    {
        [Header("Highlight Settings")]
        public Color highlightColor = Color.yellow;
        public float highlightWidth = 5f;

        [Header("UI Prefabs")]
        [Tooltip("Prefab to spawn above the object when highlighted (e.g., Hazard Icon)")]
        public GameObject hazardIndicatorPrefab;
        public Vector3 indicatorOffset = new Vector3(0, 0.5f, 0);

        private Outline outlineComponent;
        private GameObject spawnedIndicator;
        private ARInteractiveObject interactiveObject;

        private void Awake()
        {
            interactiveObject = GetComponent<ARInteractiveObject>();

            outlineComponent = GetComponent<Outline>();
            if (outlineComponent == null)
                outlineComponent = gameObject.AddComponent<Outline>();

            // Hazards must NEVER be highlighted at scene start
            outlineComponent.enabled = false;
            outlineComponent.OutlineColor = highlightColor;
            outlineComponent.OutlineWidth = highlightWidth;
        }

        /// <summary>
        /// Enables the outline highlight.
        /// <para>
        /// When <paramref name="force"/> is <c>false</c>: hazard objects are blocked from
        /// highlighting unless the ScenarioEngine is currently AWAITING_ACTION AND
        /// this object is the current step's target.
        /// </para>
        /// <para>
        /// When <paramref name="force"/> is <c>true</c>: called after the user has already
        /// taken an action, so the highlight is always applied to confirm the interaction.
        /// </para>
        /// </summary>
        public void EnableHighlight(bool force = false)
        {
            if (!force && interactiveObject != null)
            {
                ObjectState state = interactiveObject.GetCurrentState();
                bool isHazard = state == ObjectState.CriticalHazard || state == ObjectState.MinorHazard;

                if (isHazard)
                {
                    // FIX: Block highlight unless the engine IS in AWAITING_ACTION for this object.
                    // Previously the condition was inverted — it blocked highlight when state WAS
                    // AWAITING_ACTION, which is exactly when we WANT to allow it.
                    ScenarioEngine engine = FindFirstObjectByType<ScenarioEngine>();

                    if (engine == null || engine.CurrentState != ScenarioState.AWAITING_ACTION)
                    {
                        // Engine is not ready (e.g. PLACING, INSTRUCTION) — block highlight
                        return;
                    }

                    // Additional guard: only highlight if this object is the current step's target
                    if (engine.activeScenario != null &&
                        engine.CurrentStepIndex < engine.activeScenario.steps.Length)
                    {
                        string targetID = engine.activeScenario.steps[engine.CurrentStepIndex].targetObjectID;
                        if (interactiveObject.ObjectID != targetID)
                        {
                            return; // Not the current step's target — keep hidden
                        }
                    }
                    else
                    {
                        return; // No valid step data — play it safe and block
                    }
                }
            }

            if (outlineComponent != null)
                outlineComponent.enabled = true;
        }

        /// <summary>Removes the outline highlight.</summary>
        public void DisableHighlight()
        {
            if (outlineComponent != null)
                outlineComponent.enabled = false;
        }

        /// <summary>
        /// Spawns a world-space 3D indicator above the object (e.g., hazard warning icon).
        /// Guards against double-spawning.
        /// </summary>
        public void SpawnHazardIndicator()
        {
            if (hazardIndicatorPrefab != null && spawnedIndicator == null)
            {
                spawnedIndicator = Instantiate(
                    hazardIndicatorPrefab,
                    transform.position + indicatorOffset,
                    Quaternion.identity,
                    transform);
            }
        }

        /// <summary>Destroys the spawned indicator, if any.</summary>
        public void DespawnHazardIndicator()
        {
            if (spawnedIndicator != null)
            {
                Destroy(spawnedIndicator);
                spawnedIndicator = null;
            }
        }
    }
}
