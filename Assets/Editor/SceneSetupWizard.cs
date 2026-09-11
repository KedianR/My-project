// ============================================================
// SceneSetupWizard.cs  — AR Safety Inspection MVP
// Auto-wires all cross-script Inspector references.
// Run via:  Tools > AR Safety > Run Scene Setup Wizard
// ============================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SafetyTraining.AR;
using SafetyTraining.Core;
using SafetyTraining.Localization;
using SafetyTraining.UI;

public static class SceneSetupWizard
{
    private const string SCENE_PATH        = "Assets/Scenes/AR_TEst.unity";
    private const string SCENARIO_ASSET    = "Assets/Data/scenario_main.asset";
    private const string LOCALIZATION_ASSET= "Assets/Data/MasterLocalizationData.asset";

    [MenuItem("Tools/AR Safety/Run Scene Setup Wizard", priority = 1)]
    public static void RunWizard()
    {
        // ── 1. Make sure the correct scene is open ──────────────────────────
        if (SceneManager.GetActiveScene().path != SCENE_PATH)
        {
            bool open = EditorUtility.DisplayDialog(
                "Scene Setup Wizard",
                $"The wizard needs to open:\n{SCENE_PATH}\n\nAny unsaved changes will be lost. Continue?",
                "Open Scene", "Cancel");

            if (!open) return;
            EditorSceneManager.OpenScene(SCENE_PATH);
        }

        int fixes = 0;
        string log = "";

        // ── 2. Load ScriptableObject assets ────────────────────────────────
        var scenarioData    = AssetDatabase.LoadAssetAtPath<TrainingScenarioData>(SCENARIO_ASSET);
        var localizationData= AssetDatabase.LoadAssetAtPath<LocalizationData>(LOCALIZATION_ASSET);

        if (scenarioData     == null) LogWarn(ref log, $"Could not load TrainingScenarioData at: {SCENARIO_ASSET}");
        if (localizationData == null) LogWarn(ref log, $"Could not load LocalizationData at: {LOCALIZATION_ASSET}");

        // ── 3. Find scene components by GameObject names (fallback to Type) ──
        var coreManager = GameObject.Find("CoreManager");
        var scenarioEngine   = coreManager != null ? coreManager.GetComponent<ScenarioEngine>() : Object.FindFirstObjectByType<ScenarioEngine>(FindObjectsInactive.Include);
        var assessmentEngine = coreManager != null ? coreManager.GetComponent<AssessmentEngine>() : Object.FindFirstObjectByType<AssessmentEngine>(FindObjectsInactive.Include);
        
        var feedbackCanvas = GameObject.Find("Feedback_Canvas") ?? GameObject.Find("Feedback_Background");
        var feedbackCtrl     = feedbackCanvas != null ? feedbackCanvas.GetComponent<FeedbackController>() : Object.FindFirstObjectByType<FeedbackController>(FindObjectsInactive.Include);
        
        var uiCanvas = GameObject.Find("UICanvas") ?? GameObject.Find("HUD_Panel");
        var uiManager        = uiCanvas != null ? uiCanvas.GetComponentInChildren<UIManager>(true) : Object.FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);
        
        var locManagerObj = GameObject.Find("LocalizationManager");
        var locManager       = locManagerObj != null ? locManagerObj.GetComponent<LocalizationManager>() : Object.FindFirstObjectByType<LocalizationManager>(FindObjectsInactive.Include);
        
        var gameModeManagerObj = GameObject.Find("GameModeManager");
        var gameModeManager  = gameModeManagerObj != null ? gameModeManagerObj.GetComponent<GameModeManager>() : Object.FindFirstObjectByType<GameModeManager>(FindObjectsInactive.Include);

        var arActionMenu = GameObject.Find("AR_ActionMenu_Canvas");
        var interactionUI    = arActionMenu != null ? arActionMenu.GetComponent<ARInteractionUI>() : Object.FindFirstObjectByType<ARInteractionUI>(FindObjectsInactive.Include);
        
        var xrOrigin = GameObject.Find("XR Origin");
        var interactionHandler = xrOrigin != null ? xrOrigin.GetComponentInChildren<ARInteractionHandler>(true) : Object.FindFirstObjectByType<ARInteractionHandler>(FindObjectsInactive.Include);
        var placementCtrl    = xrOrigin != null ? xrOrigin.GetComponentInChildren<ARPlacementController>(true) : Object.FindFirstObjectByType<ARPlacementController>(FindObjectsInactive.Include);

        // ── 4. ScenarioEngine wiring ────────────────────────────────────────
        if (scenarioEngine != null)
        {
            SerializedObject so = new SerializedObject(scenarioEngine);

            // activeScenario
            if (scenarioData != null)
                SetObjectRef(so, "activeScenario", scenarioData, ref fixes, ref log, "ScenarioEngine.activeScenario");

            // placementController
            if (placementCtrl != null)
                SetObjectRef(so, "placementController", placementCtrl, ref fixes, ref log, "ScenarioEngine.placementController");

            // interactionHandler (stored as Behaviour)
            if (interactionHandler != null)
                SetObjectRef(so, "interactionHandler", interactionHandler, ref fixes, ref log, "ScenarioEngine.interactionHandler");

            // feedbackController  ← NEW field added in code review
            if (feedbackCtrl != null)
                SetObjectRef(so, "feedbackController", feedbackCtrl, ref fixes, ref log, "ScenarioEngine.feedbackController");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(scenarioEngine);
        }
        else LogWarn(ref log, "ScenarioEngine not found in scene.");

        // ── 5. UIManager wiring ─────────────────────────────────────────────
        if (uiManager != null)
        {
            SerializedObject so = new SerializedObject(uiManager);

            if (scenarioEngine != null)
                SetObjectRef(so, "scenarioEngine", scenarioEngine, ref fixes, ref log, "UIManager.scenarioEngine");

            if (assessmentEngine != null)
                SetObjectRef(so, "assessmentEngine", assessmentEngine, ref fixes, ref log, "UIManager.assessmentEngine");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(uiManager);
        }
        else LogWarn(ref log, "UIManager not found in scene.");

        // ── 6. LocalizationManager wiring ───────────────────────────────────
        if (locManager != null && localizationData != null)
        {
            SerializedObject so = new SerializedObject(locManager);
            SetObjectRef(so, "database", localizationData, ref fixes, ref log, "LocalizationManager.database");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(locManager);
        }
        else if (locManager == null) LogWarn(ref log, "LocalizationManager not found in scene.");

        // ── 7. ARInteractionUI wiring ────────────────────────────────────────
        if (interactionUI != null)
        {
            SerializedObject so = new SerializedObject(interactionUI);

            // feedbackControllerSource  ← public GameObject field, link it
            if (feedbackCtrl != null)
                SetObjectRef(so, "feedbackControllerSource", feedbackCtrl.gameObject, ref fixes, ref log, "ARInteractionUI.feedbackControllerSource");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(interactionUI);
        }
        else LogWarn(ref log, "ARInteractionUI not found in scene (may be on a prefab — link feedbackControllerSource there).");

        // ── 8. Save the scene ───────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        // ── 9. Summary dialog ───────────────────────────────────────────────
        string summary =
            $"✅  Scene Setup Wizard Complete!\n\n" +
            $"Auto-wired:  {fixes} reference(s)\n\n" +
            (string.IsNullOrEmpty(log) ? "No warnings." : $"⚠️  Warnings:\n{log}") +
            "\n\n──────────────────────────────\n" +
            "Remaining manual steps (prefab-internal):\n" +
            "• ARInteractionUI prefab: link btnAvoid, btnReport, btnIsolate buttons\n" +
            "• ARInteractionUI prefab: link promptText, avoidLabel, reportLabel, isolateLabel (TextMeshProUGUI)\n" +
            "• ARInteractionUI prefab: link AudioSource\n" +
            "• FeedbackController: link feedbackBackground (Image) and feedbackText (TMP)\n" +
            "• UIManager: re-link all Text fields → now TextMeshProUGUI after type change\n" +
            "• ObjectHighlighter (on each hazard prefab): link hazardIndicatorPrefab";

        EditorUtility.DisplayDialog("Scene Setup Wizard", summary, "OK");
        Debug.Log($"[SceneSetupWizard] Done — {fixes} reference(s) wired.\n{log}");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static void SetObjectRef(SerializedObject so, string propName,
        Object value, ref int fixes, ref string log, string label)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop == null)
        {
            LogWarn(ref log, $"Property '{propName}' not found on {so.targetObject.GetType().Name}. " +
                             "Field may have been renamed.");
            return;
        }

        if (prop.objectReferenceValue == value) return; // already wired

        prop.objectReferenceValue = value;
        fixes++;
        Debug.Log($"[SceneSetupWizard] ✓ Wired {label} → {value.name}");
    }

    private static void LogWarn(ref string log, string msg)
    {
        log += $"\n• {msg}";
        Debug.LogWarning($"[SceneSetupWizard] ⚠ {msg}");
    }
}
