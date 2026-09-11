namespace SafetyTraining.AR
{
    public enum ActionType { Tap, Inspect, Isolate, Report, Avoid }

    public enum ObjectState { Safe, MinorHazard, CriticalHazard }

    public interface IInteractable
    {
        string ObjectID { get; }
        bool CanInteract();
        void OnInteract(ActionType action);
        ObjectState GetCurrentState();
    }
}