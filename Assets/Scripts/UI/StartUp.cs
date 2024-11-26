using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartUp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI awayText;
    [SerializeField] private TextMeshProUGUI standardMoneyText;
    [SerializeField] private TextMeshProUGUI fullMoneyText;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject startUpPanel;

    private float rewardMoney = -1;

    private void Start()
    {
        GameManager.SetGameState(Utilities.Enums.GameState.Menu);

        try
        {
            TimeSpan dateDifference = DateTime.UtcNow - DateTime.Parse(Player.instance.LastEnter);
            if (dateDifference.Days > 0)
            {
                awayText.text = "You were away for more than 24 hours!";
                rewardMoney = 24f * 60f * 60f / 1000f;
            }
            else
            {
                awayText.text = "You were away for " + dateDifference.Hours + "." + dateDifference.Minutes.ToString("00") + " hours!";
                rewardMoney = (float)dateDifference.TotalSeconds / 1000;
            }

            standardMoneyText.text = rewardMoney.ToString("0.00");
            fullMoneyText.text = (rewardMoney * 1.5f).ToString("0.00");
        }
        catch
        {
            Debug.LogError("StartUp: Start, previous date parsing error!");
        }
    }

    public void AddStartUpMoney(bool isStandardMoney)
    {
        Player.instance.EarnMoney(isStandardMoney ? rewardMoney : (rewardMoney * 1.5f));
        print("Money += " + (isStandardMoney ? rewardMoney : (rewardMoney * 1.5f)));

        gameManager.StartLastEnterTimer();
        GameManager.SetGameState(Utilities.Enums.GameState.Play);
     
        startUpPanel.SetActive(false);
    }

}
