using UnityEngine;

namespace SafetyTraining.Localization
{
    [System.Serializable]
    public class LocalizedTextData
    {
        public Language language;
        [TextArea(2, 5)]
        public string text;
        public AudioClip voiceClip;
    }

    [System.Serializable]
    public class LocalizedEntry
    {
        [Tooltip("The unique key for this localization entry (e.g., 'inspection_start')")]
        public string key;
        
        [Tooltip("Translations and voice clips for each language")]
        public LocalizedTextData[] translations;

        public LocalizedTextData GetTranslation(Language lang)
        {
            if (translations == null) return null;
            foreach (var t in translations)
            {
                if (t.language == lang) return t;
            }
            return null;
        }
    }
}
