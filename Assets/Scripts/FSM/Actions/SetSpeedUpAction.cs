using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "PluggableSM/Actions/Set Speed Up")]
    public class SetSpeedUpAction : Action
    {
        public bool speedUp;

        public override void Act(StateController controller)
        {
            BuffStateController buff = (BuffStateController)controller;
            buff.ApplySpeedUp(speedUp);
        }
    }
}
