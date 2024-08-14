using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI moneyText;

    private float money = 0;

    public void AddMoney(float plusMoney)
    {
        money += plusMoney;
        moneyText.text = (money).ToString("00.0");
    }

    private void Start()
    {
        AddMoney(Player.instance.Money);
    }

}
