using UnityEngine;

namespace JanitorMario.FSM
{
    [DefaultExecutionOrder(400)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement))]
    public class BuffStateController : StateController
    {
        [SerializeField] private PlayerMovement player;
        [SerializeField] private IntVariable gameScore;
        public State speedUpState;
        private BuffProgress progress;
        private int runVersion;
        private bool restoring;

        public bool HasSpeedUpPickup => progress.PickupPending && !gameScore.GameOver;

        private void Awake()
        {
            if (player == null)
                player = GetComponent<PlayerMovement>();
        }

        public override void Start()
        {
            progress = player.BuffProgress;
            if (progress == null || startState == null || speedUpState == null ||
                startState == speedUpState)
            {
                Debug.LogError("Assign Buff Progress on GameManager and distinct Normal/SpeedUp states on BuffStateController.", this);
                enabled = false;
                return;
            }

            runVersion = progress.RunVersion;
            float savedElapsed = progress.PickupPending ? 0f : progress.Elapsed;
            bool savedActive = progress.SpeedUpActive;
            restoring = true;
            TransitionToState(savedActive ? speedUpState : startState);
            RestoreStateTimer(savedActive ? savedElapsed : 0f);
            restoring = false;

            // A pickup just before scene unload still needs to take effect.
            if (!gameScore.GameOver)
                CurrentState.CheckTransitions(this);
            SaveProgress();
        }

        public override void ResetToInitialState()
        {
            progress.ConsumePickup();
            base.ResetToInitialState();
            SaveProgress();
        }

        protected override void Update()
        {
            if (!isActive || gameScore.GameOver || progress.RunVersion != runVersion ||
                Time.deltaTime <= 0f)
                return;

            // Refresh before the countdown decision can expire the existing buff.
            if (player.isSpedup && progress.PickupPending)
            {
                progress.ConsumePickup();
                ResetStateTimer();
            }

            base.Update();
            SaveProgress();
        }

        public void ApplySpeedUp(bool active)
        {
            if (active)
                progress.ConsumePickup();
            player.SetSpeedUp(active);
            SaveProgress();
        }

        private void OnDisable()
        {
            SaveProgress();
        }

        private void SaveProgress()
        {
            if (progress != null && CurrentState != null && !restoring)
                progress.SaveControllerState(player.isSpedup, StateTimeElapsed, runVersion);
        }
    }
}
