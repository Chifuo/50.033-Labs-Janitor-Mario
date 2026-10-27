using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "PluggableSM/Decisions/Speed Up Pickup")]
    public class SpeedUpPickupDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            BuffStateController buff = (BuffStateController)controller;
            return buff.HasSpeedUpPickup;
        }
    }
}
