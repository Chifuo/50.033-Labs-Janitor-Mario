using UnityEngine;

namespace JanitorMario.FSM
{
    // Store configuration here and keep changing gameplay data on the controller.
    public abstract class Action : ScriptableObject
    {
        public abstract void Act(StateController controller);
    }
}
