namespace JanitorMario.FSM
{
    public enum ActionType
    {
        Default = -1,
        Attack = 0
    }

    [System.Serializable]
    public struct EventAction
    {
        public Action action;
        public ActionType type;
    }
}
