using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // FIX: Use TextMeshProUGUI throughout for consistency with FeedbackController

/// <summary>
/// Listens to ScenarioEngine and AssessmentEngine events and drives the screen-space HUD.
/// All text fields use TextMeshProUGUI for render quality consistency.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Engine References")]
    public ScenarioEngine scenarioEngine;
    public AssessmentEngine assessmentEngine;

    [Header("UI Panels")]
    public GameObject hudPanel;
    public GameObject instructionPanel;
    public GameObject resultsPanel;

    [Header("UI Text Elements")]
    public UnityEngine.UI.Text statusText;
    public UnityEngine.UI.Text instructionText;
    public UnityEngine.UI.Text resultsScoreText;
    public UnityEngine.UI.Text resultsCompetencyText;
    public UnityEngine.UI.Text resultsFeedbackText;

    private void OnEnable()
    {
        if (scenarioEngine != null)
        {
            scenarioEngine.OnStateChanged       += HandleStateChanged;
            scenarioEngine.OnInstructionUpdated += HandleInstructionUpdated;
        }

        if (assessmentEngine != null)
        {
            assessmentEngine.OnAssessmentComplete += HandleAssessmentComplete;
        }
    }

    private void OnDisable()
    {
        if (scenarioEngine != null)
        {
            scenarioEngine.OnStateChanged       -= HandleStateChanged;
            scenarioEngine.OnInstructionUpdated -= HandleInstructionUpdated;
        }

        if (assessmentEngine != null)
        {
            assessmentEngine.OnAssessmentComplete -= HandleAssessmentComplete;
        }
    }

    private void Start()
    {
        if (hudPanel         != null) hudPanel.SetActive(true);
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (resultsPanel     != null) resultsPanel.SetActive(false);
    }

    // ── Event Handlers ───────────────────────────────────────────────────────

    private void HandleStateChanged(ScenarioState newState)
    {
        if (statusText != null)
            statusText.text = $"State: {newState}";

        if (instructionPanel != null)
        {
            bool show = newState == ScenarioState.INSTRUCTION || newState == ScenarioState.AWAITING_ACTION;
            instructionPanel.SetActive(show);
        }
    }

    private void HandleInstructionUpdated(string instruction)
    {
        if (instructionText != null)
            instructionText.text = instruction;
    }

    private void HandleAssessmentComplete(AssessmentResult result)
    {
        if (hudPanel         != null) hudPanel.SetActive(false);
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (resultsPanel     != null) resultsPanel.SetActive(true);

        if (resultsScoreText != null)
            resultsScoreText.text = $"Score: {result.TotalScore}";

        if (resultsCompetencyText != null)
            resultsCompetencyText.text = $"Competency: {result.CompetencyLevel}";

        if (resultsFeedbackText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("Feedback:\n");
            foreach (var note in result.FeedbackNotes)
                sb.AppendLine($"- {note}");
            resultsFeedbackText.text = sb.ToString();
        }
    }

    // ── Public Button Callbacks ───────────────────────────────────────────────

    /// <summary>Called by the Restart button on the Results panel.</summary>
    public void RestartScenario()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
