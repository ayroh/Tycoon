using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Enums;

public class GameManager : MonoBehaviour
{
    private IEnumerator lastEnterTimerNumerator;

    public static GameState GameState { get; private set; } = GameState.Play;

    

    public void StartLastEnterTimer()
    {
        if (lastEnterTimerNumerator != null) StopCoroutine(lastEnterTimerNumerator);
        StartCoroutine(lastEnterTimerNumerator = LastEnterTimer());
    }
    private IEnumerator LastEnterTimer()
    {
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
