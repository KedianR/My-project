using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // Assuming TextMeshPro is used for UI

namespace SafetyTraining.UI
{
    public class FeedbackController : MonoBehaviour
    {
        [Header("UI References")]
        public Image feedbackBackground;
        public TextMeshProUGUI feedbackText;

        [Header("Colors")]
        public Color successColor = new Color(0, 1, 0, 0.8f);
        public Color failureColor = new Color(1, 0, 0, 0.8f);

        [Header("Settings")]
        public float displayDuration = 3f;

        private Coroutine activeFeedbackRoutine;

        private void Awake()
        {
            if (feedbackBackground != null) feedbackBackground.gameObject.SetActive(false);
            if (feedbackText != null) feedbackText.gameObject.SetActive(false);
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
            if (feedbackBackground != null)
            {
                feedbackBackground.color = isCorrect ? successColor : failureColor;
                feedbackBackground.gameObject.SetActive(true);
            }

            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.gameObject.SetActive(true);
            }

            // Optional: Play a sound effect here
            // if (isCorrect) PlaySuccessSound(); else PlayErrorSound();

            yield return new WaitForSeconds(displayDuration);

            if (feedbackBackground != null) feedbackBackground.gameObject.SetActive(false);
            if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        }
    }
}
