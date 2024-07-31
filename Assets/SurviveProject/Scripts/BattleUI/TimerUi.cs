using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerUi : MonoBehaviour
{

    public TextMeshProUGUI timerText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var sgm = SurviveGameManager.GetInstance();
        if (sgm != null)
        {
            var time = sgm.surviveRuleManager.GetCurrentSecond();

            var tSpan = new TimeSpan(0,0,time);
            if (tSpan.Hours > 0)
            {
                var mmss = tSpan.ToString(@"hh\:mm\:ss");
            timerText.text = mmss;
            }
            else
            {
                var mmss = tSpan.ToString(@"mm\:ss");
            timerText.text = mmss;
            }
        }
    }
}
