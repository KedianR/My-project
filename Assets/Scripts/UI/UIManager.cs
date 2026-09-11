using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public Text statusText;
    public Text instructionText;
    public Text resultsScoreText;
    public Text resultsCompetencyText;
    public Text resultsFeedbackText;

    private void OnEnable()
    {
        if (scenarioEngine != null)
        {
            scenarioEngine.OnStateChanged += HandleStateChanged;
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
            scenarioEngine.OnStateChanged -= HandleStateChanged;
            scenarioEngine.OnInstructionUpdated -= HandleInstructionUpdated;
        }

        if (assessmentEngine != null)
        {
            assessmentEngine.OnAssessmentComplete -= HandleAssessmentComplete;
        }
    }

    private void Start()
    {
        // Initial UI state
        if (hudPanel != null) hudPanel.SetActive(true);
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }

    private void HandleStateChanged(ScenarioState newState)
    {
        if (statusText != null)
            statusText.text = $"State: {newState}";

        // Show/hide instruction panel based on state
        if (instructionPanel != null)
        {
            bool showInstruction = newState == ScenarioState.INSTRUCTION || newState == ScenarioState.AWAITING_ACTION;
            instructionPanel.SetActive(showInstruction);
        }
    }

    private void HandleInstructionUpdated(string instruction)
    {
        if (instructionText != null)
        {
            instructionText.text = instruction;
        }
    }

    private void HandleAssessmentComplete(AssessmentResult result)
    {
        // Hide other panels
        if (hudPanel != null) hudPanel.SetActive(false);
        if (instructionPanel != null) instructionPanel.SetActive(false);

        // Show results panel
        if (resultsPanel != null) resultsPanel.SetActive(true);

        if (resultsScoreText != null)
            resultsScoreText.text = $"Score: {result.TotalScore}";

        if (resultsCompetencyText != null)
            resultsCompetencyText.text = $"Competency: {result.CompetencyLevel}";

        if (resultsFeedbackText != null)
        {
            string feedbackStr = "Feedback:\n";
            foreach (var note in result.FeedbackNotes)
            {
                feedbackStr += $"- {note}\n";
            }
            resultsFeedbackText.text = feedbackStr;
        }
    }

    // Called by the Restart Button
    public void RestartScenario()
    {
        // For MVP, just reload the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
