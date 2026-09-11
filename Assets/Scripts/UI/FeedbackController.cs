using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // Assuming TextMeshPro is used for UI

namespace SafetyTraining.UI
{
    public class FeedbackController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Assign the Feedback_Background GameObject")]
        public GameObject feedbackBackgroundObj;
        [Tooltip("Assign the Feedback Text GameObject")]
        public GameObject feedbackTextObj;

        private Image feedbackImage;
        private UnityEngine.UI.Text legacyText;
        private TextMeshProUGUI tmpText;

        [Header("Colors")]
        public Color successColor = new Color(0, 1, 0, 0.8f);
        public Color failureColor = new Color(1, 0, 0, 0.8f);

        [Header("Settings")]
        public float displayDuration = 3f;

        private Coroutine activeFeedbackRoutine;

        private void Awake()
        {
            if (feedbackBackgroundObj != null) 
            {
                feedbackImage = feedbackBackgroundObj.GetComponent<Image>();
                feedbackBackgroundObj.SetActive(false);
            }
            if (feedbackTextObj != null) 
            {
                legacyText = feedbackTextObj.GetComponent<UnityEngine.UI.Text>();
                tmpText = feedbackTextObj.GetComponent<TextMeshProUGUI>();
                feedbackTextObj.SetActive(false);
            }
        }

        /// <summary>
        /// Displays visual feedback with action-oriented text.
        /// </summary>
        public void ShowFeedback(bool isCorrect, string message)
        {
            if (activeFeedbackRoutine != null)
            {
                StopCoroutine(activeFeedbackRoutine);
            }
            activeFeedbackRoutine = StartCoroutine(FeedbackRoutine(isCorrect, message));
        }

        private IEnumerator FeedbackRoutine(bool isCorrect, string message)
        {
            if (feedbackBackgroundObj != null)
            {
                if (feedbackImage != null) feedbackImage.color = isCorrect ? successColor : failureColor;
                feedbackBackgroundObj.SetActive(true);
            }

            if (feedbackTextObj != null)
            {
                if (legacyText != null) legacyText.text = message;
                if (tmpText != null) tmpText.text = message;
                feedbackTextObj.SetActive(true);
            }

            // Optional: Play a sound effect here
            // if (isCorrect) PlaySuccessSound(); else PlayErrorSound();

            yield return new WaitForSeconds(displayDuration);

            if (feedbackBackgroundObj != null) feedbackBackgroundObj.SetActive(false);
            if (feedbackTextObj != null) feedbackTextObj.SetActive(false);
        }
    }
}
