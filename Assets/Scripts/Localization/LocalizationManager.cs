using UnityEngine;

namespace SafetyTraining.Localization
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [Header("Settings")]
        public Language currentLanguage = Language.English;
        
        [Header("Data")]
        [Tooltip("Assign the master LocalizationData ScriptableObject here")]
        public LocalizationData database;

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

        public void SetLanguage(Language newLanguage)
        {
            currentLanguage = newLanguage;
            Debug.Log($"[LocalizationManager] Language switched to {newLanguage}");
            // Optionally fire an event here so UI text updates instantly
        }

        public string GetText(string key)
        {
            if (database == null) return $"[{key}]";
            
            LocalizedEntry entry = database.GetEntry(key);
            if (entry == null) return $"[{key} NOT FOUND]";

            LocalizedTextData data = entry.GetTranslation(currentLanguage);
            return data != null ? data.text : $"[{key} MISSING LANG]";
        }

        public void PlayVoice(string key, AudioSource audioSource)
        {
            if (database == null || audioSource == null) return;

            LocalizedEntry entry = database.GetEntry(key);
            if (entry == null) return;

            LocalizedTextData data = entry.GetTranslation(currentLanguage);
            if (data != null && data.voiceClip != null)
            {
                audioSource.clip = data.voiceClip;
                audioSource.Play();
            }
        }
    }
}
