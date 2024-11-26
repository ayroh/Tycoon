using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Enums;

public class UpgradeMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StatsMenu statsMenu;
    [SerializeField] private List<UpgradeMenu> menus;
    [SerializeField] private Exhibition exhibition;

    private void Awake()
    {
        if(statsMenu == null || menus.Count != 3)
        {
            Debug.LogError("UpgradeMenuController, Menu references are empty");
            return;
        }
    }

    public void Initialise(StatsMenu.Stats newStats, float newExhibitionCost, float newCapacityCost, float newTimeCost)
    {
        statsMenu.Initialise(ref newStats);
        menus[0].SetCost(newExhibitionCost);
        menus[1].SetCost(newCapacityCost);
        menus[2].SetCost(newTimeCost);
    }

    public void SetUpgradeCost(UpgradeType upgradeType, float newCost) => menus[(int)upgradeType].SetCost(newCost);

    public void SetStat(UpgradeType upgradeType, params float[] values)
    {
        switch (upgradeType)
        {
            case UpgradeType.Exhibition:
                statsMenu.SetLevel((int)values[0], values[1]);
                break;
            case UpgradeType.Capacity:
                statsMenu.SetCapacity((int)values[0]);
                break;
            case UpgradeType.Time:
                statsMenu.SetTime((int)values[0]);
                break;
        }
    }



    private void OnEnable()
    {
        GameManager.SetGameState(GameState.Menu);
    }

    private void OnDisable()
    {
        GameManager.SetGameState(GameState.Play);
    }
}
