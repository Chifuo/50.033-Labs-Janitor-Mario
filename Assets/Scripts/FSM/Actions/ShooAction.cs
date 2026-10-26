using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "PluggableSM/Actions/Shoo")]
    public class ShooAction : Action
    {
        public ShooMode mode = ShooMode.Contact;
        [Min(1f)] public float strength = 1f;
        [Tooltip("World units. Only used by Area mode.")]
        [Min(0f)] public float radius = 32f;
        [Min(0f)] public float cooldown;

        public override void Act(StateController controller)
        {
            MarioStateController mario = (MarioStateController)controller;
            mario.PerformShoo(mode, strength, radius, cooldown);
        }
    }
}
