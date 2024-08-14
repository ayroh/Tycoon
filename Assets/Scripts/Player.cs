using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    public string Name
    {
        get => PlayerPrefs.GetString("Name");
        set => PlayerPrefs.SetString("Name", value);
    }

    public float Money 
    { 
        get => PlayerPrefs.GetFloat("Money");
        set => PlayerPrefs.SetFloat("Money", value);
    }

    protected override void Awake()
    {
        base.Awake();
        CreatePrefs();
    }

    private void CreatePrefs()
    {
        if (!PlayerPrefs.HasKey("Name")) PlayerPrefs.SetString("Name", "NULL");
        if (!PlayerPrefs.HasKey("Money")) PlayerPrefs.SetFloat("Money", 0);
    }

    #region Money

    public void EarnMoney(float amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Player: AddMoney, Trying to add negative amount of money!");
            return;
        }
        Money += amount;
    }

    public bool HasEnoughMoney(float amount) => Money >= amount;

    public bool SpendMoney(float amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Player: SpendMoney, Trying to spend negative amount of money!");
            return false;
        }

        if (!HasEnoughMoney(amount))
            return false;

        Money -= amount;
        return true;
    }

    #endregion
}
