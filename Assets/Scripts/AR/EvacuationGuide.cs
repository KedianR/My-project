using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SafetyTraining.AR
{
    public class EvacuationGuide : MonoBehaviour
    {
        [Header("Evacuation Setup")]
        public GameObject arrowPrefab;
        public Transform[] evacuationPathPoints;
        public float spacing = 1.0f;
        public float animationSpeed = 2f;

        private List<GameObject> spawnedArrows = new List<GameObject>();

        public void TriggerEvacuation()
        {
            StartCoroutine(SpawnArrowsRoutine());
        }

        private IEnumerator SpawnArrowsRoutine()
        {
            if (arrowPrefab == null || evacuationPathPoints == null || evacuationPathPoints.Length < 2)
            {
                Debug.LogWarning("[EvacuationGuide] Missing setup for evacuation arrows.");
                yield break;
            }

            for (int i = 0; i < evacuationPathPoints.Length - 1; i++)
            {
                Vector3 start = evacuationPathPoints[i].position;
                Vector3 end = evacuationPathPoints[i + 1].position;
                float distance = Vector3.Distance(start, end);
                int arrowsCount = Mathf.FloorToInt(distance / spacing);

                for (int j = 0; j <= arrowsCount; j++)
                {
                    float t = j / (float)arrowsCount;
                    Vector3 position = Vector3.Lerp(start, end, t);
                    Quaternion rotation = Quaternion.LookRotation(end - start);

                    GameObject arrow = Instantiate(arrowPrefab, position, rotation);
                    spawnedArrows.Add(arrow);

                    // Optional: Animate arrow material or scale here
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        public void ClearEvacuation()
        {
            foreach (var arrow in spawnedArrows)
            {
                Destroy(arrow);
            }
            spawnedArrows.Clear();
        }
    }
}
