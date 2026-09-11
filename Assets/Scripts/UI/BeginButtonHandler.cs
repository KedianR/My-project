using UnityEngine;

/// <summary>
/// Attached to the Begin_Button inside BeginModule_Dialog.
/// Self-contained: closes its own parent panel and starts the scenario.
/// Zero serialized references — fully safe in IL2CPP Android builds.
/// </summary>
public class BeginButtonHandler : MonoBehaviour
{
    public void OnClick()
    {
        // Close the dialog — this button is a direct child of the dialog panel
        // so transform.parent IS the dialog. No external reference needed.
        if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(false);
            Debug.Log("[BeginButtonHandler] Dialog closed.");
        }

        // Start the scenario directly — no UIManager reference needed
        ScenarioEngine engine = FindObjectOfType<ScenarioEngine>();
        if (engine != null)
        {
            engine.BeginScenario();
            Debug.Log("[BeginButtonHandler] BeginScenario() called.");
        }
        else
        {
            Debug.LogError("[BeginButtonHandler] ScenarioEngine not found in scene!");
        }
    }
}
