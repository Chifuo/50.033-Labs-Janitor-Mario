using UnityEngine;

[CreateAssetMenu(menuName = "Janitor Mario/Int Variable")]
public class IntVariable : ScriptableObject
{
    [SerializeField] private int baseValue = 0;
    [SerializeField] private int previousHighestValue;
    [SerializeField] private int baseDeliveries = 0;
    [SerializeField] private int baseShooed = 0; 
    [SerializeField] private bool baseGameOver = false;
    [System.NonSerialized] private int value;
    [System.NonSerialized] private int deliveries;
    [System.NonSerialized] private int shooed;
   
    [System.NonSerialized] private bool gameOver;

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

    public int Deliveries
    {
        get => deliveries;
        set
        {
            this.deliveries = value;
        }
    }
    public int Shooed
    {
        get => shooed;
        set
        {
            this.shooed = value;
        }
    }
    public bool GameOver
    {
        get => gameOver;
        set
        {
            this.gameOver = value;
        }
    }

    public int PreviousHighestValue => previousHighestValue;
    
    public void ApplyChange(string var,int amount)
    {
        switch (var)
        {
            case "Value":
                Value += amount;
                break;
            case "Deliveries":
                Deliveries += amount;
                break;
            case "Shooed":
                Shooed += amount;
                break;
        }
    }
    public void ApplyDefaultValues()
    {
        Value = baseValue;
        Deliveries = baseDeliveries;
        Shooed = baseShooed;
        GameOver = baseGameOver;

    }
    public void ApplyGameOver()
    {
        GameOver = true;
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
