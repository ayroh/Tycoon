using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Signals;

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

    public string LastEnter
    {
        get => PlayerPrefs.GetString("LastEnter");
        set => PlayerPrefs.SetString("LastEnter", value);
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
        if (!PlayerPrefs.HasKey("LastEnter")) PlayerPrefs.SetString("LastEnter", DateTime.UtcNow.ToString());
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
        Signals.OnMoneyUpdated?.Invoke();
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
        Signals.OnMoneyUpdated?.Invoke();
        return true;
    }

    #endregion
}
