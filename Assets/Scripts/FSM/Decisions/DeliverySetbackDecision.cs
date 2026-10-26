using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "PluggableSM/Decisions/Delivery Setback")]
    public class DeliverySetbackDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            MarioStateController mario = (MarioStateController)controller;
            return mario.HasPendingSetback;
        }
    }
}
