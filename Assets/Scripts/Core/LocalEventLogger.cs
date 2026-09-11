using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using SafetyTraining.AR;

[Serializable]
public struct WorkerAction
{
    public string workerID;
    public string objectID;
    public ActionType actionTaken;
    public string timestamp;
    public bool isCorrect;
    public bool wasCritical;
}

[Serializable]
public class LogDataWrapper
{
    public List<WorkerAction> logs;
}

public class LocalEventLogger : MonoBehaviour
{
    public static LocalEventLogger Instance { get; private set; }

    [SerializeField]
    private List<WorkerAction> currentLogs = new List<WorkerAction>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void LogAction(string workerID, string objectID, ActionType actionTaken, bool isCorrect, bool wasCritical)
    {
        WorkerAction newAction = new WorkerAction
        {
            workerID = workerID,
            objectID = objectID,
            actionTaken = actionTaken,
            timestamp = DateTime.UtcNow.ToString("o"), // ISO 8601 format
            isCorrect = isCorrect,
            wasCritical = wasCritical
        };
        
        currentLogs.Add(newAction);
        Debug.Log($"[LocalEventLogger] Action Logged: {objectID} - {actionTaken} (Correct: {isCorrect})");
    }

    public List<WorkerAction> GetLogs()
    {
        return currentLogs;
    }

    public void ClearLogs()
    {
        currentLogs.Clear();
        Debug.Log("[LocalEventLogger] Logs Cleared.");
    }

    public void SaveLogsToJSON()
    {
        LogDataWrapper wrapper = new LogDataWrapper { logs = currentLogs };
        string json = JsonUtility.ToJson(wrapper, true);
        
        // persistentDataPath works on Android, iOS, Windows, etc.
        string path = Path.Combine(Application.persistentDataPath, "worker_logs.json");
        
        File.WriteAllText(path, json);
        Debug.Log($"[LocalEventLogger] Logs saved successfully to: {path}");
    }
}
