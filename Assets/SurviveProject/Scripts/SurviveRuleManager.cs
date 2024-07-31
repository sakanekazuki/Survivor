using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームルール　
/// 時間
/// 
/// 
/// </summary>
public class SurviveRuleManager
{

    //現在の時間
    private float CurrentTime = 0f;

    //-1は無限（どこかしらで限界は作るべきではあるが） 一旦1分
    private float MaxTime = 60f;

    public bool GameEnd = false;

    
    public float GetCurrentTime()
    {
        return CurrentTime;
    }
    public void ResetCurrentTime()
    {
        CurrentTime = 0f;
    }
    public int GetCurrentSecond()
    {
        return (int)CurrentTime;
    }

#if DEBUG || UNITY_EDITOR
    public void AddCurrentTime(int time)
    {
        CurrentTime += time;
    }
#endif



    public void TimeUpdate()
    {
        //マネージャー側とかで呼び出し
        CurrentTime += Time.deltaTime;
    }


    public bool TimeEnd()
    {
        return CurrentTime >= MaxTime;
    }

    //とりあえず時間設定だけ
    public void Initialize(float time)
    {
        MaxTime = time;
        CurrentTime = 0f;
        GameEnd = false;



        // 音量再設定も混ぜさせて…
        // 修正：「死亡」→「リトライ」でゲームスタート時、SEの音量がデフォルトに戻る
        GameObject.Find("SettingManager")?.GetComponent<SurviveSetting>().InitSoundVolumes();
    }
}
