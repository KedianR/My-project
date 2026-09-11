using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARDiagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log("ARDiagnostic STARTED");
    }

    void OnGUI()
    {
        GUI.color = Color.red;

        GUI.Label(
            new Rect(30, 30, 1000, 200),
            "ARDIAGNOSTIC IS RUNNING\nAR STATE: " + ARSession.state
        );
    }
}