using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Enums;

public class GameManager : MonoBehaviour
{
    private IEnumerator timerNumerator;

    public static GameState GameState { get; private set; } = GameState.Play;

    private void Start()
    {
        StartCoroutine(timerNumerator = Timer());
    }

    public IEnumerator Timer()
    {
        string lastEnter = Player.instance.LastEnter;

        try
        {
            DateTime previousDate = DateTime.Parse(lastEnter);
            Player.instance.Money += (float)(DateTime.UtcNow - previousDate).TotalSeconds / 1000;
            print("Money+: " + ((float)(DateTime.UtcNow - previousDate).TotalSeconds / 1000));
        }
        catch
        {
            Debug.LogError("GameManager: Timer, previous date parsing error!");
            timerNumerator = null;
            yield break;
        }

        WaitForSecondsRealtime waitTime = new WaitForSecondsRealtime(10);
        while (true)
        {
            Player.instance.LastEnter = DateTime.UtcNow.ToString();
            yield return waitTime;
        }
    }


    public static void SetGameState(GameState newState)
    {
        GameState = newState;
    }

}
