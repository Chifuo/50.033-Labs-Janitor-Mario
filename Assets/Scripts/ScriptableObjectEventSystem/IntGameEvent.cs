using UnityEngine;

[CreateAssetMenu(fileName = "IntGameEvent", menuName = "Scriptable Objects/IntGameEvent")]
public class IntGameEvent : GameEvent<int>
{
    public void Raise() => Raise(new int());
}
