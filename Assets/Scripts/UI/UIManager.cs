using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI moneyText;

    public void RefreshMoney()
    {
        moneyText.text = (Player.instance.Money).ToString("00.0");
    }

    private void Start()
    {
        RefreshMoney();
    }

}
