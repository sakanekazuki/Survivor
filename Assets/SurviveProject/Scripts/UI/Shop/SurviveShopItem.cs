using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class SurviveShopItem : MonoBehaviour, IPointerEnterHandler
{
    public int ID;
    public GameObject[] starGroups;
    [SerializeField] Image icon;
    [SerializeField] SurviveShop surviveShop;
    int cost;

    public void OnPointerEnter(PointerEventData eventData)
    {
        surviveShop.SelectItem(ID);
    }

    public void SetLevelLimitBG(int count)
    {
        for(int i = 0; i< starGroups.Length; i++)
        {
            if (i < count)
            {
                starGroups[i].SetActive(true);
            }
            else
            {
                starGroups[i].SetActive(false);
            }
        }
    }

    public void SetLevelIcon()
    {
        var level = SurviveProgressManager.Instance.permanentEffectLevels[ID];
        for (int i = 0; i < starGroups.Length; i++)
        {
            var on = starGroups[i].transform.Find("ON").gameObject;
            if (i < level)
            {
                on.SetActive(true);
            }
            else
            {
                break;
            }
        }
    }

    public void ResetLevelIcon()
    {
        for (int i = 0; i < starGroups.Length; i++)
        {
            var on = starGroups[i].transform.Find("ON").gameObject;
            on.SetActive(false);
        }
    }

    public void SetItemIcon(Sprite _icon)
    {
        icon.sprite = _icon;
    }

    private void Start()
    {
        SurviveShop.OnUpgrade += CheckUpgrade;
        SurviveShop.OnReset += ResetLevelIcon;
    }

    private void OnDestroy()
    {
        SurviveShop.OnUpgrade -= CheckUpgrade;
        SurviveShop.OnReset -= ResetLevelIcon;
    }

    private void CheckUpgrade(int _ID)
    {
        if(ID == _ID)
        {
            SetLevelIcon();
        }
    }
}
