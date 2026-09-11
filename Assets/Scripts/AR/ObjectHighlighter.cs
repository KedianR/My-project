using UnityEngine;

namespace SafetyTraining.AR
{
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
            {
                outlineComponent = gameObject.AddComponent<Outline>();
            }
            
            // Start disabled
            outlineComponent.enabled = false;
            outlineComponent.OutlineColor = highlightColor;
            outlineComponent.OutlineWidth = highlightWidth;
        }

        /// <summary>
        /// Enables the outline shader. Logic ensures it only highlights if discovered/allowed.
        /// </summary>
        public void EnableHighlight(bool force = false)
        {
            // Do not highlight hazards before they are officially discovered unless forced
            if (!force && interactiveObject != null)
            {
                if (interactiveObject.GetCurrentState() == ObjectState.CriticalHazard ||
                    interactiveObject.GetCurrentState() == ObjectState.MinorHazard)
                {
                    // For hazards, only highlight if the engine says it's time to inspect
                    ScenarioEngine engine = FindObjectOfType<ScenarioEngine>();
                    if (engine != null && engine.CurrentState != ScenarioState.AWAITING_ACTION)
                    {
                        return; // Prevent highlighting undiscovered hazards
                    }
                }
            }

            if (outlineComponent != null)
            {
                outlineComponent.enabled = true;
            }
        }

        public void DisableHighlight()
        {
            if (outlineComponent != null)
            {
                outlineComponent.enabled = false;
            }
        }

        /// <summary>
        /// Spawns a 3D UI indicator above the object.
        /// </summary>
        public void SpawnHazardIndicator()
        {
            if (hazardIndicatorPrefab != null && spawnedIndicator == null)
            {
                spawnedIndicator = Instantiate(hazardIndicatorPrefab, transform.position + indicatorOffset, Quaternion.identity, transform);
            }
        }
    }
}
