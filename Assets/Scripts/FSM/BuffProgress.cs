using UnityEngine;

namespace JanitorMario.FSM
{
    [CreateAssetMenu(menuName = "Janitor Mario/Buff Progress")]
    public class BuffProgress : ScriptableObject
    {
        [SerializeField] private bool speedUpActive;
        [SerializeField] private float elapsed;
        [SerializeField] private bool pickupPending;
        private int runVersion;

        public bool SpeedUpActive => speedUpActive;
        public float Elapsed => elapsed;
        public bool PickupPending => pickupPending;
        public int RunVersion => runVersion;

        public void RequestSpeedUp()
        {
            pickupPending = true;
        }

        public void ConsumePickup()
        {
            pickupPending = false;
        }

        public void ResetForNewRun()
        {
            runVersion++;
            speedUpActive = false;
            elapsed = 0f;
            pickupPending = false;
        }

        public void SaveControllerState(bool active, float timeElapsed, int controllerRunVersion)
        {
            // Don't let the departing scene overwrite a restart's reset.
            if (controllerRunVersion != runVersion)
                return;

            speedUpActive = active;
            elapsed = active ? timeElapsed : 0f;
        }
    }
}
