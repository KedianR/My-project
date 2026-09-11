using UnityEngine;

[CreateAssetMenu(fileName = "NewTrainingScenario", menuName = "SafetyTraining/Training Scenario Data")]
public class TrainingScenarioData : ScriptableObject
{
    [Tooltip("Unique identifier for the scenario")]
    public string scenarioID;

    [Tooltip("Human-readable name for the scenario")]
    public string scenarioName;

    [Tooltip("An ordered array of the steps that make up this scenario")]
    public TrainingStepData[] steps;

    [Tooltip("Minimum score or maximum allowed errors to pass")]
    public int passingThresholdScore;

    [Tooltip("Maximum time allowed to complete the scenario (0 for no limit)")]
    public float timeLimitSeconds;
}
