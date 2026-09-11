using UnityEngine;

namespace SafetyTraining.Core
{
    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }

        public GameMode CurrentMode = GameMode.TrainingMode;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetMode(GameMode newMode)
        {
            CurrentMode = newMode;
            Debug.Log($"[GameModeManager] Mode Switched to: {newMode}");
        }

        public bool IsTrainingMode()
        {
            return CurrentMode == GameMode.TrainingMode;
        }

        public bool IsAssessmentMode()
        {
            return CurrentMode == GameMode.AssessmentMode;
        }
    }
}
