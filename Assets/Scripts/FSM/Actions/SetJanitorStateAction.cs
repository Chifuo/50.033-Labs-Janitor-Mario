using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "PluggableSM/Actions/Set Janitor State")]
    public class SetJanitorStateAction : Action
    {
        public JanitorState state;
        public AudioClip entrySound;

        public override void Act(StateController controller)
        {
            MarioStateController mario = (MarioStateController)controller;
            mario.EnterJanitorState(state, entrySound);
        }
    }
}
