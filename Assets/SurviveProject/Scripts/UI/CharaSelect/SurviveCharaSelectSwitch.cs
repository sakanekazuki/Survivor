using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviveCharaSelectSwitch : MonoBehaviour
{
    //[SerializeField] SurviveCharacterSelect scs;
    [SerializeField] int charaId;
    [SerializeField] GameObject lockObj;
    [SerializeField] GameObject unlockObj;

    [SerializeField] GameObject coinObj;
    [SerializeField] TMPro.TextMeshProUGUI coinText;

    private void Start()
    {
        bool lockStatus = SurviveProgressManager.Instance.charaUnlocks.Find(_ => _.ID == charaId).isLocked;
        lockObj.SetActive(lockStatus);
        unlockObj.SetActive(!lockStatus);
        SurviveCharacterSelect.OnUnlockChara += UpdateCharaSelectStatus;
    }

    private void UpdateCharaSelectStatus(int index)
    {
        if (index != charaId) return;
        bool lockStatus = SurviveProgressManager.Instance.charaUnlocks.Find(_ => _.ID == charaId).isLocked;
        lockObj.SetActive(lockStatus);
        unlockObj.SetActive(!lockStatus);
    }


    public GameObject GetCharaSelectObj()
    {
        bool lockStatus = SurviveProgressManager.Instance.charaUnlocks.Find(_ => _.ID == charaId).isLocked;
        if(lockStatus)
        {
            return lockObj;

        }
        else
        {
            return unlockObj;

        }
    }

    private void OnDestroy()
    {
        SurviveCharacterSelect.OnUnlockChara -= UpdateCharaSelectStatus;
    }


    public int GetID()
    {
        return charaId;
    }

    public void VisibleCoin(int id, int cost, int currentCoin)
    {
        bool lockStatus = SurviveProgressManager.Instance.charaUnlocks.Find(_ => _.ID == charaId).isLocked;
        if (lockStatus == false)
        {
            return;
        }
        if (id == charaId)
        {
            coinObj.SetActive(true);
            coinText.text = cost.ToString();
            if(cost > currentCoin)
            {
                coinText.color = Color.red;
            }
            else
            {
                coinText.color= Color.white;
            }
        }
        else
        {
            coinObj.SetActive(false);
        }
    }
}
