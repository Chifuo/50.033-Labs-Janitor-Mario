using UnityEngine;

[CreateAssetMenu(menuName = "Janitor Mario/Int Variable")]
public class IntVariable : ScriptableObject
{
    [SerializeField] private int value;
    [SerializeField] private int previousHighestValue;

    public int Value
    {
        get => value;
        set
        {
            this.value = value;
            if (value > previousHighestValue)
            {
                previousHighestValue = value;
                SaveHighScore();
            }
        }
    }

    public int PreviousHighestValue => previousHighestValue;

    public void ApplyChange(int amount)
    {
        Value += amount;
    }

    public void ResetHighestValue()
    {
        previousHighestValue = 0;
        SaveHighScore();
    }

    private void SaveHighScore()
    {
        // Save the asset so the high score survives another Editor Play session.
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
#endif
    }
}
