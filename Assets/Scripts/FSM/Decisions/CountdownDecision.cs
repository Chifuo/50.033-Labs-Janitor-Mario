using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "PluggableSM/Decisions/Countdown")]
    public class CountdownDecision : Decision
    {
        [Min(0f)] public float duration = 3f;

        public override bool Decide(StateController controller)
        {
            return controller.CheckIfCountDownElapsed(duration);
        }
    }
}
