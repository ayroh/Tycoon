using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI fpsText;

    int frameCount = 0;

    private void Update()
    {
        if(++frameCount == 20)
        {
            fpsText.text = (1 / Time.deltaTime).ToString("00");
            frameCount = 0;
        }
    }

}
