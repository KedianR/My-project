using UnityEngine;
using UnityEngine.UI;
using SafetyTraining.AR;
using TMPro;

public class InspectionUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inspectionPanel;
    public Button pinButton;
    public Button gaugeButton;
    public Button hoseButton;
    public Button completeButton;
    public TextMeshProUGUI statusText;

    private int checkedCount = 0;
    private const int requiredChecks = 2; // User requested "maybe 2 of them"

    private ARInteractiveObject currentExtinguisher;

    private void Start()
    {
        // Hide panel by default
        inspectionPanel.SetActive(false);

        // Bind buttons
        pinButton.onClick.AddListener(() => OnItemChecked(pinButton));
        gaugeButton.onClick.AddListener(() => OnItemChecked(gaugeButton));
        hoseButton.onClick.AddListener(() => OnItemChecked(hoseButton));
        
        completeButton.onClick.AddListener(OnCompleteInspection);
        completeButton.interactable = false;
    }

    public void ShowInspectionPanel(ARInteractiveObject extinguisher)
    {
        currentExtinguisher = extinguisher;
        checkedCount = 0;
        
        // Reset buttons
        ResetButton(pinButton);
        ResetButton(gaugeButton);
        ResetButton(hoseButton);
        
        completeButton.interactable = false;
        statusText.text = $"Check at least {requiredChecks} items to proceed.";
        
        inspectionPanel.SetActive(true);
    }

    private void ResetButton(Button btn)
    {
        btn.interactable = true;
        btn.GetComponent<Image>().color = Color.white;
    }

    private void OnItemChecked(Button btn)
    {
        btn.interactable = false;
        btn.GetComponent<Image>().color = Color.green; // Visual feedback for checked
        checkedCount++;

        statusText.text = $"Checked {checkedCount}/{requiredChecks} items.";

        if (checkedCount >= requiredChecks)
        {
            completeButton.interactable = true;
            statusText.text = "Inspection complete. You may proceed.";
        }
    }

    private void OnCompleteInspection()
    {
        inspectionPanel.SetActive(false);
        
        // Signal the engine that the interaction is complete
        if (currentExtinguisher != null)
        {
            // We simulate a tap on the extinguisher to advance the scenario
            currentExtinguisher.OnInteract(ActionType.Tap);
        }
    }
}
