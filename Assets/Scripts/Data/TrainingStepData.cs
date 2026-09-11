using UnityEngine;
using SafetyTraining.AR;

[CreateAssetMenu(fileName = "NewTrainingStep", menuName = "SafetyTraining/Training Step Data")]
public class TrainingStepData : ScriptableObject
{
    [Tooltip("Unique identifier for the step")]
    public string stepID;

    [TextArea(3, 5)]
    [Tooltip("Instructions displayed to the user")]
    public string instructionText;

    [Tooltip("The ID of the object the user needs to interact with")]
    public string targetObjectID;

    [Tooltip("The required interaction type")]
    public ActionType expectedAction;

    [Tooltip("Flag indicating if failing this step causes immediate failure or severe penalty")]
    public bool isCriticalErrorIfMissed;

    [TextArea(2, 4)]
    [Tooltip("Text to show upon successful completion")]
    public string successFeedback;

    [TextArea(2, 4)]
    [Tooltip("Text to show upon failure/incorrect action")]
    public string failureFeedback;
}
