using UnityEngine;
using UnityEngine.UI;

namespace SafetyTraining.AR
{
    public class ARInteractionUI : MonoBehaviour
    {
        [Header("UI Buttons")]
        public Button btnAvoid;
        public Button btnReport;
        public Button btnIsolate;

        private ARInteractiveObject currentTarget;
        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = false;

            if (btnAvoid != null) btnAvoid.onClick.AddListener(() => OnActionSelected(ActionType.Avoid));
            if (btnReport != null) btnReport.onClick.AddListener(() => OnActionSelected(ActionType.Report));
            if (btnIsolate != null) btnIsolate.onClick.AddListener(() => OnActionSelected(ActionType.Isolate));
        }

        public void ShowUI(ARInteractiveObject target)
        {
            currentTarget = target;
            
            // Position the UI near the object
            transform.position = target.transform.position + new Vector3(0, 0.5f, 0);
            
            // Face the camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
            }

            if (canvas != null) canvas.enabled = true;
        }

        private void OnActionSelected(ActionType action)
        {
            if (currentTarget != null)
            {
                currentTarget.OnInteract(action);
                
                // Highlight object after an action is taken
                ObjectHighlighter highlighter = currentTarget.GetComponent<ObjectHighlighter>();
                if (highlighter != null)
                {
                    highlighter.EnableHighlight(force: true);
                    highlighter.SpawnHazardIndicator();
                }
            }

            // Hide the UI
            if (canvas != null) canvas.enabled = false;
        }
    }
}
