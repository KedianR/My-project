using UnityEngine;

namespace SafetyTraining.AR
{
    public class OnScreenDebug : MonoBehaviour
    {
        private static string myLog = "Debug Log Started...\n";

        void OnEnable() => Application.logMessageReceived += HandleLog;
        void OnDisable() => Application.logMessageReceived -= HandleLog;

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            myLog = $"[{type}] {logString}\n" + myLog;
            if (myLog.Length > 800) myLog = myLog.Substring(0, 800);
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = (int)(Screen.height * 0.022f);
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.UpperLeft;
            style.wordWrap = true;

            GUI.Box(new Rect(10, 10, Screen.width - 20, Screen.height * 0.35f), myLog, style);
        }
    }
}