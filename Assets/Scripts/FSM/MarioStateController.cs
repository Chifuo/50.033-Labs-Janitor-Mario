using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace JanitorMario.FSM
{
    public enum JanitorState { Rookie, Equipped, Power, Recovering, Fired }
    public enum ShooMode { Contact, Area }

    [System.Serializable]
    public struct JanitorStateReference
    {
        public JanitorState rank;
        public State state;
    }

    // PlayerMovement (300) connects to GameManager before this starts.
    [DefaultExecutionOrder(350)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement))]
    public class MarioStateController : StateController
    {
        [SerializeField] private PlayerMovement player;
        [SerializeField] private AudioSource stateAudio;
        [Tooltip("Include the layers containing Goomba colliders.")]
        [SerializeField] private LayerMask shooLayers = ~0;
        [Tooltip("Map all five ranks to their State assets, for resuming across scenes.")]
        [SerializeField]
        private JanitorStateReference[] resumeStates = new[]
        {
            new JanitorStateReference { rank = JanitorState.Rookie },
            new JanitorStateReference { rank = JanitorState.Equipped },
            new JanitorStateReference { rank = JanitorState.Power },
            new JanitorStateReference { rank = JanitorState.Recovering },
            new JanitorStateReference { rank = JanitorState.Fired }
        };
        public UnityEvent recoveryStarted = new UnityEvent();
        public UnityEvent recoveryEnded = new UnityEvent();
        public UnityEvent specialShooPerformed = new UnityEvent();

        [SerializeField] private JanitorState janitorState;
        [SerializeField] private bool recovering;
        [SerializeField] private float shooCooldownRemaining;
        private JanitorRankProgress progress;
        private int runVersion;
        private bool restoring;

        public bool ControlsGameplay => isActiveAndEnabled && isActive &&
            CurrentState != null && progress != null &&
            progress.RunVersion == runVersion;
        public JanitorState CurrentJanitorState => janitorState;
        public bool HasPendingSetback => !IsGameOver &&
            janitorState != JanitorState.Recovering && janitorState != JanitorState.Fired &&
            progress.NextEventIs(RankProgressEventType.Delivery);
        public bool HasPendingPromotion => !IsGameOver &&
            (janitorState == JanitorState.Rookie || janitorState == JanitorState.Equipped) &&
            progress.NextEventIs(RankProgressEventType.Promotion);
        public bool IsGameOver => progress.GameOver;

        private void Awake()
        {
            if (player == null)
                player = GetComponent<PlayerMovement>();
        }

        public override void Start()
        {
            progress = player.RankProgress;
            if (progress == null)
            {
                Debug.LogError("Assign one shared Rank Progress asset to each scene's GameManager.", this);
                enabled = false;
                return;
            }

            foreach (JanitorState rank in System.Enum.GetValues(typeof(JanitorState)))
            {
                State state = FindResumeState(rank);
                if (state == null || state == remainState)
                {
                    Debug.LogError("Assign all five Resume States on MarioStateController.", this);
                    enabled = false;
                    return;
                }
            }

            runVersion = progress.RunVersion;
            float savedRecoveryTime = progress.RecoveryElapsed;
            shooCooldownRemaining = progress.SpecialShooCooldown;
            restoring = true;
            TransitionToState(FindResumeState(progress.Rank));
            RestoreStateTimer(savedRecoveryTime);
            restoring = false;

            ResolveQueuedTransitions();
            SaveProgress();
        }

        private State FindResumeState(JanitorState rank)
        {
            foreach (JanitorStateReference reference in resumeStates)
                if (reference.rank == rank)
                    return reference.state;
            return null;
        }

        public override void ResetToInitialState()
        {
            shooCooldownRemaining = 0f;
            restoring = true;
            base.ResetToInitialState();
            restoring = false;
            SaveProgress();
        }

        protected override void Update()
        {
            if (!ControlsGameplay || Time.deltaTime <= 0f)
                return;

            shooCooldownRemaining = Mathf.Max(0f, shooCooldownRemaining - Time.deltaTime);
            PreparePendingEvents();
            base.Update();
            ResolveQueuedTransitions();
            SaveProgress();
        }

        private void PreparePendingEvents()
        {
            if (IsGameOver)
                return;
            if (janitorState == JanitorState.Power)
                progress.DiscardCappedPromotions();
            else if (janitorState == JanitorState.Recovering)
                progress.DiscardRecoveryDeliveries();
        }

        private void ResolveQueuedTransitions()
        {
            while (ControlsGameplay)
            {
                PreparePendingEvents();
                int version = StateVersion;
                CurrentState.CheckTransitions(this);
                if (StateVersion == version)
                    break;
            }
        }

        private void OnDisable()
        {
            SaveProgress();
        }

        private void SaveProgress()
        {
            if (progress != null && CurrentState != null && !restoring)
                progress.SaveControllerState(janitorState, StateTimeElapsed,
                    shooCooldownRemaining, runVersion);
        }

        public void Shoo()
        {
            if (ControlsGameplay && !IsGameOver && janitorState != JanitorState.Fired)
                TriggerEvent(ActionType.Attack);
        }

        public void EnterJanitorState(JanitorState state, AudioClip entrySound)
        {
            if (!restoring)
            {
                if (state == JanitorState.Fired)
                    progress.MarkGameOver();
                else if (HasPendingSetback || HasPendingPromotion)
                    progress.ConsumeNextEvent();
            }

            janitorState = state;

            if (state == JanitorState.Recovering)
            {
                recovering = true;
                recoveryStarted.Invoke();
            }
            else
            {
                EndRecovery();
            }

            if (!restoring && entrySound != null && stateAudio != null)
                stateAudio.PlayOneShot(entrySound);

            if (state == JanitorState.Fired)
                player.BeginDeath();
            SaveProgress();
        }

        public void EndRecovery()
        {
            if (!recovering)
                return;
            recovering = false;
            recoveryEnded.Invoke();
        }

        public void PerformShoo(ShooMode mode, float strength, float radius, float cooldown)
        {
            if (!ControlsGameplay || IsGameOver || janitorState == JanitorState.Fired ||
                !player.isActiveAndEnabled ||
                Time.timeScale <= 0f || (mode == ShooMode.Area && shooCooldownRemaining > 0f))
                return;

            if (mode == ShooMode.Contact)
            {
                player.ShooContact(strength);
                return;
            }

            shooCooldownRemaining = Mathf.Max(0f, cooldown);
            SaveProgress();
            var affected = new HashSet<GoombaShoo>();
            foreach (Collider2D hit in Physics2D.OverlapCircleAll(player.transform.position,
                Mathf.Max(0f, radius), shooLayers))
            {
                GoombaShoo enemy = hit.GetComponentInParent<GoombaShoo>();
                if (enemy != null && affected.Add(enemy))
                    enemy.TryAreaShoo(player, strength);
            }
            specialShooPerformed.Invoke();
        }
    }
}
