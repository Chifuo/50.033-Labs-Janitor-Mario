namespace JanitorMario.FSM
{
    [System.Serializable]
    public class Transition
    {
        public Decision decision;
        // An empty destination or the controller's Remain State means stay here.
        public State trueState;
        public State falseState;
    }
}
