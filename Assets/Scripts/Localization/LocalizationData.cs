using System.Collections.Generic;
using UnityEngine;

namespace SafetyTraining.Localization
{
    [CreateAssetMenu(fileName = "NewLocalizationData", menuName = "SafetyTraining/Localization Data")]
    public class LocalizationData : ScriptableObject
    {
        public List<LocalizedEntry> entries = new List<LocalizedEntry>();

        public LocalizedEntry GetEntry(string key)
        {
            return entries.Find(e => e.key == key);
        }
    }
}
