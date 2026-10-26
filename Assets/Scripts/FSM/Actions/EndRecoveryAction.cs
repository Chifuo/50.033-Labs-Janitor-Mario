using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "PluggableSM/Actions/End Recovery")]
    public class EndRecoveryAction : Action
    {
        public override void Act(StateController controller)
        {
            MarioStateController mario = (MarioStateController)controller;
            mario.EndRecovery();
        }
    }
}
