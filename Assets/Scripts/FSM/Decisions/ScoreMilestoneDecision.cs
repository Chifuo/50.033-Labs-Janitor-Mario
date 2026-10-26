using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "PluggableSM/Decisions/Score Milestone")]
    public class ScoreMilestoneDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            MarioStateController mario = (MarioStateController)controller;
            return mario.HasPendingPromotion;
        }
    }
}
