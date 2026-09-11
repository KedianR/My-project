using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SafetyTraining.AR
{
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARPlacementController : MonoBehaviour
    {
        [Header("AR References")]
        [Tooltip("Reference to the AR Raycast Manager (usually on XR Origin).")]
        [SerializeField] private ARRaycastManager raycastManager;
        [Tooltip("Reference to the AR Plane Manager to disable planes after placement.")]
        [SerializeField] private ARPlaneManager planeManager;

        [Header("Placement Objects")]
        [Tooltip("The reticle prefab to show where the object will be placed.")]
        [SerializeField] private GameObject reticlePrefab;

        [Header("Warehouse")]
        [Tooltip("The Warehouse_Environment root GameObject in the scene. Will be moved to the tap point.")]
        public GameObject warehouseRoot;

        private GameObject reticleInstance;
        private bool isPlaced = false;
        private List<ARRaycastHit> hits = new List<ARRaycastHit>();

        void Start()
        {
            // Auto-fetch components if not assigned
            if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>();
            if (planeManager == null) planeManager = FindObjectOfType<ARPlaneManager>();

            // Auto-find warehouse if not assigned
            if (warehouseRoot == null)
                warehouseRoot = GameObject.Find("Warehouse_Environment");

            // Hide warehouse until placement is confirmed
            if (warehouseRoot != null)
                warehouseRoot.SetActive(false);

            // Instantiate reticle and hide it initially
            if (reticlePrefab != null)
            {
                reticleInstance = Instantiate(reticlePrefab);
                reticleInstance.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Reticle Prefab is not assigned in ARPlacementController.");
            }
        }

        void Update()
        {
            // Stop updating placement once the scenario is placed
            if (isPlaced) return;

            UpdateReticlePosition();

            // Check for user screen tap using New Input System
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                // UI passthrough — don't place if tapping a UI element
                int fingerId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                if (UnityEngine.EventSystems.EventSystem.current == null ||
                    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(fingerId))
                {
                    TryPlaceObject();
                }
            }
            // Add mouse support for easy testing in the editor
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (UnityEngine.EventSystems.EventSystem.current == null ||
                    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
                {
                    TryPlaceObject();
                }
            }
        }

        private void UpdateReticlePosition()
        {
            // Shoot a ray from the center of the screen
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                
                if (reticleInstance != null)
                {
                    reticleInstance.SetActive(true);
                    reticleInstance.transform.position = hitPose.position;
                    reticleInstance.transform.rotation = hitPose.rotation;
                }
            }
            else
            {
                // Hide reticle if no plane is detected at the center
                if (reticleInstance != null)
                {
                    reticleInstance.SetActive(false);
                }
            }
        }

        private void TryPlaceObject()
        {
            // If the reticle is active, we have a valid plane to place on
            if (reticleInstance != null && reticleInstance.activeInHierarchy)
            {
                PlaceScenario(reticleInstance.transform.position, reticleInstance.transform.rotation);
            }
            // Fallback: If no reticle is used but we still want to raycast on touch
            else 
            {
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    PlaceScenario(hitPose.position, hitPose.rotation);
                }
            }
        }

        private void PlaceScenario(Vector3 tapPosition, Quaternion rotation)
        {
            if (warehouseRoot == null)
            {
                Debug.LogError("[ARPlacementController] Warehouse root not found!");
                return;
            }

            // Move the warehouse so its floor aligns to the AR plane tap point.
            // The worker is now standing inside the warehouse at the tap point.
            warehouseRoot.transform.position = tapPosition;
            warehouseRoot.transform.rotation = rotation;
            warehouseRoot.SetActive(true);

            Debug.Log($"[ARPlacementController] Warehouse placed at {tapPosition}.");

            isPlaced = true;

            // Hide the reticle permanently
            if (reticleInstance != null)
                reticleInstance.SetActive(false);

            // Lock the scene by hiding and disabling AR planes
            DisableARPlanes();

            // Notify ScenarioEngine that placement is done — show the "Begin" dialog
            ScenarioEngine scenarioEngine = FindObjectOfType<ScenarioEngine>();
            if (scenarioEngine != null)
            {
                scenarioEngine.OnWarehousePlaced();
            }
            else
            {
                Debug.LogWarning("[ARPlacementController] No ScenarioEngine found in scene.");
            }
        }

        private void DisableARPlanes()
        {
            if (planeManager != null)
            {
                // Disable the manager so it stops generating/updating planes
                planeManager.enabled = false;

                // Hide all existing visual planes
                foreach (var plane in planeManager.trackables)
                {
                    plane.gameObject.SetActive(false);
                }
                Debug.Log("AR Planes disabled and hidden (Scene Locked).");
            }
            else
            {
                Debug.LogWarning("ARPlaneManager reference is missing. Cannot disable planes.");
            }
        }
    }
}
