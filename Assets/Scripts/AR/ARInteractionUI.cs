using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SafetyTraining.Core;
using SafetyTraining.Localization;

namespace SafetyTraining.AR
{
    /// <summary>
    /// World-Space Canvas UI that appears over an interactive AR object.
    /// Queries GameModeManager to adapt prompts, pulls text from LocalizationManager,
    /// and fires OnActionSelected so the Scenario/Assessment Engine can subscribe.
    /// </summary>
    public class ARInteractionUI : MonoBehaviour
    {
        // ── Public Event ────────────────────────────────────────────────────────
        /// <summary>
        /// Fired when the user selects an action. Subscribe from ScenarioEngine or
        /// AssessmentEngine to track the player's choice.
        /// (ActionType chosen, ARInteractiveObject target)
        /// </summary>
        public event System.Action<ActionType, ARInteractiveObject> OnActionSelected;

        // ── Inspector References ────────────────────────────────────────────────
        [Header("UI Buttons")]
        public Button btnAvoid;
        public Button btnReport;
        public Button btnIsolate;

        [Header("UI Text")]
        [Tooltip("Optional prompt label shown in Training Mode only.")]
        public TextMeshProUGUI promptText;
        [Tooltip("Label on the Avoid button")]
        public TextMeshProUGUI avoidLabel;
        [Tooltip("Label on the Report button")]
        public TextMeshProUGUI reportLabel;
        [Tooltip("Label on the Isolate button")]
        public TextMeshProUGUI isolateLabel;

        [Header("Localization Keys")]
        public string promptTrainingKey = "ui_prompt_training";
        public string avoidLabelKey     = "ui_btn_avoid";
        public string reportLabelKey    = "ui_btn_report";
        public string isolateLabelKey   = "ui_btn_isolate";
        public string feedbackCorrectKey   = "feedback_correct";
        public string feedbackIncorrectKey = "feedback_incorrect";

        [Header("Dependencies")]
        [Tooltip("Assign the FeedbackController from the scene.")]
        public SafetyTraining.UI.FeedbackController feedbackController;

        [Tooltip("AudioSource used for localized voice prompts.")]
        public AudioSource audioSource;

        // ── Private State ────────────────────────────────────────────────────────
        private ARInteractiveObject currentTarget;
        private Canvas canvas;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = false;

            if (btnAvoid   != null) btnAvoid.onClick.AddListener(()   => HandleActionSelected(ActionType.Avoid));
            if (btnReport  != null) btnReport.onClick.AddListener(()  => HandleActionSelected(ActionType.Report));
            if (btnIsolate != null) btnIsolate.onClick.AddListener(() => HandleActionSelected(ActionType.Isolate));
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Called by ARInteractionHandler when the user taps an ARInteractiveObject.
        /// Positions the canvas, resolves localized strings, and adapts to game mode.
        /// </summary>
        public void ShowUI(ARInteractiveObject target)
        {
            currentTarget = target;

            // ── Position & Orient the World-Space Canvas ──────────────────────
            transform.position = target.transform.position + new Vector3(0, 0.5f, 0);

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                // FIX: direction must point FROM canvas TOWARD camera so the canvas faces the player.
                transform.rotation = Quaternion.LookRotation(mainCam.transform.position - transform.position);
            }

            // ── Localize Button Labels ────────────────────────────────────────
            if (avoidLabel   != null) avoidLabel.text   = GetLocalized(avoidLabelKey);
            if (reportLabel  != null) reportLabel.text  = GetLocalized(reportLabelKey);
            if (isolateLabel != null) isolateLabel.text = GetLocalized(isolateLabelKey);

            // ── Training Mode: show a prompt and play voice ───────────────────
            bool isTraining = GameModeManager.Instance != null && GameModeManager.Instance.IsTrainingMode();

            if (promptText != null)
            {
                promptText.gameObject.SetActive(isTraining);
                if (isTraining)
                    promptText.text = GetLocalized(promptTrainingKey);
            }

            if (isTraining && audioSource != null && LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.PlayVoice(promptTrainingKey, audioSource);
            }

            if (canvas != null) canvas.enabled = true;
        }

        /// <summary>Programmatically hides the interaction UI.</summary>
        public void HideUI()
        {
            if (canvas != null) canvas.enabled = false;
            currentTarget = null;
        }

        // ── Private Helpers ──────────────────────────────────────────────────────
        private void HandleActionSelected(ActionType action)
        {
            if (currentTarget == null) return;

            // 1. Notify listeners (ScenarioEngine, AssessmentEngine, etc.)
            OnActionSelected?.Invoke(action, currentTarget);

            // 2. Drive the target object's interaction logic
            currentTarget.OnInteract(action);

            // 3. Highlight the object after user interaction (post-discovery)
            ObjectHighlighter highlighter = currentTarget.GetComponent<ObjectHighlighter>();
            if (highlighter != null)
            {
                highlighter.EnableHighlight(force: true);
                highlighter.SpawnHazardIndicator();
            }

            // 4. Determine correctness and show localized feedback
            bool isCorrect = (action == currentTarget.ExpectedAction);
            string feedbackKey = isCorrect ? feedbackCorrectKey : feedbackIncorrectKey;
            string feedbackMsg = GetLocalized(feedbackKey);

            if (feedbackController != null)
            {
                feedbackController.ShowFeedback(isCorrect, feedbackMsg);
            }
            else
            {
                Debug.LogWarning("[ARInteractionUI] FeedbackController is not assigned. Please link it in the Inspector.");
            }

            // 5. Hide the action UI
            HideUI();
        }

        private string GetLocalized(string key)
        {
            if (LocalizationManager.Instance != null)
                return LocalizationManager.Instance.GetText(key);

            // Graceful fallback — key shown in brackets so missing localization is obvious in testing
            return $"[{key}]";
        }
    }
}
