using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SafetyTraining.AR
{
    [DisallowMultipleComponent]
    public class Outline : MonoBehaviour
    {
        private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

        public enum Mode
        {
            OutlineAll,
            OutlineVisible,
            OutlineHidden,
            OutlineAndSilhouette,
            SilhouetteOnly
        }

        public Mode OutlineMode
        {
            get { return outlineMode; }
            set { outlineMode = value; needsUpdate = true; }
        }

        public Color OutlineColor
        {
            get { return outlineColor; }
            set { outlineColor = value; needsUpdate = true; }
        }

        public float OutlineWidth
        {
            get { return outlineWidth; }
            set { outlineWidth = value; needsUpdate = true; }
        }

        [SerializeField] private Mode outlineMode;
        [SerializeField] private Color outlineColor = Color.white;
        [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;
        [SerializeField] private bool precomputeOutline;

        private Renderer[] renderers;
        private Material outlineMaskMaterial;
        private Material outlineFillMaterial;
        private bool needsUpdate;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();

            outlineMaskMaterial = new Material(Shader.Find("Hidden/Outline Mask"));
            outlineFillMaterial = new Material(Shader.Find("Hidden/Outline Fill"));

            outlineMaskMaterial.name = "OutlineMask (Instance)";
            outlineFillMaterial.name = "OutlineFill (Instance)";

            // Fallback if shaders are not found (e.g., standard project without custom hidden shaders)
            if (outlineFillMaterial.shader == null || !outlineFillMaterial.shader.isSupported)
            {
                // Basic fallback: just use a standard unlit color
                outlineFillMaterial = new Material(Shader.Find("Unlit/Color"));
                outlineFillMaterial.color = outlineColor;
            }
        }

        private void OnEnable()
        {
            var materials = new List<Material>();

            foreach (var renderer in renderers)
            {
                materials.Clear();
                materials.AddRange(renderer.sharedMaterials);
                if (!materials.Contains(outlineMaskMaterial))
                {
                    materials.Add(outlineMaskMaterial);
                    materials.Add(outlineFillMaterial);
                    renderer.materials = materials.ToArray();
                }
            }
        }

        private void OnDisable()
        {
            var materials = new List<Material>();
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                materials.Clear();
                materials.AddRange(renderer.sharedMaterials);
                materials.Remove(outlineMaskMaterial);
                materials.Remove(outlineFillMaterial);
                renderer.materials = materials.ToArray();
            }
        }

        private void Update()
        {
            if (needsUpdate)
            {
                needsUpdate = false;
                UpdateMaterialProperties();
            }
        }

        private void UpdateMaterialProperties()
        {
            outlineFillMaterial.SetColor("_OutlineColor", outlineColor);
            outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
            if (outlineFillMaterial.HasProperty("_Color"))
                outlineFillMaterial.SetColor("_Color", outlineColor);
        }
    }
}
