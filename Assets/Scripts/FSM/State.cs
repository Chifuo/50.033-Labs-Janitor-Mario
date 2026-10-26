using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "PluggableSM/State")]
    public class State : ScriptableObject
    {
        public Action[] setupActions = new Action[0];
        public Action[] actions = new Action[0];
        public EventAction[] eventTriggeredActions = new EventAction[0];
        public Action[] exitActions = new Action[0];
        [Tooltip("Checked in order. Put highest-priority decisions first.")]
        public Transition[] transitions = new Transition[0];
        public Color sceneGizmoColor = Color.grey;

        public void UpdateState(StateController controller)
        {
            int version = controller.StateVersion;
            DoActions(actions, controller);
            if (controller.StateVersion == version)
                CheckTransitions(controller);
        }

        // Scene startup can check transitions without running regular actions.
        public void CheckTransitions(StateController controller)
        {
            if (!controller.isActive || !controller.isActiveAndEnabled)
                return;

            int version = controller.StateVersion;
            foreach (Transition transition in transitions)
            {
                bool result = transition.decision.Decide(controller);
                controller.TransitionToState(result ? transition.trueState : transition.falseState);
                if (controller.StateVersion != version)
                    return;
            }
        }

        public void DoSetupActions(StateController controller)
        {
            DoActions(setupActions, controller);
        }

        public void DoExitActions(StateController controller)
        {
            DoActions(exitActions, controller);
        }

        public void DoEventTriggeredActions(StateController controller, ActionType type = ActionType.Default)
        {
            int version = controller.StateVersion;
            foreach (EventAction eventAction in eventTriggeredActions)
            {
                if (eventAction.type == type)
                    eventAction.action.Act(controller);
                if (controller.StateVersion != version)
                    return;
            }
        }

        private static void DoActions(Action[] list, StateController controller)
        {
            int version = controller.StateVersion;
            foreach (Action action in list)
            {
                action.Act(controller);
                if (controller.StateVersion != version)
                    return;
            }
        }
    }
}
