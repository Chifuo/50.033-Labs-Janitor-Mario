using UnityEngine;
using UnityEngine.Events;

//namespace Assets.Scripts.ScriptableObjects
//{
    public class GameEventListener<T> : MonoBehaviour
    {
        public GameEvent<T> Event;
        public UnityEvent<T> Response;
        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }
        public void OnEventRaised(T data)
        {
            Response.Invoke(data);
        }
    }
//}