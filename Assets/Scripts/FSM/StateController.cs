using UnityEngine;

namespace JanitorMario.FSM
{
    public abstract class StateController : MonoBehaviour
    {
        public State startState;
        [Tooltip("Optional placeholder asset used as a stay-in-current-state destination.")]
        public State remainState;
        public bool isActive = true;

        [SerializeField] private State currentState;
        [SerializeField] private State previousState;
        [SerializeField] private float stateTimeElapsed;
        private bool isChangingState;

        public State CurrentState => currentState;
        public State PreviousState => previousState;
        public float StateTimeElapsed => stateTimeElapsed;
        // A reset can re-enter the same State asset.
        internal int StateVersion { get; private set; }

        public virtual void Start()
        {
            ResetToInitialState();
        }

        protected virtual void Update()
        {
            if (!isActive || currentState == null || Time.deltaTime <= 0f)
                return;

            stateTimeElapsed += Time.deltaTime;
            currentState.UpdateState(this);
        }

        public virtual void OnSetupState()
        {
            if (currentState != null)
                currentState.DoSetupActions(this);
        }

        public virtual void OnExitState()
        {
            if (currentState != null)
                currentState.DoExitActions(this);
        }

        public void TransitionToState(State nextState)
        {
            if (isChangingState || nextState == null || nextState == remainState || nextState == currentState)
                return;

            ChangeState(nextState);
        }

        public virtual void ResetToInitialState()
        {
            if (startState == null || startState == remainState)
            {
                Debug.LogWarning("Assign a Start State distinct from Remain State before starting the FSM.", this);
                return;
            }

            if (isChangingState)
                return;
            isActive = true;
            ChangeState(startState);
            previousState = null;
        }

        public void TriggerEvent(ActionType type = ActionType.Default)
        {
            if (!isActiveAndEnabled || !isActive || isChangingState ||
                currentState == null || Time.timeScale <= 0f)
                return;

            currentState.DoEventTriggeredActions(this, type);
        }

        public bool CheckIfCountDownElapsed(float duration)
        {
            return stateTimeElapsed >= Mathf.Max(0f, duration);
        }

        public void ResetStateTimer()
        {
            stateTimeElapsed = 0f;
        }

        protected void RestoreStateTimer(float elapsed)
        {
            stateTimeElapsed = Mathf.Max(0f, elapsed);
        }

        private void ChangeState(State nextState)
        {
            isChangingState = true;
            try
            {
                OnExitState();
                previousState = currentState;
                currentState = nextState;
                stateTimeElapsed = 0f;
                StateVersion++;
                OnSetupState();
            }
            finally
            {
                isChangingState = false;
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (currentState == null)
                return;

            Gizmos.color = currentState.sceneGizmoColor;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
