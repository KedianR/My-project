using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AssessmentResult
{
    public string CompetencyLevel;
    public int TotalScore;
    public int CriticalErrorsMade;
    public List<string> FeedbackNotes = new List<string>();
}

public class AssessmentEngine : MonoBehaviour
{
    // UI Event
    public event System.Action<AssessmentResult> OnAssessmentComplete;
    [Tooltip("If checked, prints the assessment result to the console when evaluated.")]
    public bool debugLogResult = true;

    /// <summary>
    /// Evaluates a completed scenario using the provided scenario data and logged worker actions.
    /// </summary>
    public AssessmentResult EvaluateSession(TrainingScenarioData scenario, List<WorkerAction> logs)
    {
        AssessmentResult result = new AssessmentResult();
        
        // Safety checks
        if (scenario == null || scenario.steps == null || logs == null)
        {
            Debug.LogError("[AssessmentEngine] Cannot evaluate session. Scenario or logs are missing.");
            result.CompetencyLevel = "FAIL";
            return result;
        }

        // Loop through the logs to score
        // For the MVP, we assume the logs sequentially match the expected steps
        int stepCount = Mathf.Min(scenario.steps.Length, logs.Count);

        for (int i = 0; i < stepCount; i++)
        {
            TrainingStepData step = scenario.steps[i];
            WorkerAction action = logs[i];

            // Verify if action matches step requirements
            bool isCorrectAction = action.isCorrect && (action.actionTaken == step.expectedAction) && (action.objectID == step.targetObjectID);

            if (isCorrectAction)
            {
                result.TotalScore += 1;
                if (!string.IsNullOrEmpty(step.successFeedback))
                {
                    result.FeedbackNotes.Add($"Step {i+1} ({step.stepID}): {step.successFeedback}");
                }
            }
            else
            {
                if (step.isCriticalErrorIfMissed)
                {
                    result.CriticalErrorsMade += 1;
                }
                
                if (!string.IsNullOrEmpty(step.failureFeedback))
                {
                    result.FeedbackNotes.Add($"Step {i+1} ({step.stepID}): {step.failureFeedback}");
                }
            }
        }

        // Determine final competency (Simple PASS/FAIL)
        if (result.CriticalErrorsMade > 0 || result.TotalScore < scenario.passingThresholdScore)
        {
            result.CompetencyLevel = "FAIL"; // Fails immediately due to a critical error or low score
        }
        else
        {
            result.CompetencyLevel = "PASS"; // Passes all criteria
        }

        if (debugLogResult)
        {
            Debug.Log($"[AssessmentEngine] --- Final Evaluation ---");
            Debug.Log($"[AssessmentEngine] Competency: {result.CompetencyLevel}");
            Debug.Log($"[AssessmentEngine] Score: {result.TotalScore} / {scenario.steps.Length} (Pass Threshold: {scenario.passingThresholdScore})");
            Debug.Log($"[AssessmentEngine] Critical Errors: {result.CriticalErrorsMade}");
            foreach (var note in result.FeedbackNotes)
            {
                Debug.Log($"[AssessmentEngine] Feedback: {note}");
            }
        }

        return result;
    }

    /// <summary>
    /// Utility method to trigger evaluation from a UI Button or other event,
    /// automatically grabbing the current scenario and logs.
    /// Can also be triggered via right-click in Inspector during Play Mode.
    /// </summary>
    [ContextMenu("Trigger Evaluation")]
    public void TriggerEvaluation()
    {
        // Require ScenarioEngine to get the active scenario
        ScenarioEngine scenarioEngine = FindObjectOfType<ScenarioEngine>();
        if (scenarioEngine == null || scenarioEngine.activeScenario == null)
        {
            Debug.LogError("[AssessmentEngine] Cannot find active ScenarioEngine or TrainingScenarioData.");
            return;
        }

        // Require LocalEventLogger to get the logs
        if (LocalEventLogger.Instance == null)
        {
            Debug.LogError("[AssessmentEngine] Cannot find LocalEventLogger Instance.");
            return;
        }

        List<WorkerAction> currentLogs = LocalEventLogger.Instance.GetLogs();
        AssessmentResult finalResult = EvaluateSession(scenarioEngine.activeScenario, currentLogs);
        
        // Dispatch result to UI
        OnAssessmentComplete?.Invoke(finalResult);
    }
}
