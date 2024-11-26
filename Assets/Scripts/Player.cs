using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Utilities.Constants;
using Utilities.Enums;
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

    #region Exhibition

    public void SaveExhibition(Exhibition.Data exhibitionData, ExhibitionType type)
    {
        try 
        {
            File.WriteAllText(Application.persistentDataPath + "/Exhibition" + type + ".json", JsonConvert.SerializeObject(exhibitionData));
        }
        catch
        {
            Debug.LogError("Player: SaveExhibition, catched an error while writing!");
        }
    }

    public Exhibition.Data LoadExhibition(ExhibitionType type)
    {
        if(!File.Exists(Application.persistentDataPath + "/Exhibition" + type + ".json"))
        {
            Exhibition.Data newData = new Exhibition.Data()
            {
                level = 1,
                capacity = Constants.InitialExhibitionCapacity,
                time = Constants.InitialExhibitionTime
            };

            File.WriteAllText(Application.persistentDataPath + "/Exhibition" + type + ".json", JsonConvert.SerializeObject(newData));
            return newData;
        }

        try
        {
            string text = File.ReadAllText(Application.persistentDataPath + "/Exhibition" + type + ".json");
            Exhibition.Data exhibitionData = JsonConvert.DeserializeObject<Exhibition.Data>(text);
            return exhibitionData;
        }
        catch
        {
            Debug.LogError("Player: SaveExhibition, catched an error while reading!");
            return null;
        }
    }

    #endregion
}
