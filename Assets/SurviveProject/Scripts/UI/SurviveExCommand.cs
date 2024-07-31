using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

/// <summary>
/// 情報保存クラス
/// </summary>
[Serializable]
public class SurviveExCmdClass
{
    public int cnt = 0;
}

public class SurviveExCommand : MonoBehaviour
{
    private int exCmdClearCnt = 0;
    private List<KeyCode> exCmd = new List<KeyCode>() {
        KeyCode.S,KeyCode.A,KeyCode.M,KeyCode.U,KeyCode.R,KeyCode.A,KeyCode.I
    };

    [SerializeField] public AudioClip CmdSfx;

    protected const string _saveFileName = "survive_cmd.settings";
    private SurviveExCmdClass surviveExCmdData;

    private int addNum = 5000;

#if UNITY_EDITOR
    private int CmdClearMax = 99999;
#else
    //リリース時には1にしたい
    private int CmdClearMax = 99999;
#endif

    private void OnEnable()
    {
        LoadCntDataSerial();
        resetKeys();
    }

    private void LoadCntDataSerial()
    {
        if (surviveExCmdData == null)
        {
            SurviveExCmdClass _surviveExCmdData = (SurviveExCmdClass)MMSaveLoadManager.Load(typeof(SurviveExCmdClass), _saveFileName);
            if (_surviveExCmdData != null)
            {
                surviveExCmdData = _surviveExCmdData;
            }
            if (surviveExCmdData == null)
            {
                surviveExCmdData = new SurviveExCmdClass();
            }
        }
    }


    private void resetKeys()
    {
        exCmdClearCnt = 0;
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (exCmdClearCnt < exCmd.Count)
            {
                if (Input.GetKeyDown(exCmd[exCmdClearCnt]))
                {
#if UNITY_EDITOR
                    Debug.Log($"key:{exCmd[exCmdClearCnt].ToString()}");
#endif
                    exCmdClearCnt++;

                    if (exCmdClearCnt >= exCmd.Count)
                    {
                        cmdClear();
                        exCmdClearCnt = 0;
                    }
                }
                else
                {
                    exCmdClearCnt = 0;
                }
            }
        }
    }

    private void cmdClear()
    {
#if UNITY_EDITOR
        Debug.Log($"Cmd:OK");
#endif
        if (surviveExCmdData != null)
        {
            if (surviveExCmdData.cnt < CmdClearMax)
            {
                surviveExCmdData.cnt++;
                MMSaveLoadManager.Save(surviveExCmdData, _saveFileName);

#if UNITY_EDITOR
                Debug.Log($"Cmd:Cnt OK {surviveExCmdData.cnt}");
#endif
                PlaySe(CmdSfx);

                //お金を増やす
                SurviveProgressManager.Instance.Coin += addNum;
                SurviveSaveEvent.Trigger();
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"Cmd:Cnt OVER {surviveExCmdData.cnt}");
#endif
            }
        }
    }


    private void PlaySe(AudioClip ac)
    {
        MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
        options.Loop = false;
        options.Location = Vector3.zero;
        options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;

        MMSoundManagerSoundPlayEvent.Trigger(ac, options);
    }

}
