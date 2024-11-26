using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Signals;

public class UpgradeMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image coinImage;

    private float cost;
    private bool CanUpgrade => cost != -1;

    public void SetCost(float newCost)
    {
        cost = newCost;
        costText.text = cost.ToString("0.0");
        RefreshUpgradeButton();

        if (!CanUpgrade)
            FinishUpgrade();
    }

    public void Upgrade(float newCost)
    {
        SetCost(newCost);
    }

    private void RefreshUpgradeButton()
    {
        upgradeButton.interactable = Player.instance.Money >= cost && CanUpgrade;
    }

    public void FinishUpgrade()
    {
        upgradeText.text = "Upgraded";
        upgradeText.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        costText.gameObject.SetActive(false);
        coinImage.gameObject.SetActive(false);
        upgradeButton.interactable = false;
    }

    private void OnEnable()
    {
        Signals.OnMoneyUpdated += RefreshUpgradeButton;
        RefreshUpgradeButton();
    }

    private void OnDisable()
    {
        Signals.OnMoneyUpdated -= RefreshUpgradeButton;
    }
}
