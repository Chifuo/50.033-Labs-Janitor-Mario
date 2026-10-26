using System.Collections.Generic;
using UnityEngine;

namespace JanitorMario.FSM
{
    public enum RankProgressEventType { Promotion, Delivery }

    [System.Serializable]
    public struct RankProgressEvent
    {
        public RankProgressEventType type;
        public int count;
    }

    [CreateAssetMenu(menuName = "Janitor Mario/Rank Progress")]
    public class JanitorRankProgress : ScriptableObject
    {
        [Min(1)] public int milestoneInterval = 10;
        [SerializeField] private JanitorState rank = JanitorState.Rookie;
        [SerializeField] private long nextScoreMilestone = 10;
        [SerializeField] private int observedDeliveries;
        [SerializeField] private bool gameOver;
        [SerializeField] private float recoveryElapsed;
        [SerializeField] private float specialShooCooldown;
        [SerializeField] private List<RankProgressEvent> pendingEvents = new List<RankProgressEvent>();
        private int runVersion;

        public JanitorState Rank => rank;
        public long NextScoreMilestone => nextScoreMilestone;
        public bool GameOver => gameOver;
        public float RecoveryElapsed => recoveryElapsed;
        public float SpecialShooCooldown => specialShooCooldown;
        public int RunVersion => runVersion;
        public int PendingEventGroups => pendingEvents.Count;

        public bool NextEventIs(RankProgressEventType type)
        {
            return pendingEvents.Count > 0 && pendingEvents[0].type == type;
        }

        public void ResetForNewRun()
        {
            runVersion++;
            rank = JanitorState.Rookie;
            nextScoreMilestone = Mathf.Max(1, milestoneInterval);
            observedDeliveries = 0;
            gameOver = false;
            recoveryElapsed = 0f;
            specialShooCooldown = 0f;
            pendingEvents.Clear();
        }

        // Record milestones before a score listener loads the next scene.
        public void RecordScore(int score)
        {
            if (gameOver || score < nextScoreMilestone)
                return;

            int interval = Mathf.Max(1, milestoneInterval);
            int crossed = (int)(((long)score - nextScoreMilestone) / interval + 1);
            nextScoreMilestone += (long)crossed * interval;
            pendingEvents.Add(new RankProgressEvent
            {
                type = RankProgressEventType.Promotion,
                count = crossed
            });
        }

        public void RecordDeliveries(int count)
        {
            if (!gameOver && count > observedDeliveries)
            {
                pendingEvents.Add(new RankProgressEvent
                {
                    type = RankProgressEventType.Delivery,
                    count = count - observedDeliveries
                });
            }
            observedDeliveries = count;
        }

        public void MarkGameOver()
        {
            gameOver = true;
            pendingEvents.Clear();
        }

        public void ConsumeNextEvent()
        {
            RankProgressEvent next = pendingEvents[0];
            if (--next.count <= 0)
                pendingEvents.RemoveAt(0);
            else
                pendingEvents[0] = next;
        }

        public void DiscardCappedPromotions()
        {
            // Don't bank promotions at Power for use after a demotion.
            while (NextEventIs(RankProgressEventType.Promotion))
                pendingEvents.RemoveAt(0);
        }

        // Keep recovery's promotions
        public void DiscardRecoveryDeliveries()
        {
            pendingEvents.RemoveAll(item => item.type == RankProgressEventType.Delivery);
        }

        public void SaveControllerState(JanitorState currentRank, float elapsed, float cooldown,
            int controllerRunVersion)
        {
            if (controllerRunVersion != runVersion)
                return;

            rank = currentRank;
            recoveryElapsed = currentRank == JanitorState.Recovering ? elapsed : 0f;
            specialShooCooldown = cooldown;
        }
    }
}
