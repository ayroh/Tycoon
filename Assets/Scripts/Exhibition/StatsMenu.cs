using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Constants;

public class StatsMenu : MonoBehaviour
{
    public struct Stats
    {
        public Stats(int level, float income,  int capacity, int time)
        {
            this.level = level;
            this.income = income;
            this.capacity = capacity;
            this.time = time;
        }

        public int level;
        public float income;
        public int capacity;
        public int time;
    }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI sliderLevelText;
    [SerializeField] private Slider levelSlider;
    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private TextMeshProUGUI capacityText;
    [SerializeField] private TextMeshProUGUI timeText;


    public void Initialise(ref Stats newStats)
    {
        SetLevel(newStats.level, newStats.income);
        SetCapacity(newStats.capacity);
        SetTime(newStats.time);
    }

    public void SetLevel(int level, float income)
    {
        levelText.text = "Level " + level.ToString();
        incomeText.text = income.ToString("0.0");
        sliderLevelText.text = "Level " + (level - level % Constants.LevelBaseDivider + Constants.LevelBaseDivider).ToString();
        levelSlider.value = (float)(level % Constants.LevelBaseDivider) / Constants.LevelBaseDivider;
    }

    public void SetCapacity(int capacity)
    {
        capacityText.text = capacity.ToString();
    }

    public void SetTime(int time)
    {
        timeText.text = time.ToString();
    }


}
