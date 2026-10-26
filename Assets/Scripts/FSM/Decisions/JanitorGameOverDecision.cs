using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "PluggableSM/Decisions/Janitor Game Over")]
    public class JanitorGameOverDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            MarioStateController mario = (MarioStateController)controller;
            return mario.IsGameOver;
        }
    }
}
